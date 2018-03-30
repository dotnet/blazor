var assign = require('lodash').assign;

describe('programmatic use', function () {

  this.timeout(120000);

  var containsChrome = function(string) {
    return /chrome/i.test(string);
  };

  var testInstall = function(done, rawOptions, callback) {
    var selenium = require('../');
    // Capture the log output
    var log = '';
    var logger = function(message) {
      log += message;
    };
    var options = assign({ logger: logger }, rawOptions);
    selenium.install(options, function(err) {
      if (err) {
        done(err);
        return;
      }
      if (callback(log) !== false) {
        done();
      }
    });
  };

  var testStart = function(done, options, callback) {
    var selenium = require('../');
    selenium.start(options, function(err, cp) {
      if (err) {
        done(err);
        return;
      }
      cp.kill();
      if (callback(cp) !== false) {
        done();
      }
    });
  };

  it('should install', function(done) {
    testInstall(done, {}, function(log) {
      if (!containsChrome(log)) {
        done(new Error('Chrome driver should be installed'));
        return false;
      }
    });
  });

  it('should install with the given drivers', function(done) {
    testInstall(done, { drivers: {} }, function(log) {
      if (containsChrome(log)) {
        done(new Error('Chrome driver should not be installed'));
        return false;
      }
    });
  });

  it('should start', function(done) {
    testStart(done, {}, function(cp) {
      if (cp.spawnargs && !cp.spawnargs.some(containsChrome)) {
        done(new Error('Chrome driver should be loaded'));
        return false;
      }
    });
  });

  it('should start with custom seleniumArgs', function(done) {
    testStart(done, { seleniumArgs: ['-port', '12345'] }, function(cp) {
      if (cp.spawnargs && !cp.spawnargs.some(containsChrome)) {
        done(new Error('Chrome driver should be loaded'));
        return false;
      }
    });
  });

  it('should start with the given drivers', function(done) {
    testStart(done, { drivers: {} }, function(cp) {
      if (cp.spawnargs && cp.spawnargs.some(containsChrome)) {
       done(new Error('Chrome driver should not be loaded'));
        return false;
      }
    });
  });

  it('should start and merge drivers', function(done) {
    var options = { drivers: { chrome: {} } };
    testStart(done, options, function(cp) {
      if (cp.spawnargs && !cp.spawnargs.some(containsChrome)) {
        done(new Error('Chrome driver should be loaded'));
        return false;
      }
    });
  });

  it('can listen to stderr', function(done) {
    var selenium = require('../');
    selenium.start(function(err, cp) {
      if (err) {
        done(err);
        return;
      }

      cp.stderr.once('data', function() {
        cp.kill();
        done();
      });
    });
  });
});
