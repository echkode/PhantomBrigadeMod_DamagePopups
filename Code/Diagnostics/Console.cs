using System.Collections.Generic;
using System.Reflection;
using System.Text;

using QFSW.QC;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups.Diagnostics
{
	using CommandSupplier = System.Func<List<(string QCName, string Description, MethodInfo Method)>>;

	static class Console
	{
		private static bool commandsRegistered;
		private static List<CommandSupplier> commandSuppliers = new List<CommandSupplier>()
		{
			ReplayTables.Commands,
		};

		internal static void RegisterCommands()
		{
			if (commandsRegistered)
			{
				return;
			}

			var registeredFunctions = new StringBuilder();
			var k = 0;

			foreach (var commandSupplier in commandSuppliers)
			{
				foreach (var (qcName, desc, method) in commandSupplier())
				{
					var functionName = $"{method.DeclaringType.Name}.{method.Name}";
					var commandName = Constants.CommandPrefix + qcName;
					var commandInfo = new CommandAttribute(
						commandName,
						desc,
						MonoTargetType.Single);
					var commandData = new CommandData(method, commandInfo);
					if (!QuantumConsoleProcessor.TryAddCommand(commandData))
					{
						Debug.LogWarningFormat(
							"Mod {0} ({1}) did not register QC command successfully: {2} <{3}>",
							ModLink.modIndex,
							ModLink.modId,
							qcName,
							functionName);
						continue;
					}
					registeredFunctions.Append(System.Environment.NewLine + $"  {commandName} <{functionName}>");
					k += 1;
				}
			}

			if (ModLink.Settings.IsLoggingEnabled(ModLink.ModSettings.LoggingFlag.System))
			{
				Debug.LogFormat("Mod {0} ({1}) loaded QC commands | count: {2}{3}",
					ModLink.modIndex,
					ModLink.modId,
					k,
					registeredFunctions);
			}

			commandsRegistered = true;
		}
	}
}
