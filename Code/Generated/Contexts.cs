namespace EchKode.PBMods.DamagePopups.ECS
{
	//------------------------------------------------------------------------------
	// <auto-generated>
	//     This code was generated by Entitas.CodeGeneration.Plugins.ContextsGenerator.
	//
	//     Changes to this file may cause incorrect behavior and will be lost if
	//     the code is regenerated.
	// </auto-generated>
	//------------------------------------------------------------------------------
	public partial class Contexts : Entitas.IContexts
	{
		public static Contexts sharedInstance
		{
			get
			{
				if (_sharedInstance == null)
				{
					_sharedInstance = new Contexts();
				}

				return _sharedInstance;
			}
			set { _sharedInstance = value; }
		}

		static Contexts _sharedInstance;

		public EkPopupContext ekPopup { get; set; }
		public EkReplayContext ekReplay { get; set; }
		public EkRequestContext ekRequest { get; set; }
		public EkTrackingContext ekTracking { get; set; }

		public Entitas.IContext[] allContexts { get { return new Entitas.IContext[] { ekPopup, ekReplay, ekRequest, ekTracking }; } }

		public Contexts()
		{
			ekPopup = new EkPopupContext();
			ekReplay = new EkReplayContext();
			ekRequest = new EkRequestContext();
			ekTracking = new EkTrackingContext();

			var postConstructors = System.Linq.Enumerable.Where(
				GetType().GetMethods(),
				method => System.Attribute.IsDefined(method, typeof(Entitas.CodeGeneration.Attributes.PostConstructorAttribute))
			);

			foreach (var postConstructor in postConstructors)
			{
				postConstructor.Invoke(this, null);
			}
		}

		public void Reset()
		{
			var contexts = allContexts;
			for (int i = 0; i < contexts.Length; i++)
			{
				contexts[i].Reset();
			}
		}
	}

	//------------------------------------------------------------------------------
	// <auto-generated>
	//     This code was generated by Entitas.CodeGeneration.Plugins.EntityIndexGenerator.
	//
	//     Changes to this file may cause incorrect behavior and will be lost if
	//     the code is regenerated.
	// </auto-generated>
	//------------------------------------------------------------------------------
	public partial class Contexts
	{
		public const string CombatUnitID = "CombatUnitID";

		[Entitas.CodeGeneration.Attributes.PostConstructor]
		public void InitializeEntityIndices()
		{
			ekPopup.AddEntityIndex(new Entitas.EntityIndex<EkPopupEntity, int>(
				CombatUnitID,
				ekPopup.GetGroup(EkPopupMatcher.CombatUnitID),
				(e, c) => ((CombatUnitID)c).id));
			ekRequest.AddEntityIndex(new Entitas.EntityIndex<EkRequestEntity, int>(
				CombatUnitID,
				ekRequest.GetGroup(EkRequestMatcher.CombatUnitID),
				(e, c) => ((CombatUnitID)c).id));
			ekTracking.AddEntityIndex(new Entitas.EntityIndex<EkTrackingEntity, int>(
				CombatUnitID,
				ekTracking.GetGroup(EkTrackingMatcher.CombatUnitID),
				(e, c) => ((CombatUnitID)c).id));
			ekReplay.AddEntityIndex(new Entitas.EntityIndex<EkReplayEntity, int>(
				CombatUnitID,
				ekReplay.GetGroup(EkReplayMatcher.CombatUnitID),
				(e, c) => ((CombatUnitID)c).id));
		}
	}

	public static class ContextsExtensions
	{
		public static System.Collections.Generic.HashSet<EkPopupEntity> GetEntitiesWithCombatUnitID(this EkPopupContext context, int id)
		{
			return ((Entitas.EntityIndex<EkPopupEntity, int>)context.GetEntityIndex(Contexts.CombatUnitID)).GetEntities(id);
		}

		public static System.Collections.Generic.HashSet<EkRequestEntity> GetEntitiesWithCombatUnitID(this EkRequestContext context, int id)
		{
			return ((Entitas.EntityIndex<EkRequestEntity, int>)context.GetEntityIndex(Contexts.CombatUnitID)).GetEntities(id);
		}

		public static System.Collections.Generic.HashSet<EkTrackingEntity> GetEntitiesWithCombatUnitID(this EkTrackingContext context, int id)
		{
			return ((Entitas.EntityIndex<EkTrackingEntity, int>)context.GetEntityIndex(Contexts.CombatUnitID)).GetEntities(id);
		}

		public static System.Collections.Generic.HashSet<EkReplayEntity> GetEntitiesWithCombatUnitID(this EkReplayContext context, int id)
		{
			return ((Entitas.EntityIndex<EkReplayEntity, int>)context.GetEntityIndex(Contexts.CombatUnitID)).GetEntities(id);
		}
	}
}
