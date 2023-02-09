using System.Collections.Generic;

using Entitas;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	sealed class SampleCarryOverSystem : ReactiveSystem<CombatEntity>, IInitializeSystem
	{
		private static bool logEnabled;

		private readonly CombatContext combat;
		private float playbackSpeed;

		public SampleCarryOverSystem(Contexts contexts)
			: base(contexts.combat)
		{
			combat = contexts.combat;
		}

		public void Initialize()
		{
			logEnabled = ModLink.Settings.IsLoggingEnabled(ModLink.ModSettings.LoggingFlag.Tracking);
			playbackSpeed = CIViewCombatPopups.PlaybackSpeed;
		}

		protected override ICollector<CombatEntity> GetTrigger(IContext<CombatEntity> context) => context.CreateCollector(CombatMatcher.CurrentTurn);
		protected override bool Filter(CombatEntity entity) => entity.hasCurrentTurn;

		protected override void Execute(List<CombatEntity> entities)
		{
			var now = combat.simulationTime.f;
			var (turn, sampleIndex) = ReplayHelper.GetSampleIndex(now);

			foreach (var tracking in ECS.Contexts.sharedInstance.ekTracking.GetEntities())
			{
				if (!tracking.hasDamageHistory)
				{
					continue;
				}
				if (tracking.damageHistory.samples.Count == 0)
				{
					continue;
				}

				var samples = tracking.damageHistory.samples;
				var lastSample = samples[samples.Count - 1];
				samples.Clear();

				var oldDuration = lastSample.DisplayDuration;
				AdjustSample(tracking, lastSample, now);
				lastSample.Turn = turn;
				var oldSampleIndex = lastSample.Index;
				lastSample.Index = sampleIndex;
				samples.Add(lastSample);

				if (logEnabled)
				{
					Debug.LogFormat(
						"Mod {0} ({1}) SampleCarryOverSystem -- carrying last sample | turn: {2} | combat unit: C-{3} | old sample: {4} | value: {5:F1} | acc: {6:F1} | old duration: {7} | display duration: {8}",
						ModLink.modIndex,
						ModLink.modId,
						turn,
						tracking.combatUnitID.id,
						oldSampleIndex,
						lastSample.Value,
						lastSample.Accumulated,
						oldDuration,
						lastSample.DisplayDuration);
				}
			}
		}

		void AdjustSample(
			ECS.EkTrackingEntity tracking,
			DamageHistorySample sample,
			float now)
		{
			var elapsedTime = (now - tracking.damageTracker.timeLast) * playbackSpeed;
			var definition = CIViewCombatPopups.GetDefinition(tracking.animationKey.s);
			var remainingTime = definition.timeTotal - elapsedTime;
			var duration = remainingTime > 0f
				? Mathf.FloorToInt(remainingTime * ModLink.Settings.samplesPerSecond)
				: 0;
			if (duration != 0)
			{
				sample.DisplayDuration = duration;
				return;
			}
			sample.Value = 0f;
			sample.DisplayDuration = 0;
		}
	}
}
