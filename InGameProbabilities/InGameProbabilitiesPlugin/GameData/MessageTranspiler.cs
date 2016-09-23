using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InGameProbabilitiesPlugin.GameData
{
    public enum MessageType
    {
        GameTime,
        BaronKills,
        DragonKills,
        TowerKills,
        Kills,
        Deaths,
        Assists,
        ChampLvls,
        Gold,
        MinionKills
    }

    public enum TeamID
    {
        Blue = 0,
        Red = 1,
        None = -1
    }

    public class GameMessage
    {
        public MessageType type;
        public TeamID teamId;
        public int value;
    }

    public class MessageTranspiler
    {
        private IDictionary<MessageType, string> messageMap;
        private int[] baronKills;
        private double lastBaronTimes;
        private int[] dragonKills;
        private int[] towerKills;
        private int[] kills;
        private int[] deaths;
        private int[] assists;
        private int[] champLevels;
        private int[] goldTotal;
        private int[] minionKills;

        public MessageTranspiler()
        {
            baronKills = new int[2];
            lastBaronTimes = Double.MaxValue;
            dragonKills = new int[2];
            towerKills = new int[2];
            kills = new int[10];
            deaths = new int[10];
            assists = new int[10];
            champLevels = new int[10];
            goldTotal = new int[10];
            minionKills = new int[10];

            messageMap = new Dictionary<MessageType, string>();
            Initialize();
        }

        private void Initialize()
        {
            messageMap.Add(MessageType.GameTime, "GameTime");
            messageMap.Add(MessageType.BaronKills, "BaronTime");
            messageMap.Add(MessageType.DragonKills, "DragonBuffs");
            messageMap.Add(MessageType.TowerKills, "TowerKills");
            messageMap.Add(MessageType.Kills, "Kills");
            messageMap.Add(MessageType.Deaths, "Deaths");
            messageMap.Add(MessageType.Assists, "Assists");
            messageMap.Add(MessageType.ChampLvls, "Level");
            messageMap.Add(MessageType.Gold, "GoldTotal");
            messageMap.Add(MessageType.MinionKills, "MinionKills");
        }

        private int SumValuesByTeam(TeamID teamId, int[] data)
        {
            var start = teamId == TeamID.Blue ? 0 : 5;
            int result = 0;
            for (var i = 0; i < 5; i++)
            {
                result += data[start + i];
            }
            return result;
        }

        public List<GameMessage> Translate(string rawMessage)
        {
            var result = new List<GameMessage>();
            string[] lines = rawMessage.Split('\n');

            foreach (var line in lines)
            {
                var tokens = line.Split(',');

                // Ignore the first token (Update)
                for (var i = 1; i < tokens.Length; i++)
                {
                    var token = tokens[i];
                    if (token.StartsWith(messageMap[MessageType.GameTime]))
                    {
                        var time = Int32.Parse(tokens[++i]);
                        var message = new GameMessage
                        {
                            teamId = TeamID.None,
                            type = MessageType.GameTime,
                            value = time
                        };
                        result.Add(message);
                    }
                    else if (token.StartsWith(messageMap[MessageType.BaronKills]))
                    {
                        // TODO: Determine team
                        var time = Double.Parse(token.Split('_')[1]);
                        if (time < lastBaronTimes)
                        {
                            continue;
                        }
                        lastBaronTimes = time;
                        var message = new GameMessage
                        {
                            teamId = TeamID.None,
                            type = MessageType.BaronKills,
                            value = 0
                        };
                        result.Add(message);
                    }
                    else if (token.StartsWith(messageMap[MessageType.DragonKills]))
                    {
                        var teamId = Int32.Parse(token.Split('_')[1]);
                        var val = Int32.Parse(tokens[++i]);
                        if (dragonKills[teamId] == val)
                        {
                            continue;
                        }

                        dragonKills[teamId] = val;
                        var message = new GameMessage
                        {
                            teamId = (TeamID)teamId,
                            type = MessageType.DragonKills,
                            value = val
                        };
                        result.Add(message);
                    }
                    else if (token.StartsWith(messageMap[MessageType.TowerKills]))
                    {
                        var teamId = Int32.Parse("" + token.Last());
                        var val = Int32.Parse(tokens[++i]);
                        towerKills[teamId] = val;
                        var message = new GameMessage
                        {
                            teamId = (TeamID) teamId,
                            type = MessageType.TowerKills,
                            value = val
                        };
                        result.Add(message);
                    }
                    else if (token.StartsWith(messageMap[MessageType.Kills]))
                    {
                        var playerId = Int32.Parse(token.Split('_')[1]);
                        var teamId = playerId < 5 ? TeamID.Blue : TeamID.Red;
                        kills[playerId] = Int32.Parse(tokens[++i]);

                        var message = new GameMessage
                        {
                            teamId = teamId,
                            type = MessageType.Kills,
                            value = SumValuesByTeam(teamId, kills)
                        };
                        result.Add(message);
                    }
                    else if (token.StartsWith(messageMap[MessageType.Deaths]))
                    {
                        var playerId = Int32.Parse(token.Split('_')[1]);
                        var teamId = playerId < 5 ? TeamID.Blue : TeamID.Red;
                        deaths[playerId] = Int32.Parse(tokens[++i]);

                        var message = new GameMessage
                        {
                            teamId = teamId,
                            type = MessageType.Deaths,
                            value = SumValuesByTeam(teamId, deaths)
                        };
                        result.Add(message);
                    }
                    else if (token.StartsWith(messageMap[MessageType.Assists]))
                    {
                        var playerId = Int32.Parse(token.Split('_')[1]);
                        var teamId = playerId < 5 ? TeamID.Blue : TeamID.Red;
                        assists[playerId] = Int32.Parse(tokens[++i]);

                        var message = new GameMessage
                        {
                            teamId = teamId,
                            type = MessageType.Assists,
                            value = SumValuesByTeam(teamId, assists)
                        };
                        result.Add(message);
                    }
                    else if (token.StartsWith(messageMap[MessageType.ChampLvls]))
                    {
                        var playerId = Int32.Parse(token.Split('_')[1]);
                        var teamId = playerId < 5 ? TeamID.Blue : TeamID.Red;
                        champLevels[playerId] = Int32.Parse(tokens[++i]);

                        var message = new GameMessage
                        {
                            teamId = teamId,
                            type = MessageType.ChampLvls,
                            value = SumValuesByTeam(teamId, champLevels)
                        };
                        result.Add(message);
                    }
                    else if (token.StartsWith(messageMap[MessageType.Gold]))
                    {
                        var playerId = Int32.Parse(token.Split('_')[1]);
                        var teamId = playerId < 5 ? TeamID.Blue : TeamID.Red;
                        goldTotal[playerId] = Int32.Parse(tokens[++i]);

                        var message = new GameMessage
                        {
                            teamId = teamId,
                            type = MessageType.Gold,
                            value = SumValuesByTeam(teamId, goldTotal)
                        };
                        result.Add(message);
                    }
                    else if (token.StartsWith(messageMap[MessageType.MinionKills]))
                    {
                        var playerId = Int32.Parse(token.Split('_')[1]);
                        var teamId = playerId < 5 ? TeamID.Blue : TeamID.Red;
                        minionKills[playerId] = Int32.Parse(tokens[++i]);

                        var message = new GameMessage
                        {
                            teamId = teamId,
                            type = MessageType.MinionKills,
                            value = SumValuesByTeam(teamId, minionKills)
                        };
                        result.Add(message);
                    }
                }
            }

            return result;
        }
    }
}
