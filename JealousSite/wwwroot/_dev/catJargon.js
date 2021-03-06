﻿/// <reference path="../_lib/jquery-3.1.1.min.js" />

var JM = JM || {};

JM.catJargon = (function () {
  "use strict";

  $(document).ready(function () {
    if ($("#catjargon").length == 0) return;

    if ("onhashchange" in window) {
      window.onhashchange = function () {
        var entryId = window.location.hash.substr(1);
        var elmEntry = $("a[name='" + entryId + "']").closest(".entry");
        elmEntry.addClass("flash");
        setTimeout(function () {
          elmEntry.removeClass("flash");
        }, 500);
      }
    }

    $(window).scroll(onScroll);
    onScroll();

    $(".toTop").click(function () {
      // Navigate to pure URL without anchor
      document.location.hash = "";
    });

    $("#txtSearch").val("");
    $("#txtSearch").bind("input", function () {
      $(".disclaimer").removeClass("visible"); $("#toggleDisclaimer").removeClass("selected");
      $(".glossary-toc").removeClass("visible"); $("#toggleTOC").removeClass("selected");
      onSearchChanged();
    });
    if (window.location.hash == "") {
      // This screws in-page navigation thru anchors
      setTimeout(function () { $("#txtSearch").focus(); }, 50);
    }

    $("#toggleDisclaimer").click(function (evt) {
      if ($(".disclaimer").hasClass("visible")) {
        $(".disclaimer").removeClass("visible"); $("#toggleDisclaimer").removeClass("selected");
      }
      else {
        $(".disclaimer").addClass("visible"); $(this).addClass("selected");
        $(".glossary-toc").removeClass("visible"); $("#toggleTOC").removeClass("selected");
      }
      evt.preventDefault();
    });
    $("#toggleTOC").click(function (evt) {
      if ($(".glossary-toc").hasClass("visible")) {
        $(".glossary-toc").removeClass("visible"); $("#toggleTOC").removeClass("selected");
      }
      else {
        $(".disclaimer").removeClass("visible"); $("#toggleDisclaimer").removeClass("selected");
        $(".glossary-toc").addClass("visible"); $(this).addClass("selected");
      }
      evt.preventDefault();
    });

    buildTOC();

    //buildIndex();

  });

  function onScroll() {
    var scrollTop = $(window).scrollTop();
    var headerHeight = $(".header-wrap").height();
    if (scrollTop > 3 * headerHeight) showToTop();
    else $(".toTop").removeClass("visible");
  }

  function showToTop() {
    if ($(".toTop").hasClass("visible")) return;
    var cr = $(".content").offset().left + $(".content").width();
    var bw = $("body").width();
    $(".toTop").css("right", bw - cr);
    $(".toTop").addClass("visible");
  }

  function esc(s) {
    return s.replace(/&/g, '&amp;')
            .replace(/"/g, '&quot;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;');
  }

  function buildTOC() {
    var html = "";
    $(".entry h2 a").each(function () {
      var hw = $(this).text();
      var id = $(this).attr("name");
      if (html != "") html += "&nbsp;&bull; ";
      html += "<a href='#" + id + "'>" + esc(hw) + "</a>";
    });
    $(".glossary-toc").html(html);
  }

  function buildIndex() {
    //$(".entry h2 a").each(function () {
    //  var headword = $(this).text().toLowerCase();
    //  var words = getWords(headword);
    //  var id = "#" + $(this).attr("name");
    //});
    //$(".entry p.cref a").each(function () {
    //  var elmParent = $(this).closest(".entry");
    //  var headword = elmParent.find("h2 a").text().toLowerCase();
    //  var words = getWords(headword);
    //  var id = $(this).attr("href");

    //  headword = $(this).text().toLowerCase();
    //  words = getWords(headword);
    //  id = "#" + elmParent.find("h2 a").attr("name");
    //});
  }

  function getWords(hw) {
    var parts = hw.split(/[\s\-&]+/);
    var trimmedParts = [];
    for (var i = 0; i != parts.length; ++i) {
      var trimmed = parts[i].replace(/[.,\/#!$%\^&\*;:{}=\-_`~()]/g,"");
      if (trimmed != "") trimmedParts.push(trimmed);
    }
    return trimmedParts;
  }

  function getNorm(hw) {
    if (hw.startsWith("ap")) {
      var xdffs = -1;
    }
    var parts = getWords(hw);
    var norm = "";
    for (var i = 0; i != parts.length; ++i) {
      if (i > 0) norm += " ";
      norm += parts[i];
    }
    return norm;
  }

  function onSearchChanged() {
    var query = $("#txtSearch").val().toLowerCase();
    var qnorm = getNorm(query);
    $(".entry").each(function () {
      var head = $(this).find("h2 a").text().toLowerCase();
      var hwords = head.split(";");
      var matches = false;
      for (var i = 0; i != hwords.length && !matches; ++i) {
        var hnorm = getNorm(hwords[i]);
        if (hnorm.startsWith(qnorm)) matches = true;
      }
      $(this).find("p.cref a").each(function () {
        var refhead = $(this).text().toLowerCase();
        var refNorm = getNorm(refhead);
        if (refNorm.startsWith(qnorm)) matches = true;
      });
      if (!matches) $(this).addClass("hidden");
      else $(this).removeClass("hidden");
    });
  }

})();
