using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiotSharp.LeagueEndpoint;

namespace InGameProbabilitiesPlugin.GameData
{
    public class StateManager
    {
        private int gameTime;
        private long[] championIds;
        private string maxLeague1;
        private string maxLeague2;
        private string minLeague1;
        private string minLeague2;

        private IDictionary<string, string> teamState;

        public StateManager(long[] champIds, Tier[] leaguesBlue, Tier[] leaguesRed)
        {
            gameTime = 0;
            championIds = champIds;
            teamState = new Dictionary<string, string>();
            Initialize();

            var minMax1 = FindMinMaxLeague(leaguesBlue);
            minLeague1 = TierToString(minMax1[0]);
            maxLeague1 = TierToString(minMax1[1]);
            var minMax2 = FindMinMaxLeague(leaguesRed);
            minLeague2 = TierToString(minMax2[0]);
            maxLeague2 = TierToString(minMax2[1]);
        }

        private void Initialize()
        {
            teamState["baronKillsTeam1"] = "0";
            teamState["baronKillsTeam2"] = "0";
            teamState["dragonKillsTeam1"] = "0";
            teamState["dragonKillsTeam2"] = "0";
            teamState["towerKillsTeam1"] = "0";
            teamState["towerKillsTeam2"] = "0";
            teamState["killsTeam1"] = "0";
            teamState["killsTeam2"] = "0";
            teamState["deathsTeam1"] = "0";
            teamState["deathsTeam2"] = "0";
            teamState["assistsTeam1"] = "0";
            teamState["assistsTeam2"] = "0";
            teamState["champLvlsTeam1"] = "0";
            teamState["champLvlsTeam2"] = "0";
            teamState["goldTeam1"] = "0";
            teamState["goldTeam2"] = "0";
            teamState["minionKillsTeam1"] = "0";
            teamState["minionKillsTeam2"] = "0";
        }

        public string TierToString(Tier tier)
        {
            return Enum.GetName(typeof(RiotSharp.LeagueEndpoint.Tier), tier).ToUpper();
        }
        
        public Tier[] FindMinMaxLeague(Tier[] leagues)
        {
            // bad. dont do this.
            Tier[] minMax = new Tier[2];

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
            else if(leagues.Contains(Tier.Bronze))
                minMax[1] = Tier.Bronze;
            else
                minMax[1] = Tier.Unranked;

            return minMax;
        }

        public void UpdateState(GameMessage message)
        {
            if (message.type == MessageType.GameTime)
            {
                gameTime = message.value;
            }
            else if (message.type == MessageType.BaronKills)
            {
                // Do nothing for now
            }
            else
            {
                var name = "Team" + message.type;
                name = char.ToLower(name[0]) + name.Substring(1);
                var key = name + message.teamId;
                teamState[key] = "" + message.value;
            }
        }

        public IDictionary<string, string> GetCurrentState()
        {
            var result = new Dictionary<string, string>(teamState);
            result["gameTimeMS"] = "" + gameTime;
            result["championIds"] = "[" + string.Join(",", championIds) + "]";
            result["minLeagueTeam1"] = minLeague1;
            result["maxLeagueTeam1"] = maxLeague1;
            result["minLeagueTeam2"] = minLeague2;
            result["maxLeagueTeam2"] = maxLeague2;
            return result;
        }
    }
}
