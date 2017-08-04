/// <reference path="../_lib/jquery-3.1.1.min.js" />
/// <reference path="../_lib/chart-2.1.6.min.js" />

var JM = JM || {};

JM.nmt = (function () {
  "use strict";

  $(document).ready(function () {
    $(".nmtHeader .button").click(function () {
      if ($(this).hasClass("disabled")) return;

      var dtStart = new Date();
      var request = $.ajax({
        url: "https://mtserv.kginno.tk/xlate",
        type: "POST",
        contentType: "application/x-www-form-urlencoded; charset=UTF-8",
        data: { src: $("#txtInput").val() }
      });
      $(".nmtHeader .button").addClass("disabled");
      $(".nmtHeader i").addClass("visible");
      setTargetMeta();
      request.done(function (data) {
        $(".nmtHeader .button").removeClass("disabled");
        $(".nmtHeader i").removeClass("visible");
        if (data.status == "ok") {
          $("#xlStraight").text(data.real.tgt);
          $("#xlStraight").removeClass("meta");
          //var elapsed = (new Date() - dtStart) / 1000;
          var diagStr = data.real_sec.toFixed(2) + "s; " + data.real.pred_score.toFixed(2);
          $("#xlStraight").append("<div class='xlstats'>" + diagStr + "</div>");
          $("#xlCrazy").text(data.crazy.tgt);
          $("#xlCrazy").removeClass("meta");
          diagStr = data.crazy_sec.toFixed(2) + "s; " + data.crazy.pred_score.toFixed(2);
          $("#xlCrazy").append("<div class='xlstats'>" + diagStr + "</div>");
        }
        else {
          setTargetMeta("** The server is overloaded. Please try again in a bit. **");
        }
      });
      request.fail(function (xhr, status, error) {
        $(".nmtHeader .button").removeClass("disabled");
        $(".nmtHeader i").removeClass("visible");
        setTargetMeta("** Sorry; the service seems to be unavailable right now. **");
      });
    });
  });

  function setTargetMeta(msg) {
    $(".nmtBlock .xl").addClass("meta");
    $("#xlStraight").text(msg ? msg : "Regular translation");
    $("#xlCrazy").text(msg ? msg : "Translation from garbled model");
  }


})();
