namespace EchKode.PBMods.DamagePopups.ECS
{
	//------------------------------------------------------------------------------
	// <auto-generated>
	//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
	//
	//     Changes to this file may cause incorrect behavior and will be lost if
	//     the code is regenerated.
	// </auto-generated>
	//------------------------------------------------------------------------------
	public partial class EkReplayEntity
	{
		public ReplaySlots replaySlots { get { return (ReplaySlots)GetComponent(EkReplayComponentsLookup.ReplaySlots); } }
		public bool hasReplaySlots { get { return HasComponent(EkReplayComponentsLookup.ReplaySlots); } }

		public void AddReplaySlots(int[] newA)
		{
			var index = EkReplayComponentsLookup.ReplaySlots;
			var component = (ReplaySlots)CreateComponent(index, typeof(ReplaySlots));
			component.a = newA;
			AddComponent(index, component);
		}

		public void ReplaceReplaySlots(int[] newA)
		{
			var index = EkReplayComponentsLookup.ReplaySlots;
			var component = (ReplaySlots)CreateComponent(index, typeof(ReplaySlots));
			component.a = newA;
			ReplaceComponent(index, component);
		}

		public void RemoveReplaySlots()
		{
			RemoveComponent(EkReplayComponentsLookup.ReplaySlots);
		}
	}

	//------------------------------------------------------------------------------
	// <auto-generated>
	//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentMatcherApiGenerator.
	//
	//     Changes to this file may cause incorrect behavior and will be lost if
	//     the code is regenerated.
	// </auto-generated>
	//------------------------------------------------------------------------------
	public sealed partial class EkReplayMatcher
	{
		static Entitas.IMatcher<EkReplayEntity> _matcherReplaySlots;

		public static Entitas.IMatcher<EkReplayEntity> ReplaySlots
		{
			get
			{
				if (_matcherReplaySlots == null)
				{
					var matcher = (Entitas.Matcher<EkReplayEntity>)Entitas.Matcher<EkReplayEntity>.AllOf(EkReplayComponentsLookup.ReplaySlots);
					matcher.componentNames = EkReplayComponentsLookup.componentNames;
					_matcherReplaySlots = matcher;
				}

				return _matcherReplaySlots;
			}
		}
	}
}
