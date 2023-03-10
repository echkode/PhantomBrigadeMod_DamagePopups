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
	public partial class EkPopupEntity
	{
		public SlidePosition slidePosition { get { return (SlidePosition)GetComponent(EkPopupComponentsLookup.SlidePosition); } }
		public bool hasSlidePosition { get { return HasComponent(EkPopupComponentsLookup.SlidePosition); } }

		public void AddSlidePosition(UnityEngine.Vector2 newV)
		{
			var index = EkPopupComponentsLookup.SlidePosition;
			var component = (SlidePosition)CreateComponent(index, typeof(SlidePosition));
			component.v = newV;
			AddComponent(index, component);
		}

		public void ReplaceSlidePosition(UnityEngine.Vector2 newV)
		{
			var index = EkPopupComponentsLookup.SlidePosition;
			var component = (SlidePosition)CreateComponent(index, typeof(SlidePosition));
			component.v = newV;
			ReplaceComponent(index, component);
		}

		public void RemoveSlidePosition()
		{
			RemoveComponent(EkPopupComponentsLookup.SlidePosition);
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
	public sealed partial class EkPopupMatcher
	{
		static Entitas.IMatcher<EkPopupEntity> _matcherSlidePosition;

		public static Entitas.IMatcher<EkPopupEntity> SlidePosition
		{
			get
			{
				if (_matcherSlidePosition == null)
				{
					var matcher = (Entitas.Matcher<EkPopupEntity>)Entitas.Matcher<EkPopupEntity>.AllOf(EkPopupComponentsLookup.SlidePosition);
					matcher.componentNames = EkPopupComponentsLookup.componentNames;
					_matcherSlidePosition = matcher;
				}

				return _matcherSlidePosition;
			}
		}
	}
}
