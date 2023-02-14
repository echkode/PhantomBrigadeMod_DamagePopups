// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using Entitas;

namespace EchKode.PBMods.DamagePopups
{
	sealed class ReplayTablesDestroySystem : ITearDownSystem
	{
		public void TearDown()
		{
			foreach (var ekr in ECS.Contexts.sharedInstance.ekReplay.GetEntities())
			{
				if (ekr.hasDamageAccumulation || ekr.hasDamageSummary)
				{
					ekr.Destroy();
				}
			}
		}
	}
}
