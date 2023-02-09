namespace EchKode.PBMods.DamagePopups
{
	sealed class DamagePopupFeature : Feature
	{
		public DamagePopupFeature()
		{
			Add(new DamagePopupInitializeSystem());
			Add(new DamagePopupTrackerSystem());
			Add(new DamagePopupBuildSystem());
			Add(new DamagePopupSortSystem());
			Add(new DamagePopupSlideSystem());
			Add(new DamagePopupAnimationSystem());
			Add(new SpriteDisposalSystem());
			if (ModLink.Settings.replayPopups != ModLink.ModSettings.ReplayPopup.None)
			{
				Add(new ReplayPopupFeature());
			}
			Add(new RequestDestroySystem());
		}

		internal static void Install()
		{
			Heartbeat.SystemInstalls.Add(gc =>
			{
				var gcs = gc.m_stateDict["combat"];
				var combatFeature = gcs.m_systems[0];
				var installee = new DamagePopupFeature();
				SystemInstaller.InstallBefore<PhantomBrigade.Combat.Systems.CombatMusicSystem>(combatFeature, installee);
			});

		}
	}
}
