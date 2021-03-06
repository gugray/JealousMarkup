﻿/// <reference path="../_lib/jquery-3.1.1.min.js" />
/// <reference path="../_lib/chart-2.1.6.min.js" />

var JM = JM || {};

JM.lspKeywords = (function () {
  "use strict";

  var colors = ["#f9bd4e", "#50cea8", "#a6d5e8", "#c493ff", "#e1708c", "#3eabb4"];
  var plottables = {};
  var reqCount = 0;
  var walkStep = 0;

  $(document).ready(function () {
    if ($("#lspKeyViz").length == 0) return;

    reqCount = 11;
    loadJson("/files/lsp-keywords/data-stem-6.json", "stem-6");
    loadJson("/files/lsp-keywords/data-stem-5.json", "stem-5");
    loadJson("/files/lsp-keywords/data-stem-4.json", "stem-4");
    loadJson("/files/lsp-keywords/data-stem-3.json", "stem-3");
    loadJson("/files/lsp-keywords/data-stem-2.json", "stem-2");
    loadJson("/files/lsp-keywords/data-nostem-6.json", "nostem-6");
    loadJson("/files/lsp-keywords/data-nostem-5.json", "nostem-5");
    loadJson("/files/lsp-keywords/data-nostem-4.json", "nostem-4");
    loadJson("/files/lsp-keywords/data-nostem-3.json", "nostem-3");
    loadJson("/files/lsp-keywords/data-nostem-2.json", "nostem-2");
    loadChartJS();

    $(".ctrl-minimize").click(function () {
      $("#lspKeyViz").removeClass("large");
      $("body").css("overflow-y", "scroll");
      $("#lspKeyChart canvas").css("height", "17rem");
      $(".lspKeyWalker").removeClass("visible");
    });
    $(".enlarge").click(function () {
      $("#lspKeyViz").addClass("large");
      $("body").css("overflow-y", "hidden");
      $("#lspKeyChart canvas").css("height", "100% !important");
      $(".lspKeyWalker").removeClass("visible");
    });
    $(".walkthrough").click(function () {
      if ($(".lspKeyWalker").hasClass("visible")) {
        $(".lspKeyWalker").removeClass("visible");
        return;
      }
      walkStep = 0;
      doWalk();
    });
    $(".walkCommands .next").click(function () {
      ++walkStep;
      doWalk();
    });
    $(".walkCommands .close").click(function () {
      $(".lspKeyWalker").removeClass("visible");
    });

    // DBG
    //doWalk();
  });

  function doWalk() {
    $(".lspWalkStep").removeClass("visible");
    if (walkStep == 6) {
      $(".lspKeyWalker").removeClass("visible");
      return;
    }
    var currStepClass = ".lspWalkStep.step" + walkStep;
    $(currStepClass).addClass("visible");
    $(".lspKeyWalker").addClass("visible");

    // First/last step: switch button texts etc.
    if (walkStep == 0) {
      $(".walkCommands .next").text("Next");
      $(".walkCommands .close").text("Close");
    }
    else if (walkStep == 5) {
      $(".walkCommands .next").text("Finish");
      $(".walkCommands .close").text("");
    }

    // Different positioning logic in full screen mode
    if ($("#lspKeyViz").hasClass("large")) {
      if (walkStep == 0) {
        $(".lspKeyWalker").css("top", "10rem");
        $(".lspKeyWalker").css("left", "4rem");
        $(".lspKeyWalker").css("right", "");
      }
      else if (walkStep == 1) {
        $(".lspKeyWalker").css("top", "2.5rem");
        $(".lspKeyWalker").css("left", "2rem");
      }
      else if (walkStep == 2) {
        $(".lspKeyWalker").css("top", "6rem");
        $(".lspKeyWalker").css("left", "");
        $(".lspKeyWalker").css("right", "2rem");
      }
      else if (walkStep == 5) {
        $(".lspKeyWalker").css("top", "2.5rem");
        $(".lspKeyWalker").css("left", "17rem");
        $(".lspKeyWalker").css("right", "");
      }
    }
    else {
      var offset = null;
      if (walkStep == 0) {
        var top = $("#lspKeyChart").position().top + 120;
        $(".lspKeyWalker").css("top", top);
        $(".lspKeyWalker").css("left", "");
        $(".lspKeyWalker").css("right", "");
        offset = $(".lspKeyWalker").offset();
        offset.top -= 140;
      }
      else if (walkStep == 1) {
        var top = $("#lspKeyChart").position().top + 5;
        $(".lspKeyWalker").css("top", top);
        $(".lspKeyWalker").css("left", "100px");
      }
      else if (walkStep == 2) {
        var top = $("#lspKeyLegend").position().top;
        $(".lspKeyWalker").css("top", top + 100);
        $(".lspKeyWalker").css("left", "50px");
        offset = $(".lspKeyWalker").offset();
        offset.top -= 140;
      }
      else if (walkStep == 5) {
        var top = $("#lspKeyChart").position().top + 5;
        $(".lspKeyWalker").css("top", top);
        $(".lspKeyWalker").css("left", "");
        $(".lspKeyWalker").css("right", "20px");
        offset = $(".lspKeyWalker").offset();
        offset.top -= 140;
      }
      if (offset) {
        $('html, body').animate({
          scrollTop: offset.top - 100,
          scrollLeft: offset.left
        });
      }
    }
  }

  function esc(s) {
    return s.replace(/&/g, '&amp;')
            .replace(/"/g, '&quot;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;');
  }

  function datasetsFromPlottable(pbl) {
    var res = [];
    for (var i = 0; i != pbl.length; ++i) {
      var dataset = {
        data: [],
        backgroundColor: colors[i],
        hoverBackgroundColor: colors[i]
      };
      var pblPoints = pbl[i].data;
      for (var j = 0; j != pblPoints.length; ++j) {
        var pblPoint = pblPoints[j];
        dataset.data.push({
          x: pblPoint.x,
          y: pblPoint.y,
          r: 20 - Math.round(pblPoint.rank / 3),
          tooltip: pblPoint.url
        });
      }
      res.push(dataset);
    }
    return res;
  }

  function initLegend(pbl) {
    var abc = "ABCDEF";
    var html = "";
    for (var i = 0; i != pbl.length; ++i) {
      var clust = pbl[i];
      html += "<div class='cluster" + i + " cluster'>";
      html += "<span class='label'>";
      html += "Cluster " + abc[i] + ": " + (clust.sites.length);
      html += "</span> ";
      html += "<b>" + esc(clust.words) + "</b>";
      html += "<br/><i>";
      for (var j = 0; j != clust.data.length; ++j) {
        var dpoint = clust.data[j];
        if (j != 0) html += ", ";
        html += "<u>" + (dpoint.rank + 1) + "</u>&nbsp;";
        html += esc(dpoint.url);
      }
      html += "</i>";
      html += "</div>";
    }
    $("#lspKeyLegend").html(html);
  }

  function hiliteUrl(dsix, ix) {
    $(".cluster" + dsix + " u:nth-child(" + (ix + 1) + ")").addClass("hilite");
  }

  function initChart(pbl) {
    initLegend(pbl);
    var chartData = {
      datasets: datasetsFromPlottable(pbl)
    }

    var ctxTCD = document.getElementById("lsp-keychart-canvas").getContext("2d");
    window.lspKeyChart = new Chart(ctxTCD, {
      type: 'bubble',
      data: chartData,
      options: {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          xAxes: [{
            display: false
          }],
          yAxes: [{
            display: false
          }]
        },
        legend: { display: false },
        tooltips: {
          callbacks: {
            label: function (tti, data) {
              var dataset = data.datasets[tti.datasetIndex];
              var datapoint = dataset.data[tti.index];
              return datapoint.tooltip;
            }
          }
        },
        hover: {
          onHover: function (arg) {
            if (!arg || !arg.length || arg.length == 0) $(".cluster u").removeClass("hilite");
            else hiliteUrl(arg[0]._datasetIndex, arg[0]._index);
          }
        }
      }
    });
  }

  function initCurrentChart() {
    var dataName;
    if ($("#stem").text() == "stemmed") dataName = "stem-";
    else dataName = "nostem-";
    if ($("#clust2").hasClass("selected")) dataName += "2";
    else if ($("#clust3").hasClass("selected")) dataName += "3";
    else if ($("#clust4").hasClass("selected")) dataName += "4";
    else if ($("#clust5").hasClass("selected")) dataName += "5";
    else dataName += "6";

    $("#lspKeyChart canvas").remove();
    $("#lspKeyChart").html("<canvas id='lsp-keychart-canvas'></canvas>");
    initChart(plottables[dataName]);
  }

  function dataReady() {
    initCurrentChart();
    $(".ctrl-left").click(function (evt) {
      $(".ctrl-left").removeClass("selected");
      $(this).addClass("selected");
      initCurrentChart();
      evt.preventDefault();
    });
    $(".ctrl-right").click(function (evt) {
      if ($(this).text() == "stemmed") $(this).text("surface");
      else $(this).text("stemmed");
      initCurrentChart();
      evt.preventDefault();
    });
  }

  function loadJson(url, varName) {
    var xhr = new XMLHttpRequest();
    xhr.open("GET", url, true);
    xhr.onreadystatechange = function () {
      if (this.readyState !== 4) return;
      if (this.status === 200) {
        plottables[varName] = JSON.parse(xhr.responseText);
        --reqCount;
        if (reqCount == 0) dataReady();
      }
      else { };
    };
    xhr.send();
  }

  function loadChartJS() {
    $.ajax({
      type: "GET",
      url: "/static/lib/chart-2.1.6.min.js",
      success: function () {
        --reqCount;
        if (reqCount == 0) dataReady();
      },
      dataType: "script",
      cache: false
    });
  }

})();
