
namespace InGameProbabilitiesPlugin.GameData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal enum MessageType
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

    internal enum TeamID
    {
        None,
        Blue,
        Red
    }

    internal class GameMessage
    {
        public MessageType type;
        public TeamID teamId;
        public int value;
    }

    internal class MessageTranspiler
    {
        private readonly IDictionary<MessageType, string> _messageMap;
        private int[] baronKills;
        private double lastBaronTimes;
        private readonly int[] _dragonKills;
        private readonly int[] _towerKills;
        private readonly int[] _kills;
        private readonly int[] _deaths;
        private readonly int[] _assists;
        private readonly int[] _champLevels;
        private readonly int[] _goldTotal;
        private readonly int[] _minionKills;

        public MessageTranspiler()
        {
            this.baronKills = new int[2];
            this.lastBaronTimes = Double.MaxValue;
            this._dragonKills = new int[2];
            this._towerKills = new int[2];
            this._kills = new int[10];
            this._deaths = new int[10];
            this._assists = new int[10];
            this._champLevels = new int[10];
            this._goldTotal = new int[10];
            this._minionKills = new int[10];

            this._messageMap = new Dictionary<MessageType, string>();
            this.Initialize();
        }

        private void Initialize()
        {
            this._messageMap.Add(MessageType.GameTime, "GameTime");
            this._messageMap.Add(MessageType.BaronKills, "BaronTime");
            this._messageMap.Add(MessageType.DragonKills, "DragonBuffs");
            this._messageMap.Add(MessageType.TowerKills, "TowerKills");
            this._messageMap.Add(MessageType.Kills, "Kills");
            this._messageMap.Add(MessageType.Deaths, "Deaths");
            this._messageMap.Add(MessageType.Assists, "Assists");
            this._messageMap.Add(MessageType.ChampLvls, "Level");
            this._messageMap.Add(MessageType.Gold, "GoldTotal");
            this._messageMap.Add(MessageType.MinionKills, "MinionKills");
        }

        private static int SumValuesByTeam(TeamID teamId, int[] data)
        {
            var start = teamId == TeamID.Blue ? 0 : 5;
            var result = 0;
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
                    if (token.StartsWith(this._messageMap[MessageType.GameTime]))
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
                    else if (token.StartsWith(this._messageMap[MessageType.BaronKills]))
                    {
                        // TODO: Determine team
                        var time = Double.Parse(token.Split('_')[1]);
                        if (time < this.lastBaronTimes)
                        {
                            continue;
                        }
                        this.lastBaronTimes = time;
                        var message = new GameMessage
                        {
                            teamId = TeamID.None,
                            type = MessageType.BaronKills,
                            value = 0
                        };
                        result.Add(message);
                    }
                    else if (token.StartsWith(this._messageMap[MessageType.DragonKills]))
                    {
                        var teamId = Int32.Parse(token.Split('_')[1]);
                        var val = Int32.Parse(tokens[++i]);
                        if (this._dragonKills[teamId] == val)
                        {
                            continue;
                        }

                        this._dragonKills[teamId] = val;
                        teamId++;
                        var message = new GameMessage
                        {
                            teamId = (TeamID)teamId,
                            type = MessageType.DragonKills,
                            value = val
                        };
                        result.Add(message);
                    }
                    else if (token.StartsWith(this._messageMap[MessageType.TowerKills]))
                    {
                        var teamId = Int32.Parse("" + token.Last());
                        var val = Int32.Parse(tokens[++i]);
                        this._towerKills[teamId] = val;
                        teamId++;
                        var message = new GameMessage
                        {
                            teamId = (TeamID) teamId,
                            type = MessageType.TowerKills,
                            value = val
                        };
                        result.Add(message);
                    }
                    else if (token.StartsWith(this._messageMap[MessageType.Kills]))
                    {
                        var playerId = Int32.Parse(token.Split('_')[1]);
                        var teamId = playerId < 5 ? TeamID.Blue : TeamID.Red;
                        this._kills[playerId] = Int32.Parse(tokens[++i]);

                        var message = new GameMessage
                        {
                            teamId = teamId,
                            type = MessageType.Kills,
                            value = MessageTranspiler.SumValuesByTeam(teamId, this._kills)
                        };
                        result.Add(message);
                    }
                    else if (token.StartsWith(this._messageMap[MessageType.Deaths]))
                    {
                        var playerId = Int32.Parse(token.Split('_')[1]);
                        var teamId = playerId < 5 ? TeamID.Blue : TeamID.Red;
                        this._deaths[playerId] = Int32.Parse(tokens[++i]);

                        var message = new GameMessage
                        {
                            teamId = teamId,
                            type = MessageType.Deaths,
                            value = MessageTranspiler.SumValuesByTeam(teamId, this._deaths)
                        };
                        result.Add(message);
                    }
                    else if (token.StartsWith(this._messageMap[MessageType.Assists]))
                    {
                        var playerId = Int32.Parse(token.Split('_')[1]);
                        var teamId = playerId < 5 ? TeamID.Blue : TeamID.Red;
                        this._assists[playerId] = Int32.Parse(tokens[++i]);

                        var message = new GameMessage
                        {
                            teamId = teamId,
                            type = MessageType.Assists,
                            value = MessageTranspiler.SumValuesByTeam(teamId, this._assists)
                        };
                        result.Add(message);
                    }
                    else if (token.StartsWith(this._messageMap[MessageType.ChampLvls]))
                    {
                        var playerId = Int32.Parse(token.Split('_')[1]);
                        var teamId = playerId < 5 ? TeamID.Blue : TeamID.Red;
                        this._champLevels[playerId] = Int32.Parse(tokens[++i]);

                        var message = new GameMessage
                        {
                            teamId = teamId,
                            type = MessageType.ChampLvls,
                            value = MessageTranspiler.SumValuesByTeam(teamId, this._champLevels)
                        };
                        result.Add(message);
                    }
                    else if (token.StartsWith(this._messageMap[MessageType.Gold]))
                    {
                        var playerId = Int32.Parse(token.Split('_')[1]);
                        var teamId = playerId < 5 ? TeamID.Blue : TeamID.Red;
                        this._goldTotal[playerId] = Int32.Parse(tokens[++i]);

                        var message = new GameMessage
                        {
                            teamId = teamId,
                            type = MessageType.Gold,
                            value = MessageTranspiler.SumValuesByTeam(teamId, this._goldTotal)
                        };
                        result.Add(message);
                    }
                    else if (token.StartsWith(this._messageMap[MessageType.MinionKills]))
                    {
                        var playerId = Int32.Parse(token.Split('_')[1]);
                        var teamId = playerId < 5 ? TeamID.Blue : TeamID.Red;
                        this._minionKills[playerId] = Int32.Parse(tokens[++i]);

                        var message = new GameMessage
                        {
                            teamId = teamId,
                            type = MessageType.MinionKills,
                            value = MessageTranspiler.SumValuesByTeam(teamId, this._minionKills)
                        };
                        result.Add(message);
                    }
                }
            }

            return result;
        }
    }
}
