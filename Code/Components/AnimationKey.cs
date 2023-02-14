// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using Entitas;

namespace EchKode.PBMods.DamagePopups.ECS
{
	[EkPopup, EkRequest, EkTracking, EkReplay]
	public sealed class AnimationKey : IComponent
	{
		public string s;
	}
}
