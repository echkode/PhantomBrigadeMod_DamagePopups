using System.Collections.Generic;

using Entitas;

using PhantomBrigade;

namespace EchKode.PBMods.DamagePopups
{
	using static ECS.ContextsExtensions;

	sealed class ReplayPopupSlotSystem : ReactiveSystem<ECS.EkReplayEntity>
	{
		private static readonly List<(string AnimationKey, ECS.EkReplayEntity Table)> collectedTables = new List<(string, ECS.EkReplayEntity)>();
		private static readonly List<float[]> collectedValues = new List<float[]>();
		private static readonly System.Comparison<(string, ECS.EkReplayEntity)> animationKeyComparison =
			new System.Comparison<(string, ECS.EkReplayEntity)>(CompareCollectedValues);

		private readonly ECS.EkReplayContext ekReplay;
		private int turn;

		public ReplayPopupSlotSystem(ECS.Contexts contexts)
			: base(contexts.ekReplay)
		{
			ekReplay = contexts.ekReplay;
			turn = -1;
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

			var participants = ScenarioUtility.GetCombatParticipantUnits();
			if (participants == null)
			{
				return;
			}

			foreach (var unit in participants)
			{
				var combatUnit = IDUtility.GetLinkedCombatEntity(unit);
				if (combatUnit == null)
				{
					continue;
				}

				CollectTables(combatUnit.id.id);
				if (collectedTables.Count == 0)
				{
					continue;
				}

				for (var i = 0; i < collectedTables.Count; i += 1)
				{
					var table = collectedTables[i].Table;
					if (!table.hasReplaySlots)
					{
						table.AddReplaySlots(new int[ReplayHelper.SummarySize]);
					}
					FillSlots(i, table.replaySlots.a, collectedValues);
				}
			}
		}

		void CollectTables(int combatUnitID)
		{
			collectedTables.Clear();
			var entities = ekReplay.GetEntitiesWithCombatUnitID(combatUnitID);
			if (entities == null)
			{
				return;
			}
			foreach (var ekr in entities)
			{
				if (ekr.hasDamageAccumulation || ekr.hasDamageSummary)
				{
					collectedTables.Add((ekr.animationKey.s, ekr));
				}
			}
			collectedTables.Sort(animationKeyComparison);

			collectedValues.Clear();
			foreach (var (_, table) in collectedTables)
			{
				if (table.hasDamageAccumulation)
				{
					collectedValues.Add(table.damageAccumulation.a);
					continue;
				}
				if (table.hasDamageSummary)
				{
					collectedValues.Add(table.damageSummary.a);
				}
			}
		}

		static void FillSlots(int rank, int[] slots, List<float[]> values)
		{
			var length = ReplayHelper.SummarySize;
			for (var i = 0; i < length; i += 1)
			{
				if (values[rank][i] == 0f)
				{
					slots[i] = -1;
					continue;
				}

				var slot = 0;
				for (var j = 0; j < rank; j += 1)
				{
					if (values[j][i] == 0f)
					{
						continue;
					}
					slot += 1;
				}
				slots[i] = slot;
			}
		}

		static int CompareCollectedValues((string AnimationKey, ECS.EkReplayEntity) x, (string AnimationKey, ECS.EkReplayEntity) y) =>
			ReplayHelper.CompareAnimationKey(x.AnimationKey, y.AnimationKey);
	}
}
