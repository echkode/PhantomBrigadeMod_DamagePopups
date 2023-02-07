using Entitas;

namespace EchKode.PBMods.DamagePopups.ECS
{
	[EkPopup, EkRequest, EkTracking]
	public sealed class AnimationKey : IComponent
	{
		public string s;
	}
}
