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
		public SpriteDisposal spriteDisposal { get { return (SpriteDisposal)GetComponent(EkPopupComponentsLookup.SpriteDisposal); } }
		public bool hasSpriteDisposal { get { return HasComponent(EkPopupComponentsLookup.SpriteDisposal); } }

		public void AddSpriteDisposal(int newPopupID, int newSpriteIDBase, int newCount)
		{
			var index = EkPopupComponentsLookup.SpriteDisposal;
			var component = (SpriteDisposal)CreateComponent(index, typeof(SpriteDisposal));
			component.popupID = newPopupID;
			component.spriteIDBase = newSpriteIDBase;
			component.count = newCount;
			AddComponent(index, component);
		}

		public void ReplaceSpriteDisposal(int newPopupID, int newSpriteIDBase, int newCount)
		{
			var index = EkPopupComponentsLookup.SpriteDisposal;
			var component = (SpriteDisposal)CreateComponent(index, typeof(SpriteDisposal));
			component.popupID = newPopupID;
			component.spriteIDBase = newSpriteIDBase;
			component.count = newCount;
			ReplaceComponent(index, component);
		}

		public void RemoveSpriteDisposal()
		{
			RemoveComponent(EkPopupComponentsLookup.SpriteDisposal);
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
		static Entitas.IMatcher<EkPopupEntity> _matcherSpriteDisposal;

		public static Entitas.IMatcher<EkPopupEntity> SpriteDisposal
		{
			get
			{
				if (_matcherSpriteDisposal == null)
				{
					var matcher = (Entitas.Matcher<EkPopupEntity>)Entitas.Matcher<EkPopupEntity>.AllOf(EkPopupComponentsLookup.SpriteDisposal);
					matcher.componentNames = EkPopupComponentsLookup.componentNames;
					_matcherSpriteDisposal = matcher;
				}

				return _matcherSpriteDisposal;
			}
		}
	}
}
