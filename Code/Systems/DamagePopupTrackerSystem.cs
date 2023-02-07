using System.Collections.Generic;

using Entitas;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	using static ECS.ContextsExtensions;

	sealed class DamagePopupTrackerSystem : IExecuteSystem, ITearDownSystem
	{
		internal static bool logEnabled = false;

		public void Execute()
		{
			var now = Time.realtimeSinceStartupAsDouble;
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
				tracking.Destroy();
			}
		}

		static bool TryUpdateTracker(
			HashSet<ECS.EkTrackingEntity> trackingEntities,
			ECS.EkRequestEntity req,
			double now)
		{
			foreach (var tracking in trackingEntities)
			{
				if (req.animationKey.s != tracking.animationKey.s)
				{
					continue;
				}

				if (logEnabled)
				{
					Debug.LogFormat(
						"Mod {0} ({1}) DamagePopupTrackerSystem found tracker | time: {2:F3} | unit: C-{3} | type: {4} | value: {5} | acc value: {6} | last time: {7:F3}",
						ModLink.modIndex,
						ModLink.modId,
						now,
						req.combatUnitID.id,
						req.animationKey.s,
						req.damageText.value,
						tracking.damageTracker.accumulatedValue,
						tracking.damageTracker.timeLast);
				}

				var acc = tracking.damageTracker.accumulatedValue + req.damageText.value;
				tracking.ReplaceDamageTracker(
					FormatValue(req.damageText.format, acc),
					acc,
					now);
				tracking.isDirty = true;

				return true;
			}

			return false;
		}

		static void AddTracker(ECS.EkRequestEntity req, double now)
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
	}
}
