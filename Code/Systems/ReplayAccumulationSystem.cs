using System.Collections.Generic;
using System.Text;

using Entitas;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	sealed class ReplayAccumulationSystem : ReactiveSystem<ECS.EkReplayEntity>, IInitializeSystem
	{
		private static bool logEnabled;

		private readonly ECS.EkReplayContext ekReplay;
		private int turn;

		public ReplayAccumulationSystem(ECS.Contexts contexts)
			: base(contexts.ekReplay)
		{
			ekReplay = contexts.ekReplay;
			turn = -1;
		}

		public void Initialize()
		{
			logEnabled = ModLink.Settings.IsLoggingEnabled(ModLink.ModSettings.LoggingFlag.ReplayTables);
		}

		protected override ICollector<ECS.EkReplayEntity> GetTrigger(IContext<ECS.EkReplayEntity> context) =>
			context.CreateCollector(ECS.EkReplayMatcher.Active.Added());
		protected override bool Filter(ECS.EkReplayEntity entity) => true;

		protected override void Execute(List<ECS.EkReplayEntity> entities)
		{
			if (ekReplay.hasTurn && ekReplay.turn.i == turn)
			{
				return;
			}
			turn = ekReplay.turn.i;

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) ReplayAccumulationSystem | turn: {2}",
					ModLink.modIndex,
					ModLink.modId,
					turn);
			}

			foreach (var tracking in ECS.Contexts.sharedInstance.ekTracking.GetEntities())
			{
				if (!tracking.hasDamageHistory)
				{
					continue;
				}

				if (logEnabled)
				{
					if (tracking.damageHistory.samples.Count == 0)
					{
						Debug.LogFormat(
							"Mod {0} ({1}) ReplayAccumulationSystem -- zero damage history samples | turn: {2} | combat unit: C-{3}",
							ModLink.modIndex,
							ModLink.modId,
							turn,
							tracking.combatUnitID.id);
					}
					else
					{
						var samples = tracking.damageHistory.samples;
						var sb = new StringBuilder(tracking.animationKey.s);
						foreach (var sample in samples)
						{
							sb.AppendLine()
								.AppendFormat("  {0}", sample.Index)
								.AppendFormat(",{0:F1}", sample.Accumulated)
								.AppendFormat(",{0:F1}", sample.Value);
						}
						Debug.LogFormat(
							"Mod {0} ({1}) ReplayAccumulationSystem -- history samples | turn: {2} | combat unit: C-{3}\n  {4}",
							ModLink.modIndex,
							ModLink.modId,
							turn,
							tracking.combatUnitID.id,
							sb);
					}
				}

				var tables = ReplayHelper.GetReplayTables(tracking.combatUnitID.id, tracking.animationKey.s);
				if (!tables.hasDamageAccumulation)
				{
					tables.AddDamageAccumulation(new float[ReplayHelper.SummarySize]);
				}

				FillAccumulation(tables, tracking.damageHistory.samples);
			}
		}

		static void FillAccumulation(ECS.EkReplayEntity ekr, List<DamageHistorySample> samples)
		{
			var accumulation = ekr.damageAccumulation.a;
			accumulation[0] = accumulation[accumulation.Length - 1];

			var sampleIndex = 0;
			if (samples[0].Index == sampleIndex)
			{
				accumulation[0] = samples[0].Accumulated;
				sampleIndex += 1;
			}

			for (var i = 1; i < accumulation.Length; i += 1)
			{
				accumulation[i] = accumulation[i - 1];
				if (sampleIndex == samples.Count)
				{
					continue;
				}
				var sample = samples[sampleIndex];
				if (sample.Index != i)
				{
					continue;
				}
				accumulation[i] = sample.Accumulated;
				sampleIndex += 1;
			}
		}
	}
}
