using Entitas;

namespace EchKode.PBMods.DamagePopups.ECS
{
	[EkTracking]
	public sealed class DamageTracker : IComponent
	{
		public string text;
		public float accumulatedValue;
		public double timeLast;
	}
}
