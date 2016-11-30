
namespace InGameProbabilitiesPlugin
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using GameData;
    using InjectionManager;
    using Network;

    public class EntryPoint
    {
        private const string InjectionDll = @"LeagueReplayHook.dll";

        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        
        private Task _predictionTask;

        private Task _hookListenerTask;

        private double _winChance;

        private readonly NetworkInterface _networkInterface;

        private StateManager _stateManager;

        private readonly ConfigSettings _configuration;

        public EntryPoint()
        {
            this._configuration = new ConfigSettings();
            this._networkInterface = new NetworkInterface(this._configuration.PredictionServiceHost, this._configuration.PredictionServicePort, this._configuration.ApiKey);
        }

        public void StartApp(Action<object> callback)
        {
            Task.Run(() =>
            {
                var listener = new GameEventListener(this._configuration.GameHookPort);
                var transpiler = new MessageTranspiler();
                var injector = new LeagueInjectionManager();
                
                var path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\{EntryPoint.InjectionDll}";
                if (!injector.Inject(path))
                {
                    Console.Error.WriteLine("league appears to not be running (or injection failed)!");
                }

                this._hookListenerTask = Task.Run(() =>
                    {
                        while (!this._tokenSource.IsCancellationRequested)
                        {
                            var rawMessage = listener.GetMessage();
                            var messages = transpiler.Translate(rawMessage);
                            foreach (var message in messages)
                            {
                                this._stateManager.UpdateState(message);
                            }
                        }
                    },
                    this._tokenSource.Token);
                
                this._predictionTask = Task.Run(async () =>
                    {
                        try
                        {
                            while (!this._tokenSource.IsCancellationRequested)
                            {
                                await Task.Delay(1000, this._tokenSource.Token);

                                var result = await this._networkInterface.GetPrediction(this._stateManager.GameState);
                                this.WinChance = result?.team1 ?? .5;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }

                    },
                    this._tokenSource.Token);

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
                var championIds = this._networkInterface?.GetChampionIds(championNames);

                this._stateManager = new StateManager(championIds, null, null);

                callback?.Invoke(true);
            });
        }

        private double WinChance
        {
            get
            {
                return this._winChance;
            }
            set
            {
                if (Math.Abs(value - this._winChance) > double.Epsilon)
                {
                    this._winChance = value;
                    this.WinChanceChanged?.Invoke(this.BlueWinChance, this.RedWinChange);
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
    }
}
