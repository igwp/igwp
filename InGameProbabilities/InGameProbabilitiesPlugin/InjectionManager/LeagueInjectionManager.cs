
using System.Diagnostics;
using System.Linq;

namespace InGameProbabilitiesPlugin.InjectionManager
{
    class LeagueInjectionManager
    {
        private const string LeagueClientName = "League of Legends.exe";

        private const string SpectatorCommandLineFlag = "spectator";

        public LeagueInjectionManager()
        {
            
        }

        public bool Inject(string dllName)
        {
            var processes = Process.GetProcessesByName(LeagueClientName);

            if (processes.Length != 1)
            {
                Debugger.Break();
                return false;
            }

            var league = processes.First();

            if(!league.StartInfo.Arguments.Contains(SpectatorCommandLineFlag))
            {
                return false;
            }

            return ClrInjectionLib.Injector.Inject32(league.Id, dllName);
        }
    }
}
