// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using Entitas;

namespace EchKode.PBMods.DamagePopups.ECS
{
	[EkRequest]
	public sealed class DamageText : IComponent
	{
		public float value;
		public string format;
	}
}
