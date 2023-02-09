﻿using Entitas;

namespace EchKode.PBMods.DamagePopups.ECS
{
	[EkPopup, EkReplay]
	public sealed class DisplayText : IComponent
	{
		public string text;
		public int spriteIDBase;
		public float startTime;
	}
}
