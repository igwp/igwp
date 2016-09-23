lazy val sparkVersion = "2.0.0"
lazy val circeVersion = "0.5.1"
lazy val skrVersion = "0.2.0-SNAPSHOT"

lazy val compilerOptions = Seq(
  "-deprecation",
  "-encoding", "UTF-8",
  "-feature",
  "-language:existentials",
  "-language:higherKinds",
  "-language:implicitConversions",
  "-unchecked",
  "-Yno-adapted-args",
  "-Ywarn-dead-code",
  "-Ywarn-numeric-widen",
  "-Xfuture",
  "-Xlint"
)

lazy val model = project.in(file("."))
  .settings(
    organization := "com.team.proba",
    version := "0.1",
    scalaVersion := "2.11.8",
    name := "streaming",
    libraryDependencies ++= Seq(
      "org.apache.spark" %% "spark-core",
      "org.apache.spark" %% "spark-mllib",
      "org.apache.spark" %% "spark-sql",
      "org.apache.spark" %% "spark-streaming"
    ).map(_ % sparkVersion % "provided")  ++ Seq(
      "io.circe" %% "circe-core",
      "io.circe" %% "circe-generic",
      "io.circe" %% "circe-parser"
    ).map(_ % circeVersion) ++ Seq(
      ("org.apache.spark" %% "spark-streaming-kafka-0-10" % sparkVersion)
        .exclude("org.spark-project.spark", "unused"),
      "com.github.benfradet" %% "spark-kafka-0-10-writer" % skrVersion
    ),
    scalacOptions ++= compilerOptions
  )
