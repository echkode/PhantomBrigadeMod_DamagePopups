using HarmonyLib;

using PBCIViewCombatPopups = CIViewCombatPopups;

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

		[HarmonyPatch(typeof(PhantomBrigade.Heartbeat), "Start")]
		[HarmonyPrefix]
		static void Hb_StartPrefix()
		{
			Heartbeat.Start();
		}
	}
}
