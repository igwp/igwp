using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using InGameProbabilitiesPlugin.GameData;
using InGameProbabilitiesPlugin.InjectionManager;
using InGameProbabilitiesPlugin.Network;

namespace InGameProbabilitiesPlugin
{
    public class EntryPoint
    {
        private const string InjectionDll = "LeagueReplayHook.dll";

        public static void Main()
        {
            var listener = new GameEventListener(7000);
            var transpiler = new MessageTranspiler();
            var stateManager = new StateManager();
            var networkInterface = new NetworkInterface("http://54.183.147.234", 3000);

            var injector = new LeagueInjectionManager();
            if (!injector.Inject(Path.GetFullPath(InjectionDll)))
            {
                Console.Error.WriteLine("league appears to not be running (or injection failed)!");
                return;
            }

            Console.WriteLine("successfully injected {0} into league!", InjectionDll);

            var done = false;
            try
            {
                var count = 0;
                while (!done)
                {
                    count++;
                    var rawMessage = listener.GetMessage();
                    var messages = transpiler.Translate(rawMessage);
                    foreach (var message in messages)
                    {
                        stateManager.UpdateState(message);
                    }

                    if (count > 50)
                    {
                        var result = networkInterface.Post("/getmodel", stateManager.GetCurrentState());
                        Console.WriteLine("Probability: " + result.probability);
                        count = 0;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void printMessages(GameMessage[] messages)
        {
            foreach (var message in messages)
            {
                var teamId = Enum.GetName(typeof(TeamID), message.teamId);
                var type = Enum.GetName(typeof(MessageType), message.type);
                Console.WriteLine("Team: {0}, Type: {1}, Value: {2}", teamId, type, message.value);
            }
        }
    }
}
