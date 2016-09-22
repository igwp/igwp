var express = require('express');
var app = express();
var bodyParser = require('body-parser');

var port = 3000;

app.use(bodyParser.json());

app.post('/getmodel', function (req, res) {
  console.log(req.body);
  var kill1 = parseFloat(req.body["KillBlue"]);
  var kill2 = parseFloat(req.body["KillRed"]);
  console.log(kill1 + kill2);
  var prob = 0.5;
  if (kill1 + kill2 > 5) {
    prob = kill1 / (kill1 + kill2);
  }
  res.json({ probability: prob });
});

app.listen(port, function () {
  console.log('running on port ' + port);
});
