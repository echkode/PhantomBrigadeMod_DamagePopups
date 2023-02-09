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
		public DamageAccumulation damageAccumulation { get { return (DamageAccumulation)GetComponent(EkReplayComponentsLookup.DamageAccumulation); } }
		public bool hasDamageAccumulation { get { return HasComponent(EkReplayComponentsLookup.DamageAccumulation); } }

		public void AddDamageAccumulation(float[] newA)
		{
			var index = EkReplayComponentsLookup.DamageAccumulation;
			var component = (DamageAccumulation)CreateComponent(index, typeof(DamageAccumulation));
			component.a = newA;
			AddComponent(index, component);
		}

		public void ReplaceDamageAccumulation(float[] newA)
		{
			var index = EkReplayComponentsLookup.DamageAccumulation;
			var component = (DamageAccumulation)CreateComponent(index, typeof(DamageAccumulation));
			component.a = newA;
			ReplaceComponent(index, component);
		}

		public void RemoveDamageAccumulation()
		{
			RemoveComponent(EkReplayComponentsLookup.DamageAccumulation);
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
		static Entitas.IMatcher<EkReplayEntity> _matcherDamageAccumulation;

		public static Entitas.IMatcher<EkReplayEntity> DamageAccumulation
		{
			get
			{
				if (_matcherDamageAccumulation == null)
				{
					var matcher = (Entitas.Matcher<EkReplayEntity>)Entitas.Matcher<EkReplayEntity>.AllOf(EkReplayComponentsLookup.DamageAccumulation);
					matcher.componentNames = EkReplayComponentsLookup.componentNames;
					_matcherDamageAccumulation = matcher;
				}

				return _matcherDamageAccumulation;
			}
		}
	}
}