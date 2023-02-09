using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace EchKode.PBMods.DamagePopups.ECS
{
	[EkPopup, EkRequest, EkTracking, EkReplay]
	public sealed class CombatUnitID : IComponent
	{
		[EntityIndex]
		public int id;
	}
}
