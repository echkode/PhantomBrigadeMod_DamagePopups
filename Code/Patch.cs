using HarmonyLib;

using PBCIViewPopups = CIViewPopups;
using PBCIViewCombatPopups = CIViewCombatPopups;
using PBCombatBootstrap = PhantomBrigade.Combat.Systems.CombatBootstrap;

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

		[HarmonyPatch(typeof(PBCombatBootstrap), "Disable")]
		[HarmonyPostfix]
		static void Cb_DisablePostfix()
		{
			CombatBootstrap.Disable();
		}

		[HarmonyPatch(typeof(PBCIViewPopups), "AnimateAll")]
		[HarmonyPostfix]
		static void Civp_AnimateAllPostfix(PBCIViewPopups __instance)
		{
			CIViewCombatPopups.AnimateAll(__instance);
		}

		[HarmonyPatch(typeof(PhantomBrigade.Heartbeat), "Start")]
		[HarmonyPrefix]
		static void Hb_StartPrefix()
		{
			Heartbeat.Start();
		}
	}
}
