/// <reference path="../_lib/jquery-3.1.1.min.js" />

"use strict";
var JM = JM || {};

JM.site = (function () {

  $(document).ready(function () {
    // Cookie warning
    var cookies = localStorage.getItem("cookies");
    if (cookies != "munch") {
      var html = "<div class='cookies-wrap'><div class='cookies'><img src='/static/cookiemonster-white.svg' /><div>" +
        "<span class='warning'>Cookies!</span><br />" +
        "<span class='readMoreCookie'>Grrr?</span>" +
        "<span class='acceptCookie'>Munch</span>" +
        "</div></div></div>";
      $("body").append(html);
      $(".readMoreCookie").click(function () { window.location.href = "/cookies" });
      $(".acceptCookie").click(function () {
        $(".cookies-wrap").remove();
        localStorage.setItem("cookies", "munch");
      });
    }
    //$("body").append("<div class='cookies'><div><img src='/static/cookiemonster-white.svg'/></div></div>");

    // Category filter (front page)
    $(".catSelector span").click(function (evt) {
      evt.stopPropagation();
      // Show all
      if ($(this).hasClass("primary")) {
        $(".catSelector span").removeClass("selected");
        $(this).addClass("selected");
      }
      // Specific category: toggle
      else {
        // Just toggled away from Show all
        if ($(".catSelector span.primary").hasClass("selected")) {
          if ($(".catSelector span.primary").removeClass("selected"));
          $(this).addClass("selected");
        }
        // Already in filter mode: toggle if we can
        else {
          // You can always turn on a new category
          if (!$(this).hasClass("selected")) $(this).addClass("selected");
          // Otherwise, you can turn on a non-primary category if it's not the last one
          else {
            if ($(".catSelector span.selected").length > 1) $(this).removeClass("selected");
          }
        }
      }
      // Update TOC items
      filterTOC();
    });
  });

  function filterTOC() {
    // Show all? Easy.
    if ($(".catSelector span.primary").hasClass("selected")) {
      $(".toc-item").removeClass("hidden");
      return;
    }
    // Selected categories
    var cats = {};
    $(".catSelector span.selected").each(function () {
      cats[$(this).text()] = true;
    });
    // Look at each TOC item, add/remove "hidden"
    $(".toc-item").each(function () {
      // Gather item's categories
      var itemCats = [];
      $(this).find(".filedunder span").each(function () {
        itemCats.push($(this).text());
      });
      // If at least one of the item's categories is selected, it's a keeper
      var keeper = false;
      for (var i = 0; i != itemCats.length; ++i) {
        var thisCat = itemCats[i];
        if (cats.hasOwnProperty(thisCat)) {
          keeper = true;
          break;
        }
      }
      // Show/hide
      if (keeper) $(this).removeClass("hidden");
      else $(this).addClass("hidden");
    });
  }

})();
