using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	static class CombatReplayHelper
	{
		internal static void SetReplayActive(bool active)
		{
			ECS.Contexts.sharedInstance.ekReplay.isActive = active;
		}

		internal static void ApplyTime(float timeRequestedLocal, bool timeStep)
		{
			var turn = Contexts.sharedInstance.combat.currentTurn.i - 1;
			var sampleIndex = Mathf.FloorToInt(timeRequestedLocal * ModLink.Settings.samplesPerSecond);
			ECS.Contexts.sharedInstance.ekReplay.ReplaceSample(turn, timeRequestedLocal, timeStep, sampleIndex);
		}
	}
}
