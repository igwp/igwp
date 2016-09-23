var express = require('express');
var app = express();
var bodyParser = require('body-parser');

var kafka = require('kafka-node');
var Producer = kafka.Producer;
var Consumer = kafka.Consumer;
var client = new kafka.Client('54.183.147.234:2181/');

var port = 3000;

app.use(bodyParser.json());

app.post('/getmodel', function (req, res) {
  console.log(req.body);
  var producer = new Producer(client);
  var consumer = new Consumer(client, [{ topic: 'prediction' }], { autoCommit: false });

  var payloads =[
    {
      topic: 'game-state', messages: req.body
    }
  ]

  consumer.on('message', function(msg) {
    consumer.close();
    res.send({ probability: msg });
  });

  producer.on('ready', function () {
    producer.send(payloads, function(err, data) {
      if (err) console.log(err);
      else console.log('Sent data successfully.');
      producer.close();
    });
  });
});

app.listen(port, function () {
  console.log('running on port ' + port);
});
