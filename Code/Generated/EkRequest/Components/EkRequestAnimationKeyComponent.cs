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
		public AnimationKey animationKey { get { return (AnimationKey)GetComponent(EkRequestComponentsLookup.AnimationKey); } }
		public bool hasAnimationKey { get { return HasComponent(EkRequestComponentsLookup.AnimationKey); } }

		public void AddAnimationKey(string newS)
		{
			var index = EkRequestComponentsLookup.AnimationKey;
			var component = (AnimationKey)CreateComponent(index, typeof(AnimationKey));
			component.s = newS;
			AddComponent(index, component);
		}

		public void ReplaceAnimationKey(string newS)
		{
			var index = EkRequestComponentsLookup.AnimationKey;
			var component = (AnimationKey)CreateComponent(index, typeof(AnimationKey));
			component.s = newS;
			ReplaceComponent(index, component);
		}

		public void RemoveAnimationKey()
		{
			RemoveComponent(EkRequestComponentsLookup.AnimationKey);
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
	public partial class EkRequestEntity : IAnimationKeyEntity { }

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
		static Entitas.IMatcher<EkRequestEntity> _matcherAnimationKey;

		public static Entitas.IMatcher<EkRequestEntity> AnimationKey
		{
			get
			{
				if (_matcherAnimationKey == null)
				{
					var matcher = (Entitas.Matcher<EkRequestEntity>)Entitas.Matcher<EkRequestEntity>.AllOf(EkRequestComponentsLookup.AnimationKey);
					matcher.componentNames = EkRequestComponentsLookup.componentNames;
					_matcherAnimationKey = matcher;
				}

				return _matcherAnimationKey;
			}
		}
	}
}