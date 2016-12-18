
namespace InGameProbabilitiesPlugin
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using GameData;
    using InjectionManager;
    using Network;

    public class EntryPoint
    {
        private const string InjectionDll = @"LeagueReplayHook.dll";

        private CancellationTokenSource _tokenSource;
        
        private Task _predictionTask;

        private Task _hookListenerTask;

        private double _winChance;

        private readonly NetworkInterface _networkInterface;

        private StateManager _stateManager;

        private readonly ConfigSettings _configuration;

        public EntryPoint()
        {
            this._configuration = new ConfigSettings();
            this._networkInterface = new NetworkInterface(this._configuration.PredictionServiceHost, this._configuration.PredictionServicePort);
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
                    callback?.Invoke(false);
                    return;
                }

                this._hookListenerTask = Task.Run(() =>
                    {
                        this.Log?.Invoke("started listening task for league hook...");

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
                            this.Log?.Invoke("started task for sending predictions...");

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
            Action<object> initState = (object res) =>
            {
                var championIds = championNames.Select(name => this._configuration.ChampIds[name]);

                this._tokenSource = new CancellationTokenSource();
                this._stateManager = new StateManager(championIds);

                callback?.Invoke(true);
            };

            if (this._tokenSource != null || this._hookListenerTask != null || this._predictionTask != null)
            {
                this.Reset(initState);
            }
            else
            {
                Task.Run(() => initState(true));
            }
        }

        public void Reset(Action<object> callback)
        {
            Task.Run(() =>
            {
                this.Log?.Invoke("reset called...");

                this._tokenSource?.Cancel();
                this._tokenSource?.Dispose();

                this._hookListenerTask?.Wait();
                this._predictionTask?.Wait();

                this._tokenSource = null;
                this._hookListenerTask = null;
                this._predictionTask = null;

                this.WinChance = .5;

                callback?.Invoke(true);
                this.Log?.Invoke("reset finished.");
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

        public event Action<object> Log;
    }
}
