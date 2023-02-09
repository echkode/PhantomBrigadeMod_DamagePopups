using System.Collections.Generic;

using PBCIViewPopups = CIViewPopups;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	using static ECS.ContextsExtensions;

	using SegmentCtor = System.Func<string, Vector2, Vector2, Vector2, PBCIViewPopups.PopupNestedSegment>;

	static class ReplayHelper
	{
		private static readonly System.Comparison<(string, string)> textFormatComparison =
			new System.Comparison<(string, string)>(CompareTextFormat);

		internal static (int, int) GetSampleIndex(float now)
		{
			var combat = Contexts.sharedInstance.combat;
			var turn = combat.currentTurn.i - 1;
			var timeInTurn = turn >= 0
				? now - turn * combat.turnLength.i
				: now;
			timeInTurn *= ModLink.Settings.samplesPerSecond;
			return (turn, Mathf.FloorToInt(timeInTurn));
		}

		internal static int TurnLengthInSamples =>
			Contexts.sharedInstance.combat.turnLength.i * ModLink.Settings.samplesPerSecond;
		internal static bool HasSetting(ModLink.ModSettings.ReplayPopup flag) => flag == (ModLink.Settings.replayPopups & flag);
		internal static int SummarySize => TurnLengthInSamples + 1;

		internal static void AddSummaryFormats(string animationKey, string format)
		{
			if (!ECS.Contexts.sharedInstance.ekReplay.hasSummaryTextFormat)
			{
				ECS.Contexts.sharedInstance.ekReplay.SetSummaryTextFormat(new List<(string, string)>());
			}

			var summaryFormats = ECS.Contexts.sharedInstance.ekReplay.summaryTextFormat.formats;
			foreach (var (akey, _) in summaryFormats)
			{
				if (animationKey == akey)
				{
					return;
				}
			}

			summaryFormats.Add((animationKey, format));
			summaryFormats.Sort(textFormatComparison);
		}

		internal static int CompareAnimationKey(string x, string y)
		{
			if (x == y)
			{
				return 0;
			}

			switch (x)
			{
				case CombatTextAnimations.damageIntegrity:
					return -1;
				case CombatTextAnimations.damageBarrier:
					if (y == CombatTextAnimations.damageIntegrity)
					{
						return 1;
					}
					return -1;
				case CombatTextAnimations.damageConcussion:
					switch (y)
					{
						case CombatTextAnimations.damageIntegrity:
						case CombatTextAnimations.damageBarrier:
							return 1;
					}
					return -1;
				case CombatTextAnimations.damageHeat:
					if (y == CombatTextAnimations.damageStability)
					{
						return -1;
					}
					return 1;
				case CombatTextAnimations.damageStability:
					return 1;
			}

			return 0;
		}

		internal static void DestroyPopup(ECS.EkReplayEntity ekr)
		{
			if (ModLink.Settings.IsLoggingEnabled(ModLink.ModSettings.LoggingFlag.Build))
			{
				Debug.LogFormat(
					"Mod {0} ({1}) ReplayHelper.DestroyPopup | popupID: {2} | sprite ID base: {3} | segments: {4}",
					ModLink.modIndex,
					ModLink.modId,
					ekr.popup.popupID,
					ekr.displayText.spriteIDBase,
					ekr.popup.segments.Count);
			}

			DisposeSegments(ekr, log: false);
			ekr.Destroy();
		}

		internal static void DisposeSegments(ECS.EkReplayEntity ekr, bool log = true)
		{
			if (ModLink.Settings.IsLoggingEnabled(ModLink.ModSettings.LoggingFlag.Build) && log)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) ReplayHelper.DisposeSegments | popupID: {2} | sprite ID base: {3} | segments: {4}",
					ModLink.modIndex,
					ModLink.modId,
					ekr.popup.popupID,
					ekr.displayText.spriteIDBase,
					ekr.popup.segments.Count);
			}

			AnimationHelper.DisposeSprites(ekr.displayText.spriteIDBase, ekr.popup.segments.Count);
			CIViewCombatPopups.ReleasePooledSegmentList(ekr.popup.segments);
		}

		internal static (bool, Vector3[]) GetTrackedPositions(int combatUnitID)
		{
			var trackers = ECS.Contexts.sharedInstance.ekTracking.GetEntitiesWithCombatUnitID(combatUnitID);
			foreach (var tracker in trackers)
			{
				if (tracker.hasPositionTracker)
				{
					return (true, tracker.positionTracker.a);
				}
			}
			return (false, null);
		}

		internal static ECS.EkReplayEntity GetReplayTables(int combatUnitID, string animationKey)
		{
			var replayEntities = ECS.Contexts.sharedInstance.ekReplay.GetEntitiesWithCombatUnitID(combatUnitID);
			if (null != replayEntities)
			{
				foreach (var ekr in replayEntities)
				{
					if (animationKey != ekr.animationKey.s)
					{
						continue;
					}

					if (ekr.hasDamageAccumulation)
					{
						return ekr;
					}
					if (ekr.hasDamageSummary)
					{
						return ekr;
					}
				}
			}

			var tables = ECS.Contexts.sharedInstance.ekReplay.CreateEntity();
			tables.AddCombatUnitID(combatUnitID);
			tables.AddAnimationKey(animationKey);
			return tables;
		}

		internal static (int, int) AddTextSegments(
			int popupID,
			List<PBCIViewPopups.PopupNestedSegment> segments,
			string text,
			int textStartOffset,
			int spriteIDNext,
			INGUIAtlas atlas,
			SegmentCtor ctor)
		{
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
				segments.Add(ctor(
					spriteName,
					pivot,
					pos,
					new Vector2(width, CIViewCombatPopups.Constants.SpriteHeight)));

				spriteIDNext += 1;
				textStartOffset += offset;
			}

			return (textStartOffset, spriteIDNext);
		}

		internal static PBCIViewPopups.PopupNestedSegment CreateTextSegment(
			string spriteName,
			Vector2 pivot,
			Vector2 pos,
			Vector2 size) =>
				new PBCIViewPopups.PopupNestedSegment()
				{
					sprite = spriteName,
					pivot = pivot,
					position = pos,
					size = size,
				};

		static int CompareTextFormat((string AnimationKey, string) x, (string AnimationKey, string) y) =>
			CompareAnimationKey(x.AnimationKey, y.AnimationKey);
	}
}
