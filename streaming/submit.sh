#!/bin/bash

spark-submit \
  --class com.team.proba.igwp.streaming.StreamingPredictions \
  --master local[2] \
  --driver-memory 2G \
  --executor-memory 4G \
  target/scala-2.11/streaming-assembly-0.1.jar
