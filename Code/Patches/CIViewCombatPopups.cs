using System.Collections.Generic;

using HarmonyLib;

using PhantomBrigade;
using PBCIViewPopups = CIViewPopups;
using PBCIViewCombatPopups = CIViewCombatPopups;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	using AllocateDelegate = System.Func<List<PBCIViewPopups.PopupNestedSegment>>;
	using ReleaseDelegate = System.Action<List<PBCIViewPopups.PopupNestedSegment>>;
	using AnimateDelegate = System.Action<PBCIViewPopups.PopupNestedSegment, int, float, Vector2, float, Vector2, Color>;

	static class CIViewCombatPopups
	{
		private sealed class DamageTracker
		{
			internal CombatEntity CombatUnit;
			internal string AnimationKey;
			internal string Text;
			internal float AccumulatedValue;
			internal double TimeLast;
			internal bool Dirty;
		}

		internal sealed class PopupText
		{
			internal int PopupID;
			internal int Index;
			internal double StartTime;
			internal string AnimationKey;
			internal CombatEntity CombatUnit;
			internal string Text;
			internal int SpriteIDBase;
			internal List<PBCIViewPopups.PopupNestedSegment> Segments;
			internal List<int> SpriteDisposal;
			internal int Slot;
			internal Vector2 Position;
			internal bool IsSliding;
			internal double SlideStartTime;
			internal Vector2 SlideToOffset;
			internal int SlideSlot;
			internal Vector2 SlidePosition;
		}

		internal static bool logEnabled = false;

		private const float spriteHeight = 24f;
		private const float slotHeight = spriteHeight + 4f;
		private const float slideAnimationTime = 0.150f;
		private const double slideThreshold = 0.5;

		private static bool isInitialized;
		private static Traverse instTraverse;
		private static AllocateDelegate getPooledSegmentList;
		private static ReleaseDelegate releasePooledSegmentList;
		private static AnimateDelegate animateSegment;
		private static Dictionary<string, PBCIViewPopups.PopupDefinition> textAnimationsLookup;
		private static Dictionary<char, PBCIViewPopups.CharacterSprite> characterSpriteLookup;
		private static Camera worldCamera;
		private static Camera uiCamera;

		private static readonly Dictionary<string, List<DamageTracker>> damageTrackers = new Dictionary<string, List<DamageTracker>>();
		private static readonly List<DamageTracker> collectedTrackers = new List<DamageTracker>();
		private static readonly Dictionary<int, List<PopupText>> popupsLookup = new Dictionary<int, List<PopupText>>();
		private static readonly List<PopupText> slidingPopups = new List<PopupText>();
		private static readonly List<PopupText> animatingPopups = new List<PopupText>();
		private static readonly System.Comparison<PopupText> recencyComparison = new System.Comparison<PopupText>(ComparePopupsByRecency);
		private static readonly System.Comparison<PopupText> slotComparison = new System.Comparison<PopupText>(ComparePopupsBySlot);

		internal static void Clear()
		{
			if (PBCIViewCombatPopups.ins == null)
			{
				return;
			}
			if (!InitializeInstance(PBCIViewCombatPopups.ins))
			{
				return;
			}

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) CIViewCombatPopups.Clear | time: {2:F3}",
					ModLink.modIndex,
					ModLink.modId,
					Time.realtimeSinceStartupAsDouble);
			}

			damageTrackers.Clear();
			foreach (var kvp in popupsLookup)
			{
				foreach (var popup in kvp.Value)
				{
					if (popup.Segments != null)
					{
						releasePooledSegmentList(popup.Segments);
					}
				}
			}
			popupsLookup.Clear();
		}

		internal static void AnimateAll(PBCIViewPopups inst)
		{
			if (inst != PBCIViewCombatPopups.ins)
			{
				return;
			}
			if (!IDUtility.IsGameState(GameStates.combat))
			{
				return;
			}

			if (!InitializeInstance((PBCIViewCombatPopups)inst))
			{
				return;
			}

			CollectTrackers();
			BuildPopups((PBCIViewCombatPopups)inst);

			foreach (var entry in popupsLookup)
			{
				AnimateText((PBCIViewCombatPopups)inst, entry.Value);
			}
		}

		internal static void AddDamageText(
			CombatEntity unitCombat,
			string animKey,
			float value,
			string format)
		{
			if (string.IsNullOrEmpty(animKey))
			{
				return;
			}

			if (value.RoughlyEqual(0f))
			{
				return;
			}

			var now = Time.realtimeSinceStartupAsDouble;

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) CIViewCombatPopups.AddDamageText | time: {2:F3} | unit: C-{3} | type: {4} | value: {5} | position: {6}",
					ModLink.modIndex,
					ModLink.modId,
					now,
					unitCombat.id.id,
					animKey,
					value,
					unitCombat.position.v + unitCombat.localCenterPoint.v);
			}

			UpdateTracker(
				unitCombat,
				animKey,
				value,
				format,
				now);
		}

		static void AnimateText(PBCIViewCombatPopups inst, List<PopupText> popups)
		{
			SortPopups(popups);
			foreach (var popup in slidingPopups)
			{
				var elapsedTime = (Time.unscaledTime - popup.SlideStartTime) * inst.playbackSpeed;
				if (elapsedTime >= slideAnimationTime)
				{
					popup.IsSliding = false;
					popup.Slot = popup.SlideSlot;
					animatingPopups.Add(popup);
					continue;
				}

				var (positionOK, position) = GetPosition(inst, popup);
				if (!positionOK)
				{
					HidePopup(inst, popup);
					continue;
				}

				var interpolantShared = Mathf.Clamp01((float)elapsedTime / slideAnimationTime);
				var definition = textAnimationsLookup[popup.AnimationKey];
				SlidePopup(
					inst,
					popup,
					definition,
					position,
					interpolantShared);
			}

			foreach (var popup in animatingPopups)
			{
				DisposeSprites(inst, popup);

				var definition = textAnimationsLookup[popup.AnimationKey];
				var elapsedTime = (Time.unscaledTime - popup.StartTime) * inst.playbackSpeed;
				if (elapsedTime >= definition.timeTotal)
				{
					DestroyPopup(inst, popup);
					continue;
				}

				var (positionOK, position) = GetPosition(inst, popup);
				if (!positionOK)
				{
					HidePopup(inst, popup);
					continue;
				}

				var slotOffset = Vector2.up * (popup.Slot * slotHeight);
				var interpolantShared = Mathf.Clamp01((float)elapsedTime / definition.timeTotal);

				if (logEnabled)
				{
					Debug.LogFormat(
						"Mod {0} ({1}) CIViewCombatPopups.AnimateText | time: {2} | popup: {3} | key: {4} | start time: {5} | slot: {6} | unit position: {7} | slot offset: {8} | interpolant: {9} | segment count: {10}",
						ModLink.modIndex,
						ModLink.modId,
						Time.realtimeSinceStartupAsDouble,
						popup.PopupID,
						definition.key,
						popup.StartTime,
						popup.Slot,
						position,
						slotOffset,
						interpolantShared,
						popup.Segments.Count);
				}

				AnimatePopup(
					popup,
					definition,
					position,
					slotOffset,
					interpolantShared);
			}
		}

		static void AnimatePopup(
			PopupText popup,
			PBCIViewPopups.PopupDefinition definition,
			Vector2 position,
			Vector2 slotOffset,
			float interpolantShared)
		{
			if (definition.timeCurveUsed)
			{
				interpolantShared = definition.timeCurve.Evaluate(interpolantShared);
			}
			position += slotOffset;
			if (definition.positionRounded)
			{
				position = new Vector2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
			}
			popup.Position = position;
			var rotationBase = definition.rotation;
			if (definition.rotationAnimation)
			{
				var t = definition.rotationCurveUsed ? definition.rotationCurve.Evaluate(interpolantShared) : interpolantShared;
				rotationBase = Mathf.Lerp(definition.rotation, definition.rotationTo, t);
			}
			var sizeBase = definition.size;
			if (definition.sizeAnimation)
			{
				var t = definition.sizeCurveUsed ? definition.sizeCurve.Evaluate(interpolantShared) : interpolantShared;
				sizeBase = Vector2.Lerp(definition.size, definition.sizeTo, t);
			}
			Color colorBase = definition.color;
			if (definition.colorAnimation)
			{
				colorBase = !definition.colorGradientUsed ? Color.Lerp(definition.color, definition.colorTo, interpolantShared) : definition.colorGradient.Evaluate(interpolantShared);
			}

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) CIViewCombatPopups.AnimatePopup | time: {2} | popup: {3} | key: {4} | position: {5} | offset: {6} | interpolant: {7} | segments: {8}",
					ModLink.modIndex,
					ModLink.modId,
					Time.realtimeSinceStartupAsDouble,
					popup.PopupID,
					definition.key,
					position,
					slotOffset,
					interpolantShared,
					popup.Segments.Count);
			}

			for (var i = 0; i < popup.Segments.Count; i += 1)
			{
				AnimateSegment(popup.Segments[i], popup.SpriteIDBase + i, interpolantShared, position, rotationBase, sizeBase, colorBase);
			}
		}

		static void AnimateSegment(
			PBCIViewPopups.PopupNestedSegment segment,
			int spriteIDLocal,
			float interpolantShared,
			Vector2 positionBase,
			float rotationBase,
			Vector2 sizeBase,
			Color colorBase)
		{
			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) CIViewCombatPopups.AnimateSegment | time: {2} | sprite ID: {3} | sprite: {4} | position base: {5} | interpolant: {6}",
					ModLink.modIndex,
					ModLink.modId,
					Time.realtimeSinceStartupAsDouble,
					spriteIDLocal,
					segment.sprite,
					positionBase,
					interpolantShared);
			}

			animateSegment(segment, spriteIDLocal, interpolantShared, positionBase, rotationBase, sizeBase, colorBase);
		}

		static void SlidePopup(
			PBCIViewPopups inst,
			PopupText popup,
			PBCIViewPopups.PopupDefinition definition,
			Vector2 position,
			float interpolantShared)
		{
			var positionTo = position + popup.SlideToOffset;
			if (definition.positionRounded)
			{
				positionTo = new Vector2(Mathf.RoundToInt(positionTo.x), Mathf.RoundToInt(positionTo.y));
			}
			popup.SlidePosition = Vector2.Lerp(popup.Position, positionTo, interpolantShared);

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) CIViewCombatPopups.SlidePopup | time: {2} | popup: {3} | key: {4} | start time: {5} | position: {6} | target slot: {7} | slide position: {8} | interpolant: {9} | segment count: {10}",
					ModLink.modIndex,
					ModLink.modId,
					Time.realtimeSinceStartupAsDouble,
					popup.PopupID,
					popup.AnimationKey,
					popup.SlideStartTime,
					popup.Position,
					popup.SlideSlot,
					popup.SlidePosition,
					interpolantShared,
					popup.Segments.Count);
			}

			for (var i = 0; i < popup.Segments.Count; i += 1)
			{
				inst.spriteCollection.SetPosition(popup.SpriteIDBase + i, popup.SlidePosition);
			}
		}

		static void CollectTrackers()
		{
			var now = Time.unscaledTimeAsDouble;
			collectedTrackers.Clear();
			foreach (var kvp in damageTrackers)
			{
				foreach (var tracker in kvp.Value)
				{
					if (!tracker.Dirty)
					{
						continue;
					}

					if (tracker.Dirty
						&& now - tracker.TimeLast < PBCIViewCombatPopups.ins.intervalMinText)
					{
						continue;
					}

					collectedTrackers.Add(tracker);
					tracker.Dirty = false;
				}
			}
		}

		static void UpdateTracker(
			CombatEntity combatUnit,
			string animationKey,
			float value,
			string format,
			double now)
		{
			if (!damageTrackers.TryGetValue(animationKey, out var trackers))
			{
				trackers = new List<DamageTracker>();
				damageTrackers.Add(animationKey, trackers);
			}

			foreach (var tracker in trackers)
			{
				if (tracker.CombatUnit != combatUnit)
				{
					continue;
				}

				if (logEnabled)
				{
					Debug.LogFormat(
						"Mod {0} ({1}) CIViewCombatPopups.UpdateTrackers found tracker | time: {2:F3} | unit: C-{3} | type: {4} | value: {5} | acc value: {6} | last time: {7:F3}",
						ModLink.modIndex,
						ModLink.modId,
						now,
						combatUnit.id.id,
						animationKey,
						value,
						tracker.AccumulatedValue,
						tracker.TimeLast);
				}

				tracker.AccumulatedValue += value;
				tracker.Text = FormatValue(format, tracker.AccumulatedValue);
				tracker.TimeLast = now;
				tracker.Dirty = true;

				return;
			}

			trackers.Add(new DamageTracker()
			{
				CombatUnit = combatUnit,
				AnimationKey = animationKey,
				AccumulatedValue = value,
				Text = FormatValue(format, value),
				TimeLast = now,
				Dirty = true,
			});

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) CIViewCombatPopups.UpdateTrackers new tracker | time: {2:F3} | unit: C-{3} | type: {4} | value: {5}",
					ModLink.modIndex,
					ModLink.modId,
					now,
					combatUnit.id.id,
					animationKey,
					value);
			}
		}

		static void ResetTracker(PopupText popup)
		{
			var trackers = damageTrackers[popup.AnimationKey];
			foreach (var tracker in trackers)
			{
				if (tracker.CombatUnit == popup.CombatUnit)
				{
					tracker.AccumulatedValue = 0f;
					tracker.Text = "0";
					tracker.Dirty = false;
					break;
				}
			}
		}

		static string FormatValue(string format, float value)
		{
			if (format == "F0" && !value.RoughlyEqual(0f))
			{
				value = Mathf.Round(value);
				if (value.RoughlyEqual(0f))
				{
					value = 1f;
				}
			}
			return value.ToString(format);
		}

		static (bool, Vector2) GetPosition(PBCIViewPopups inst, PopupText popup)
		{
			var position = popup.CombatUnit.position.v + popup.CombatUnit.localCenterPoint.v * 2;
			var direction = Utilities.GetDirection(worldCamera.transform.position, position);
			if (Vector3.Dot(worldCamera.transform.forward, direction) <= 0.1f)
			{
				return (false, position);
			}

			var worldPoint = uiCamera.ViewportToWorldPoint(worldCamera.WorldToViewportPoint(position));
			var localPosition = inst.transform.InverseTransformPoint(worldPoint);
			return (true, new Vector2(localPosition.x, localPosition.y));
		}

		static void SortPopups(List<PopupText> popups)
		{
			animatingPopups.Clear();
			animatingPopups.AddRange(popups);
			animatingPopups.Sort(recencyComparison);

			slidingPopups.Clear();
			var now = Time.realtimeSinceStartupAsDouble;
			var isSliding = false;
			for (var i = animatingPopups.Count - 1; i >= 0; i -= 1)
			{
				var popup = animatingPopups[i];
				if (popup.IsSliding)
				{
					animatingPopups.RemoveAt(i);
					slidingPopups.Add(popup);
					isSliding = true;
					continue;
				}
				if (isSliding)
				{
					continue;
				}
				if (popup.Slot == i)
				{
					continue;
				}

				popup.IsSliding = true;
				popup.SlideSlot = i;
				popup.SlideStartTime = now;
				popup.SlideToOffset = Vector2.up * (i * slotHeight);
				animatingPopups.RemoveAt(i);
				slidingPopups.Add(popup);
			}

			if (isSliding)
			{
				animatingPopups.Sort(slotComparison);
			}
		}

		static void BuildPopups(PBCIViewCombatPopups inst)
		{
			if (collectedTrackers.Count == 0)
			{
				return;
			}

			foreach (var tracker in collectedTrackers)
			{
				if (!popupsLookup.TryGetValue(tracker.CombatUnit.id.id, out var popups))
				{
					popups = new List<PopupText>();
					popupsLookup.Add(tracker.CombatUnit.id.id, popups);
				}
				BuildPopupForTracker(inst, popups, tracker);
			}
		}

		static void BuildPopupForTracker(
			PBCIViewCombatPopups inst,
			List<PopupText> popups,
			DamageTracker tracker)
		{
			foreach (var popup in popups)
			{
				if (popup.CombatUnit != tracker.CombatUnit)
				{
					continue;
				}
				if (popup.AnimationKey != tracker.AnimationKey)
				{
					continue;
				}

				if (popup.IsSliding)
				{
					return;
				}

				BuildPopupText(
					inst,
					tracker.CombatUnit,
					tracker.AnimationKey,
					tracker.Text,
					popup: popup);
				return;
			}

			var (ok, p) = BuildPopupText(
				inst,
				tracker.CombatUnit,
				tracker.AnimationKey,
				tracker.Text,
				index: popups.Count);
			if (ok)
			{
				popups.Add(p);
			}
		}

		static (bool, PopupText) BuildPopupText(
			PBCIViewCombatPopups inst,
			CombatEntity combatUnit,
			string animationKey,
			string text,
			PopupText popup = null,
			int index = 0)
		{
			if (!textAnimationsLookup.ContainsKey(animationKey))
			{
				return (false, null);
			}

			var definition = textAnimationsLookup[animationKey];
			if (definition.timeTotal <= 0f)
			{
				return (false, null);
			}

			var atlas = inst.spriteCollection.atlas;
			if (atlas == null)
			{
				return (false, null);
			}

			var spriteIDNext = instTraverse.Field<int>("spriteIDNext").Value;
			if (popup != null)
			{
				spriteIDNext = RefreshPopup(
					inst,
					popup,
					definition,
					text,
					spriteIDNext);
				instTraverse.Field<int>("spriteIDNext").Value = spriteIDNext;
				return (true, popup);
			}

			var (nextID, p) = CreatePopup(
				inst,
				index,
				combatUnit,
				animationKey,
				definition,
				text,
				spriteIDNext);
			instTraverse.Field<int>("spriteIDNext").Value = nextID;
			return (true, p);
		}

		static (int, PopupText) CreatePopup(
			PBCIViewCombatPopups inst,
			int index,
			CombatEntity combatUnit,
			string animationKey,
			PBCIViewPopups.PopupDefinition definition,
			string text,
			int spriteIDNext)
		{
			var now = Time.realtimeSinceStartupAsDouble;
			var popup = new PopupText()
			{
				PopupID = spriteIDNext,
				Index = index,
				StartTime = now,
				AnimationKey = animationKey,
				CombatUnit = combatUnit,
				Text = text,
				SpriteIDBase = spriteIDNext,
				Segments = getPooledSegmentList(),
				Slot = index,
				Position = combatUnit.position.v,
			};
			popup.Segments.Clear();
			spriteIDNext = AddDefinitionSegments(inst, popup, definition);
			spriteIDNext = AddTextSegments(inst, popup, text, spriteIDNext);

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) CIViewCombatPopups.CreatePopup | time: {2} | popup: {3} | key: {4} | unit: C-{5} | text: {6} | segments: {7}",
					ModLink.modIndex,
					ModLink.modId,
					now,
					popup.PopupID,
					animationKey,
					combatUnit.id.id,
					text,
					popup.Segments.Count);
			}

			return (spriteIDNext, popup);
		}

		static int RefreshPopup(
			PBCIViewCombatPopups inst,
			PopupText popup,
			PBCIViewPopups.PopupDefinition definition,
			string text,
			int spriteIDNext)
		{
			var now = Time.realtimeSinceStartupAsDouble;
			RemoveAllSegments(popup, now);

			popup.Text = text;
			popup.StartTime = now;
			popup.SpriteIDBase = spriteIDNext;
			spriteIDNext = AddDefinitionSegments(inst, popup, definition);
			spriteIDNext = AddTextSegments(inst, popup, text, spriteIDNext);

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) CIViewCombatPopups.RefreshPopup | time: {2} | popup: {3} | text: {4} | spriteID base: {5} | segments: {6}",
					ModLink.modIndex,
					ModLink.modId,
					now,
					popup.PopupID,
					text,
					popup.SpriteIDBase,
					popup.Segments.Count);
			}

			return spriteIDNext;
		}

		static void HidePopup(PBCIViewCombatPopups inst, PopupText popup)
		{
			var spriteIDBase = popup.SpriteIDBase;
			for (var i = 0; i < popup.Segments.Count; i += 1)
			{
				inst.spriteCollection.SetActive(spriteIDBase + i, false);
			}
		}

		static void DestroyPopup(PBCIViewCombatPopups inst, PopupText popup)
		{
			var popups = popupsLookup[popup.CombatUnit.id.id];
			popups.RemoveAt(popup.Index);
			for (var i = 0; i < popups.Count; i += 1)
			{
				popups[i].Index = i;
			}
			ResetTracker(popup);

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) CIViewCombatPopups.DestroyPopup | time: {2} | popup: {3} | unit: C-{4} | spriteID base: {5} | segments: {6}",
					ModLink.modIndex,
					ModLink.modId,
					Time.realtimeSinceStartupAsDouble,
					popup.PopupID,
					popup.CombatUnit.id.id,
					popup.SpriteIDBase,
					popup.Segments.Count);
			}

			DisposeSprites(inst, popup);
			for (var i = 0; i < popup.Segments.Count; i += 1)
			{
				inst.spriteCollection.RemoveSprite(popup.SpriteIDBase + i);
			}
			releasePooledSegmentList(popup.Segments);
		}

		static int AddDefinitionSegments(
			PBCIViewCombatPopups inst,
			PopupText popup,
			PBCIViewPopups.PopupDefinition definition)
		{
			var atlas = inst.spriteCollection.atlas;
			var spriteIDNext = popup.SpriteIDBase;
			for (var i = 0; i < definition.segments.Count; i += 1)
			{
				var segment = definition.segments[i];
				if (atlas.GetSprite(segment.sprite) == null)
				{
					Debug.LogWarningFormat(
						"Mod {0} ({1}) CIViewCombatPopups.AddDefinitionSegments can't find sprite in atlas | key: {2} | popup: {3} | name: {4}",
						ModLink.modIndex,
						ModLink.modId,
						definition.key,
						popup.PopupID,
						segment.sprite);

					spriteIDNext += 1;
					continue;
				}
				inst.spriteCollection.AddSprite(
					spriteIDNext,
					segment.sprite,
					Vector2.zero,
					segment.size.x,
					segment.size.y,
					(Color32)segment.color,
					segment.pivot,
					type: segment.type,
					flip: segment.flip,
					fillDirection: segment.fillDirection,
					fillInvert: segment.fillInvert,
					enabled: false);
				popup.Segments.Add(segment);
				spriteIDNext += 1;
			}

			return spriteIDNext;
		}

		static int AddTextSegments(
			PBCIViewCombatPopups inst,
			PopupText popup,
			string text,
			int spriteIDNext)
		{
			var textStartOffset = inst.textStartOffset;
			var atlas = inst.spriteCollection.atlas;
			var pivot = new Vector2(0.5f, 0.5f);
			var color = new Color(1f, 1f, 1f, 1f);

			foreach (var key in text)
			{
				var center = 6;
				var offset = 12;
				var width = 24;
				var spriteName = "s_text_24_" + key;

				if (characterSpriteLookup.TryGetValue(key, out var characterSprite))
				{
					spriteName = characterSprite.sprite;
					center = characterSprite.center;
					offset = characterSprite.offset;
					width = characterSprite.width;
				}

				if (atlas.GetSprite(spriteName) == null)
				{
					Debug.LogWarningFormat(
						"Mod {0} ({1}) CIViewCombatPopups.AddTextSegments can't find sprite in atlas | popup: {2} | name: {3}",
						ModLink.modIndex,
						ModLink.modId,
						popup.PopupID,
						spriteName);

					spriteIDNext += 1;
					continue;
				}

				var pos = new Vector2(textStartOffset + center, 0f);
				inst.spriteCollection.AddSprite(
					spriteIDNext,
					spriteName,
					pos,
					width,
					spriteHeight,
					(Color32)color,
					pivot,
					enabled: false);
				popup.Segments.Add(new PBCIViewPopups.PopupNestedSegment()
				{
					sprite = spriteName,
					pivot = pivot,
					position = pos,
					size = new Vector2(width, spriteHeight),
				});

				spriteIDNext += 1;
				textStartOffset += offset;
			}

			return spriteIDNext;
		}

		static void RemoveAllSegments(PopupText popup, double now)
		{
			if (popup.SpriteDisposal == null)
			{
				popup.SpriteDisposal = new List<int>();
			}

			for (var i = 0; i < popup.Segments.Count; i += 1)
			{
				popup.SpriteDisposal.Add(popup.SpriteIDBase + i);
			}

			var count = popup.Segments.Count;
			popup.Segments.Clear();

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) CIViewCombatPopups.RemoveAllSegments | time: {2} | popup: {3} | text sprite range: {4}-{5}",
					ModLink.modIndex,
					ModLink.modId,
					now,
					popup.PopupID,
					popup.SpriteIDBase,
					popup.SpriteIDBase + count - 1);
			}
		}

		static void DisposeSprites(PBCIViewPopups inst, PopupText popup)
		{
			if (popup.SpriteDisposal == null || popup.SpriteDisposal.Count == 0)
			{
				return;
			}

			var firstID = popup.SpriteDisposal[0];
			var lastID = popup.SpriteDisposal[popup.SpriteDisposal.Count - 1];
			foreach (var spriteID in popup.SpriteDisposal)
			{
				inst.spriteCollection.RemoveSprite(spriteID);
			}
			popup.SpriteDisposal.Clear();
			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) CIViewCombatPopups.DisposeSprites | time: {2} | popup: {3} | sprites: {4}-{5}",
					ModLink.modIndex,
					ModLink.modId,
					Time.realtimeSinceStartupAsDouble,
					popup.PopupID,
					firstID,
					lastID);
			}
		}

		static int ComparePopupsByRecency(PopupText x, PopupText y)
		{
			if (System.Math.Abs(x.StartTime - y.StartTime) < slideThreshold)
			{
				return 0;
			}
			return -x.StartTime.CompareTo(y.StartTime);
		}

		static int ComparePopupsBySlot(PopupText x, PopupText y) => x.Slot.CompareTo(y.Slot);

		static bool InitializeInstance(PBCIViewCombatPopups inst)
		{
			if (isInitialized)
			{
				return true;
			}

			var mi = AccessTools.DeclaredMethod(typeof(PBCIViewPopups), "ReleasePooledSegmentList");
			if (mi == null)
			{
				Debug.LogWarningFormat(
					"Mod {0} ({1}) Unable to initialize CIViewCombatPopups | reflection failed to find method ReleasePooledSegmentList",
					ModLink.modIndex,
					ModLink.modId);
				return false;
			}
			releasePooledSegmentList = (ReleaseDelegate)mi.CreateDelegate(typeof(ReleaseDelegate), inst);

			mi = AccessTools.DeclaredMethod(typeof(PBCIViewPopups), "GetPooledSegmentList");
			if (mi == null)
			{
				Debug.LogWarningFormat(
					"Mod {0} ({1}) Unable to initialize CIViewCombatPopups | reflection failed to find method GetPooledSegmentList",
					ModLink.modIndex,
					ModLink.modId);
				return false;
			}
			getPooledSegmentList = (AllocateDelegate)mi.CreateDelegate(typeof(AllocateDelegate), inst);

			mi = AccessTools.DeclaredMethod(typeof(PBCIViewPopups), "AnimateSegment");
			if (mi == null)
			{
				Debug.LogWarningFormat(
					"Mod {0} ({1}) Unable to initialize CIViewCombatPopups | reflection failed to find method AnimateSegment",
					ModLink.modIndex,
					ModLink.modId);
				return false;
			}
			animateSegment = (AnimateDelegate)mi.CreateDelegate(typeof(AnimateDelegate), inst);

			instTraverse = Traverse.Create(inst);
			textAnimationsLookup = instTraverse.Field<Dictionary<string, PBCIViewPopups.PopupDefinition>>("textAnimationsLookup").Value;
			characterSpriteLookup = instTraverse.Field<Dictionary<char, PBCIViewPopups.CharacterSprite>>("characterSpriteLookup").Value;
			worldCamera = instTraverse.Field<Camera>("worldCamera").Value;
			uiCamera = instTraverse.Field<Camera>("uiCamera").Value;

			foreach (var kvp in textAnimationsLookup)
			{
				if (!PBCIViewCombatPopups.damageTypeKeys.Contains(kvp.Key))
				{
					continue;
				}
				if (kvp.Value.timeTotal < 2f)
				{
					continue;
				}
				kvp.Value.timeTotal = 2f;
			}

			isInitialized = true;

			Debug.LogFormat(
				"Mod {0} ({1}) CIViewCombatPopups.InitializeInstance -- patched in instance fields/methods with reflection",
				ModLink.modIndex,
				ModLink.modId);

			return true;
		}
	}
}
