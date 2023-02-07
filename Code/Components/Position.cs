using Entitas;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups.ECS
{
	[EkPopup]
	public sealed class Position : IComponent
	{
		public Vector2 v;
	}
}
