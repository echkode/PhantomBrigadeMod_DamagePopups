namespace EchKode.PBMods.DamagePopups.ECS
{
	//------------------------------------------------------------------------------
	// <auto-generated>
	//     This code was generated by Entitas.CodeGeneration.Plugins.ContextMatcherGenerator.
	//
	//     Changes to this file may cause incorrect behavior and will be lost if
	//     the code is regenerated.
	// </auto-generated>
	//------------------------------------------------------------------------------
	public sealed partial class EkRequestMatcher
	{
		public static Entitas.IAllOfMatcher<EkRequestEntity> AllOf(params int[] indices)
		{
			return Entitas.Matcher<EkRequestEntity>.AllOf(indices);
		}

		public static Entitas.IAllOfMatcher<EkRequestEntity> AllOf(params Entitas.IMatcher<EkRequestEntity>[] matchers)
		{
			return Entitas.Matcher<EkRequestEntity>.AllOf(matchers);
		}

		public static Entitas.IAnyOfMatcher<EkRequestEntity> AnyOf(params int[] indices)
		{
			return Entitas.Matcher<EkRequestEntity>.AnyOf(indices);
		}

		public static Entitas.IAnyOfMatcher<EkRequestEntity> AnyOf(params Entitas.IMatcher<EkRequestEntity>[] matchers)
		{
			return Entitas.Matcher<EkRequestEntity>.AnyOf(matchers);
		}
	}
}
