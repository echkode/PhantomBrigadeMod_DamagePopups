using Entitas;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	sealed class SpriteDisposalSystem : IExecuteSystem, ITearDownSystem
	{
		internal static bool logEnabled = false;

		public void Execute()
		{
			foreach (var ekp in ECS.Contexts.sharedInstance.ekPopup.GetEntities())
			{
				if (!ekp.hasSpriteDisposal)
				{
					continue;
				}

				AnimationHelper.DisposeSprites(ekp.spriteDisposal.spriteIDBase, ekp.spriteDisposal.count);

				if (logEnabled)
				{
					Debug.LogFormat(
						"Mod {0} ({1}) CIViewCombatPopups.DisposeSprites | time: {2} | popup: {3} | sprites: {4}-{5}",
						ModLink.modIndex,
						ModLink.modId,
						Time.realtimeSinceStartupAsDouble,
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
