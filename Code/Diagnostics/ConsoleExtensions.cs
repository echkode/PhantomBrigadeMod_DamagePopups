using QFSW.QC;

namespace EchKode.PBMods.DamagePopups.Diagnostics
{
	static class ConsoleExtensions
	{
		public static void LogAllToConsole(this QuantumConsole instance, string message)
		{
			while (message.Length > Constants.ConsoleOutputLengthMax)
			{
				var pos = message.LastIndexOf('\n', Constants.ConsoleOutputLengthMax - 1);
				instance.LogToConsole(message.Substring(0, pos));
				message = message.Substring(pos + 1);
			}
			instance.LogToConsole(message);
		}
	}
}
