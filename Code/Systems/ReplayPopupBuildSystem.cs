// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using System.Collections.Generic;

using Entitas;

using PBCIViewPopups = CIViewPopups;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	using static ECS.ContextsExtensions;

	using SegmentCtor = System.Func<string, Vector2, Vector2, Vector2, PBCIViewPopups.PopupNestedSegment>;

	sealed class ReplayPopupBuildSystem : ReactiveSystem<ECS.EkReplayEntity>, IInitializeSystem, ITearDownSystem
	{
		private static readonly Dictionary<string, ECS.EkReplayEntity> existingPopups = new Dictionary<string, ECS.EkReplayEntity>();
		private static readonly Dictionary<int, Stack<float[]>> interpolantPool = new Dictionary<int, Stack<float[]>>();
		private static bool logEnabled;

		private readonly ECS.EkReplayContext ekReplay;
		private static int turnLast = -1;
		private static int sampleIndexLast = -1;

		public ReplayPopupBuildSystem(ECS.Contexts contexts)
			: base(contexts.ekReplay)
		{
			ekReplay = contexts.ekReplay;
		}

		public void Initialize()
		{
			logEnabled = ModLink.Settings.IsLoggingEnabled(ModLink.ModSettings.LoggingFlag.ReplayBuild);
		}

		public void TearDown()
		{
			existingPopups.Clear();
			interpolantPool.Clear();
			foreach (var ekr in ekReplay.GetEntities())
			{
				if (!ekr.hasPopup)
				{
					continue;
				}
				ReplayHelper.DestroyPopup(ekr);
			}
		}

		protected override ICollector<ECS.EkReplayEntity> GetTrigger(IContext<ECS.EkReplayEntity> context) =>
			context.CreateCollector(ECS.EkReplayMatcher.Sample.Added());
		protected override bool Filter(ECS.EkReplayEntity entity) => true;

		protected override void Execute(List<ECS.EkReplayEntity> entities)
		{
			var turn = ekReplay.sample.turn;
			var sampleIndex = ekReplay.sample.index;
			if (turn == turnLast && sampleIndex == sampleIndexLast)
			{
				return;
			}
			turnLast = turn;
			sampleIndexLast = sampleIndex;

			foreach (var ekr in ekReplay.GetEntities())
			{
				if (ekr.hasDamageAccumulation || ekr.hasDamageSummary)
				{
					BuildPopup(ekr, sampleIndex);
				}
			}
		}

		void BuildPopup(ECS.EkReplayEntity ekr, int sampleIndex)
		{
			var combatUnitID = ekr.combatUnitID.id;
			FindPopups(combatUnitID);

			var (ok, positions) = ReplayHelper.GetTrackedPositions(ekr.combatUnitID.id);
			if (!ok)
			{
				if (existingPopups.TryGetValue(ekr.animationKey.s, out var p))
				{
					ReplayHelper.DestroyPopup(p);
				}
				return;
			}

			var slot = ekr.replaySlots.a[sampleIndex];
			if (slot == -1)
			{
				if (existingPopups.TryGetValue(ekr.animationKey.s, out var p))
				{
					ReplayHelper.DestroyPopup(p);
				}
				return;
			}

			var accumulatedValue = ekr.hasDamageAccumulation
				? ekr.damageAccumulation.a[sampleIndex]
				: 0f;
			var summaryValue = ekr.hasDamageSummary
				? ekr.damageSummary.a[sampleIndex]
				: 0f;

			if (accumulatedValue == 0f && summaryValue == 0f)
			{
				if (existingPopups.TryGetValue(ekr.animationKey.s, out var p))
				{
					ReplayHelper.DestroyPopup(p);
				}
				return;
			}

			var interpolant = ekr.hasDisplayInterpolation
				? ekr.displayInterpolation.a[sampleIndex]
				: 1f;

			if (existingPopups.TryGetValue(ekr.animationKey.s, out var popup))
			{
				UpdatePopup(
					popup,
					accumulatedValue,
					summaryValue,
					interpolant,
					sampleIndex);
				return;
			}

			CreatePopup(
				combatUnitID,
				ekr.animationKey.s,
				positions,
				ekr.replaySlots.a,
				accumulatedValue,
				summaryValue,
				interpolant,
				sampleIndex);
		}

		void FindPopups(int combatUnitID)
		{
			existingPopups.Clear();
			var entities = ekReplay.GetEntitiesWithCombatUnitID(combatUnitID);
			if (null == entities)
			{
				return;
			}
			foreach (var ekr in entities)
			{
				if (!ekr.hasPopup)
				{
					continue;
				}
				existingPopups.Add(ekr.animationKey.s, ekr);
			}
		}

		void CreatePopup(
			int combatUnitID,
			string animationKey,
			Vector3[] positions,
			int[] slots,
			float accumulatedValue,
			float summaryValue,
			float interpolant,
			int sampleIndex)
		{
			var ekr = ekReplay.CreateEntity();
			ekr.AddCombatUnitID(combatUnitID);
			ekr.AddAnimationKey(animationKey);
			ekr.AddPositionTracker(positions);
			ekr.AddReplaySlots(slots);
			ekr.AddReplayValue(accumulatedValue, summaryValue);
			var format = GetFormat(ekr.animationKey.s);
			AddContent(
				ekr,
				CIViewCombatPopups.SpriteIDNext,
				GetTextValue(accumulatedValue, format),
				GetTextValue(summaryValue, format),
				interpolant,
				sampleIndex);

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) ReplayPopupBuildSystem.BuildPopup | sample: {2} | combat unit: C-{3} | key: {4} | popup: {5} | text: {6} | sprite ID base: {7} | segment count: {8}",
					ModLink.modIndex,
					ModLink.modId,
					sampleIndex,
					combatUnitID,
					animationKey,
					ekr.popup.popupID,
					ekr.displayText.text,
					ekr.displayText.spriteIDBase,
					ekr.popup.segments.Count);
			}
		}

		void UpdatePopup(
			ECS.EkReplayEntity ekr,
			float accumulatedValue,
			float summaryValue,
			float interpolant,
			int sampleIndex)
		{
			var format = GetFormat(ekr.animationKey.s);
			if (ekr.replayValue.accumulated == accumulatedValue
				&& ekr.replayValue.summary == summaryValue)
			{
				UpdateInterpolation(ekr, format, interpolant);
				return;
			}

			ReplayHelper.DisposeSegments(ekr);
			ekr.ReplaceReplayValue(accumulatedValue, summaryValue);
			AddContent(
				ekr,
				ekr.popup.popupID,
				GetTextValue(accumulatedValue, format),
				GetTextValue(summaryValue, format),
				interpolant,
				sampleIndex);

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) ReplayPopupBuildSystem.UpdatePopup | sample: {2} | combat unit: C-{3} | key: {4} | popup: {5} | start index: {6} | text: {7} | sprite ID base: {8} | segment count: {9}",
					ModLink.modIndex,
					ModLink.modId,
					sampleIndex,
					ekr.combatUnitID.id,
					ekr.animationKey.s,
					ekr.popup.popupID,
					ekr.displayText.startTime,
					ekr.displayText.text,
					ekr.displayText.spriteIDBase,
					ekr.popup.segments.Count);
			}
		}

		static void AddContent(
			ECS.EkReplayEntity ekr,
			int popupID,
			string accumulatedText,
			string summaryText,
			float interpolant,
			int sampleIndex)
		{
			var segments = CIViewCombatPopups.GetPooledSegmentList();
			segments.Clear();
			ekr.ReplacePopup(popupID, segments);
			var hasAccumulated = accumulatedText != "";
			var text = hasAccumulated
				? summaryText != ""
					? accumulatedText + "/" + summaryText
					: accumulatedText
				: summaryText;
			var spriteIDStart = CIViewCombatPopups.SpriteIDNext;
			ekr.ReplaceDisplayText(text, spriteIDStart, sampleIndex);
			var textStartOffset = CIViewCombatPopups.TextStartOffset;
			var spriteIDNext = AddSymbol(ekr.animationKey.s, segments, spriteIDStart, !hasAccumulated);
			if (hasAccumulated)
			{
				(textStartOffset, spriteIDNext) = ReplayHelper.AddTextSegments(
					popupID,
					segments,
					accumulatedText,
					textStartOffset,
					spriteIDNext,
					CIViewCombatPopups.SpriteAtlas,
					ReplayHelper.CreateTextSegment);
			}
			if (summaryText != "")
			{
				var definition = CIViewCombatPopups.GetDefinition(ekr.animationKey.s);
				if (hasAccumulated)
				{
					summaryText = "/" + summaryText;
				}
				(textStartOffset, spriteIDNext) = ReplayHelper.AddTextSegments(
					popupID,
					segments,
					summaryText,
					textStartOffset,
					spriteIDNext,
					CIViewCombatPopups.SpriteAtlas,
					CreateFadeSegment(definition));
				AddInterpolation(
					ekr,
					accumulatedText,
					summaryText,
					interpolant);
			}
			CIViewCombatPopups.SpriteIDNext = spriteIDNext;
		}

		static int AddSymbol(
			string animationKey,
			List<PBCIViewPopups.PopupNestedSegment> segments,
			int spriteIDNext,
			bool fade)
		{
			var definition = CIViewCombatPopups.GetDefinition(animationKey);
			var sprite = definition.segments[0].sprite;
			var pos = definition.segments[0].position;
			var pivot = definition.segments[0].pivot;

			CIViewCombatPopups.AllocateSprite(
				spriteIDNext,
				sprite,
				pos,
				definition.segments[0].sizeTo.x,
				definition.segments[0].sizeTo.y,
				(Color32)definition.segments[0].colorTo,
				pivot,
				type: definition.segments[0].type,
				flip: definition.segments[0].flip,
				fillDirection: definition.segments[0].fillDirection,
				fillInvert: definition.segments[0].fillInvert,
				enabled: false);

			var ctor = fade
				? CreateFadeSegment(definition)
				: ReplayHelper.CreateTextSegment;
			segments.Add(ctor(
				sprite,
				pivot,
				pos,
				definition.segments[0].sizeTo));

			return spriteIDNext + 1;
		}

		static void AddInterpolation(
			ECS.EkReplayEntity ekr, 
			string accumulatedText,
			string summaryText,
			float interpolant)
		{
			var size = accumulatedText.Length + summaryText.Length + 1;
			if (ekr.hasDisplayInterpolation && ekr.displayInterpolation.a.Length != size)
			{
				var oldSize = ekr.displayInterpolation.a.Length;
				interpolantPool[oldSize].Push(ekr.displayInterpolation.a);
				ekr.RemoveDisplayInterpolation();
			}
			if (!ekr.hasDisplayInterpolation)
			{
				if (!interpolantPool.TryGetValue(size, out var pool))
				{
					pool = new Stack<float[]>();
					interpolantPool.Add(size, pool);
				}
				if (pool.Count == 0)
				{
					pool.Push(new float[size]);
				}
				ekr.AddDisplayInterpolation(pool.Pop());
			}

			var interp = ekr.displayInterpolation.a;
			interp[0] = accumulatedText == "" ? interpolant : 1f;
			var offset = 1;
			for (; offset <= accumulatedText.Length; offset += 1)
			{
				interp[offset] = 1f;
			}
			for (var i = 0; i < summaryText.Length; i += 1)
			{
				interp[offset + i] = interpolant;
			}
		}

		static void UpdateInterpolation(ECS.EkReplayEntity ekr, string format, float interpolant)
		{
			if (ekr.replayValue.summary == 0f)
			{
				return;
			}

			var accumulatedText = GetTextValue(ekr.replayValue.accumulated, format);
			var summaryText = GetTextValue(ekr.replayValue.summary, format);
			var offset = 0;
			var length = summaryText.Length + 1;
			if (accumulatedText != "")
			{
				offset = accumulatedText.Length + 1;
				length += offset;
			}
			var interp = ekr.displayInterpolation.a;
			for (var i = offset; i < length; i += 1)
			{
				interp[i] = interpolant;
			}
		}

		static SegmentCtor CreateFadeSegment(PBCIViewPopups.PopupDefinition definition) =>
			(sprite, pivot, pos, size) =>
				new PBCIViewPopups.PopupNestedSegment()
				{
					sprite = sprite,
					pivot = pivot,
					position = pos,
					size = size,
					colorAnimation = definition.colorAnimation,
					colorGradientUsed = definition.colorGradientUsed,
					colorGradient = definition.colorGradient,
					color = definition.color,
					colorTo = definition.colorTo,
				};

		string GetFormat(string animationKey)
		{
			foreach (var (akey, format) in ekReplay.summaryTextFormat.formats)
			{
				if (animationKey == akey)
				{
					return format;
				}
			}
			return "";
		}

		static string GetTextValue(float value, string format)
		{
			if (value == 0f)
			{
				return "";
			}
			if (value < 1f && format == "F0")
			{
				value = 1f;
			}
			return value.ToString(format);
		}
	}
}
