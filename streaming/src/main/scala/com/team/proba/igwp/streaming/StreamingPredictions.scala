package com.team.proba.igwp.streaming

import java.util.Properties

import com.github.benfradet.spark.kafka010.writer._
import com.team.proba.igwp.streaming.model.{Probability, Example}
import io.circe.parser.decode
import io.circe.generic.auto._
import io.circe.syntax._
import org.apache.kafka.clients.producer.ProducerRecord
import org.apache.kafka.common.serialization.{StringSerializer, StringDeserializer}
import org.apache.log4j.{Level, Logger}
import org.apache.spark.SparkConf
import org.apache.spark.ml.linalg.DenseVector
import org.apache.spark.ml.tuning.CrossValidatorModel
import org.apache.spark.sql.SparkSession
import org.apache.spark.streaming.kafka010.{KafkaUtils, LocationStrategies, ConsumerStrategies}
import org.apache.spark.streaming.{StreamingContext, Seconds}

object StreamingPredictions {

  def main(args: Array[String]): Unit = {
    val conf = new SparkConf().setAppName("streaming-predictions")
    val ssc = new StreamingContext(conf, Seconds(1))

    Logger.getRootLogger.setLevel(Level.WARN)

    val inputTopic = "game-state"
    val kafkaParams = Map(
      "bootstrap.servers" -> "localhost:9092",
      "auto.offset.reset" -> "earliest",
      "key.deserializer" -> classOf[StringDeserializer],
      "value.deserializer" -> classOf[StringDeserializer],
      "group.id" -> "spark-consumer"
    )

    val outputTopic = "prediction"
    val producerConfig = {
      val p = new Properties()
      p.setProperty("bootstrap.servers", "localhost:9092")
      p.setProperty("key.serializer", classOf[StringSerializer].getName)
      p.setProperty("value.serializer", classOf[StringSerializer].getName)
      p
    }

    KafkaUtils.createDirectStream[String, String](
      ssc,
      LocationStrategies.PreferConsistent,
      ConsumerStrategies.Subscribe[String, String](Set(inputTopic), kafkaParams)
    ).map(m => decode[Example](m.value()).toOption)
      .filter(_.isDefined)
      .map(_.get)
      .transform { examplesRDD =>
        val spark = SparkSession.builder().config(examplesRDD.sparkContext.getConf).getOrCreate()
        import spark.implicits._

        val examplesDF = examplesRDD.toDF()
        val model = CrossValidatorModel
          .load("/home/ec2-user/builtModels/model")
        val probability = model.transform(examplesDF).select("probability")
        probability.rdd.zip(examplesRDD.map(_.id)).map { case (v, id) =>
          val vec = v.getAs[DenseVector]("probability")
          Probability(id, vec(0), vec(1)).asJson.noSpaces
        }
      }
      .writeToKafka(producerConfig, s => new ProducerRecord[String, String](outputTopic, s))

    ssc.start()
    ssc.awaitTermination()
  }
}
