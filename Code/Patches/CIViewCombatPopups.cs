// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	static partial class CIViewCombatPopups
	{
		internal static void AddDamageText(
			CombatEntity unitCombat,
			string animKey,
			float value,
			string format)
		{
			if (!initialized)
			{
				return;
			}
			if (string.IsNullOrEmpty(animKey))
			{
				return;
			}
			if (value.RoughlyEqual(0f))
			{
				return;
			}

			if (ModLink.Settings.IsLoggingEnabled(ModLink.ModSettings.LoggingFlag.Tracking))
			{
				Debug.LogFormat(
					"Mod {0} ({1}) CIViewCombatPopups.AddDamageText | time: {2:F3} | unit: C-{3} | position: {4} | type: {5} | value: {6}",
					ModLink.modIndex,
					ModLink.modId,
					Contexts.sharedInstance.combat.simulationTime.f,
					unitCombat.id.id,
					UnitHelper.GetPosition(unitCombat),
					animKey,
					value);
			}

			var ekc = ECS.Contexts.sharedInstance.ekRequest.CreateEntity();
			ekc.AddCombatUnitID(unitCombat.id.id);
			ekc.AddAnimationKey(animKey);
			ekc.AddDamageText(value, format);

			if (ModLink.Settings.replayPopups != ModLink.ModSettings.ReplayPopup.None)
			{
				ReplayHelper.AddSummaryFormats(animKey, format);
			}
		}
	}
}
