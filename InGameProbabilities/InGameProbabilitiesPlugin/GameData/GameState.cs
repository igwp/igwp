
namespace InGameProbabilitiesPlugin.GameData
{
    public class GameState
    {
        public string gameTimeMS { get; set; }

        public int[] championIds { get; set; }

        public int baronKillsTeam1 { get; set; }
        public int baronKillsTeam2 { get; set; }

        public int dragonKillsTeam1 { get; set; }
        public int dragonKillsTeam2 { get; set; }

        public int towerKillsTeam1 { get; set; }
        public int towerKillsTeam2 { get; set; }

        public int killsTeam1 { get; set; }
        public int killsTeam2 { get; set; }

        public int deathsTeam1 { get; set; }
        public int deathsTeam2 { get; set; }

        public int assistsTeam1 { get; set; }
        public int assistsTeam2 { get; set; }

        public int champLvlsTeam1 { get; set; }
        public int champLvlsTeam2 { get; set; }

        public int goldTeam1 { get; set; }
        public int goldTeam2 { get; set; }

        public int minionKillsTeam1 { get; set; }
        public int minionKillsTeam2 { get; set; }

        public string minLeagueTeam1 { get; set; }
        public string minLeagueTeam2 { get; set; }

        public string maxLeagueTeam1 { get; set; }
        public string maxLeagueTeam2 { get; set; }
    }
}
