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
	public partial class EkTrackingEntity
	{
		static readonly Dirty dirtyComponent = new Dirty();

		public bool isDirty
		{
			get { return HasComponent(EkTrackingComponentsLookup.Dirty); }
			set
			{
				if (value != isDirty)
				{
					var index = EkTrackingComponentsLookup.Dirty;
					if (value)
					{
						var componentPool = GetComponentPool(index);
						var component = componentPool.Count > 0
								? componentPool.Pop()
								: dirtyComponent;

						AddComponent(index, component);
					}
					else
					{
						RemoveComponent(index);
					}
				}
			}
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
	public sealed partial class EkTrackingMatcher
	{
		static Entitas.IMatcher<EkTrackingEntity> _matcherDirty;

		public static Entitas.IMatcher<EkTrackingEntity> Dirty
		{
			get
			{
				if (_matcherDirty == null)
				{
					var matcher = (Entitas.Matcher<EkTrackingEntity>)Entitas.Matcher<EkTrackingEntity>.AllOf(EkTrackingComponentsLookup.Dirty);
					matcher.componentNames = EkTrackingComponentsLookup.componentNames;
					_matcherDirty = matcher;
				}

				return _matcherDirty;
			}
		}
	}
}