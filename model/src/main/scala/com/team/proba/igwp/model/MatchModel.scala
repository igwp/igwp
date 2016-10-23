package com.team.proba.igwp.model

import org.apache.spark.ml.{PipelineModel, Pipeline}
import org.apache.spark.ml.classification.{RandomForestClassificationModel, RandomForestClassifier}
import org.apache.spark.ml.evaluation.MulticlassClassificationEvaluator
import org.apache.spark.ml.feature.{VectorAssembler, StringIndexer}
import org.apache.spark.ml.tuning.{CrossValidator, ParamGridBuilder}
import org.apache.spark.sql.SparkSession

object MatchModel {

  def main(args: Array[String]): Unit = {
    if (args.length < 1) {
      System.err.println("Usage: MatchModel <train file>")
      System.exit(1)
    }

    val spark = SparkSession.builder().appName("match-model").getOrCreate()
    import spark.implicits._

    val examples = spark
      .read
      .format("csv")
      .option("header", "true")
      .option("inferSchema", "true")
      .load(args(0))

    val categoricalCols =
      Seq("minLeagueTeam1", "minLeagueTeam2", "maxLeagueTeam1", "maxLeagueTeam2")
    val idxdCategoricalCols = categoricalCols.map(_ + "Indexed")
    val indexers = categoricalCols
      .map(colName => new StringIndexer()
        .setInputCol(colName)
        .setOutputCol(colName + "Indexed")
      )

    val numericalCols = Seq("gameTimeMS",
      "baronKillsTeam1", "baronKillsTeam2", "dragonKillsTeam1", "dragonKillsTeam2",
      "towerKillsTeam1", "towerKillsTeam2", "goldTeam1", "goldTeam2",
      "killsTeam1", "killsTeam2", "deathsTeam1", "deathsTeam2", "assistsTeam1", "assistsTeam2",
      "champLvlsTeam1", "champLvlsTeam2", "minionKillsTeam1", "minionKillsTeam2")
    val featuresCol = "features"
    val assembler = new VectorAssembler()
      .setInputCols((idxdCategoricalCols ++ numericalCols).toArray)
      .setOutputCol(featuresCol)

    val labelCol = "winner"
    val idxdLabelCol = labelCol + "Indexed"
    val labelIndexer = new StringIndexer()
      .setInputCol(labelCol)
      .setOutputCol(idxdLabelCol)

    val randomForest = new RandomForestClassifier()
      .setLabelCol(idxdLabelCol)
      .setFeaturesCol(featuresCol)
      .setNumTrees(22)

    val pipeline = new Pipeline()
      .setStages(Array(indexers: _*) ++ Array(labelIndexer, assembler, randomForest))

    val evaluator = new MulticlassClassificationEvaluator()
      .setLabelCol(idxdLabelCol)
      .setMetricName("accuracy")

    val paramGrid = new ParamGridBuilder()
      .addGrid(randomForest.maxDepth, Array(16))
      .build()

    val cv = new CrossValidator()
      .setEstimator(pipeline)
      .setEvaluator(evaluator)
      .setEstimatorParamMaps(paramGrid)
      .setNumFolds(3)

    val Array(trainingData, testData) = examples.randomSplit(Array(0.7d, 0.3d))

    val cvModel = cv.fit(trainingData)

    val predictions = cvModel.transform(testData)
    val accuracy = evaluator.evaluate(predictions)
    println(s"Accuracy: $accuracy")

    val featureImportances = cvModel
      .bestModel.asInstanceOf[PipelineModel]
      .stages(categoricalCols.size + 2)
      .asInstanceOf[RandomForestClassificationModel].featureImportances
    assembler.getInputCols
      .zip(featureImportances.toArray)
      .sortBy(-_._2)
      .foreach { case (feat, imp) => println(s"feature: $feat, importance: $imp") }

    val bestEstimatorParamMap = cvModel.getEstimatorParamMaps
      .zip(cvModel.avgMetrics)
      .maxBy(_._2)
      ._1
    println(s"Best params:\n$bestEstimatorParamMap")

    cvModel.save("src/main/resources/model")

    spark.stop()
  }
}
