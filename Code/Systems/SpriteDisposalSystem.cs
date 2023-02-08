﻿using Entitas;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	sealed class SpriteDisposalSystem : IExecuteSystem, ITearDownSystem
	{
		public void Execute()
		{
			var now = Contexts.sharedInstance.combat.simulationTime.f;
			foreach (var ekp in ECS.Contexts.sharedInstance.ekPopup.GetEntities())
			{
				if (!ekp.hasSpriteDisposal)
				{
					continue;
				}

				AnimationHelper.DisposeSprites(ekp.spriteDisposal.spriteIDBase, ekp.spriteDisposal.count);

				if (ModLink.Settings.enableLogging)
				{
					Debug.LogFormat(
						"Mod {0} ({1}) CIViewCombatPopups.DisposeSprites | time: {2:F3} | popup: {3} | sprites: {4}-{5}",
						ModLink.modIndex,
						ModLink.modId,
						now,
						ekp.spriteDisposal.popupID,
						ekp.spriteDisposal.spriteIDBase,
						ekp.spriteDisposal.spriteIDBase + ekp.spriteDisposal.count - 1);
				}

				ekp.Destroy();
			}
		}

		public void TearDown()
		{
			foreach (var ekp in ECS.Contexts.sharedInstance.ekPopup.GetEntities())
			{
				if (!ekp.hasSpriteDisposal)
				{
					continue;
				}
				AnimationHelper.DisposeSprites(ekp.spriteDisposal.spriteIDBase, ekp.spriteDisposal.count);
				ekp.Destroy();
			}
		}
	}
}
