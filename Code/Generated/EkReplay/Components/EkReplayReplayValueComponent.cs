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
		public ReplayValue replayValue { get { return (ReplayValue)GetComponent(EkReplayComponentsLookup.ReplayValue); } }
		public bool hasReplayValue { get { return HasComponent(EkReplayComponentsLookup.ReplayValue); } }

		public void AddReplayValue(float newAccumulated, float newSummary)
		{
			var index = EkReplayComponentsLookup.ReplayValue;
			var component = (ReplayValue)CreateComponent(index, typeof(ReplayValue));
			component.accumulated = newAccumulated;
			component.summary = newSummary;
			AddComponent(index, component);
		}

		public void ReplaceReplayValue(float newAccumulated, float newSummary)
		{
			var index = EkReplayComponentsLookup.ReplayValue;
			var component = (ReplayValue)CreateComponent(index, typeof(ReplayValue));
			component.accumulated = newAccumulated;
			component.summary = newSummary;
			ReplaceComponent(index, component);
		}

		public void RemoveReplayValue()
		{
			RemoveComponent(EkReplayComponentsLookup.ReplayValue);
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
		static Entitas.IMatcher<EkReplayEntity> _matcherReplayValue;

		public static Entitas.IMatcher<EkReplayEntity> ReplayValue
		{
			get
			{
				if (_matcherReplayValue == null)
				{
					var matcher = (Entitas.Matcher<EkReplayEntity>)Entitas.Matcher<EkReplayEntity>.AllOf(EkReplayComponentsLookup.ReplayValue);
					matcher.componentNames = EkReplayComponentsLookup.componentNames;
					_matcherReplayValue = matcher;
				}

				return _matcherReplayValue;
			}
		}
	}
}