using System.Collections.Generic;

using Entitas;

using PBCIViewPopups = CIViewPopups;

namespace EchKode.PBMods.DamagePopups.ECS
{
	[EkPopup]
	public sealed class Popup : IComponent
	{
		public int popupID;
		public List<PBCIViewPopups.PopupNestedSegment> segments;
	}
}
