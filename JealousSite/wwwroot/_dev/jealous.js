/// <reference path="../_lib/jquery-3.1.1.min.js" />

"use strict";
var JM = JM || {};

JM.site = (function () {

  $(document).ready(function () {
    // Show debug info
    //$("body").append("<div class='debug'></div>");

    // Responsive layout
    $(window).resize(onResize);
    onResize();
  });

  function onResize() {
    var width = $("html").innerWidth();
    $(".debug").html("Wnd: " + width);
    if (width >= 800) { $("html").removeClass("smaller"); $("html").removeClass("mobile"); }
    else if (width >= 620 && width < 800) { $("html").addClass("smaller"); $("html").removeClass("mobile"); }
    else { $("html").addClass("smaller"); $("html").addClass("mobile"); }
  }
})();
