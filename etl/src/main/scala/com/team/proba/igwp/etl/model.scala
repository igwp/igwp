package com.team.proba.igwp.etl

import io.circe.Decoder

object model {
  case class ParticipantFrame(
    participantId: Int,
    currentGold: Int,
    totalGold:	Int,
    level: Int,
    minionsKilled: Int
  )
  object ParticipantFrame {
    implicit val decode: Decoder[ParticipantFrame] = Decoder.instance { c =>
      for {
        pid <- c.downField("participantId").as[Int]
        cg <- c.downField("currentGold").as[Int]
        tg <- c.downField("totalGold").as[Int]
        l <- c.downField("level").as[Int]
        mk <- c.downField("minionsKilled").as[Int]
      } yield ParticipantFrame(pid, cg, tg, l, mk)
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
    implicit val decode: Decoder[Event] = Decoder.instance { c =>
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
        es <- c.downField("events").as[Seq[Event]]
        pfs <- c.downField("participantFrames").as[Map[String, ParticipantFrame]]
        gt <- c.downField("timestamp").as[Long]
      } yield Frame(es, pfs, gt)
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
        participants.map(_.championId).toArray,
        0,
        0,
        0,
        0,
        0,
        0,
        Array.fill(10)(0),
        Array.fill(10)(0),
        Array.fill(10)(0),
        Array.fill(10)(1),
        Array.fill(10)(500),
        Array.fill(10)(0),
        Array.fill(10)(0),
        participants.map(_.highestAchievedSeasonTier).toArray,
        winner
      )
      timeline.frames.scanLeft(zeroExample) { (ex, f) =>
        val kills = f.events.filter(evt => evt.eventType == "CHAMPION_KILL" && evt.killerId != 0)
        kills.foreach { evt =>
          ex.kills.update(evt.killerId - 1, ex.kills(evt.killerId - 1) + 1)
          ex.deaths.update(evt.victimId - 1, ex.deaths(evt.victimId - 1) + 1)
          evt.assistingParticipantIds.foreach(id => ex.assists.update(id - 1, ex.assists(id - 1) + 1))
        }

        val towerKills = f.events.filter(_.eventType == "BUILDING_KILL")
        val towersKillsPerTeam = towerKills.map { evt =>
          if (evt.killerId != 0) {
            if (evt.killerId <= 5) (1, 0)
            else (0, 1)
          } else {
            val side = (botRight.x - topLeft.x) * (evt.position.y - topLeft.y) -
              (evt.position.x - topLeft.x) * (botRight.y - topLeft.y)
            if (side > 0) (1, 0)
            else (0, 1)
          }
        }.foldLeft((0, 0))((s, e) => (s._1 + e._1, s._2 + e._2))
        val exWithTowerKills = ex.copy(towerKillsTeam1 = ex.towerKillsTeam1 + towersKillsPerTeam._1,
          towerKillsTeam2 = ex.towerKillsTeam2 + towersKillsPerTeam._2)

        val monsterKills = f.events.filter(_.eventType == "ELITE_MONSTER_KILL")
        val dragonKillsPerTeam = monsterKills.filter(_.monsterType == "DRAGON").map { evt =>
          if (evt.killerId <= 5) (1, 0)
          else (0, 1)
        }.foldLeft((0, 0))((s, e) => (s._1 + e._1, s._2 + e._2))
        val exWithDragonKills = exWithTowerKills.copy(
          dragonKillsTeam1 = exWithTowerKills.dragonKillsTeam1 + dragonKillsPerTeam._1,
          dragonKillsTeam2 = exWithTowerKills.dragonKillsTeam2 + dragonKillsPerTeam._2)
        val baronKillsPerTeam = monsterKills.filter(_.monsterType == "DRAGON").map { evt =>
          if (evt.killerId <= 5) (1, 0)
          else (0, 1)
        }.foldLeft((0, 0))((s, e) => (s._1 + e._1, s._2 + e._2))
        exWithDragonKills.copy(
          baronKillsTeam1 = exWithDragonKills.dragonKillsTeam1 + baronKillsPerTeam._1,
          baronKillsTeam2 = exWithDragonKills.dragonKillsTeam2 + baronKillsPerTeam._2)
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
    championIds: Array[Int],
    baronKillsTeam1: Int,
    baronKillsTeam2: Int,
    dragonKillsTeam1: Int,
    dragonKillsTeam2: Int,
    towerKillsTeam1: Int,
    towerKillsTeam2: Int,
    kills: Array[Int],
    deaths: Array[Int],
    assists: Array[Int],
    champLevels: Array[Int],
    currentGold: Array[Int],
    spentGold: Array[Int],
    minionKills: Array[Int],
    leagues: Array[String],
    winner: Int
  )
}
