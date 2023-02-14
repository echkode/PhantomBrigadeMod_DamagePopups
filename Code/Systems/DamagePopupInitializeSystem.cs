// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using Entitas;

namespace EchKode.PBMods.DamagePopups
{
	sealed class DamagePopupInitializeSystem : IInitializeSystem
	{
		public void Initialize()
		{
			CIViewCombatPopups.Initialize();
		}
	}
}
