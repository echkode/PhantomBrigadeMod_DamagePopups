namespace EchKode.PBMods.DamagePopups.ECS
{
	//------------------------------------------------------------------------------
	// <auto-generated>
	//     This code was generated by Entitas.CodeGeneration.Plugins.ContextGenerator.
	//
	//     Changes to this file may cause incorrect behavior and will be lost if
	//     the code is regenerated.
	// </auto-generated>
	//------------------------------------------------------------------------------
	public sealed partial class EkReplayContext : Entitas.Context<EkReplayEntity>
	{
		public EkReplayContext()
			: base(
				EkReplayComponentsLookup.TotalComponents,
				0,
				new Entitas.ContextInfo(
					"EkReplay",
					EkReplayComponentsLookup.componentNames,
					EkReplayComponentsLookup.componentTypes
				),
				(entity) =>

#if (ENTITAS_FAST_AND_UNSAFE)
                new Entitas.UnsafeAERC(),
#else
					new Entitas.SafeAERC(entity),
#endif
				() => new EkReplayEntity()
			)
		{
		}
	}
}