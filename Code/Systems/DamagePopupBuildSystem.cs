// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using System.Collections.Generic;

using Entitas;

using PhantomBrigade;
using PBCIViewPopups = CIViewPopups;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	using static ECS.ContextsExtensions;

	sealed class DamagePopupBuildSystem : IInitializeSystem, IExecuteSystem, ITearDownSystem
	{
		private static bool logEnabled;

		private static bool initialized;
		private static INGUIAtlas atlas;

		public void Initialize()
		{
			logEnabled = ModLink.Settings.IsLoggingEnabled(ModLink.ModSettings.LoggingFlag.Build);
			atlas = CIViewCombatPopups.SpriteAtlas;
			initialized = atlas != null;
		}

		public void Execute()
		{
			if (!initialized)
			{
				return;
			}
			if (!Contexts.sharedInstance.combat.Simulating)
			{
				return;
			}

			var now = Contexts.sharedInstance.combat.simulationTime.f;
			foreach (var tracking in ECS.Contexts.sharedInstance.ekTracking.GetEntities())
			{
				if (!tracking.hasDamageTracker)
				{
					continue;
				}
				if (!tracking.isDirty)
				{
					continue;
				}
				if (now - tracking.damageTracker.timeLast < CIViewCombatPopups.UpdateInterval)
				{
					continue;
				}

				var popupEntities = ECS.Contexts.sharedInstance.ekPopup.GetEntitiesWithCombatUnitID(tracking.combatUnitID.id);
				if (popupEntities != null)
				{
					if (TryRefreshPopup(popupEntities, tracking, now))
					{
						tracking.isDirty = false;
						continue;
					}
				}

				CreatePopup(tracking, popupEntities?.Count ?? 0, now);
				tracking.isDirty = false;
			}
		}

		public void TearDown()
		{
			foreach (var ekp in ECS.Contexts.sharedInstance.ekPopup.GetEntities())
			{
				if (ekp.hasSpriteDisposal)
				{
					continue;
				}

				if (ekp.hasPopup)
				{
					AnimationHelper.DisposeSprites(ekp.displayText.spriteIDBase, ekp.popup.segments.Count);
					CIViewCombatPopups.ReleasePooledSegmentList(ekp.popup.segments);
				}
				ekp.Destroy();
			}
		}

		static void CreatePopup(ECS.EkTrackingEntity tracking, int index, float now)
		{
			if (!CIViewCombatPopups.HasDefinition(tracking.animationKey.s))
			{
				return;
			}

			var definition = CIViewCombatPopups.GetDefinition(tracking.animationKey.s);
			if (definition.timeTotal <= 0f)
			{
				return;
			}

			var ekp = ECS.Contexts.sharedInstance.ekPopup.CreateEntity();
			ekp.AddCombatUnitID(tracking.combatUnitID.id);
			ekp.AddAnimationKey(tracking.animationKey.s);
			ekp.AddSlot(index);

			var combatUnit = IDUtility.GetCombatEntity(tracking.combatUnitID.id);
			var position = UnitHelper.GetPopupPosition(combatUnit);
			ekp.AddPosition(position);

			var spriteIDNext = CIViewCombatPopups.SpriteIDNext;
			var segments = CIViewCombatPopups.GetPooledSegmentList();
			segments.Clear();
			ekp.AddPopup(spriteIDNext, segments);
			ekp.AddDisplayText(tracking.damageTracker.text, spriteIDNext, now);
			spriteIDNext = AddDefinitionSegments(ekp, definition);
			spriteIDNext = AnimationHelper.AddTextSegments(
				ekp.popup.popupID,
				segments,
				tracking.damageTracker.text,
				spriteIDNext,
				atlas);
			CIViewCombatPopups.SpriteIDNext = spriteIDNext;

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) DamagePopupBuildSystem.CreatePopup | time: {2:F3} | popup: {3} | key: {4} | unit: C-{5} | unit position: {6} | text: {7} | segments: {8}",
					ModLink.modIndex,
					ModLink.modId,
					now,
					ekp.popup.popupID,
					tracking.animationKey.s,
					tracking.combatUnitID.id,
					ekp.position.v,
					tracking.damageTracker.text,
					ekp.popup.segments.Count);
			}
		}

		static bool TryRefreshPopup(
			HashSet<ECS.EkPopupEntity> popupEntities,
			ECS.EkTrackingEntity tracking,
			float now)
		{
			foreach (var ekp in popupEntities)
			{
				if (tracking.animationKey.s != ekp.animationKey.s)
				{
					continue;
				}

				if (ekp.hasSlideAnimation)
				{
					return true;
				}

				RefreshPopup(ekp, tracking, now);
				return true;
			}

			return false;
		}

		static void RefreshPopup(
			ECS.EkPopupEntity ekp,
			ECS.EkTrackingEntity tracking,
			float now)
		{
			RemoveAllSegments(ekp, now);

			var spriteIDNext = CIViewCombatPopups.SpriteIDNext;
			ekp.ReplaceDisplayText(
				tracking.damageTracker.text,
				spriteIDNext,
				now);

			var definition = CIViewCombatPopups.GetDefinition(tracking.animationKey.s);
			spriteIDNext = AddDefinitionSegments(ekp, definition);
			spriteIDNext = AnimationHelper.AddTextSegments(
				ekp.popup.popupID,
				ekp.popup.segments,
				tracking.damageTracker.text,
				spriteIDNext,
				atlas);
			CIViewCombatPopups.SpriteIDNext = spriteIDNext;

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) DamagePopupBuildSystem.RefreshPopup | time: {2:F3} | popup: {3} | text: {4} | spriteID base: {5} | segments: {6}",
					ModLink.modIndex,
					ModLink.modId,
					now,
					ekp.popup.popupID,
					tracking.damageTracker.text,
					ekp.displayText.spriteIDBase,
					ekp.popup.segments.Count);
			}
		}

		static int AddDefinitionSegments(
			ECS.EkPopupEntity ekp,
			PBCIViewPopups.PopupDefinition definition)
		{
			var spriteIDNext = ekp.displayText.spriteIDBase;
			for (var i = 0; i < definition.segments.Count; i += 1)
			{
				var segment = definition.segments[i];
				if (atlas.GetSprite(segment.sprite) == null)
				{
					Debug.LogWarningFormat(
						"Mod {0} ({1}) DamagePopupBuildSystem.AddDefinitionSegments can't find sprite in atlas | key: {2} | popup: {3} | name: {4}",
						ModLink.modIndex,
						ModLink.modId,
						definition.key,
						ekp.popup.popupID,
						segment.sprite);

					spriteIDNext += 1;
					continue;
				}
				CIViewCombatPopups.AllocateSprite(
					spriteIDNext,
					segment.sprite,
					Vector2.zero,
					segment.size.x,
					segment.size.y,
					(Color32)segment.color,
					segment.pivot,
					type: segment.type,
					flip: segment.flip,
					fillDirection: segment.fillDirection,
					fillInvert: segment.fillInvert,
					enabled: false);
				ekp.popup.segments.Add(segment);
				spriteIDNext += 1;
			}

			return spriteIDNext;
		}

		static void RemoveAllSegments(ECS.EkPopupEntity ekp, double now)
		{
			AnimationHelper.DisposeSprites(ekp);
			var count = ekp.popup.segments.Count;
			ekp.popup.segments.Clear();

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) DamagePopupBuildSystem.RemoveAllSegments | time: {2} | popup: {3} | text sprite range: {4}-{5}",
					ModLink.modIndex,
					ModLink.modId,
					now,
					ekp.popup.popupID,
					ekp.displayText.spriteIDBase,
					ekp.displayText.spriteIDBase + count - 1);
			}
		}
	}
}
