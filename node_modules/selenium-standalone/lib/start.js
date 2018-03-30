module.exports = start;

var debug = require('debug')('selenium-standalone:start');
var mapValues = require('lodash').mapValues;
var merge = require('lodash').merge;
var spawn = require('child_process').spawn;
var which = require('which');

var checkPathsExistence = require('./check-paths-existence');
var checkStarted = require('./check-started');
var computeFsPaths = require('./compute-fs-paths');
var defaultConfig = require('./default-config');
var noop = require('./noop');

function start(opts, cb) {
  debug('Start API called with', opts);

  if (typeof opts === 'function') {
    cb = opts;
    opts = {};
  }

  if (!opts.javaArgs) {
    opts.javaArgs = [];
  }

  if (!opts.seleniumArgs) {
    opts.seleniumArgs = [];
  }

  if (!opts.version) {
    opts.version = defaultConfig.version;
  }

  if (!opts.spawnCb) {
    opts.spawnCb = noop;
  }

  if (opts.drivers) {
    // Merge in missing driver options for those specified
    opts.drivers = mapValues(opts.drivers, function(config, name) {
      return merge({}, defaultConfig.drivers[name], config);
    });
  } else {
    opts.drivers = defaultConfig.drivers;
  }

  var fsPaths = computeFsPaths({
    seleniumVersion: opts.version,
    drivers: opts.drivers,
    basePath: opts.basePath
  });

  if (typeof cb !== 'function') {
    throw new Error('You must provide a callback when starting selenium');
  }

  // programmatic use, did not give javaPath
  if (!opts.javaPath) {
    opts.javaPath = which.sync('java');
  }

  /* Command to run selenium is build in the following order:
      0) Java executable
      1) System level properties
      2) Jar binary
      3) Selenium specific arguments

     Example:
       java -Dwebdriver.chrome.driver=./.selenium/chromedriver/2.27-x64-chromedriver \
          -jar ./.selenium/selenium-server/3.0.1-server.jar \
          -role hub
   */
  var args = [];

  if (fsPaths.chrome) {
    args.push('-Dwebdriver.chrome.driver=' + fsPaths.chrome.installPath);
  }

  if (process.platform === 'win32' && fsPaths.ie) {
    args.push('-Dwebdriver.ie.driver=' + fsPaths.ie.installPath);
  } else {
    delete fsPaths.ie;
  }

  if (process.platform === 'win32' && fsPaths.edge) {
    args.push('-Dwebdriver.edge.driver=' + fsPaths.edge.installPath);
  } else {
    delete fsPaths.edge;
  }

  if (fsPaths.firefox) {
    args.push('-Dwebdriver.gecko.driver=' + fsPaths.firefox.installPath);
  }

  args = args.concat(opts.javaArgs);

  args = args.concat(['-jar', fsPaths.selenium.installPath]);

  args = args.concat(opts.seleniumArgs);

  checkPathsExistence(getInstallPaths(fsPaths), function(err) {
    if (err) {
      cb(err);
      return;
    }

    var neverStarted = false;
    debug('Spawning Selenium Server process', opts.javaPath, args);
    var selenium = spawn(opts.javaPath, args, opts.spawnOptions);

    opts.spawnCb(selenium);

    selenium.on('exit', errorIfNeverStarted);

    checkStarted(args, function started(err) {
      process.nextTick(function() {
        // Add empty handler to stdout and stderr so the buffers can be flushed
        // otherwise the process would eat up memory for nothing and crash
        // we add it here so that users can register their own listeners
        if (selenium.stdout && selenium.stderr) {
          if (selenium.stdout.listeners('data').length === 0) {
            selenium.stdout.on('data', noop);
          }
          if (selenium.stderr.listeners('data').length === 0) {
            selenium.stderr.on('data', noop);
          }
        }
      });

      selenium.removeListener('exit', errorIfNeverStarted);

      if (err) {
        cb(err);
        return;
      }

      if (!neverStarted) {
        cb(null, selenium);
      } // else ignore, callback has already been called in errorIfNeverStarted()
    });

    function errorIfNeverStarted(code) {
      neverStarted = true;

      var errorMsg;
      if (code === 1) {
        errorMsg = 'Selenium server did not start.\n';
      } else {
       errorMsg = 'Selenium exited before it could start\n';
      }
      errorMsg += 'Another Selenium process may already be running or your java version may be out of date.\n';

      // TODO: Is there a way to get this info from the api?
      // 3.x requires Java 8+, 2.47.0+ requires Java 7 - 7 is also end-of-life apparently ?
      errorMsg += 'Be sure to check the official Selenium release notes for minimum required java version: https://raw.githubusercontent.com/SeleniumHQ/selenium/master/java/CHANGELOG\n';

      cb(new Error(errorMsg));
    }
  });
}

function getInstallPaths(fsPaths) {
  return Object.keys(fsPaths).map(function(name) {
    return fsPaths[name].installPath;
  });
}
