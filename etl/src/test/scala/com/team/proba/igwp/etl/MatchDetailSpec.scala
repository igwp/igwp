package com.team.proba.igwp.etl

import com.team.proba.igwp.etl.model._
import org.scalatest.{WordSpec, Matchers}

class MatchDetailSpec extends WordSpec with Matchers {
  "a MatchDetail" when {
    "converting to examples" should {
      val zeroExample =
        Example(
          0L,
          Seq.empty,
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
          Seq.empty,
          1
        )
      "give back the zero example if there is no timeline" in {
        MatchDetail(Seq.empty, Timeline(Seq.empty), 1).toExamples shouldBe
          Seq(zeroExample)
      }
      "update the kdas" in {
        val events = Seq(Event("CHAMPION_KILL", 2, 6, Seq(3, 5), "", "", Position(0, 0)))
        MatchDetail(Seq.empty, Timeline(Seq(Frame(events, Map.empty, 12L))), 1).toExamples shouldBe
          Seq(zeroExample, Example(
            12L,
            Seq.empty,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            Seq.fill(10)(0).updated(1, 1),
            Seq.fill(10)(0).updated(5, 1),
            Seq.fill(10)(0).updated(2, 1).updated(4, 1),
            Seq.empty,
            Seq.empty,
            Seq.empty,
            1
          ))
      }
      "update the tower kills based on killer" in {
        val events = Seq(Event("BUILDING_KILL", 2, 0, Seq.empty, "", "", Position(0, 0)))
        MatchDetail(Seq.empty, Timeline(Seq(Frame(events, Map.empty, 12L))), 1).toExamples shouldBe
          Seq(zeroExample, Example(
            12L,
            Seq.empty,
            0,
            0,
            0,
            0,
            1,
            0,
            0,
            0,
            Seq.fill(10)(0),
            Seq.fill(10)(0),
            Seq.fill(10)(0),
            Seq.empty,
            Seq.empty,
            Seq.empty,
            1
          ))
      }
      "update the tower kills based on position" in {
        val events = Seq(Event("BUILDING_KILL", 0, 0, Seq.empty, "", "", Position(0, 0)))
        MatchDetail(Seq.empty, Timeline(Seq(Frame(events, Map.empty, 12L))), 1).toExamples shouldBe
          Seq(zeroExample, Example(
            12L,
            Seq.empty,
            0,
            0,
            0,
            0,
            1,
            0,
            0,
            0,
            Seq.fill(10)(0),
            Seq.fill(10)(0),
            Seq.fill(10)(0),
            Seq.empty,
            Seq.empty,
            Seq.empty,
            1
          ))
      }
      "update the dragon kills" in {
        val events = Seq(Event("ELITE_MONSTER_KILL", 2, 0, Seq.empty, "DRAGON", "", Position(0, 0)))
        MatchDetail(Seq.empty, Timeline(Seq(Frame(events, Map.empty, 12L))), 1).toExamples shouldBe
          Seq(zeroExample, Example(
            12L,
            Seq.empty,
            0,
            0,
            1,
            0,
            0,
            0,
            0,
            0,
            Seq.fill(10)(0),
            Seq.fill(10)(0),
            Seq.fill(10)(0),
            Seq.empty,
            Seq.empty,
            Seq.empty,
            1
          ))
      }
      "update the nashor kills" in {
        val events = Seq(Event("ELITE_MONSTER_KILL", 2, 0, Seq.empty, "BARON_NASHOR", "",
          Position(0, 0)))
        MatchDetail(Seq.empty, Timeline(Seq(Frame(events, Map.empty, 12L))), 1).toExamples shouldBe
          Seq(zeroExample, Example(
            12L,
            Seq.empty,
            1,
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
            Seq.empty,
            Seq.empty,
            Seq.empty,
            1
          ))
      }
      "update gold, champ levels and minions kills" in {
        val pfs = Map(1 -> ParticipantFrame(1, 13, 1, 0), 3 -> ParticipantFrame(3, 14, 1, 0))
        MatchDetail(Seq.empty, Timeline(Seq(Frame(Seq.empty, pfs, 12L))), 1).toExamples shouldBe
          Seq(zeroExample, Example(
            12L,
            Seq.empty,
            0,
            0,
            0,
            0,
            0,
            0,
            27,
            0,
            Seq.fill(10)(0),
            Seq.fill(10)(0),
            Seq.fill(10)(0),
            Seq(1, 1),
            Seq(0, 0),
            Seq.empty,
            1
          ))
      }
    }
  }
}
