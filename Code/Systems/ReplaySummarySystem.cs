// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using System.Collections.Generic;

using Entitas;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	sealed class ReplaySummarySystem : ReactiveSystem<ECS.EkReplayEntity>, IInitializeSystem
	{
		private static bool logEnabled;

		private readonly ECS.EkReplayContext ekReplay;
		private int turn;

		public ReplaySummarySystem(ECS.Contexts contexts)
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
					"Mod {0} ({1}) ReplaySummarySystem | turn: {2}",
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

				if (tracking.damageHistory.samples.Count == 0)
				{
					if (logEnabled)
					{
						Debug.LogFormat(
							"Mod {0} ({1}) ReplaySummarySystem -- zero damage history samples | turn: {2} | combat unit: C-{3}",
							ModLink.modIndex,
							ModLink.modId,
							turn,
							tracking.combatUnitID.id);
					}
				}

				var tables = ReplayHelper.GetReplayTables(tracking.combatUnitID.id, tracking.animationKey.s);
				if (!tables.hasDamageSummary)
				{
					tables.AddDamageSummary(new float[ReplayHelper.SummarySize]);
					tables.AddDisplayInterpolation(new float[ReplayHelper.SummarySize]);
				}
				FillSummary(tables, tracking);
			}
		}

		void FillSummary(ECS.EkReplayEntity ekr, ECS.EkTrackingEntity tracking)
		{
			var summary = ekr.damageSummary.a;
			var interpolation = ekr.displayInterpolation.a;
			var samples = tracking.damageHistory.samples;
			var sampleIndex = 0;
			var displayUntil = 0;
			var duration = 0;
			var start = 0;
			for (var i = 0; i < summary.Length; i += 1)
			{
				var (incr, value, until) = GetSampleValue(samples, sampleIndex, i, displayUntil);
				sampleIndex += incr;
				displayUntil = until;
				duration = incr == 1 ? until - i : duration;
				start = incr == 1 ? i : start;
				summary[i] = value;
				interpolation[i] = duration != 0 ? (float)(i - start) / duration : 1f;
			}
		}

		static (int, float, int) GetSampleValue(
			List<DamageHistorySample> samples,
			int sampleIndex,
			int index,
			int displayUntil)
		{
			if (sampleIndex < samples.Count)
			{
				var sample = samples[sampleIndex];
				if (index == sample.Index)
				{
					return (1, sample.Value, sample.Index + sample.DisplayDuration);
				}
			}

			if (sampleIndex == 0)
			{
				return (0, 0f, 0);
			}
			if (displayUntil <= index)
			{
				return (0, 0f, 0);
			}
			return (0, samples[sampleIndex - 1].Value, displayUntil);
		}
	}
}
