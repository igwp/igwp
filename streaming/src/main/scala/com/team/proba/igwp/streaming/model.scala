package com.team.proba.igwp.streaming

object model {
  case class Example(
    gameTimeMS: Long,
    championIds: Seq[Int],
    baronKillsTeam1: Int,
    baronKillsTeam2: Int,
    dragonKillsTeam1: Int,
    dragonKillsTeam2: Int,
    towerKillsTeam1: Int,
    towerKillsTeam2: Int,
    goldTeam1: Int,
    goldTeam2: Int,
    killsTeam1: Int,
    killsTeam2: Int,
    deathsTeam1: Int,
    deathsTeam2: Int,
    assistsTeam1: Int,
    assistsTeam2: Int,
    champLvlsTeam1: Int,
    champLvlsTeam2: Int,
    minionKillsTeam1: Int,
    minionKillsTeam2: Int,
    minLeagueTeam1: String,
    minLeagueTeam2: String,
    maxLeagueTeam1: String,
    maxLeagueTeam2: String
  )

  case class Probability(
    team1: Double,
    team2: Double
  )
}
