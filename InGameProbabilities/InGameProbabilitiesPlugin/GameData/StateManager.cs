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
            teamState["BaronKillBlue"] = "0";
            teamState["BaronKillRed"] = "0";
            teamState["DragonKillBlue"] = "0";
            teamState["DragonKillRed"] = "0";
            teamState["TowerKillBlue"] = "0";
            teamState["TowerKillRed"] = "0";
            teamState["KillBlue"] = "0";
            teamState["KillRed"] = "0";
            teamState["DeathBlue"] = "0";
            teamState["DeathsRed"] = "0";
            teamState["AssistBlue"] = "0";
            teamState["AssistRed"] = "0";
            teamState["LevelBlue"] = "0";
            teamState["LevelRed"] = "0";
            teamState["TotalGoldBlue"] = "0";
            teamState["TotalGoldRed"] = "0";
            teamState["MinionKillBlue"] = "0";
            teamState["MinionKillRed"] = "0";
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
                var key = Enum.GetName(typeof(MessageType), message.type) + message.teamId;
                teamState[key] = "" + message.value;
            }
        }

        public IDictionary<string, string> GetCurrentState()
        {
            var result = new Dictionary<string, string>(teamState);
            result["GameTime"] = "" + gameTime;
            result["ChampionIds"] = "[" + string.Join(",", championIds) + "]";
            return result;
        }
    }
}
