using System.Collections.Generic;

using Entitas;

using PBCIViewPopups = CIViewPopups;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	sealed class DamagePopupSlideSystem : IInitializeSystem, IExecuteSystem, ITearDownSystem
	{
		private static bool logEnabled;

		private static readonly List<ECS.EkPopupEntity> popups = new List<ECS.EkPopupEntity>();
		private static float playbackSpeed = 1f;
		private static bool shown;

		public void Initialize()
		{
			logEnabled = ModLink.Settings.enableLogging;
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
					if (!ekp.hasSlideAnimation)
					{
						continue;
					}
					popups.Add(ekp);
				}
			}

			if (combat.Simulating)
			{
				shown = true;
				SlidePopups();
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

		static void SlidePopups()
		{
			var now = Contexts.sharedInstance.combat.simulationTime.f;
			foreach (var ekp in popups)
			{
				var elapsedTime = (now - ekp.slideAnimation.startTime) * playbackSpeed;
				if (elapsedTime >= CIViewCombatPopups.Constants.SlideAnimationTime)
				{
					ekp.ReplaceSlot(ekp.slideAnimation.slot);
					ekp.RemoveSlideAnimation();
					continue;
				}

				var (positionOK, position) = AnimationHelper.GetPosition(ekp);
				if (!positionOK)
				{
					AnimationHelper.HidePopup(ekp);
					continue;
				}

				var interpolantShared = Mathf.Clamp01((float)elapsedTime / CIViewCombatPopups.Constants.SlideAnimationTime);
				var definition = CIViewCombatPopups.GetDefinition(ekp.animationKey.s);
				SlidePopup(
					ekp,
					definition,
					position,
					interpolantShared,
					now);
			}
		}

		static void SlidePopup(
			ECS.EkPopupEntity ekp,
			PBCIViewPopups.PopupDefinition definition,
			Vector2 position,
			float interpolantShared,
			float now)
		{
			var positionTo = position + ekp.slideAnimation.slideToOffset;
			if (definition.positionRounded)
			{
				positionTo = new Vector2(Mathf.RoundToInt(positionTo.x), Mathf.RoundToInt(positionTo.y));
			}
			ekp.ReplaceSlidePosition(Vector2.Lerp(ekp.position.v, positionTo, interpolantShared));

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) DamagePopupAnimationSystem.SlidePopup | time: {2:F3} | popup: {3} | key: {4} | start time: {5} | position: {6} | target slot: {7} | slide position: {8} | interpolant: {9} | segment count: {10}",
					ModLink.modIndex,
					ModLink.modId,
					now,
					ekp.popup.popupID,
					ekp.animationKey.s,
					ekp.slideAnimation.startTime,
					ekp.position.v,
					ekp.slideAnimation.slot,
					ekp.slidePosition.v,
					interpolantShared,
					ekp.popup.segments.Count);
			}

			var positionBase = ekp.slidePosition.v;
			var spriteIDBase = ekp.displayText.spriteIDBase;
			for (var i = 0; i < ekp.popup.segments.Count; i += 1)
			{
				CIViewCombatPopups.SetSpritePosition(spriteIDBase + i, positionBase + ekp.popup.segments[i].position);
			}
		}
	}
}
