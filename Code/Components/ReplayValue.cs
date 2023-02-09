using Entitas;

namespace EchKode.PBMods.DamagePopups.ECS
{
	[EkReplay]
	public sealed class ReplayValue : IComponent
	{
		public float accumulated;
		public float summary;
	}
}
