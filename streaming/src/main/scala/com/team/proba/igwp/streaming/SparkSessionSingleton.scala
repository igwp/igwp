package com.team.proba.igwp.streaming

import org.apache.spark.SparkConf
import org.apache.spark.sql.SparkSession

/** Lazily instantiated singleton instance of SparkSession */
object SparkSessionSingleton {
  @transient private var instance: SparkSession = _

  def getInstance(sparkConf: SparkConf): SparkSession = {
    if (instance == null) {
      instance = SparkSession
        .builder
        .config(sparkConf)
        .getOrCreate()
    }
    instance
  }
}
