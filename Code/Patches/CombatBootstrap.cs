using PBCombatBootstrap = PhantomBrigade.Combat.Systems.CombatBootstrap;

namespace EchKode.PBMods.DamagePopups
{
	static class CombatBootstrap
	{
		internal static void Disable()
		{
			CIViewCombatPopups.Clear();
		}
	}
}
