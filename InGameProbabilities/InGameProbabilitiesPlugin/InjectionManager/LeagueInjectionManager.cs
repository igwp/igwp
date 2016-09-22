using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;

namespace InGameProbabilitiesPlugin.InjectionManager
{
    class LeagueInjectionManager
    {
        public const string LeagueClientName = "League of Legends";

        private const string SpectatorCommandLineFlag = "spectator";

        public LeagueInjectionManager()
        {
            
        }

        public bool Inject(string dllName)
        {
            var processes = Process.GetProcessesByName(LeagueClientName);

            if (processes.Length != 1)
            {
                return false;
            }

            var league = processes.First();

            if(!league.GetCommandLine().Contains(SpectatorCommandLineFlag))
            {
                return false;
            }
            
            return ClrInjectionLib.Injector.Inject32(league.Id, dllName);
        }
    }

    static class ProcessExt
    {
        public static string GetCommandLine(this Process process)
        {
            var commandLine = new StringBuilder(process.MainModule.FileName);

            commandLine.Append(" ");
            using (var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            {
                foreach (var @object in searcher.Get())
                {
                    commandLine.Append(@object["CommandLine"]);
                    commandLine.Append(" ");
                }
            }

            return commandLine.ToString();
        }
    }
}
