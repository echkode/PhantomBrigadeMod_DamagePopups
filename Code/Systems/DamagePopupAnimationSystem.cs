using System.Collections.Generic;

using Entitas;

using PBCIViewPopups = CIViewPopups;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	using static ECS.ContextsExtensions;

	sealed class DamagePopupAnimationSystem : IInitializeSystem, IExecuteSystem, ITearDownSystem
	{
		private static bool logEnabled;

		private static readonly List<ECS.EkPopupEntity> popups = new List<ECS.EkPopupEntity>();
		private static readonly System.Comparison<ECS.EkPopupEntity> popupComparison = new System.Comparison<ECS.EkPopupEntity>(ComparePopups);
		private static float playbackSpeed = 1f;
		private static bool shown;

		public void Initialize()
		{
			logEnabled = ModLink.Settings.IsLoggingEnabled(ModLink.ModSettings.LoggingFlag.Animation);
			playbackSpeed = CIViewCombatPopups.PlaybackSpeed;
		}

		public void Execute()
		{
			var combat = Contexts.sharedInstance.combat;
			if (combat.Simulating && combat.simulationDeltaTime.f == 0f)
			{
				return;
			}

			if (combat.Simulating || shown)
			{
				popups.Clear();
				foreach (var ekp in ECS.Contexts.sharedInstance.ekPopup.GetEntities())
				{
					if (!ekp.hasPopup)
					{
						continue;
					}
					if (ekp.hasSlideAnimation)
					{
						continue;
					}
					popups.Add(ekp);
				}
			}

			if (combat.Simulating)
			{
				shown = true;
				AnimateText();
				return;
			}

			if (shown)
			{
				shown = false;
				foreach (var ekp in popups)
				{
					AnimationHelper.HidePopup(ekp);
				}
			}
		}

		public void TearDown()
		{
			popups.Clear();
		}

		static void AnimateText()
		{
			var now = Contexts.sharedInstance.combat.simulationTime.f;
			popups.Sort(popupComparison);
			foreach (var ekp in popups)
			{
				var definition = CIViewCombatPopups.GetDefinition(ekp.animationKey.s);
				var elapsedTime = (now - ekp.displayText.startTime) * playbackSpeed;
				if (elapsedTime >= definition.timeTotal)
				{
					DestroyPopup(ekp, now);
					continue;
				}

				var (positionOK, position) = AnimationHelper.GetUIPosition(ekp);
				if (!positionOK)
				{
					AnimationHelper.HidePopup(ekp);
					continue;
				}

				var slotOffset = Vector2.up * (ekp.slot.i * CIViewCombatPopups.Constants.SlotHeight);
				var interpolantShared = Mathf.Clamp01((float)elapsedTime / definition.timeTotal);

				if (logEnabled)
				{
					Debug.LogFormat(
						"Mod {0} ({1}) DamagePopupAnimationSystem.AnimateText | time: {2:F3} | popup: {3} | key: {4} | start time: {5:F3} | unit position: {6} | slot: {7} | slot offset: {8} | interpolant: {9} | segment count: {10}",
						ModLink.modIndex,
						ModLink.modId,
						now,
						ekp.popup.popupID,
						definition.key,
						ekp.displayText.startTime,
						position,
						ekp.slot.i,
						slotOffset,
						interpolantShared,
						ekp.popup.segments.Count);
				}

				AnimatePopup(
					ekp,
					definition,
					position,
					slotOffset,
					interpolantShared,
					now);
			}
		}

		static void AnimatePopup(
			ECS.EkPopupEntity ekp,
			PBCIViewPopups.PopupDefinition definition,
			Vector2 position,
			Vector2 slotOffset,
			float interpolantShared,
			float now)
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
			ekp.ReplacePosition(position);
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
					"Mod {0} ({1}) DamagePopupAnimationSystem.AnimatePopup | time: {2:F3} | popup: {3} | key: {4} | position: {5} | offset: {6} | interpolant: {7} | segments: {8}",
					ModLink.modIndex,
					ModLink.modId,
					now,
					ekp.popup.popupID,
					definition.key,
					position,
					slotOffset,
					interpolantShared,
					ekp.popup.segments.Count);
			}

			var spriteIDBase = ekp.displayText.spriteIDBase;
			for (var i = 0; i < ekp.popup.segments.Count; i += 1)
			{
				AnimateSegment(
					ekp.popup.segments[i],
					spriteIDBase + i,
					interpolantShared,
					position,
					rotationBase,
					sizeBase,
					colorBase,
					now);
			}
		}

		static void AnimateSegment(
			PBCIViewPopups.PopupNestedSegment segment,
			int spriteIDLocal,
			float interpolantShared,
			Vector2 positionBase,
			float rotationBase,
			Vector2 sizeBase,
			Color colorBase,
			float now)
		{
			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) DamagePopupAnimationSystem.AnimateSegment | time: {2:F3} | sprite ID: {3} | sprite: {4} | position base: {5} | interpolant: {6}",
					ModLink.modIndex,
					ModLink.modId,
					now,
					spriteIDLocal,
					segment.sprite,
					positionBase,
					interpolantShared);
			}

			CIViewCombatPopups.AnimateSegment(segment, spriteIDLocal, interpolantShared, positionBase, rotationBase, sizeBase, colorBase);
		}

		static void DestroyPopup(ECS.EkPopupEntity ekp, float now)
		{
			ResetTracker(ekp);

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) DamagePopupAnimationSystem.DestroyPopup | time: {2:F3} | popup: {3} | unit: C-{4} | spriteID base: {5} | segments: {6}",
					ModLink.modIndex,
					ModLink.modId,
					now,
					ekp.popup.popupID,
					ekp.combatUnitID.id,
					ekp.displayText.spriteIDBase,
					ekp.popup.segments.Count);
			}

			AnimationHelper.DisposeSprites(ekp);
			CIViewCombatPopups.ReleasePooledSegmentList(ekp.popup.segments);
			ekp.Destroy();
		}

		static void ResetTracker(ECS.EkPopupEntity ekp)
		{
			foreach (var tracking in ECS.Contexts.sharedInstance.ekTracking.GetEntitiesWithCombatUnitID(ekp.combatUnitID.id))
			{
				if (!tracking.hasAnimationKey)
				{
					continue;
				}
				if (ekp.animationKey.s == tracking.animationKey.s)
				{
					tracking.ReplaceDamageTracker("0", 0f, tracking.damageTracker.timeLast);
					tracking.isDirty = false;
					break;
				}
			}
		}

		static int ComparePopups(ECS.EkPopupEntity x, ECS.EkPopupEntity y)
		{
			var cmp = x.combatUnitID.id.CompareTo(y.combatUnitID.id);
			if (cmp != 0)
			{
				return cmp;
			}
			return -x.displayText.startTime.CompareTo(y.displayText.startTime);
		}
	}
}
