using System.Collections.Generic;

using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace EchKode.PBMods.DamagePopups.ECS
{
	[EkReplay]
	[Unique]
	public sealed class SummaryTextFormat : IComponent
	{
		public List<(string AnimationKey, string Format)> formats;
	}
}
