package com.team.proba.igwp.etl

import java.io.FileWriter

import cats.data.Xor
import com.team.proba.igwp.etl.model._
import com.twitter.util.{Duration, TimerTask, JavaTimer}
import com.typesafe.config.ConfigFactory
import io.circe.parser.decode

import scala.collection.mutable
import scala.io.Source

object BuildDataset {
  def main(args: Array[String]): Unit = {
    val apiKey = ConfigFactory.load().getString("api.key")
    val urlTemplate = "https://na.api.pvp.net/api/lol/na/v2.2" +
      s"/match/%s?includeTimeline=true&api_key=$apiKey"
    val gameIds = Source.fromURL(getClass.getResource("/game-ids.txt")).getLines()
    val urlsQeueue = mutable.Queue(gameIds.map(urlTemplate.format(_)).toSeq: _*)
    require(urlsQeueue.nonEmpty, "there should be URLs to query")

    val fileWriter = new FileWriter("src/main/resources/matches.csv")
    val headerLine = "gameTimeMS," +
      "champion1Id,champion2Id,champion3Id,champion4Id,champion5Id," +
      "champion6Id,champion7Id,champion8Id,champion9Id,champion10Id," +
      "baronKillsTeam1,baronKillsTeam2,dragonKillsTeam1,dragonKillsTeam2," +
      "towerKillsTeam1,towerKillsTeam2,goldTeam1,goldTeam2," +
      "killsTeam1,killsTeam2,deathsTeam1,deathsTeam2,assistsTeam1,assistsTeam2," +
      "champLvlsTeam1,champLvlsTeam2,minionKillsTeam1,minionKillsTeam2," +
      "minLeagueTeam1,minLeagueTeam2,maxLeagueTeam1,maxLeagueTeam2,winner\n"
    fileWriter.write(headerLine)

    val timer = new JavaTimer(isDaemon = false)
    lazy val task: TimerTask = timer.schedule(Duration.fromMilliseconds(200)) {
      val url = urlsQeueue.dequeue()
      val matchDetail = decode[MatchDetail](Source.fromURL(url).mkString)
      matchDetail match {
        case Xor.Right(md) =>
          val examples = md.toExamples
          examples.foreach(ex => fileWriter.write(s"${ex.toCsvString}\n"))
        case Xor.Left(e) =>
          println(s"couldn't retrieve match detail data $e")
      }
      if (urlsQeueue.isEmpty) {
        task.cancel()
        timer.stop()
        fileWriter.close()
      }
    }
    require(task != null)
  }
}
