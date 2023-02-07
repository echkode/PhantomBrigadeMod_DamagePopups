using PhantomBrigade;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	internal static class AnimationHelper
	{
		internal static (bool, Vector2) GetPosition(ECS.EkPopupEntity ekp)
		{
			var combatUnit = IDUtility.GetCombatEntity(ekp.combatUnitID.id);
			return CIViewCombatPopups.GetPosition(combatUnit);
		}

		internal static void HidePopup(ECS.EkPopupEntity ekp)
		{
			var spriteIDBase = ekp.displayText.spriteIDBase;
			var stopID = spriteIDBase + ekp.popup.segments.Count;
			for (var i = spriteIDBase; i < stopID; i += 1)
			{
				CIViewCombatPopups.HideSprite(i);
			}
		}

		internal static void DisposeSprites(ECS.EkPopupEntity ekp)
		{
			var disposal = ECS.Contexts.sharedInstance.ekPopup.CreateEntity();
			disposal.AddSpriteDisposal(ekp.popup.popupID, ekp.displayText.spriteIDBase, ekp.popup.segments.Count);
		}

		internal static void DisposeSprites(int spriteIDBase, int count)
		{
			var stopID = spriteIDBase + count;
			for (var i = spriteIDBase; i < stopID; i += 1)
			{
				CIViewCombatPopups.DisposeSprite(i);
			}
		}
	}
}
