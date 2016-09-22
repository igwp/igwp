var express = require('express');
var app = express();

var port = 3000;

app.post('/getmodel', function (req, res) {
  res.json({probability: 0.5});
});

app.listen(port, function () {
  console.log('running on port ' + port);
});
