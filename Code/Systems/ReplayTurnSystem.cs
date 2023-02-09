using System.Collections.Generic;

using Entitas;

namespace EchKode.PBMods.DamagePopups
{
	sealed class ReplayTurnSystem : ReactiveSystem<CombatEntity>
	{
		private readonly CombatContext combat;

		public ReplayTurnSystem(Contexts contexts)
			: base(contexts.combat)
		{
			combat = contexts.combat;
		}

		protected override ICollector<CombatEntity> GetTrigger(IContext<CombatEntity> context) => context.CreateCollector(CombatMatcher.CurrentTurn);
		protected override bool Filter(CombatEntity entity) => entity.hasCurrentTurn;

		protected override void Execute(List<CombatEntity> entities)
		{
			var now = combat.simulationTime.f;
			var (turn, _) = ReplayHelper.GetSampleIndex(now);
			ECS.Contexts.sharedInstance.ekReplay.ReplaceTurn(turn);
		}
	}
}
