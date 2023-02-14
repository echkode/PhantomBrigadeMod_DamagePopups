// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using System.IO;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	partial class ModLink
	{
		internal sealed class ModSettings
		{
			[System.Flags]
			internal enum LoggingFlag
			{
				None = 0,
				System = 0x1,
				Tracking = 0x2,
				Build = 0x4,
				Animation = 0x8,
				ReplayTables = 0x10,
				ReplayBuild = 0x20,
				ReplayDisplay = 0x40,
				ReplayExit = 0x80,
				All = 0xFF,
			}

			[System.Flags]
			internal enum ReplayPopup
			{
				None = 0,
				Barrage = 1,
				Cumulative = 2,
			}

#pragma warning disable CS0649
			public LoggingFlag logging;
			public bool registerConsoleCommands;
			public int samplesPerSecond = 16;
			public int textUpdateDelay = 3;
			public float popupDisplayTime = 2f;
			public ReplayPopup replayPopups;
#pragma warning restore CS0649

			internal bool IsLoggingEnabled(LoggingFlag flag) => (logging & flag) == flag;
		}

		internal static ModSettings Settings;

		internal static void LoadSettings()
		{
			var settingsPath = Path.Combine(modPath, "settings.yaml");
			Settings = UtilitiesYAML.ReadFromFile<ModSettings>(settingsPath, false);
			if (Settings == null)
			{
				Settings = new ModSettings();

				Debug.LogFormat(
					"Mod {0} ({1}) no settings file found, using defaults | path: {2}",
					modIndex,
					modId,
					settingsPath);
			}
			else
			{
				Clamp(ref Settings.samplesPerSecond, 16, 40);
				Clamp(ref Settings.textUpdateDelay, 1, 4);
				Settings.popupDisplayTime = Mathf.Clamp(Settings.popupDisplayTime, 0.5f, 3f);
			}

			if (Settings.logging != ModSettings.LoggingFlag.None)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) diagnostic logging is on: {2}",
					modIndex,
					modId,
					Settings.logging);

				Debug.LogFormat(
					"Mod {0} ({1}) settings\n  samplesPerSecond: {2}\n  textUpdateDelay: {3}\n  popupDisplayTime: {4}\n  replayPopups: {5}",
					modIndex,
					modId,
					Settings.samplesPerSecond,
					Settings.textUpdateDelay,
					Settings.popupDisplayTime,
					Settings.replayPopups);
			}
		}

		static void Clamp(ref int value, int min, int max)
		{
			if (max < min)
			{
				return;
			}
			if (value < min)
			{
				value = min;
			}
			if (value > max)
			{
				value = max;
			}
		}
	}
}
