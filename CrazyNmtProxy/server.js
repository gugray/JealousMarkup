var express = require("express");
var bodyParser = require("body-parser");

var app = express();
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));
app.use(express.static("public"));
var proxy = require("./proxy.js")(app);

var server = app.listen(3050);
console.log("CrazyNMT proxy is listening on port 3050.");
