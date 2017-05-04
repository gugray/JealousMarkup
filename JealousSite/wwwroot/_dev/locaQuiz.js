/// <reference path="../_lib/jquery-3.1.1.min.js" />

"use strict";
var JM = JM || {};

JM.locaQuiz = (function () {

  var result = "";
  var score = 0;

  $(document).ready(function () {
    if ($("#localizationQuiz").length == 0) return;

    var locaQuizCount = 1;

    $(".start").click(function () {
      $(".start").addClass("hidden");
      $(".qpage[data-qix='1']").addClass("active");
      $("p.lede").css("display", "none");
      ga("send", {
        hitType: 'event',
        eventCategory: 'locaquiz',
        eventAction: 'locaquiz-start',
        eventValue: locaQuizCount
      });
    });

    $(".option").click(function () {
      var elmPage = $(this).closest(".qpage");
      if (elmPage.hasClass("solved")) return;
      elmPage.addClass("solved");
      if ($(this).hasClass("correct")) elmPage.addClass("correct");
      else elmPage.addClass("false");
      $(this).addClass("chosen");
    });

    $(".continue").click(function () {
      var elmPage = $(this).closest(".qpage");
      elmPage.removeClass("active");
      var elmChosen = elmPage.find(".option.chosen");
      var ox = elmChosen.data("ox");
      var flag = "N";
      if (elmChosen.hasClass("correct")) {
        ++score;
        flag = "Y";
      }
      var nextIx = elmPage.data("qix") + 1;
      if (nextIx > 2) result += "!";
      result += flag + "-" + ox;
      if (nextIx <= 10) {
        $(".qpage[data-qix='" + nextIx + "']").addClass("active");
      }
      else {
        $("#localizationQuiz .result").addClass("visible");
        ga("send", {
          hitType: 'event',
          eventCategory: 'locaquiz',
          eventAction: 'locaquiz-finish',
          eventValue: score + "#" + result
        });
      }
    });

  });
})();
