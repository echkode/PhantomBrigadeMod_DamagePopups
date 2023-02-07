using Entitas;

namespace EchKode.PBMods.DamagePopups.ECS
{
	[EkRequest]
	public sealed class DamageText : IComponent
	{
		public float value;
		public string format;
	}
}
