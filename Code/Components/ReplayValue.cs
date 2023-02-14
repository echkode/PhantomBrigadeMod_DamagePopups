// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using Entitas;

namespace EchKode.PBMods.DamagePopups.ECS
{
	[EkReplay]
	public sealed class ReplayValue : IComponent
	{
		public float accumulated;
		public float summary;
	}
}
