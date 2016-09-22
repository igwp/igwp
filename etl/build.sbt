lazy val circeVersion = "0.5.1"
lazy val utilVersion = "6.37.0"
lazy val configVersion = "1.3.0"
lazy val scalatestVersion = "3.0.0"

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

lazy val etl = project.in(file("."))
  .settings(
    organization := "com.team.proba",
    version := "0.1",
    scalaVersion := "2.11.8",
    name := "etl",
    libraryDependencies ++= Seq(
      "io.circe" %% "circe-core",
      "io.circe" %% "circe-generic",
      "io.circe" %% "circe-parser"
    ).map(_ % circeVersion) ++ Seq(
      "com.twitter" %% "util-core" % utilVersion,
      "com.typesafe" % "config" % configVersion,
      "org.scalatest" %% "scalatest" % scalatestVersion % "test"
    ),
    scalacOptions ++= compilerOptions
  )
