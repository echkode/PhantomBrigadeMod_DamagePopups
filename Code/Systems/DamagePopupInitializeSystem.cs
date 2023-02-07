using Entitas;

namespace EchKode.PBMods.DamagePopups
{
	sealed class DamagePopupInitializeSystem : IInitializeSystem
	{
		public void Initialize()
		{
			CIViewCombatPopups.Initialize();
		}
	}
}
