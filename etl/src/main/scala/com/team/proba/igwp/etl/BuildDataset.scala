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

    val timer = new JavaTimer(isDaemon = false)
    lazy val task: TimerTask = timer.schedule(Duration.fromSeconds(1)) {
      val url = urlsQeueue.dequeue()
      val matchDetail = decode[MatchDetail](Source.fromURL(url).mkString)
      matchDetail match {
        case Xor.Right(md) =>
          val examples = md.toExamples
          if (urlsQeueue.isEmpty) {
            task.cancel()
            timer.stop()
            examples.foreach(ex => fileWriter.write(s"${ex.toCsvString}\n"))
            fileWriter.close()
          } else {
            examples.foreach(ex => fileWriter.write(s"${ex.toCsvString}\n"))
          }
        case Xor.Left(e) =>
          println(s"couldn't retrieve match detail data $e")
          if (urlsQeueue.isEmpty) {
            task.cancel()
            timer.stop()
            fileWriter.close()
          }
      }
    }
    require(task != null)
  }
}
