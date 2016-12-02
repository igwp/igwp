
namespace InGameProbabilitiesPlugin.GameData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class StateManager
    {
        public GameState GameState { get; }

        public StateManager(IEnumerable<int> champIds)
        {
            this.GameState = new GameState
            {
                championIds = champIds.ToArray()
            };

            this.Initialize();
        }

        private void Initialize()
        {
            // Models have only seen diamond labels
            this.GameState.minLeagueTeam1 = "DIAMOND";
            this.GameState.maxLeagueTeam1 = "DIAMOND";
            this.GameState.minLeagueTeam2 = "DIAMOND";
            this.GameState.maxLeagueTeam2 = "DIAMOND";
        }

        public void UpdateState(GameMessage message)
        {
            switch (message.teamId)
            {
                case TeamID.Blue:
                    switch (message.type)
                    {
                        case MessageType.BaronKills:
                            this.GameState.baronKillsTeam1 = message.value;
                            break;
                        case MessageType.DragonKills:
                            this.GameState.dragonKillsTeam1 = message.value;
                            break;
                        case MessageType.TowerKills:
                            this.GameState.towerKillsTeam1 = message.value;
                            break;
                        case MessageType.Kills:
                            this.GameState.killsTeam1 = message.value;
                            break;
                        case MessageType.Deaths:
                            this.GameState.deathsTeam1 = message.value;
                            break;
                        case MessageType.Assists:
                            this.GameState.assistsTeam1 = message.value;
                            break;
                        case MessageType.ChampLvls:
                            this.GameState.champLvlsTeam1 = message.value;
                            break;
                        case MessageType.Gold:
                            this.GameState.goldTeam1 = message.value;
                            break;
                        case MessageType.MinionKills:
                            this.GameState.minionKillsTeam1 = message.value;
                            break;
                    }
                    break;
                case TeamID.Red:
                    switch (message.type)
                    {
                        case MessageType.BaronKills:
                            this.GameState.baronKillsTeam2 = message.value;
                            break;
                        case MessageType.DragonKills:
                            this.GameState.dragonKillsTeam2 = message.value;
                            break;
                        case MessageType.TowerKills:
                            this.GameState.towerKillsTeam2 = message.value;
                            break;
                        case MessageType.Kills:
                            this.GameState.killsTeam2 = message.value;
                            break;
                        case MessageType.Deaths:
                            this.GameState.deathsTeam2 = message.value;
                            break;
                        case MessageType.Assists:
                            this.GameState.assistsTeam2 = message.value;
                            break;
                        case MessageType.ChampLvls:
                            this.GameState.champLvlsTeam2 = message.value;
                            break;
                        case MessageType.Gold:
                            this.GameState.goldTeam2 = message.value;
                            break;
                        case MessageType.MinionKills:
                            this.GameState.minionKillsTeam2 = message.value;
                            break;
                    }
                    break;
                case TeamID.None:
                    switch (message.type)
                    {
                        case MessageType.GameTime:
                            this.GameState.gameTimeMS = $"{message.value}";
                            break;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
