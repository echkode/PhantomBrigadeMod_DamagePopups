using Entitas;

namespace EchKode.PBMods.DamagePopups.ECS
{
	[EkPopup]
	public sealed class SpriteDisposal : IComponent
	{
		public int popupID;
		public int spriteIDBase;
		public int count;
	}
}
