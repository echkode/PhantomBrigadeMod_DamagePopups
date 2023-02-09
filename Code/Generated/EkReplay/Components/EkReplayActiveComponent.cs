namespace EchKode.PBMods.DamagePopups.ECS
{
	//------------------------------------------------------------------------------
	// <auto-generated>
	//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentContextApiGenerator.
	//
	//     Changes to this file may cause incorrect behavior and will be lost if
	//     the code is regenerated.
	// </auto-generated>
	//------------------------------------------------------------------------------
	public partial class EkReplayContext
	{
		public EkReplayEntity activeEntity { get { return GetGroup(EkReplayMatcher.Active).GetSingleEntity(); } }

		public bool isActive
		{
			get { return activeEntity != null; }
			set
			{
				var entity = activeEntity;
				if (value != (entity != null))
				{
					if (value)
					{
						CreateEntity().isActive = true;
					}
					else
					{
						entity.Destroy();
					}
				}
			}
		}
	}

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
		static readonly Active activeComponent = new Active();

		public bool isActive
		{
			get { return HasComponent(EkReplayComponentsLookup.Active); }
			set
			{
				if (value != isActive)
				{
					var index = EkReplayComponentsLookup.Active;
					if (value)
					{
						var componentPool = GetComponentPool(index);
						var component = componentPool.Count > 0
								? componentPool.Pop()
								: activeComponent;

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
	public sealed partial class EkReplayMatcher
	{
		static Entitas.IMatcher<EkReplayEntity> _matcherActive;

		public static Entitas.IMatcher<EkReplayEntity> Active
		{
			get
			{
				if (_matcherActive == null)
				{
					var matcher = (Entitas.Matcher<EkReplayEntity>)Entitas.Matcher<EkReplayEntity>.AllOf(EkReplayComponentsLookup.Active);
					matcher.componentNames = EkReplayComponentsLookup.componentNames;
					_matcherActive = matcher;
				}

				return _matcherActive;
			}
		}
	}
}
