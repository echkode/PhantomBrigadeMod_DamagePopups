// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using System.Collections.Generic;
using System.Linq;
using System.Text;

using PhantomBrigade;

using QFSW.QC;

namespace EchKode.PBMods.DamagePopups.Diagnostics
{
	using static ECS.ContextsExtensions;

	static partial class ReplayTables
	{
		private sealed class UnitInfo
		{
			public int PersistentId;
			public int CombatId;
			public string Name;
			public string Faction;
			public string Flags;
			public float Level;
			public float Rating;
			public string Preset;
		}

		private static bool CombatStateCheck()
		{
			if (IDUtility.IsGameState(GameStates.combat))
			{
				return true;
			}

			QuantumConsole.Instance.LogToConsole("Command only available from combat");
			return false;
		}

		static (bool, int) UnitCheck(string unitID)
		{
			if (string.IsNullOrEmpty(unitID))
			{
				QuantumConsole.Instance.LogToConsole("Command requires a unit ID argument");
				return (false, IDUtility.invalidID);
			}

			unitID = unitID.ToUpperInvariant();
			if (!unitID.StartsWith("P-") && !unitID.StartsWith("C-"))
			{
				QuantumConsole.Instance.LogToConsole("Unit ID should begin with P- for a persistent identifier or C- for a combat identifier");
				return (false, IDUtility.invalidID);
			}

			var prefix = unitID.Substring(0, 2);
			if (!int.TryParse(unitID.Substring(2).TrimEnd(), out var id))
			{
				QuantumConsole.Instance.LogToConsole($"Invalid unit ID: the part after {prefix} should be an integer");
				return (false, IDUtility.invalidID);
			}

			System.Func<int, (bool, PersistentEntity)> find = FindPersistentEntity;
			if (prefix == "C-")
			{
				find = FindPersistentEntityFromCombatId;
			}
			var (ok, unit) = find(id);
			if (!ok)
			{
				QuantumConsole.Instance.LogToConsole("No unit in combat has identifier: " + unitID);
				return (false, IDUtility.invalidID);
			}
			var combatUnit = IDUtility.GetLinkedCombatEntity(unit);
			if (null == combatUnit)
			{
				QuantumConsole.Instance.LogToConsole("No unit in combat has identifier: " + unitID);
				return (false, IDUtility.invalidID);
			}

			return (ok, combatUnit.id.id);
		}

		private static string CompileUnitFlags(PersistentEntity unit, CombatEntity combatant)
		{
			var pilot = IDUtility.GetLinkedPilot(unit);
			var sb = new StringBuilder()
				.Append(combatant?.isPlayerControllable ?? false ? 'P' : '-')
				.Append(combatant?.isAIControllable ?? false ? 'A' : '-')
				.Append(unit.isHidden ? 'h' : '-')
				.Append(unit.isUnitDeployed ? 'd' : '-')
				.Append(combatant?.hasLandingData ?? false ? 'l' : '-')
				.Append(unit.isCombatParticipant ? 'p' : '-')
				.Append(combatant?.isCrashing ?? false ? 'C' : '-')
				.Append(unit.isDestroyed ? 'D' : '-')
				.Append(unit.isWrecked ? 'W' : '-')
				.Append(pilot?.isKnockedOut ?? false
					? 'U'
					: pilot?.isDeceased ?? false
						? 'X'
						: pilot == null || pilot.isEjected
							? 'E'
							: '-');
			return sb.ToString();
		}

		static (bool, PersistentEntity) FindPersistentEntity(int id)
		{
			var entity = IDUtility.GetPersistentEntity(id);
			return (entity != null, entity);
		}

		static (bool, PersistentEntity) FindPersistentEntityFromCombatId(int id)
		{
			var ce = IDUtility.GetCombatEntity(id);
			var entity = IDUtility.GetLinkedPersistentEntity(ce);
			return (entity != null, entity);
		}

		static void PrintAccumulation(int combatUnitID)
		{
			var entities = ECS.Contexts.sharedInstance.ekReplay.GetEntitiesWithCombatUnitID(combatUnitID);
			if (entities != null)
			{
				var collected = new List<(string, float[])>();
				foreach (var ekr in entities)
				{
					if (ekr.hasDamageAccumulation)
					{
						collected.Add((ekr.animationKey.s, ekr.damageAccumulation.a));
					}
				}

				if (collected.Count != 0)
				{
					PrintValues(collected);
					return;
				}
			}
			QuantumConsole.Instance.LogToConsole("No accumulation table for C-" + combatUnitID);
		}

		static void PrintSummary(int combatUnitID)
		{
			var entities = ECS.Contexts.sharedInstance.ekReplay.GetEntitiesWithCombatUnitID(combatUnitID);
			if (entities != null)
			{
				var collected = new List<(string, float[])>();
				foreach (var ekr in entities)
				{
					if (ekr.hasDamageSummary)
					{
						collected.Add((ekr.animationKey.s, ekr.damageSummary.a));
					}
				}

				if (collected.Count != 0)
				{
					PrintValues(collected);
					return;
				}
			}
			QuantumConsole.Instance.LogToConsole("No summary table for C-" + combatUnitID);
		}

		static void PrintSlots(int combatUnitID)
		{
			var entities = ECS.Contexts.sharedInstance.ekReplay.GetEntitiesWithCombatUnitID(combatUnitID);
			if (entities != null)
			{
				var collected = new List<(string, int[])>();
				foreach (var ekr in entities)
				{
					if (ekr.hasReplaySlots)
					{
						collected.Add((ekr.animationKey.s, ekr.replaySlots.a));
					}
				}

				if (collected.Count != 0)
				{
					PrintValues(collected);
					return;
				}
			}
			QuantumConsole.Instance.LogToConsole("No slot table for C-" + combatUnitID);
		}

		static void PrintPositions(int combatUnitID)
		{
			var entities = ECS.Contexts.sharedInstance.ekTracking.GetEntitiesWithCombatUnitID(combatUnitID);
			if (entities != null)
			{
				foreach (var tracking in entities)
				{
					if (tracking.hasPositionTracker)
					{
						var positions = tracking.positionTracker.a;
						var sb = new StringBuilder();
						for (var i = 0; i < positions.Length; i += 1)
						{
							sb.AppendFormat("{0}", i)
								.AppendFormat(",{0:F1}", positions[i])
								.AppendLine();
						}
						QuantumConsole.Instance.LogAllToConsole(sb.ToString());
						return;
					}
				}
			}

			QuantumConsole.Instance.LogToConsole("No position table for C-" + combatUnitID);
		}

		static void PrintHistory(int combatUnitID)
		{
			var entities = ECS.Contexts.sharedInstance.ekTracking.GetEntitiesWithCombatUnitID(combatUnitID);
			if (entities != null)
			{
				foreach (var tracking in entities)
				{
					if (tracking.hasDamageHistory)
					{
						var samples = tracking.damageHistory.samples;
						var sb = new StringBuilder(tracking.animationKey.s);
						foreach (var sample in samples)
						{
							sb.AppendLine()
								.AppendFormat("  {0}", sample.Index)
								.AppendFormat(",{0:F1}", sample.Accumulated)
								.AppendFormat(",{0:F1}", sample.Value);
						}
						QuantumConsole.Instance.LogAllToConsole(sb.ToString());
					}
				}
				return;
			}

			QuantumConsole.Instance.LogToConsole("No history table for C-" + combatUnitID);
		}

		static void PrintInterpolation(int combatUnitID)
		{
			var entities = ECS.Contexts.sharedInstance.ekReplay.GetEntitiesWithCombatUnitID(combatUnitID);
			if (entities != null)
			{
				var collected = new List<(string, float[])>();
				foreach (var ekr in entities)
				{
					if (ekr.hasDisplayInterpolation)
					{
						collected.Add((ekr.animationKey.s, ekr.displayInterpolation.a));
					}
				}

				if (collected.Count != 0)
				{
					PrintValues(collected);
					return;
				}
			}
			QuantumConsole.Instance.LogToConsole("No interpolation table for C-" + combatUnitID);
		}

		static void PrintValues(List<(string, float[])> collected)
		{
			PrintValues(collected, "F1");
		}

		static void PrintValues(List<(string, int[])> collected)
		{
			PrintValues(collected, "G");
		}

		static void PrintValues<T>(List<(string AnimationKey, T[])> collected, string valueFormat)
		{
			var comparison = new System.Comparison<(string, T[])>(CompareCollectedValues);
			collected.Sort(comparison);

			QuantumConsole.Instance.LogToConsole("index," + string.Join(",", collected.Select(x => x.AnimationKey)));
			var sb = new StringBuilder();
			for (var i = 0; i < ReplayHelper.SummarySize; i += 1)
			{
				sb.AppendFormat("{0}", i);
				foreach (var (_, values) in collected)
				{
					sb.AppendFormat($",{{0:{valueFormat}}}", values[i]);
				}
				sb.AppendLine();
			}
			QuantumConsole.Instance.LogAllToConsole(sb.ToString());
		}

		static int CompareCollectedValues<T>((string AnimationKey, T) x, (string AnimationKey, T) y) =>
			ReplayHelper.CompareAnimationKey(x.AnimationKey, y.AnimationKey);
	}
}
