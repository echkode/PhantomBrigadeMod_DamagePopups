using Entitas;

namespace EchKode.PBMods.DamagePopups
{
	sealed class RequestDestroySystem : ITearDownSystem
	{
		public void TearDown()
		{
			foreach (var req in ECS.Contexts.sharedInstance.ekRequest.GetEntities())
			{
				req.Destroy();
			}
		}
	}
}
