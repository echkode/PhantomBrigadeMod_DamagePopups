using System.Collections.Generic;

using Entitas;

using PhantomBrigade;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	using static ECS.ContextsExtensions;

	sealed class ReplayPositionTrackerSystem : ReactiveSystem<CombatEntity>
	{
		private readonly CombatContext combat;
		private int sampleIndexLast;

		public ReplayPositionTrackerSystem(Contexts contexts)
			: base(contexts.combat)
		{
			combat = contexts.combat;
		}

		protected override ICollector<CombatEntity> GetTrigger(IContext<CombatEntity> context) =>
			context.CreateCollector(CombatMatcher.SimulationTime);
		protected override bool Filter(CombatEntity entity) => entity.hasSimulationTime;

		protected override void Execute(List<CombatEntity> entities)
		{
			if (!combat.Simulating)
			{
				return;
			}

			var (turn, sampleIndex) = ReplayHelper.GetSampleIndex(combat.simulationTime.f);
			if (sampleIndex < 0)
			{
				if (turn != -1)
				{
					Debug.LogWarningFormat(
						"Mod {0} ({1}) ReplayPositionTrackerSystem -- sampleIndex shouldn't be negative | time: {2:F3} | turn: {3} | time in turn: {4}",
						ModLink.modIndex,
						ModLink.modId,
						combat.simulationTime.f,
						turn,
						sampleIndex);
				}
				return;
			}
			if (sampleIndex > ReplayHelper.TurnLengthInSamples)
			{
				Debug.LogWarningFormat(
					"Mod {0} ({1}) ReplayPositionTrackerSystem -- sampleIndex should be less than turn length | time: {2:F3} | turn: {3} | turn length: {4} | time in turn: {5}",
					ModLink.modIndex,
					ModLink.modId,
					combat.simulationTime.f,
					turn,
					ReplayHelper.TurnLengthInSamples,
					sampleIndex);
				return;
			}
			if (sampleIndex == sampleIndexLast)
			{
				return;
			}

			foreach (var unit in ScenarioUtility.GetCombatParticipantUnits())
			{
				var combatUnit = IDUtility.GetLinkedCombatEntity(unit);
				if (combatUnit == null)
				{
					continue;
				}

				var tracker = FindTracker(combatUnit.id.id);

				Vector3 position = default;
				if (combatUnit.hasPosition)
				{
					position = UnitHelper.GetPopupPosition(combatUnit);
				}
				if (position == default && sampleIndex != 0)
				{
					position = tracker.positionTracker.a[sampleIndex - 1];
				}
				tracker.positionTracker.a[sampleIndex] = position;

				var delta = sampleIndex - sampleIndexLast;
				if (delta > 1)
				{
					// Fill in gaps caused when the frame rate drops low enough that
					// simulation time is advanced by more than 1 sample.
					for (var i = sampleIndexLast + 1; i < sampleIndex; i += 1)
					{
						var t = ((float)i).RemapTo01(sampleIndexLast, sampleIndex);
						tracker.positionTracker.a[i] = Vector3.Lerp(
							tracker.positionTracker.a[sampleIndexLast],
							tracker.positionTracker.a[sampleIndex],
							t);
					}
				}
			}

			sampleIndexLast = sampleIndex;
		}

		private static ECS.EkTrackingEntity FindTracker(int combatUnitID)
		{
			var trackingEntities = ECS.Contexts.sharedInstance.ekTracking.GetEntitiesWithCombatUnitID(combatUnitID);
			if (trackingEntities != null)
			{
				foreach (var ekt in trackingEntities)
				{
					if (ekt.hasPositionTracker)
					{
						return ekt;
					}
				}
			}

			var tracker = ECS.Contexts.sharedInstance.ekTracking.CreateEntity();
			tracker.AddCombatUnitID(combatUnitID);
			tracker.AddPositionTracker(new Vector3[ReplayHelper.SummarySize]);
			return tracker;
		}
	}
}
