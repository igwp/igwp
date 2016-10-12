var express = require('express');
var app = express();
var bodyParser = require('body-parser');

var kafka = require('no-kafka');
var Producer = kafka.Producer;
var SimpleConsumer = kafka.SimpleConsumer;

var Promise = require('promise')
var uuid = require('node-uuid')

var port = 3000;

function PredictionClient() {
    this.producer = new Producer()
    this.consumer = new SimpleConsumer()

    this.promises = {}

    var ths = this

    this.producer.init()
        .then(function() {
            ths.consumer.init()
                .then(function() {
                    ths.consumer.subscribe('prediction', [0], function(messageSet, topic, partition) {
                        messageSet.forEach(function(m) {
                            var data = m.message.value
                            console.log('received from kafka: ' + data)
                            
                            var message = JSON.parse(data)
                            if(message.hasOwnProperty('id')) {
                                var id = message.id
                                if(id in ths.promises) {
                                    delete message.id
                                    ths.promises[id](message)
                                    delete ths.promises[id]
                                }
                                else
                                {
                                    console.log('we didnt request this, ignoring...')
                                }
                            }
                            else
                            {
                                console.log('got message from kafka without ID!!')
                            }
                        })
                    })
                })
        })

    this.getPrediction = function(data) {
        return new Promise(function(fulfill, reject) {
            var id = uuid.v4()
            data.id = id
            ths.promises[data.id] = fulfill

            var payload = JSON.stringify(data)

            console.log('sending to kafka: ' + payload)
            ths.producer.send({
                topic: 'game-state',
                partition: 0,
                message: {
                    value: payload
                }
            })
        })
    }
}

var predictionClient = new PredictionClient()

app.use(bodyParser.json());

app.post('/getmodel', function (req, res) {
    predictionClient.getPrediction(req.body)
        .then(function(prediction) {
            console.log('sending prediction to client: ' + JSON.stringify(prediction))
            res.json(prediction)
        })
});

app.listen(port, function () {
  console.log('running on port ' + port);
});

