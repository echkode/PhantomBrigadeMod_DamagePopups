// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using System.Collections.Generic;

using Entitas;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	using static ECS.ContextsExtensions;

	sealed class DamagePopupTrackerSystem : IInitializeSystem, IExecuteSystem, ITearDownSystem
	{
		private static readonly Dictionary<string, int> displayDurations = new Dictionary<string, int>();
		private static bool logEnabled;

		public void Initialize()
		{
			logEnabled = ModLink.Settings.IsLoggingEnabled(ModLink.ModSettings.LoggingFlag.Tracking);
			if (displayDurations.Count != 0)
			{
				return;
			}

			foreach (var animationKey in CIViewCombatPopups.AnimationKeys)
			{
				var definition = CIViewCombatPopups.GetDefinition(animationKey);
				var duration = definition.timeTotal * ModLink.Settings.samplesPerSecond;
				displayDurations.Add(animationKey, Mathf.FloorToInt(duration));
			}
		}

		public void Execute()
		{
			var now = Contexts.sharedInstance.combat.simulationTime.f;
			foreach (var req in ECS.Contexts.sharedInstance.ekRequest.GetEntities())
			{
				var trackingEntities = ECS.Contexts.sharedInstance.ekTracking.GetEntitiesWithCombatUnitID(req.combatUnitID.id);
				if (trackingEntities != null)
				{
					if (TryUpdateTracker(trackingEntities, req, now))
					{
						req.Destroy();
						continue;
					}
				}

				AddTracker(req, now);
				req.Destroy();
			}
		}

		public void TearDown()
		{
			foreach (var tracking in ECS.Contexts.sharedInstance.ekTracking.GetEntities())
			{
				if (tracking.hasDamageHistory)
				{
					tracking.damageHistory.samples.Clear();
				}
				tracking.Destroy();
			}
		}

		static bool TryUpdateTracker(
			HashSet<ECS.EkTrackingEntity> trackingEntities,
			ECS.EkRequestEntity req,
			float now)
		{
			foreach (var tracking in trackingEntities)
			{
				if (!tracking.hasAnimationKey)
				{
					continue;
				}

				if (req.animationKey.s != tracking.animationKey.s)
				{
					continue;
				}

				if (logEnabled)
				{
					Debug.LogFormat(
						"Mod {0} ({1}) DamagePopupTrackerSystem found tracker | time: {2:F3} | last time: {3:F3} | unit: C-{4} | type: {5} | value: {6} | acc value: {7}",
						ModLink.modIndex,
						ModLink.modId,
						now,
						tracking.damageTracker.timeLast,
						req.combatUnitID.id,
						req.animationKey.s,
						req.damageText.value,
						tracking.damageTracker.accumulatedValue);
				}

				var acc = tracking.damageTracker.accumulatedValue + req.damageText.value;
				tracking.ReplaceDamageTracker(
					FormatValue(req.damageText.format, acc),
					acc,
					now);
				tracking.isDirty = true;
				TrackHistory(req, tracking, now);

				return true;
			}

			return false;
		}

		static void AddTracker(ECS.EkRequestEntity req, float now)
		{
			var tracking = ECS.Contexts.sharedInstance.ekTracking.CreateEntity();
			tracking.AddCombatUnitID(req.combatUnitID.id);
			tracking.AddAnimationKey(req.animationKey.s);
			tracking.AddDamageTracker(
				FormatValue(req.damageText.format, req.damageText.value),
				req.damageText.value,
				now);
			tracking.isDirty = true;

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) DamagePopupTrackerSystem new tracker | time: {2:F3} | unit: C-{3} | type: {4} | value: {5}",
					ModLink.modIndex,
					ModLink.modId,
					now,
					req.combatUnitID.id,
					req.animationKey.s,
					req.damageText.value);
			}

			TrackHistory(req, tracking, now);
		}

		static string FormatValue(string format, float value)
		{
			if (format == "F0" && !value.RoughlyEqual(0f))
			{
				value = Mathf.Round(value);
				if (value.RoughlyEqual(0f))
				{
					value = 1f;
				}
			}
			return value.ToString(format);
		}

		static void TrackHistory(
			ECS.EkRequestEntity req,
			ECS.EkTrackingEntity tracking,
			float now)
		{
			if (ModLink.Settings.replayPopups == ModLink.ModSettings.ReplayPopup.None)
			{
				return;
			}

			if (!tracking.hasDamageHistory)
			{
				tracking.AddDamageHistory(new List<DamageHistorySample>());
			}

			var (turn, sampleIndex) = ReplayHelper.GetSampleIndex(now);
			var sample = FindSample(
				tracking.damageHistory.samples,
				turn,
				sampleIndex);
			sample.Value += req.damageText.value;
			sample.Accumulated += req.damageText.value;
			sample.DisplayDuration = displayDurations[req.animationKey.s];

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) DamagePopupTrackerSystem history | time: {2:F3} | unit: C-{3} | type: {4} | value: {5} | index: {6} | sample value: {7} | accumulated value: {8}",
					ModLink.modIndex,
					ModLink.modId,
					now,
					req.combatUnitID.id,
					req.animationKey.s,
					req.damageText.value,
					sampleIndex,
					sample.Value,
					sample.Accumulated);
			}
		}

		static DamageHistorySample FindSample(
			List<DamageHistorySample> samples,
			int turn,
			int sampleIndex)
		{
			if (samples.Count == 0)
			{
				return AddSample(samples, turn, sampleIndex);
			}

			var lastSample = samples[samples.Count - 1];
			if (turn != lastSample.Turn)
			{
				return AddSample(samples, turn, sampleIndex);
			}
			if (sampleIndex != lastSample.Index)
			{
				return AddSample(samples, turn, sampleIndex);
			}

			return lastSample;
		}

		static DamageHistorySample AddSample(
			List<DamageHistorySample> samples,
			int turn,
			int sampleIndex)
		{
			var sample = new DamageHistorySample()
			{
				Turn = turn,
				Index = sampleIndex,
			};
			samples.Add(sample);
			if (samples.Count != 1)
			{
				var prev = samples[samples.Count - 2];
				if (sampleIndex - prev.Index < prev.DisplayDuration)
				{
					sample.Value = prev.Value;
				}
				sample.Accumulated = prev.Accumulated;
			}
			return sample;
		}
	}
}
