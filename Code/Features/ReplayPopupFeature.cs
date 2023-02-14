// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	sealed class ReplayPopupFeature : Feature
	{
		public ReplayPopupFeature()
		{
			Add(new SampleCarryOverSystem(Contexts.sharedInstance));
			Add(new ReplayTurnSystem(Contexts.sharedInstance));
			Add(new ReplayPositionTrackerSystem(Contexts.sharedInstance));
			if (ReplayHelper.HasSetting(ModLink.ModSettings.ReplayPopup.Cumulative))
			{
				Add(new ReplayAccumulationSystem(ECS.Contexts.sharedInstance));
			}
			if (ReplayHelper.HasSetting(ModLink.ModSettings.ReplayPopup.Barrage))
			{
				Add(new ReplaySummarySystem(ECS.Contexts.sharedInstance));
			}
			Add(new ReplayPopupSlotSystem(ECS.Contexts.sharedInstance));
			Add(new ReplayPopupBuildSystem(ECS.Contexts.sharedInstance));
			Add(new ReplayPopupDisplaySystem());
			Add(new ReplayExitSystem(ECS.Contexts.sharedInstance));
			Add(new ReplayTablesDestroySystem());

			if (ModLink.Settings.IsLoggingEnabled(ModLink.ModSettings.LoggingFlag.System))
			{
				Debug.LogFormat(
					"Mod {0} ({1}) replay popup feature active",
					ModLink.modIndex,
					ModLink.modId);
			}
		}
	}
}
