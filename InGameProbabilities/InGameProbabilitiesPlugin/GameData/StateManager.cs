
namespace InGameProbabilitiesPlugin.GameData
{
    using System;
    using System.Linq;
    using RiotSharp.LeagueEndpoint;

    internal class StateManager
    {
        public GameState GameState { get; }

        public StateManager(int[] champIds, Tier[] leaguesBlue, Tier[] leaguesRed)
        {
            this.GameState = new GameState
            {
                championIds = champIds
            };

            this.Initialize();

            //var minMax1 = FindMinMaxLeague(leaguesBlue);
            //minLeague1 = TierToString(minMax1[0]);
            //maxLeague1 = TierToString(minMax1[1]);
            //var minMax2 = FindMinMaxLeague(leaguesRed);
            //minLeague2 = TierToString(minMax2[0]);
            //maxLeague2 = TierToString(minMax2[1]);
        }

        private void Initialize()
        {
            // Models have only seen diamond labels
            this.GameState.minLeagueTeam1 = "DIAMOND";
            this.GameState.maxLeagueTeam1 = "DIAMOND";
            this.GameState.minLeagueTeam2 = "DIAMOND";
            this.GameState.maxLeagueTeam2 = "DIAMOND";
        }

        public string TierToString(Tier tier)
        {
            return tier.ToString().ToUpper();
        }

        public Tier[] FindMinMaxLeague(Tier[] leagues)
        {
            // bad. dont do this.
            var minMax = new Tier[2];

            if (leagues.Contains(Tier.Unranked))
                minMax[0] = Tier.Unranked;
            else if (leagues.Contains(Tier.Bronze))
                minMax[0] = Tier.Bronze;
            else if (leagues.Contains(Tier.Silver))
                minMax[0] = Tier.Silver;
            else if (leagues.Contains(Tier.Gold))
                minMax[0] = Tier.Gold;
            else if (leagues.Contains(Tier.Platinum))
                minMax[0] = Tier.Platinum;
            else if (leagues.Contains(Tier.Diamond))
                minMax[0] = Tier.Diamond;
            else if (leagues.Contains(Tier.Master))
                minMax[0] = Tier.Master;
            else
                minMax[0] = Tier.Challenger;

            if (leagues.Contains(Tier.Challenger))
                minMax[1] = Tier.Challenger;
            else if (leagues.Contains(Tier.Master))
                minMax[1] = Tier.Master;
            else if (leagues.Contains(Tier.Diamond))
                minMax[1] = Tier.Diamond;
            else if (leagues.Contains(Tier.Platinum))
                minMax[1] = Tier.Platinum;
            else if (leagues.Contains(Tier.Gold))
                minMax[1] = Tier.Gold;
            else if (leagues.Contains(Tier.Silver))
                minMax[1] = Tier.Silver;
            else if (leagues.Contains(Tier.Bronze))
                minMax[1] = Tier.Bronze;
            else
                minMax[1] = Tier.Unranked;

            return minMax;
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
