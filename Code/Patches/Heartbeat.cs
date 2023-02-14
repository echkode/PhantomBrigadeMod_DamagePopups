// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using System;
using System.Collections.Generic;

using HarmonyLib;

using PhantomBrigade;

namespace EchKode.PBMods.DamagePopups
{
	static class Heartbeat
	{
		internal static readonly List<Action<GameController>> SystemInstalls = new List<Action<GameController>>();

		internal static void Start()
		{
			var fi = AccessTools.DeclaredField(typeof(PhantomBrigade.Heartbeat), "_gameController");
			if (fi == null)
			{
				return;
			}

			var gameController = (GameController)fi.GetValue(null);
			SystemInstalls.ForEach(install => install(gameController));
		}
	}
}
