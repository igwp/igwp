package com.team.proba.igwp.etl

import com.team.proba.igwp.etl.model._
import org.scalatest.{WordSpec, Matchers}

class MatchDetailSpec extends WordSpec with Matchers {
  "a MatchDetail" when {
    "converting to examples" should {
      val zeroExample = Example(
        gameTimeMS = 0L,
        championIds = Seq(12, 13),
        baronKillsTeam1 = 0, baronKillsTeam2 = 0,
        dragonKillsTeam1 = 0, dragonKillsTeam2 = 0,
        towerKillsTeam1 = 0, towerKillsTeam2 = 0,
        goldTeam1 = 2500, goldTeam2 = 2500,
        killsTeam1 = 0, killsTeam2 = 0,
        deathsTeam1 = 0, deathsTeam2 = 0,
        assistsTeam1 = 0, assistsTeam2 = 0,
        champLvlsTeam1 = 5, champLvlsTeam2 = 5,
        minionKillsTeam1 = 0, minionKillsTeam2 = 0,
        minLeagueTeam1 = "MASTER", minLeagueTeam2 = "GOLD",
        maxLeagueTeam1 = "MASTER", maxLeagueTeam2 = "GOLD",
        1
      )
      val parts = Seq(
        Participant("MASTER", 12, 1),
        Participant("GOLD", 13, 6)
      )
      "give back the zero example if there is no timeline" in {
        MatchDetail(parts, Timeline(Seq.empty), 1).toExamples shouldBe
          Seq(zeroExample)
      }
      "update the kdas" in {
        val events = Seq(Event("CHAMPION_KILL", 2, 6, Seq(3, 5), "", "", Position(0, 0)))
        MatchDetail(parts, Timeline(Seq(Frame(events, Map.empty, 12L))), 1).toExamples shouldBe
          Seq(zeroExample, Example(
            gameTimeMS = 12L,
            championIds = Seq(12, 13),
            baronKillsTeam1 = 0, baronKillsTeam2 = 0,
            dragonKillsTeam1 = 0, dragonKillsTeam2 = 0,
            towerKillsTeam1 = 0, towerKillsTeam2 = 0,
            goldTeam1 = 0, goldTeam2 = 0,
            killsTeam1 = 1, killsTeam2 = 0,
            deathsTeam1 = 0, deathsTeam2 = 1,
            assistsTeam1 = 2, assistsTeam2 = 0,
            champLvlsTeam1 = 0, champLvlsTeam2 = 0,
            minionKillsTeam1 = 0, minionKillsTeam2 = 0,
            minLeagueTeam1 = "MASTER", minLeagueTeam2 = "GOLD",
            maxLeagueTeam1 = "MASTER", maxLeagueTeam2 = "GOLD",
            1
          ))
      }
      "update the tower kills based on killer" in {
        val events = Seq(Event("BUILDING_KILL", 2, 0, Seq.empty, "", "", Position(0, 0)))
        MatchDetail(parts, Timeline(Seq(Frame(events, Map.empty, 12L))), 1).toExamples shouldBe
          Seq(zeroExample, Example(
            gameTimeMS = 12L,
            championIds = Seq(12, 13),
            baronKillsTeam1 = 0, baronKillsTeam2 = 0,
            dragonKillsTeam1 = 0, dragonKillsTeam2 = 0,
            towerKillsTeam1 = 1, towerKillsTeam2 = 0,
            goldTeam1 = 0, goldTeam2 = 0,
            killsTeam1 = 0, killsTeam2 = 0,
            deathsTeam1 = 0, deathsTeam2 = 0,
            assistsTeam1 = 0, assistsTeam2 = 0,
            champLvlsTeam1 = 0, champLvlsTeam2 = 0,
            minionKillsTeam1 = 0, minionKillsTeam2 = 0,
            minLeagueTeam1 = "MASTER", minLeagueTeam2 = "GOLD",
            maxLeagueTeam1 = "MASTER", maxLeagueTeam2 = "GOLD",
            1
          ))
      }
      "update the tower kills based on position" in {
        val events = Seq(Event("BUILDING_KILL", 0, 0, Seq.empty, "", "", Position(0, 0)))
        MatchDetail(parts, Timeline(Seq(Frame(events, Map.empty, 12L))), 1).toExamples shouldBe
          Seq(zeroExample, Example(
            gameTimeMS = 12L,
            championIds = Seq(12, 13),
            baronKillsTeam1 = 0, baronKillsTeam2 = 0,
            dragonKillsTeam1 = 0, dragonKillsTeam2 = 0,
            towerKillsTeam1 = 1, towerKillsTeam2 = 0,
            goldTeam1 = 0, goldTeam2 = 0,
            killsTeam1 = 0, killsTeam2 = 0,
            deathsTeam1 = 0, deathsTeam2 = 0,
            assistsTeam1 = 0, assistsTeam2 = 0,
            champLvlsTeam1 = 0, champLvlsTeam2 = 0,
            minionKillsTeam1 = 0, minionKillsTeam2 = 0,
            minLeagueTeam1 = "MASTER", minLeagueTeam2 = "GOLD",
            maxLeagueTeam1 = "MASTER", maxLeagueTeam2 = "GOLD",
            1
          ))
      }
      "update the dragon kills" in {
        val events = Seq(Event("ELITE_MONSTER_KILL", 2, 0, Seq.empty, "DRAGON", "", Position(0, 0)))
        MatchDetail(parts, Timeline(Seq(Frame(events, Map.empty, 12L))), 1).toExamples shouldBe
          Seq(zeroExample, Example(
            gameTimeMS = 12L,
            championIds = Seq(12, 13),
            baronKillsTeam1 = 0, baronKillsTeam2 = 0,
            dragonKillsTeam1 = 1, dragonKillsTeam2 = 0,
            towerKillsTeam1 = 0, towerKillsTeam2 = 0,
            goldTeam1 = 0, goldTeam2 = 0,
            killsTeam1 = 0, killsTeam2 = 0,
            deathsTeam1 = 0, deathsTeam2 = 0,
            assistsTeam1 = 0, assistsTeam2 = 0,
            champLvlsTeam1 = 0, champLvlsTeam2 = 0,
            minionKillsTeam1 = 0, minionKillsTeam2 = 0,
            minLeagueTeam1 = "MASTER", minLeagueTeam2 = "GOLD",
            maxLeagueTeam1 = "MASTER", maxLeagueTeam2 = "GOLD",
            1
          ))
      }
      "update the nashor kills" in {
        val events = Seq(Event("ELITE_MONSTER_KILL", 2, 0, Seq.empty, "BARON_NASHOR", "",
          Position(0, 0)))
        MatchDetail(parts, Timeline(Seq(Frame(events, Map.empty, 12L))), 1).toExamples shouldBe
          Seq(zeroExample, Example(
            gameTimeMS = 12L,
            championIds = Seq(12, 13),
            baronKillsTeam1 = 1, baronKillsTeam2 = 0,
            dragonKillsTeam1 = 0, dragonKillsTeam2 = 0,
            towerKillsTeam1 = 0, towerKillsTeam2 = 0,
            goldTeam1 = 0, goldTeam2 = 0,
            killsTeam1 = 0, killsTeam2 = 0,
            deathsTeam1 = 0, deathsTeam2 = 0,
            assistsTeam1 = 0, assistsTeam2 = 0,
            champLvlsTeam1 = 0, champLvlsTeam2 = 0,
            minionKillsTeam1 = 0, minionKillsTeam2 = 0,
            minLeagueTeam1 = "MASTER", minLeagueTeam2 = "GOLD",
            maxLeagueTeam1 = "MASTER", maxLeagueTeam2 = "GOLD",
            1
          ))
      }
      "update gold, champ levels and minions kills" in {
        val pfs = Map("1" -> ParticipantFrame(1, 13, 1, 12), "3" -> ParticipantFrame(3, 14, 1, 13))
        MatchDetail(parts, Timeline(Seq(Frame(Seq.empty, pfs, 12L))), 1).toExamples shouldBe
          Seq(zeroExample, Example(
            gameTimeMS = 12L,
            championIds = Seq(12, 13),
            baronKillsTeam1 = 0, baronKillsTeam2 = 0,
            dragonKillsTeam1 = 0, dragonKillsTeam2 = 0,
            towerKillsTeam1 = 0, towerKillsTeam2 = 0,
            goldTeam1 = 27, goldTeam2 = 0,
            killsTeam1 = 0, killsTeam2 = 0,
            deathsTeam1 = 0, deathsTeam2 = 0,
            assistsTeam1 = 0, assistsTeam2 = 0,
            champLvlsTeam1 = 2, champLvlsTeam2 = 0,
            minionKillsTeam1 = 25, minionKillsTeam2 = 0,
            minLeagueTeam1 = "MASTER", minLeagueTeam2 = "GOLD",
            maxLeagueTeam1 = "MASTER", maxLeagueTeam2 = "GOLD",
            1
          ))
      }
    }
  }
}
