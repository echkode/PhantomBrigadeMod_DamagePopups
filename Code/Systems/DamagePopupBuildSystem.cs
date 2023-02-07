using System.Collections.Generic;

using Entitas;

using PhantomBrigade;
using PBCIViewPopups = CIViewPopups;
using PBCIViewCombatPopups = CIViewCombatPopups;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	using static ECS.ContextsExtensions;

	sealed class DamagePopupBuildSystem : IInitializeSystem, IExecuteSystem, ITearDownSystem
	{
		internal static bool logEnabled = false;

		private static bool initialized;
		private static INGUIAtlas atlas;

		public void Initialize()
		{
			atlas = CIViewCombatPopups.SpriteAtlas;
			initialized = atlas != null;
		}

		public void Execute()
		{
			if (!initialized)
			{
				return;
			}

			var now = Time.unscaledTimeAsDouble;
			foreach (var tracking in ECS.Contexts.sharedInstance.ekTracking.GetEntities())
			{
				if (!tracking.hasDamageTracker)
				{
					continue;
				}
				if (!tracking.isDirty)
				{
					continue;
				}
				if (now - tracking.damageTracker.timeLast < PBCIViewCombatPopups.ins.intervalMinText)
				{
					continue;
				}

				var popupEntities = ECS.Contexts.sharedInstance.ekPopup.GetEntitiesWithCombatUnitID(tracking.combatUnitID.id);
				if (popupEntities != null)
				{
					if (TryRefreshPopup(popupEntities, tracking, now))
					{
						tracking.isDirty = false;
						continue;
					}
				}

				CreatePopup(tracking, popupEntities?.Count ?? 0, now);
				tracking.isDirty = false;
			}
		}

		public void TearDown()
		{
			foreach (var ekp in ECS.Contexts.sharedInstance.ekPopup.GetEntities())
			{
				if (ekp.hasSpriteDisposal)
				{
					continue;
				}

				if (ekp.hasPopup)
				{
					AnimationHelper.DisposeSprites(ekp.displayText.spriteIDBase, ekp.popup.segments.Count);
					CIViewCombatPopups.ReleasePooledSegmentList(ekp.popup.segments);
				}
				ekp.Destroy();
			}
		}

		static void CreatePopup(ECS.EkTrackingEntity tracking, int index, double now)
		{
			if (!CIViewCombatPopups.HasDefinition(tracking.animationKey.s))
			{
				return;
			}

			var definition = CIViewCombatPopups.GetDefinition(tracking.animationKey.s);
			if (definition.timeTotal <= 0f)
			{
				return;
			}

			var ekp = ECS.Contexts.sharedInstance.ekPopup.CreateEntity();
			ekp.AddCombatUnitID(tracking.combatUnitID.id);
			ekp.AddAnimationKey(tracking.animationKey.s);
			ekp.AddSlot(index);

			var combatUnit = IDUtility.GetCombatEntity(tracking.combatUnitID.id);
			ekp.AddPosition(combatUnit.position.v + combatUnit.localCenterPoint.v);

			var spriteIDNext = CIViewCombatPopups.SpriteIDNext;
			var segments = CIViewCombatPopups.GetPooledSegmentList();
			segments.Clear();
			ekp.AddPopup(spriteIDNext, segments);
			ekp.AddDisplayText(tracking.damageTracker.text, spriteIDNext, now);
			spriteIDNext = AddDefinitionSegments(ekp, definition);
			spriteIDNext = AddTextSegments(ekp, tracking.damageTracker.text, spriteIDNext);
			CIViewCombatPopups.SpriteIDNext = spriteIDNext;

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) CIViewCombatPopups.CreatePopup | time: {2} | popup: {3} | key: {4} | unit: C-{5} | text: {6} | segments: {7}",
					ModLink.modIndex,
					ModLink.modId,
					now,
					ekp.popup.popupID,
					tracking.animationKey.s,
					tracking.combatUnitID.id,
					tracking.damageTracker.text,
					ekp.popup.segments.Count);
			}
		}

		static bool TryRefreshPopup(
			HashSet<ECS.EkPopupEntity> popupEntities,
			ECS.EkTrackingEntity tracking,
			double now)
		{
			foreach (var ekp in popupEntities)
			{
				if (tracking.animationKey.s != ekp.animationKey.s)
				{
					continue;
				}

				if (ekp.hasSlideAnimation)
				{
					return true;
				}

				RefreshPopup(ekp, tracking, now);
				return true;
			}

			return false;
		}

		static void RefreshPopup(
			ECS.EkPopupEntity ekp,
			ECS.EkTrackingEntity tracking,
			double now)
		{
			RemoveAllSegments(ekp, now);

			var spriteIDNext = CIViewCombatPopups.SpriteIDNext;
			ekp.ReplaceDisplayText(
				tracking.damageTracker.text,
				spriteIDNext,
				now);

			var definition = CIViewCombatPopups.GetDefinition(tracking.animationKey.s);
			spriteIDNext = AddDefinitionSegments(ekp, definition);
			spriteIDNext = AddTextSegments(ekp, tracking.damageTracker.text, spriteIDNext);
			CIViewCombatPopups.SpriteIDNext = spriteIDNext;

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) DamagePopupBuildSystem.RefreshPopup | time: {2} | popup: {3} | text: {4} | spriteID base: {5} | segments: {6}",
					ModLink.modIndex,
					ModLink.modId,
					now,
					ekp.popup.popupID,
					tracking.damageTracker.text,
					ekp.displayText.spriteIDBase,
					ekp.popup.segments.Count);
			}
		}

		static int AddDefinitionSegments(
			ECS.EkPopupEntity ekp,
			PBCIViewPopups.PopupDefinition definition)
		{
			var spriteIDNext = ekp.displayText.spriteIDBase;
			for (var i = 0; i < definition.segments.Count; i += 1)
			{
				var segment = definition.segments[i];
				if (atlas.GetSprite(segment.sprite) == null)
				{
					Debug.LogWarningFormat(
						"Mod {0} ({1}) DamagePopupBuildSystem.AddDefinitionSegments can't find sprite in atlas | key: {2} | popup: {3} | name: {4}",
						ModLink.modIndex,
						ModLink.modId,
						definition.key,
						ekp.popup.popupID,
						segment.sprite);

					spriteIDNext += 1;
					continue;
				}
				CIViewCombatPopups.AllocateSprite(
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
				ekp.popup.segments.Add(segment);
				spriteIDNext += 1;
			}

			return spriteIDNext;
		}

		static int AddTextSegments(
			ECS.EkPopupEntity ekp,
			string text,
			int spriteIDNext)
		{
			var textStartOffset = CIViewCombatPopups.TextStartOffset;
			var pivot = new Vector2(0.5f, 0.5f);
			var color = new Color(1f, 1f, 1f, 1f);

			foreach (var key in text)
			{
				var center = 6;
				var offset = 12;
				var width = 24;
				var spriteName = "s_text_24_" + key;

				if (CIViewCombatPopups.TryGetCharacterSprite(key, out var characterSprite))
				{
					spriteName = characterSprite.sprite;
					center = characterSprite.center;
					offset = characterSprite.offset;
					width = characterSprite.width;
				}

				if (atlas.GetSprite(spriteName) == null)
				{
					Debug.LogWarningFormat(
						"Mod {0} ({1}) DamagePopupBuildSystem.AddTextSegments can't find sprite in atlas | popup: {2} | name: {3}",
						ModLink.modIndex,
						ModLink.modId,
						ekp.popup.popupID,
						spriteName);

					spriteIDNext += 1;
					continue;
				}

				var pos = new Vector2(textStartOffset + center, 0f);
				CIViewCombatPopups.AllocateSprite(
					spriteIDNext,
					spriteName,
					pos,
					width,
					CIViewCombatPopups.Constants.SpriteHeight,
					(Color32)color,
					pivot,
					enabled: false);
				ekp.popup.segments.Add(new PBCIViewPopups.PopupNestedSegment()
				{
					sprite = spriteName,
					pivot = pivot,
					position = pos,
					size = new Vector2(width, CIViewCombatPopups.Constants.SpriteHeight),
				});

				spriteIDNext += 1;
				textStartOffset += offset;
			}

			return spriteIDNext;
		}

		static void RemoveAllSegments(ECS.EkPopupEntity ekp, double now)
		{
			AnimationHelper.DisposeSprites(ekp);
			var count = ekp.popup.segments.Count;
			ekp.popup.segments.Clear();

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) DamagePopupBuildSystem.RemoveAllSegments | time: {2} | popup: {3} | text sprite range: {4}-{5}",
					ModLink.modIndex,
					ModLink.modId,
					now,
					ekp.popup.popupID,
					ekp.displayText.spriteIDBase,
					ekp.displayText.spriteIDBase + count - 1);
			}
		}
	}
}
