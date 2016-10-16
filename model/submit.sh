#!/bin/bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

sbt clean assembly

cd ${DIR}

RESOURCES_PATH="src/main/resources/"
TRAIN_FILE="matches.csv"
TRAIN_PATH="${RESOURCES_PATH}${TRAIN_FILE}"

spark-submit \
  --class com.team.proba.igwp.model.MatchModel \
  --master local[2] \
  --driver-memory 2G \
  --executor-memory 4G \
  target/scala-2.11/model-assembly-0.1.jar \
  ${TRAIN_PATH}
