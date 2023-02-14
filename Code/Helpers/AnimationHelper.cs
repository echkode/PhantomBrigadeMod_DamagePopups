// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using System.Collections.Generic;

using PhantomBrigade;
using PBCIViewPopups = CIViewPopups;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	static class AnimationHelper
	{
		internal static (bool, Vector2) GetUIPosition(ECS.EkPopupEntity ekp)
		{
			var combatUnit = IDUtility.GetCombatEntity(ekp.combatUnitID.id);
			return CIViewCombatPopups.GetUIPosition(combatUnit);
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

		internal static int AddTextSegments(
			int popupID,
			List<PBCIViewPopups.PopupNestedSegment> segments,
			string text,
			int spriteIDNext,
			INGUIAtlas atlas)
		{
			var textStartOffset = CIViewCombatPopups.TextStartOffset;
			var pivot = new Vector2(0.5f, 0.5f);
			var color = new Color(1f, 1f, 1f, 1f);

			foreach (var key in text)
			{
				var center = 6;
				var offset = 12;
				var width = 24;
				var spriteName = "s_text_24_" + key;

				if (CIViewCombatPopups.TryGetCharacterSprite(key, out var characterSprite))
				{
					spriteName = characterSprite.sprite;
					center = characterSprite.center;
					offset = characterSprite.offset;
					width = characterSprite.width;
				}

				if (atlas.GetSprite(spriteName) == null)
				{
					Debug.LogWarningFormat(
						"Mod {0} ({1}) AnimationHelper.AddTextSegments can't find sprite in atlas | popup: {2} | name: {3}",
						ModLink.modIndex,
						ModLink.modId,
						popupID,
						spriteName);

					spriteIDNext += 1;
					continue;
				}

				var pos = new Vector2(textStartOffset + center, 0f);
				CIViewCombatPopups.AllocateSprite(
					spriteIDNext,
					spriteName,
					pos,
					width,
					CIViewCombatPopups.Constants.SpriteHeight,
					(Color32)color,
					pivot,
					enabled: false);
				segments.Add(new PBCIViewPopups.PopupNestedSegment()
				{
					sprite = spriteName,
					pivot = pivot,
					position = pos,
					size = new Vector2(width, CIViewCombatPopups.Constants.SpriteHeight),
				});

				spriteIDNext += 1;
				textStartOffset += offset;
			}

			return spriteIDNext;
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
