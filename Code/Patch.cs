using HarmonyLib;

using PBCIViewCombatPopups = CIViewCombatPopups;
using PBCombatReplayHelper = CombatReplayHelper;

namespace EchKode.PBMods.DamagePopups
{
	[HarmonyPatch]
	static class Patch
	{
		[HarmonyPatch(typeof(PBCIViewCombatPopups), "AddDamageText")]
		[HarmonyPrefix]
		static bool Civcp_AddDamageTextPrefix(
			CombatEntity unitCombat,
			string animKey,
			float value,
			string format)
		{
			CIViewCombatPopups.AddDamageText(
				unitCombat,
				animKey,
				value,
				format);
			return false;
		}

		[HarmonyPatch(typeof(PBCombatReplayHelper), "SetReplayActive")]
		[HarmonyPostfix]
		static void Crh_SetReplayActivePostfix(bool active)
		{
			if (ModLink.Settings.replayPopups != ModLink.ModSettings.ReplayPopup.None)
			{
				CombatReplayHelper.SetReplayActive(active);
			}
		}

		[HarmonyPatch(typeof(PBCombatReplayHelper), "ApplyTime")]
		[HarmonyPostfix]
		static void Crh_ApplyTimePostfix(float timeRequestedLocal, bool timeCheck, bool blockAssetReuse)
		{
			if (ModLink.Settings.replayPopups != ModLink.ModSettings.ReplayPopup.None)
			{
				CombatReplayHelper.ApplyTime(timeRequestedLocal, timeCheck);
			}
		}

		[HarmonyPatch(typeof(PhantomBrigade.Heartbeat), "Start")]
		[HarmonyPrefix]
		static void Hb_StartPrefix()
		{
			Heartbeat.Start();
		}
	}
}
