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
	public partial class EkRequestEntity
	{
		public CombatUnitID combatUnitID { get { return (CombatUnitID)GetComponent(EkRequestComponentsLookup.CombatUnitID); } }
		public bool hasCombatUnitID { get { return HasComponent(EkRequestComponentsLookup.CombatUnitID); } }

		public void AddCombatUnitID(int newId)
		{
			var index = EkRequestComponentsLookup.CombatUnitID;
			var component = (CombatUnitID)CreateComponent(index, typeof(CombatUnitID));
			component.id = newId;
			AddComponent(index, component);
		}

		public void ReplaceCombatUnitID(int newId)
		{
			var index = EkRequestComponentsLookup.CombatUnitID;
			var component = (CombatUnitID)CreateComponent(index, typeof(CombatUnitID));
			component.id = newId;
			ReplaceComponent(index, component);
		}

		public void RemoveCombatUnitID()
		{
			RemoveComponent(EkRequestComponentsLookup.CombatUnitID);
		}
	}

	//------------------------------------------------------------------------------
	// <auto-generated>
	//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiInterfaceGenerator.
	//
	//     Changes to this file may cause incorrect behavior and will be lost if
	//     the code is regenerated.
	// </auto-generated>
	//------------------------------------------------------------------------------
	public partial class EkRequestEntity : ICombatUnitIDEntity { }

	//------------------------------------------------------------------------------
	// <auto-generated>
	//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentMatcherApiGenerator.
	//
	//     Changes to this file may cause incorrect behavior and will be lost if
	//     the code is regenerated.
	// </auto-generated>
	//------------------------------------------------------------------------------
	public sealed partial class EkRequestMatcher
	{
		static Entitas.IMatcher<EkRequestEntity> _matcherCombatUnitID;

		public static Entitas.IMatcher<EkRequestEntity> CombatUnitID
		{
			get
			{
				if (_matcherCombatUnitID == null)
				{
					var matcher = (Entitas.Matcher<EkRequestEntity>)Entitas.Matcher<EkRequestEntity>.AllOf(EkRequestComponentsLookup.CombatUnitID);
					matcher.componentNames = EkRequestComponentsLookup.componentNames;
					_matcherCombatUnitID = matcher;
				}

				return _matcherCombatUnitID;
			}
		}
	}
}
