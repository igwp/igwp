package com.team.proba.igwp.etl

import io.circe.Decoder
import io.circe.generic.auto._

object model {
  case class ParticipantFrame(
    participantId: Int,
    totalGold:	Int,
    level: Int,
    minionsKilled: Int
  )
  object ParticipantFrame {
    implicit val decode: Decoder[ParticipantFrame] = Decoder.instance { c =>
      for {
        pid <- c.downField("participantId").as[Int]
        tg <- c.downField("totalGold").as[Int]
        l <- c.downField("level").as[Int]
        mk <- c.downField("minionsKilled").as[Int]
      } yield ParticipantFrame(pid, tg, l, mk)
    }
  }

  case class Position(x: Int, y: Int)
  case class Event(
    eventType: String,
    killerId: Int,
    victimId: Int,
    assistingParticipantIds: Seq[Int],
    monsterType: String,
    buildingType: String,
    position: Position
  )
  object Event {
    implicit val decodeEvent: Decoder[Event] = Decoder.instance { c =>
      for {
        et <- c.downField("eventType").as[String]
        kid <- c.downField("killerId").as[Option[Int]]
        vid <- c.downField("victimId").as[Option[Int]]
        aid <- c.downField("assistingParticipantIds").as[Option[Seq[Int]]]
        mt <- c.downField("monsterType").as[Option[String]]
        bt <- c.downField("buildingType").as[Option[String]]
        p <- c.downField("position").as[Option[Position]]
      } yield Event(et, kid.getOrElse(0), vid.getOrElse(0), aid.getOrElse(Seq.empty),
        mt.getOrElse(""), bt.getOrElse(""), p.getOrElse(Position(0, 0)))
    }
  }

  case class Frame(
    events: Seq[Event],
    participantFrames: Map[String, ParticipantFrame],
    gameTimeMS: Long
  )
  object Frame {
    implicit val decode: Decoder[Frame] = Decoder.instance { c =>
      for {
        es <- c.downField("events").as[Option[Seq[Event]]]
        pfs <- c.downField("participantFrames").as[Map[String, ParticipantFrame]]
        gt <- c.downField("timestamp").as[Long]
      } yield Frame(es.getOrElse(Seq.empty), pfs, gt)
    }
  }

  case class Timeline(frames: Seq[Frame])
  object Timeline {
    implicit val decode: Decoder[Timeline] = Decoder.instance { c =>
      for {
        fs <- c.downField("frames").as[Seq[Frame]]
      } yield Timeline(fs)
    }
  }

  case class Participant(
    highestAchievedSeasonTier: String,
    championId: Int,
    participantId: Int
  )
  object Participant {
    implicit val decode: Decoder[Participant] = Decoder.instance { c =>
      for {
        hast <- c.downField("highestAchievedSeasonTier").as[String]
        cid <- c.downField("championId").as[Int]
        pid <- c.downField("participantId").as[Int]
      } yield Participant(hast, cid, pid)
    }
  }

  case class Team(
    teamId: Int,
    winner: Boolean
  )
  object Team {
    implicit val decode: Decoder[Team] = Decoder.instance { c =>
      for {
        tid <- c.downField("teamId").as[Int]
        w <- c.downField("winner").as[Boolean]
      } yield Team(tid, w)
    }
  }

  case class MatchDetail(
    participants: Seq[Participant],
    timeline: Timeline,
    winner: Int
  ) {
    def toExamples: Seq[Example] = {
      val topLeft = Position(-570, 14980)
      val botRight = Position(15220, -420)

      val tiers = List(
        "CHALLENGER", "MASTER", "DIAMOND", "PLATINUM", "GOLD", "SILVER", "BRONZE", "UNRANKED")
      val ordering = new Ordering[String] {
        override def compare(x: String, y: String): Int =
          if (tiers.contains(x) && tiers.contains(y))
            if (tiers.indexOf(x) > tiers.indexOf(y)) 1
            else 0
          else if (tiers.contains(x)) 1
          else if (tiers.contains(y)) 0
          else 1
      }
      val leaguesTeam1 = participants
        .filter(_.participantId <= 5)
        .map(_.highestAchievedSeasonTier)
        .sorted(ordering)
      val leaguesTeam2 = participants
        .filter(_.participantId > 5)
        .map(_.highestAchievedSeasonTier)
        .sorted(ordering)

      val zeroExample = Example(
        gameTimeMS = 0L,
        championIds = participants.map(_.championId),
        baronKillsTeam1 = 0, baronKillsTeam2 = 0,
        dragonKillsTeam1 = 0, dragonKillsTeam2 = 0,
        towerKillsTeam1 = 0, towerKillsTeam2 = 0,
        goldTeam1 = 2500, goldTeam2 = 2500,
        killsTeam1 = 0, killsTeam2 = 0,
        deathsTeam1 = 0, deathsTeam2 = 0,
        assistsTeam1 = 0, assistsTeam2 = 0,
        champLvlsTeam1 = 5, champLvlsTeam2 = 5,
        minionKillsTeam1 = 0, minionKillsTeam2 = 0,
        minLeagueTeam1 = leaguesTeam1.min(ordering), minLeagueTeam2 = leaguesTeam2.min(ordering),
        maxLeagueTeam1 = leaguesTeam1.max(ordering), maxLeagueTeam2 = leaguesTeam2.max(ordering),
        winner
      )

      timeline.frames.scanLeft(zeroExample) { (ex, f) =>
        val kills = f.events.filter(evt => evt.eventType == "CHAMPION_KILL" && evt.killerId != 0)
        val exWithKills = ex.copy(
          killsTeam1 = ex.killsTeam1 + kills.count(_.killerId <= 5),
          killsTeam2 = ex.killsTeam1 + kills.count(_.killerId > 5),
          deathsTeam1 = ex.deathsTeam1 + kills.count(_.victimId <= 5),
          deathsTeam2 = ex.deathsTeam1 + kills.count(_.victimId > 5),
          assistsTeam1 = ex.assistsTeam1 +
            kills.flatMap(_.assistingParticipantIds).count(_ <= 5),
          assistsTeam2 = ex.assistsTeam2 +
            kills.flatMap(_.assistingParticipantIds).count(_ > 5)
        )

        val towerKills = f.events.filter(_.eventType == "BUILDING_KILL")
        val towersKillsPerTeam = towerKills.map { evt =>
          if (evt.killerId != 0) {
            if (evt.killerId <= 5) (1, 0)
            else (0, 1)
          } else {
            val side = (botRight.x - topLeft.x) * (evt.position.y - topLeft.y) -
              (evt.position.x - topLeft.x) * (botRight.y - topLeft.y)
            if (side < 0) (1, 0)
            else (0, 1)
          }
        }.foldLeft((0, 0))((s, e) => (s._1 + e._1, s._2 + e._2))
        val exWithTowerKills = exWithKills.copy(
          towerKillsTeam1 = exWithKills.towerKillsTeam1 + towersKillsPerTeam._1,
          towerKillsTeam2 = exWithKills.towerKillsTeam2 + towersKillsPerTeam._2)

        val monsterKills = f.events.filter(_.eventType == "ELITE_MONSTER_KILL")
        val dragonKillsPerTeam = monsterKills.filter(_.monsterType == "DRAGON").map { evt =>
          if (evt.killerId <= 5) (1, 0)
          else (0, 1)
        }.foldLeft((0, 0))((s, e) => (s._1 + e._1, s._2 + e._2))
        val exWithDragonKills = exWithTowerKills.copy(
          dragonKillsTeam1 = exWithTowerKills.dragonKillsTeam1 + dragonKillsPerTeam._1,
          dragonKillsTeam2 = exWithTowerKills.dragonKillsTeam2 + dragonKillsPerTeam._2)
        val baronKillsPerTeam = monsterKills.filter(_.monsterType == "BARON_NASHOR").map { evt =>
          if (evt.killerId <= 5) (1, 0)
          else (0, 1)
        }.foldLeft((0, 0))((s, e) => (s._1 + e._1, s._2 + e._2))
        val exWithBaronKills = exWithDragonKills.copy(
          baronKillsTeam1 = exWithDragonKills.baronKillsTeam1 + baronKillsPerTeam._1,
          baronKillsTeam2 = exWithDragonKills.baronKillsTeam2 + baronKillsPerTeam._2)

        val sortedPFs = f.participantFrames.toSeq.sortBy(_._1)
        val participantsTeam1 = sortedPFs.filter(_._2.participantId <= 5)
        val participantsTeam2 = sortedPFs.filter(_._2.participantId > 5)
        exWithBaronKills.copy(gameTimeMS = f.gameTimeMS, goldTeam1 = 12)
        exWithBaronKills.copy(
          gameTimeMS = f.gameTimeMS,
          goldTeam1 = participantsTeam1.map(_._2.totalGold).sum,
          goldTeam2 = participantsTeam2.map(_._2.totalGold).sum,
          champLvlsTeam1 = participantsTeam1.map(_._2.level).sum,
          champLvlsTeam2 = participantsTeam2.map(_._2.level).sum,
          minionKillsTeam1 = participantsTeam1.map(_._2.minionsKilled).sum,
          minionKillsTeam2 = participantsTeam2.map(_._2.minionsKilled).sum
        )
      }
    }
  }
  object MatchDetail {
    implicit val decode: Decoder[MatchDetail] = Decoder.instance { c =>
      for {
        ps <- c.downField("participants").as[Seq[Participant]]
        t <- c.downField("timeline").as[Timeline]
        ts <- c.downField("teams").as[Seq[Team]]
      } yield MatchDetail(ps, t, ts.filter(_.winner).map(_.teamId).head)
    }
  }

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
    maxLeagueTeam2: String,
    winner: Int
  ) {
    def toCsvString: String = (
      Seq(gameTimeMS) ++
        championIds ++
        Seq(
          baronKillsTeam1,
          baronKillsTeam2,
          dragonKillsTeam1,
          dragonKillsTeam2,
          towerKillsTeam1,
          towerKillsTeam2,
          goldTeam1,
          goldTeam2,
          killsTeam1,
          killsTeam2,
          deathsTeam1,
          deathsTeam2,
          assistsTeam1,
          assistsTeam2,
          champLvlsTeam1,
          champLvlsTeam2,
          minionKillsTeam1,
          minionKillsTeam2,
          minLeagueTeam1,
          minLeagueTeam2,
          maxLeagueTeam1,
          maxLeagueTeam2,
          winner)
      ).mkString(",")
  }
}
