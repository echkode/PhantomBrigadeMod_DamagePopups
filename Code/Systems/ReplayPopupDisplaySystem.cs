// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using Entitas;

using PBCombatReplayHelper = CombatReplayHelper;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	sealed class ReplayPopupDisplaySystem : IInitializeSystem, IExecuteSystem
	{
		private static bool logEnabled;

		public void Execute()
		{
			if (!PBCombatReplayHelper.IsReplayActive())
			{
				return;
			}

			var ekReplay = ECS.Contexts.sharedInstance.ekReplay;
			var sample = ekReplay.sample;
			foreach (var ekr in ekReplay.GetEntities())
			{
				if (!ekr.hasPopup)
				{
					continue;
				}
				DisplayPopup(ekr, sample);
			}
		}

		public void Initialize()
		{
			logEnabled = ModLink.Settings.IsLoggingEnabled(ModLink.ModSettings.LoggingFlag.ReplayDisplay);
		}

		static void DisplayPopup(ECS.EkReplayEntity ekr, ECS.Sample sample)
		{
			var definition = CIViewCombatPopups.GetDefinition(ekr.animationKey.s);
			var (_, position) = CIViewCombatPopups.GetUIPosition(ekr.positionTracker.a[sample.index]);
			position += Vector2.up * (ekr.replaySlots.a[sample.index] * CIViewCombatPopups.Constants.SlotHeight);
			if (sample.isTimeStep)
			{
				position = TimeStepPosition(ekr, sample, position);
			}
			if (definition.positionRounded)
			{
				position = new Vector2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
			}
			var rotationBase = definition.rotationTo;
			var sizeBase = definition.sizeTo;
			var colorBase = definition.color;
			var spriteIDBase = ekr.displayText.spriteIDBase;

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) ReplayPopupDisplaySystem.DisplayPopup | sample: {2} | popup: {3} | key: {4} | start time: {5} | position: {6} | sprite ID base: {7} | segment count: {8}",
					ModLink.modIndex,
					ModLink.modId,
					sample.index,
					ekr.popup.popupID,
					definition.key,
					ekr.displayText.startTime,
					position,
					spriteIDBase,
					ekr.popup.segments.Count);
			}

			for (var i = 0; i < ekr.popup.segments.Count; i += 1)
			{
				CIViewCombatPopups.AnimateSegment(
					ekr.popup.segments[i],
					spriteIDBase + i,
					sample.isTimeStep && ekr.hasDisplayInterpolation ? ekr.displayInterpolation.a[i] : 1f,
					position,
					rotationBase,
					sizeBase,
					colorBase);
			}
		}

		static Vector2 TimeStepPosition(ECS.EkReplayEntity ekr, ECS.Sample sample, Vector2 position)
		{
			if (sample.index < 0 || sample.index >= ReplayHelper.SummarySize - 1)
			{
				return position;
			}

			var nextSample = sample.index + 1;
			var (_, positionTo) = CIViewCombatPopups.GetUIPosition(ekr.positionTracker.a[nextSample]);
			positionTo += Vector2.up * (ekr.replaySlots.a[nextSample] * CIViewCombatPopups.Constants.SlotHeight);
			var t1 = (float)sample.index / ModLink.Settings.samplesPerSecond;
			var t2 = (float)nextSample / ModLink.Settings.samplesPerSecond;
			var t = sample.timeInTurn.RemapTo01(t1, t2);
			return Vector2.Lerp(position, positionTo, t);
		}
	}
}
