using System.IO;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	partial class ModLink
	{
		internal sealed class ModSettings
		{
#pragma warning disable CS0649
			public bool enableLogging;
#pragma warning restore CS0649
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

			if (Settings.enableLogging)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) diagnostic logging is on",
					modIndex,
					modId);
			}
		}
	}
}
