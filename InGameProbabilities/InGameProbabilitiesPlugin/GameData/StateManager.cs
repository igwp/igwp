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
            teamState["baronKillBlue"] = "0";
            teamState["baronKillRed"] = "0";
            teamState["dragonKillBlue"] = "0";
            teamState["dragonKillRed"] = "0";
            teamState["towerKillBlue"] = "0";
            teamState["towerKillRed"] = "0";
            teamState["killBlue"] = "0";
            teamState["killRed"] = "0";
            teamState["deathBlue"] = "0";
            teamState["deathsRed"] = "0";
            teamState["assistBlue"] = "0";
            teamState["assistRed"] = "0";
            teamState["levelBlue"] = "0";
            teamState["levelRed"] = "0";
            teamState["totalGoldBlue"] = "0";
            teamState["totalGoldRed"] = "0";
            teamState["minionKillBlue"] = "0";
            teamState["minionKillRed"] = "0";
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
                var name = Enum.GetName(typeof(MessageType), message.type);
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
