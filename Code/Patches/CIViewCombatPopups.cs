using UnityEngine;

namespace EchKode.PBMods.DamagePopups
{
	static partial class CIViewCombatPopups
	{
		internal static bool logEnabled = false;

		internal static void AddDamageText(
			CombatEntity unitCombat,
			string animKey,
			float value,
			string format)
		{
			if (!initialized)
			{
				return;
			}
			if (string.IsNullOrEmpty(animKey))
			{
				return;
			}
			if (value.RoughlyEqual(0f))
			{
				return;
			}

			var now = Time.realtimeSinceStartupAsDouble;

			if (logEnabled)
			{
				Debug.LogFormat(
					"Mod {0} ({1}) CIViewCombatPopups.AddDamageText | time: {2:F3} | unit: C-{3} | type: {4} | value: {5} | position: {6}",
					ModLink.modIndex,
					ModLink.modId,
					now,
					unitCombat.id.id,
					animKey,
					value,
					unitCombat.position.v + unitCombat.localCenterPoint.v);
			}

			var ekc = ECS.Contexts.sharedInstance.ekRequest.CreateEntity();
			ekc.AddCombatUnitID(unitCombat.id.id);
			ekc.AddAnimationKey(animKey);
			ekc.AddDamageText(value, format);
		}
	}
}
