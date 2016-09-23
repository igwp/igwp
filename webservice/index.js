var express = require('express');
var app = express();
var bodyParser = require('body-parser');

var kafka = require('no-kafka');
var Producer = kafka.Producer;
var SimpleConsumer = kafka.SimpleConsumer;

var port = 3000;

app.use(bodyParser.json());

app.post('/getmodel', function (req, res) {
  var producer = new Producer();
  var consumer = new SimpleConsumer();
  
  consumer.init()
    .then(function () {
      return consumer.subscribe('prediction', [0], function (messageSet, topic, partition) {
        messageSet.forEach(function (m) {
          console.log('Received from kafka: ' + m);
          res.json(m.message.value.toString('utf-8'));
        });
      });
    });

  producer.init()
    .then(function () {
      var payload = JSON.stringify(req.body);
      console.log('Sending to kafka: ' + payload);
      return producer.send({
        topic: 'game-state',
        partition: 0,
        message: {
          value: payload
        }
      })
    });
});

app.listen(port, function () {
  console.log('running on port ' + port);
});
