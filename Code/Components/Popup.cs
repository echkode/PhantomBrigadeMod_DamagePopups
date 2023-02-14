// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using System.Collections.Generic;

using Entitas;

using PBCIViewPopups = CIViewPopups;

namespace EchKode.PBMods.DamagePopups.ECS
{
	[EkPopup, EkReplay]
	public sealed class Popup : IComponent
	{
		public int popupID;
		public List<PBCIViewPopups.PopupNestedSegment> segments;
	}
}
