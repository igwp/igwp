using System;
using System.Text;
using InGameProbabilitiesPlugin.GameEventsListener;

namespace InGameProbabilitiesPlugin
{
    public class EntryPoint
    {
        public static void Main()
        {
            var listener = new GameEventListener(7000);
            var transpiler = new MessageTranspiler();

            var done = false;
            try
            {
                while (!done)
                {
                    var rawMessage = listener.GetMessage();
                    var messages = transpiler.Translate(rawMessage);
                    foreach (var message in messages)
                    {
                        var teamId = Enum.GetName(typeof(TeamID), message.teamId);
                        var type = Enum.GetName(typeof(MessageType), message.type);
                        Console.WriteLine("Team: {0}, Type: {1}, Value: {2}", teamId, type, message.value);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
