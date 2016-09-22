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

  case class Event(
    eventType: String,
    killerId: Int,
    victimId: Int,
    assistingParticipantIds: Seq[Int],
    monsterType: String,
    buildingType: String
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
      } yield Event(et, kid.getOrElse(0), vid.getOrElse(0), aid.getOrElse(Seq.empty),
        mt.getOrElse(""), bt.getOrElse(""))
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
}
