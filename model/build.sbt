lazy val sparkVersion = "2.0.0"

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
    name := "model",
    libraryDependencies ++= Seq(
      "org.apache.spark" %% "spark-core",
      "org.apache.spark" %% "spark-mllib",
      "org.apache.spark" %% "spark-sql"
    ).map(_ % sparkVersion % "provided"),
    scalacOptions ++= compilerOptions
  )
