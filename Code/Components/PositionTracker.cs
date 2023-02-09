using Entitas;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups.ECS
{
	[EkTracking, EkReplay]
	public sealed class PositionTracker : IComponent
	{
		public Vector3[] a;
	}
}
