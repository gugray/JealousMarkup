/// <reference path="../_lib/jquery-3.1.1.min.js" />
/// <reference path="../_lib/chart-2.1.6.min.js" />

var JM = JM || {};

JM.nmt = (function () {
  "use strict";

  $(document).ready(function () {
    loadProxyJS();
  });

  function loadProxyJS() {
    $.ajax({
      type: "GET",
      url: "https://crazynmt.kginno.tk/crazynmt.js",
      dataType: "script",
      cache: false
    });
  }

})();
