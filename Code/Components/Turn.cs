// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace EchKode.PBMods.DamagePopups.ECS
{
	[EkReplay]
	[Unique]
	public sealed class Turn : IComponent
	{
		public int i;
	}
}
