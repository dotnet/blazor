module.exports = checkStarted;

var request = require('request').defaults({json: true});
var statusUrl = require('./get-selenium-status-url.js');

function checkStarted(seleniumArgs, cb) {
  var retries = 0;
  var hub = statusUrl.getSeleniumStatusUrl(seleniumArgs);
  // server has one minute to start
  var retryInterval = 200;
  var maxRetries = 60 * 1000 / retryInterval;

  function hasStarted() {
    retries++;

    if (retries > maxRetries) {
      cb(new Error('Unable to connect to selenium'));
      return;
    }

    request(hub, function (err, res) {
      if (err || res.statusCode !== 200) {
        setTimeout(hasStarted, retryInterval);
        return;
      }
      cb(null);
    });
  }

  setTimeout(hasStarted, 500);
}
