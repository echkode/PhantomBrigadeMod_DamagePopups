// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using System.Collections.Generic;

using Entitas;

namespace EchKode.PBMods.DamagePopups
{
	public sealed class DamageHistorySample
	{
		public int Turn;
		public int Index;
		public int DisplayDuration;
		public float Value;
		public float Accumulated;
	}

	namespace ECS
	{
		[EkTracking]
		public sealed class DamageHistory : IComponent
		{
			public List<DamageHistorySample> samples;
		}
	}
}
