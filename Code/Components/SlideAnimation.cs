// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using Entitas;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups.ECS
{
	[EkPopup]
	public sealed class SlideAnimation : IComponent
	{
		public float startTime;
		public Vector2 slideToOffset;
		public int slot;
	}
}
