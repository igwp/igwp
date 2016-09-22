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
      val zeroExample = Example(
        0L,
        participants.map(_.championId),
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        Seq.fill(10)(0),
        Seq.fill(10)(0),
        Seq.fill(10)(0),
        Seq.fill(10)(1),
        Seq.fill(10)(0),
        participants.map(_.highestAchievedSeasonTier),
        winner
      )
      timeline.frames.scanLeft(zeroExample) { (ex, f) =>
        val kills = f.events.filter(evt => evt.eventType == "CHAMPION_KILL" && evt.killerId != 0)
        val exWithKills = ex.copy(
          kills = kills.map(_.killerId)
            .foldLeft(ex.kills)((acc, i) => acc.updated(i - 1, acc(i - 1) + 1)),
          deaths = kills.map(_.victimId)
            .foldLeft(ex.deaths)((acc, i) => acc.updated(i - 1, acc(i - 1) + 1)),
          assists = kills.flatMap(_.assistingParticipantIds)
            .foldLeft(ex.assists)((acc, i) => acc.updated(i - 1, acc(i - 1) + 1))
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
        exWithBaronKills.copy(
          gameTimeMS = f.gameTimeMS,
          goldTeam1 = f.participantFrames.filter(_._2.participantId <= 5).map(_._2.totalGold).sum,
          goldTeam2 = f.participantFrames.filter(_._2.participantId > 5).map(_._2.totalGold).sum,
          champLevels = sortedPFs.map(_._2.level),
          minionKills = sortedPFs.map(_._2.minionsKilled)
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
    kills: Seq[Int],
    deaths: Seq[Int],
    assists: Seq[Int],
    champLevels: Seq[Int],
    minionKills: Seq[Int],
    leagues: Seq[String],
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
          goldTeam2
        ) ++
        kills ++
        deaths ++
        assists ++
        champLevels ++
        minionKills ++
        leagues ++
        Seq(winner)
      ).mkString(",")
  }
}
