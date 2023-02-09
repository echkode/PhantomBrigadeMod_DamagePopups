using System.Collections.Generic;

using Entitas;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	sealed class ReplayExitSystem : ReactiveSystem<ECS.EkReplayEntity>, IInitializeSystem
	{
		private static bool logEnabled;

		private readonly ECS.EkReplayContext ekReplay;

		public ReplayExitSystem(ECS.Contexts contexts)
			: base(contexts.ekReplay)
		{
			ekReplay = contexts.ekReplay;
		}

		public void Initialize()
		{
			logEnabled = ModLink.Settings.IsLoggingEnabled(ModLink.ModSettings.LoggingFlag.ReplayExit);
		}

		protected override ICollector<ECS.EkReplayEntity> GetTrigger(IContext<ECS.EkReplayEntity> context) =>
			context.CreateCollector(ECS.EkReplayMatcher.Active.Removed());
		protected override bool Filter(ECS.EkReplayEntity entity) => true;

		protected override void Execute(List<ECS.EkReplayEntity> entities)
		{
			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) ReplayExitSystem",
					ModLink.modIndex,
					ModLink.modId);
			}

			foreach (var ekr in ekReplay.GetEntities())
			{
				if (!ekr.hasPopup)
				{
					continue;
				}
				ReplayHelper.DestroyPopup(ekr);
			}
		}
	}
}
