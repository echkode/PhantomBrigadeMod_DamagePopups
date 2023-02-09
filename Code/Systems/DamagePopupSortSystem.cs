using System.Collections.Generic;

using Entitas;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	sealed class DamagePopupSortSystem : IExecuteSystem, ITearDownSystem
	{
		private static readonly SortedDictionary<int, List<ECS.EkPopupEntity>> popupsLookup = new SortedDictionary<int, List<ECS.EkPopupEntity>>();
		private static readonly System.Comparison<ECS.EkPopupEntity> recencyComparison = new System.Comparison<ECS.EkPopupEntity>(ComparePopupsByRecency);

		public void Execute()
		{
			if (!Contexts.sharedInstance.combat.Simulating)
			{
				return;
			}

			foreach (var popups in popupsLookup.Values)
			{
				popups.Clear();
			}

			foreach (var ekp in ECS.Contexts.sharedInstance.ekPopup.GetEntities())
			{
				if (!ekp.hasPopup)
				{
					continue;
				}

				if (!popupsLookup.TryGetValue(ekp.combatUnitID.id, out var popups))
				{
					popups = new List<ECS.EkPopupEntity>();
					popupsLookup.Add(ekp.combatUnitID.id, popups);
				}
				popups.Add(ekp);
			}

			foreach (var popups in popupsLookup.Values)
			{
				SortPopups(popups);
			}
		}

		public void TearDown()
		{
			foreach (var popups in popupsLookup.Values)
			{
				popups.Clear();
			}
			popupsLookup.Clear();
		}

		static void SortPopups(List<ECS.EkPopupEntity> popups)
		{
			popups.Sort(recencyComparison);

			var now = Contexts.sharedInstance.combat.simulationTime.f;
			for (var i = 0; i < popups.Count; i += 1)
			{
				var ekp = popups[i];
				if (ekp.hasSlideAnimation)
				{
					return;
				}
				if (ekp.slot.i == i)
				{
					continue;
				}

				ekp.AddSlideAnimation(
					now,
					Vector2.up * (i * CIViewCombatPopups.Constants.SlotHeight),
					i);
			}
		}

		static int ComparePopupsByRecency(ECS.EkPopupEntity x, ECS.EkPopupEntity y)
		{
			if (Mathf.Abs(x.displayText.startTime - y.displayText.startTime) < CIViewCombatPopups.Constants.SlideThreshold)
			{
				return x.slot.i.CompareTo(y.slot.i);
			}
			return -x.displayText.startTime.CompareTo(y.displayText.startTime);
		}
	}
}
