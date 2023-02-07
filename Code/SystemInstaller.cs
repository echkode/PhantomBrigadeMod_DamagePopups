using System.Collections.Generic;

using Entitas;

using HarmonyLib;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	static class SystemInstaller
	{
		private static readonly List<System.Action> installers = new List<System.Action>()
		{
			DamagePopupFeature.Install,
		};

		internal static void InstallAll()
		{
			foreach (var installer in installers)
			{
				installer();
			}
		}

		internal static void InstallAtEnd(Systems feature, ISystem installee)
		{
			feature.Add(installee);
			Debug.LogFormat(
				"Mod {0} ({1}) installed system {2}",
				ModLink.modIndex,
				ModLink.modId,
				installee.GetType().FullName);
		}

		internal static void InstallBefore<T>(Systems feature, ISystem installee)
			where T : ISystem
		{
			var installed = false;

			if (installee is IInitializeSystem init)
			{
				InstallBefore<IInitializeSystem, T>(feature, "initialize", init);
				installed = true;
			}
			if (installee is IExecuteSystem exec)
			{
				InstallBefore<IExecuteSystem, T>(feature, "execute", exec);
				installed = true;
			}
			if (installee is ICleanupSystem cleanup)
			{
				InstallBefore<ICleanupSystem, T>(feature, "cleanup", cleanup);
				installed = true;
			}
			if (installee is ITearDownSystem tearDown)
			{
				InstallBefore<ITearDownSystem, T>(feature, "tearDown", tearDown);
				installed = true;
			}
			if (installee is IEnableSystem enable)
			{
				InstallBefore<IEnableSystem, T>(feature, "enable", enable);
				installed = true;
			}
			if (installee is IDisableSystem disable)
			{
				InstallBefore<IDisableSystem, T>(feature, "disable", disable);
				installed = true;
			}

			if (!installed)
			{
				Debug.LogWarningFormat(
					"Mod {0} ({1}) InstallBefore unable to install system -- new system doesn't implement installable interface | installee: {2}",
					ModLink.modIndex,
					ModLink.modId,
					installee.GetType().FullName);
			}
		}

		static void InstallBefore<S, T>(Systems feature, string kind, S installee)
			where S : ISystem
			where T : ISystem
		{
			var fi = AccessTools.Field(feature.GetType(), $"_{kind}Systems");
			if (fi == null)
			{
				Debug.LogWarningFormat(
					"Mod {0} ({1}) InstallBefore attempted to install a system kind that the feature doesn't support | feature: {2} | kind: {3} | installee: {4}",
					ModLink.modIndex,
					ModLink.modId,
					feature.GetType().Name,
					kind,
					installee.GetType().FullName);
				return;
			}

			var systems = (List<S>)fi.GetValue(feature);
			var i = 0;
			for (; i < systems.Count; i += 1)
			{
				if (systems[i] is T)
				{
					break;
				}
			}

			var insert = i != systems.Count;
			if (insert)
			{
				systems.Insert(i, installee);
			}
			else
			{
				systems.Add(installee);
			}

			fi = AccessTools.Field(feature.GetType(), $"_{kind}SystemNames");
			if (fi != null)
			{
				var names = (List<string>)fi.GetValue(feature);
				var name = installee.GetType().FullName;
				if (insert)
				{
					names.Insert(i, name);
				}
				else
				{
					names.Add(name);
				}
			}

			var fmt = insert
				? "Mod {0} ({1}) InstallBefore inserted system {2} ({3}) before {4}"
				: "Mod {0} ({1}) InstallBefore did not find system {4} so appended system {2} ({3})";
			Debug.LogFormat(
				fmt,
				ModLink.modIndex,
				ModLink.modId,
				installee.GetType().FullName,
				typeof(S).Name,
				typeof(T).Name);
		}
	}
}
