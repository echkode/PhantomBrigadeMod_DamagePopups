// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using HarmonyLib;

using PhantomBrigade;
using PBDataHelperStats = PhantomBrigade.Data.DataHelperStats;

using QFSW.QC;

namespace EchKode.PBMods.DamagePopups.Diagnostics
{
	using CommandList = List<(string QCName, string Description, MethodInfo Method)>;

	static partial class ReplayTables
	{
		internal static CommandList Commands() => new CommandList()
		{
			("list-units", "List units in combat and some info about each", AccessTools.DeclaredMethod(typeof(ReplayTables), nameof(ListUnits))),
			("print-table", "Print replay table", AccessTools.DeclaredMethod(typeof(ReplayTables), nameof(PrintTable))),
		};

		static readonly Dictionary<string, System.Action<int>> tables = new Dictionary<string, System.Action<int>>()
		{
			["accumulation"] = PrintAccumulation,
			["summary"] = PrintSummary,
			["slots"] = PrintSlots,
			["positions"] = PrintPositions,
			["history"] = PrintHistory,
			["interp"] = PrintInterpolation,
			["interpolation"] = PrintInterpolation,
		};

		static void ListUnits()
		{
			if (!CombatStateCheck())
			{
				return;
			}

			ScenarioUtility.GetCombatParticipantUnits()
				.Select(unit =>
				{
					var combatant = IDUtility.GetLinkedCombatEntity(unit);
					return new UnitInfo()
					{
						PersistentId = unit.id.id,
						CombatId = combatant?.id.id ?? 0,
						Name = unit.hasNameInternal ? unit.nameInternal.s : "<no-name>",
						Preset = unit.hasDataKeyUnitPreset ? unit.dataKeyUnitPreset.s : "<none>",
						Faction = unit.faction.s,
						Flags = CompileUnitFlags(unit, combatant),
						Level = PBDataHelperStats.GetAverageUnitLevel(unit),
						Rating = PBDataHelperStats.GetAverageUnitRating(unit),
					};
				})
				.OrderBy(info => info.PersistentId)
				.ToList()
				.ForEach(info =>
				{
					var msg = $"P-{info.PersistentId}/C-{info.CombatId} [{info.Flags}] L{info.Level:F1} R{info.Rating:F1} faction={info.Faction}; preset={info.Preset}]";
					QuantumConsole.Instance.LogToConsole(msg);
				});
		}

		static void PrintTable(string unit, string table)
		{
			var (ok, combatUnitID) = UnitCheck(unit);
			if (!ok)
			{
				return;
			}

			if (tables.TryGetValue(table.ToLowerInvariant(), out var print))
			{
				print(combatUnitID);
				return;
			}

			QuantumConsole.Instance.LogToConsole("Table should be one of: " + string.Join(", ", tables.Keys));
		}
	}
}
