
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using InGameProbabilitiesPlugin.GameData;
using InGameProbabilitiesPlugin.InjectionManager;
using InGameProbabilitiesPlugin.Network;

namespace InGameProbabilitiesPlugin
{
    public class EntryPoint
    {
        private const string InjectionDll = @"LeagueReplayHook.dll";

        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        
        private Task listeningTask;

        private double winChance;

        private readonly NetworkInterface networkInterface;

        private StateManager stateManager;

        private readonly ConfigSettings configuration;

        public EntryPoint()
        {
            this.configuration = new ConfigSettings();
            networkInterface = new NetworkInterface(this.configuration.PredictionServiceHost, this.configuration.PredictionServicePort, this.configuration.ApiKey);
        }

        public void StartApp(Action<object> callback)
        {
            Task.Run(() =>
            {
                var listener = new GameEventListener(this.configuration.GameHookPort);
                var transpiler = new MessageTranspiler();
                var injector = new LeagueInjectionManager();
                
                var path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\{InjectionDll}";
                if (!injector.Inject(path))
                {
                    Console.Error.WriteLine("league appears to not be running (or injection failed)!");
                }
                
                this.listeningTask = Task.Run(
                    () =>
                    {
                        try
                        {
                            var time = DateTime.Now;
                            while (!this.tokenSource.IsCancellationRequested)
                            {
                                var rawMessage = listener.GetMessage();
                                var messages = transpiler.Translate(rawMessage);
                                foreach (var message in messages)
                                {
                                    stateManager.UpdateState(message);
                                }

                                if (time.AddSeconds(1) < DateTime.Now)
                                {
                                    var result = networkInterface?.Post("/getmodel", stateManager.GetCurrentState());
                                    WinChance = result?.team1 ?? .5;
                                    time = DateTime.Now;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }

                    },
                    this.tokenSource.Token);

                callback?.Invoke(true);
            });

        }

        public void InitializeState(string[] championNames, string[] blueTeam, string[] redTeam, Action<object> callback)
        {
            Task.Run(() =>
            {
                //var summonerIdsBlue = networkInterface.GetSummonerIds(blueTeam);
                //var summonerIdsRed = networkInterface.GetSummonerIds(redTeam);
                //var leaguesBlue = networkInterface.GetRank(summonerIdsBlue);
                //var leaguesRed = networkInterface.GetRank(summonerIdsRed);
                var championIds = networkInterface?.GetChampionIds(championNames);

                stateManager = new StateManager(championIds, null, null);

                callback?.Invoke(true);
            });
        }

        private double WinChance
        {
            get
            {
                return winChance;
            }
            set
            {
                if (Math.Abs(value - winChance) > double.Epsilon)
                {
                    winChance = value;
                    WinChanceChanged?.Invoke(this.BlueWinChance, this.RedWinChange);
                }
            }
        }

        public double BlueWinChance => this.WinChance;

        public double RedWinChange => 1 - this.WinChance;

        /// <summary>
        /// Arg1 is blue teams chance
        /// Arg2 is red teams chance
        /// </summary>
        public event Action<object, object> WinChanceChanged;

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
