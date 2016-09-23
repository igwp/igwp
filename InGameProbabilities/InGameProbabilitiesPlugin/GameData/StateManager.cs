using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InGameProbabilitiesPlugin.GameData
{
    public class StateManager
    {
        private int gameTime;
        private int[] championIds;
        private IDictionary<string, string> teamState;

        public StateManager()
        {
            gameTime = 0;
            championIds = new int[10];
            teamState = new Dictionary<string, string>();
            Initialize();
        }

        private void Initialize()
        {
            teamState["baronKillTeam1"] = "0";
            teamState["baronKillTeam2"] = "0";
            teamState["dragonKillTeam1"] = "0";
            teamState["dragonKillTeam2"] = "0";
            teamState["towerKillTeam1"] = "0";
            teamState["towerKillTeam2"] = "0";
            teamState["killTeam1"] = "0";
            teamState["killTeam2"] = "0";
            teamState["deathTeam1"] = "0";
            teamState["deathsTeam2"] = "0";
            teamState["assistTeam1"] = "0";
            teamState["assistTeam2"] = "0";
            teamState["levelTeam1"] = "0";
            teamState["levelTeam2"] = "0";
            teamState["totalGoldTeam1"] = "0";
            teamState["totalGoldTeam2"] = "0";
            teamState["minionKillTeam1"] = "0";
            teamState["minionKillTeam2"] = "0";
        }

        public void UpdateState(GameMessage message)
        {
            if (message.type == MessageType.GameTime)
            {
                gameTime = message.value;
            }
            else if (message.type == MessageType.BaronKill)
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
            result["gameTime"] = "" + gameTime;
            result["championIds"] = "[" + string.Join(",", championIds) + "]";
            return result;
        }
    }
}
