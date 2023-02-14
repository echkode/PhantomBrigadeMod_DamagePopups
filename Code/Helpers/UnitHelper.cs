// Copyright (c) 2023 EchKode
// SPDX-License-Identifier: BSD-3-Clause

using PhantomBrigade;

using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	static class UnitHelper
	{
		internal static Vector3 GetPosition(CombatEntity combatUnit) => combatUnit.position.v + combatUnit.localCenterPoint.v;
		internal static Vector3 GetPopupPosition(CombatEntity combatUnit)
		{
			var position = GetPosition(combatUnit) + combatUnit.localCenterPoint.v;
			var pe = IDUtility.GetLinkedPersistentEntity(combatUnit);
			if (pe == null)
			{
				return position;
			}
			if (!pe.hasDataKeyUnitClass)
			{
				return position;
			}

			if (UnitClassKeys.tank == pe.dataKeyUnitClass.s)
			{
				position += combatUnit.localCenterPoint.v;
			}

			return position;
		}
	}
}
