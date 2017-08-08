var fs = require('fs');
var request = require('request');
var snl = require("simple-node-logger");

var proxy = function (app) {
  "use strict";

  var appLog = snl.createSimpleFileLogger({
    timestampFormat: "YYYY-MM-DD HH:mm:ss",
    logFilePath: __dirname + "/log/query.log"
  });

  function doLog(src, real, crazy) {
    try {
      src = "" + src;
      real = "" + real;
      crazy = "" + crazy;
      src = src.replace(/\t/g, " ");
      real = real.replace(/\t/g, " ");
      crazy = crazy.replace(/\t/g, " ");
      appLog.info(src + "\t" + real + "\t" + crazy);
    }
    catch (ex) { }
  }

  var reqCount = 0;

  function doForward(src) {
    return new Promise((resolve, reject) => {
      if (reqCount > 10) return resolve({ status: "busy"});
      ++reqCount;
      // Send two translation request
      // Fulfill promise when both have succeeded
      // Reject promise when first fails
      var reqData = [{ src: src }];
      var xReal = null;
      var xRealSec = null;
      var xCrazy = null;
      var xCrazySec = null;
      var returnedErr = false;
      var dtStart = new Date();
      var reqOpt1 = {
        uri: 'http://127.0.0.1:7784/translator/translate',
        method: 'POST',
        headers: { "content-type": "application/json" },
        json: reqData
      };
      request(reqOpt1, function (err, res, body) {
        try {
          if (xCrazy) --reqCount;
          if (err && !returnedErr) {
            returnedErr = true;
            return reject("translate request failed (real)");
          }
          xReal = body[0][0];
          xRealSec = (new Date() - dtStart) / 1000;
          if (xCrazy) {
            doLog(src, xReal, xCrazy);
            return resolve({
              status: "ok",
              real: xReal,
              real_sec: xRealSec,
              crazy: xCrazy,
              crazy_sec: xCrazySec
            });
          }
        }
        catch (ex) { console.log("err"); return reject(ex); }
      });
      var reqOpt2 = {
        uri: 'http://127.0.0.1:7785/translator/translate',
        method: 'POST',
        headers: { "content-type": "application/json" },
        json: reqData
      };
      request(reqOpt2, function (err, res, body) {
        try {
          if (xReal) --reqCount;
          if (err && !returnedErr) {
            returnedErr = true;
            return reject("translate request failed (crazy)");
          }
          xCrazy = body[0][0];
          xCrazySec = (new Date() - dtStart) / 1000;
          if (xReal) {
            doLog(src, xReal.tgt, xCrazy.tgt);
            return resolve({
              status: "ok",
              real: xReal,
              real_sec: xRealSec,
              crazy: xCrazy,
              crazy_sec: xCrazySec
            });
          }
        }
        catch (ex) { console.log("err"); return reject(ex); }
      });
    });
  }

  const allowedOrigins = ["https://www.jealousmarkup.xyz", "https://jealousmarkup.xyz"];

  app.post("/xlate", function (req, res) {
    // Origin filtering
    var origin = req.headers.origin;
    if(allowedOrigins.indexOf(origin) == -1)
      return res.status(400).send("origin not allowed");
    res.setHeader("Access-Control-Allow-Origin", origin);

    // Verify request
    var body = req.body;
    if (!body.src) return res.status(400).send("invalid request");
    var src = "" + body.src;
    if (src.length > 256) return res.status(400).send("invalid request");

    // Forward request - promise
    doForward(src).then(
      (result) => {
        res.setHeader('Content-Type', 'application/json');
        var strRes = JSON.stringify(result);
        res.send(strRes);
      },
      (err) => { res.status(500).send("translation server error"); console.log("err"); }
    );
  });
}

module.exports = proxy;