// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using System.Collections.Generic;

using HarmonyLib;

using PhantomBrigade;
using PBCIViewPopups = CIViewPopups;
using PBCIViewCombatPopups = CIViewCombatPopups;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	using AllocateDelegate = System.Func<List<PBCIViewPopups.PopupNestedSegment>>;
	using ReleaseDelegate = System.Action<List<PBCIViewPopups.PopupNestedSegment>>;
	using AnimateDelegate = System.Action<PBCIViewPopups.PopupNestedSegment, int, float, Vector2, float, Vector2, Color>;

	static partial class CIViewCombatPopups
	{
		internal static class Constants
		{
			internal const float SpriteHeight = 24f;
			internal const float SlotHeight = SpriteHeight + 4f;
			internal const float SlideAnimationTime = 0.150f;
			internal const float SlideThreshold = 0.5f;
			internal const float SamplingBase = 128f;
		}

		private static bool initialized;
		private static Traverse instTraverse;
		private static AllocateDelegate getPooledSegmentList;
		private static ReleaseDelegate releasePooledSegmentList;
		private static AnimateDelegate animateSegment;
		private static Dictionary<string, PBCIViewPopups.PopupDefinition> textAnimationsLookup;
		private static Dictionary<char, PBCIViewPopups.CharacterSprite> characterSpriteLookup;
		private static Camera worldCamera;
		private static Camera uiCamera;
		private static Transform transform;

		internal static bool HasDefinition(string key) => textAnimationsLookup.ContainsKey(key);
		internal static PBCIViewPopups.PopupDefinition GetDefinition(string key) => textAnimationsLookup[key];

		internal static List<PBCIViewPopups.PopupNestedSegment> GetPooledSegmentList() => getPooledSegmentList();
		internal static void ReleasePooledSegmentList(List<PBCIViewPopups.PopupNestedSegment> segments) =>
			releasePooledSegmentList(segments);

		internal static float UpdateInterval => PBCIViewCombatPopups.ins.intervalMinText;
		internal static float PlaybackSpeed => PBCIViewCombatPopups.ins.playbackSpeed;
		internal static int TextStartOffset => PBCIViewCombatPopups.ins.textStartOffset;

		internal static INGUIAtlas SpriteAtlas => PBCIViewCombatPopups.ins.spriteCollection.atlas;
		internal static int SpriteIDNext
		{
			get => instTraverse.Field<int>("spriteIDNext").Value;
			set => instTraverse.Field<int>("spriteIDNext").Value = value;
		}

		internal static bool TryGetCharacterSprite(char key, out PBCIViewPopups.CharacterSprite characterSprite) =>
			characterSpriteLookup.TryGetValue(key, out characterSprite);

		internal static void AllocateSprite(
			int spriteID,
			string spriteName,
			Vector2 pos,
			float width,
			float height,
			Color32 color,
			Vector2 pivot,
			float rot = 0.0f,
			UIBasicSprite.Type type = UIBasicSprite.Type.Simple,
			UIBasicSprite.Flip flip = UIBasicSprite.Flip.Nothing,
			float fillAmount = 1f,
			UIBasicSprite.FillDirection fillDirection = UIBasicSprite.FillDirection.Horizontal,
			bool fillInvert = false,
			bool enabled = true) =>
				PBCIViewCombatPopups.ins.spriteCollection.AddSprite(
					spriteID,
					spriteName,
					pos,
					width,
					height,
					color,
					pivot,
					rot,
					type,
					flip,
					fillAmount,
					fillDirection,
					fillInvert,
					enabled);
		internal static void DisposeSprite(int spriteID) => PBCIViewCombatPopups.ins.spriteCollection.RemoveSprite(spriteID);
		internal static void HideSprite(int spriteID) => PBCIViewCombatPopups.ins.spriteCollection.SetActive(spriteID, false);
		internal static void SetSpritePosition(int spriteID, Vector2 position) =>
			PBCIViewCombatPopups.ins.spriteCollection.SetPosition(spriteID, position);

		internal static HashSet<string> AnimationKeys = new HashSet<string>(PBCIViewCombatPopups.damageTypeKeys);

		internal static (bool, Vector2) GetUIPosition(CombatEntity combatUnit)
		{
			var position = UnitHelper.GetPopupPosition(combatUnit);
			return GetUIPosition(position);
		}

		internal static (bool, Vector2) GetUIPosition(Vector3 position)
		{
			var direction = Utilities.GetDirection(worldCamera.transform.position, position);
			var worldPoint = uiCamera.ViewportToWorldPoint(worldCamera.WorldToViewportPoint(position));
			var localPosition = transform.InverseTransformPoint(worldPoint);
			var flatPosition = new Vector2(localPosition.x, localPosition.y);
			if (Vector3.Dot(worldCamera.transform.forward, direction) <= 0.1f)
			{
				return (false, flatPosition);
			}
			return (true, flatPosition);
		}

		internal static void AnimateSegment(
			PBCIViewPopups.PopupNestedSegment segment,
			int spriteIDLocal,
			float interpolantShared,
			Vector2 positionBase,
			float rotationBase,
			Vector2 sizeBase,
			Color colorBase) =>
				animateSegment(
					segment,
					spriteIDLocal,
					interpolantShared,
					positionBase,
					rotationBase,
					sizeBase,
					colorBase);

		internal static void Initialize()
		{
			InitializeInstance(PBCIViewCombatPopups.ins);
		}

		static bool InitializeInstance(PBCIViewCombatPopups inst)
		{
			if (initialized)
			{
				return true;
			}

			if (!IDUtility.IsGameState(GameStates.combat))
			{
				return false;
			}
			if (inst == null)
			{
				return false;
			}

			var mi = AccessTools.DeclaredMethod(typeof(PBCIViewPopups), "ReleasePooledSegmentList");
			if (mi == null)
			{
				Debug.LogWarningFormat(
					"Mod {0} ({1}) Unable to initialize CIViewCombatPopups | reflection failed to find method ReleasePooledSegmentList",
					ModLink.modIndex,
					ModLink.modId);
				return false;
			}
			releasePooledSegmentList = (ReleaseDelegate)mi.CreateDelegate(typeof(ReleaseDelegate), inst);

			mi = AccessTools.DeclaredMethod(typeof(PBCIViewPopups), "GetPooledSegmentList");
			if (mi == null)
			{
				Debug.LogWarningFormat(
					"Mod {0} ({1}) Unable to initialize CIViewCombatPopups | reflection failed to find method GetPooledSegmentList",
					ModLink.modIndex,
					ModLink.modId);
				return false;
			}
			getPooledSegmentList = (AllocateDelegate)mi.CreateDelegate(typeof(AllocateDelegate), inst);

			mi = AccessTools.DeclaredMethod(typeof(PBCIViewPopups), "AnimateSegment");
			if (mi == null)
			{
				Debug.LogWarningFormat(
					"Mod {0} ({1}) Unable to initialize CIViewCombatPopups | reflection failed to find method AnimateSegment",
					ModLink.modIndex,
					ModLink.modId);
				return false;
			}
			animateSegment = (AnimateDelegate)mi.CreateDelegate(typeof(AnimateDelegate), inst);

			instTraverse = Traverse.Create(inst);
			textAnimationsLookup = instTraverse.Field<Dictionary<string, PBCIViewPopups.PopupDefinition>>("textAnimationsLookup").Value;
			characterSpriteLookup = instTraverse.Field<Dictionary<char, PBCIViewPopups.CharacterSprite>>("characterSpriteLookup").Value;
			worldCamera = instTraverse.Field<Camera>("worldCamera").Value;
			uiCamera = instTraverse.Field<Camera>("uiCamera").Value;
			transform = inst.transform;

			foreach (var kvp in textAnimationsLookup)
			{
				if (!PBCIViewCombatPopups.damageTypeKeys.Contains(kvp.Key))
				{
					continue;
				}
				kvp.Value.segments[0].size = new Vector2(Constants.SpriteHeight, Constants.SpriteHeight);
				if (kvp.Value.timeTotal < ModLink.Settings.popupDisplayTime)
				{
					continue;
				}
				kvp.Value.timeTotal = ModLink.Settings.popupDisplayTime;
			}

			PBCIViewCombatPopups.ins.intervalMinText = ModLink.Settings.textUpdateDelay / Constants.SamplingBase;

			initialized = true;

			if (ModLink.Settings.logging != ModLink.ModSettings.LoggingFlag.None)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) CIViewCombatPopups.InitializeInstance -- patched in instance fields/methods with reflection\n  intervalTextMin: {2}",
					ModLink.modIndex,
					ModLink.modId,
					PBCIViewCombatPopups.ins.intervalMinText);
			}

			return true;
		}
	}
}
