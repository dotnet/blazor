var assert = require('assert');
var merge = require('lodash').merge;
var request = require('request');

var defaultConfig = require('../lib/default-config');

var computeDownloadUrls;
var computedUrls;
var opts = {
  seleniumVersion: defaultConfig.version,
  seleniumBaseURL: defaultConfig.baseURL,
  drivers: defaultConfig.drivers
};

function doesDownloadExist(url, cb) {
  var req = request.get(url);
  req.on('response', function(res) {
    req.abort();

    if (res.statusCode >= 400) {
      return cb('Error response code got from ' + url + ': ' + res.statusCode);
    }

    cb();
  }).once('error', function (err) {
    cb(new Error('Error requesting ' + url + ': ' + err));
  });
}

/**
 * Tests to ensure that all the values listed in `default-config.js`
 * are actually downloadable.
 */
describe('default-downloads', function() {
  // Allow tests to mock `process.platform`
  before(function() {
    this.originalPlatform = Object.getOwnPropertyDescriptor(process, 'platform');
  });
  after(function() {
    Object.defineProperty(process, 'platform', this.originalPlatform);
  });

  // Ensure that any internal state of the module is clean for each test
  beforeEach(function() {
    computeDownloadUrls = require('../lib/compute-download-urls');
  });
  afterEach(function() {
    delete require.cache[require.resolve('../lib/compute-download-urls')];
  });

  describe('selenium-jar', function() {
    it('selenium-jar download exists', function(done) {
      computedUrls = computeDownloadUrls(opts);
      doesDownloadExist(computedUrls.selenium, done);
    });
  });

  describe('ie', function() {
    before(function(){
      Object.defineProperty(process, 'platform', {
        value: 'win32'
      });
    });

    it('ia32 download exists', function(done) {
      opts = merge(opts, {
        drivers: {
          ie: {
            arch: 'ia32'
          }
        }
      });

      computedUrls = computeDownloadUrls(opts);

      assert(computedUrls.ie.indexOf('Win32') > 0);
      doesDownloadExist(computedUrls.ie, done);
    });

    it('x64 download exists', function(done) {
      opts = merge(opts, {
        drivers: {
          ie: {
            arch: 'x64'
          }
        }
      });

      computedUrls = computeDownloadUrls(opts);

      assert(computedUrls.ie.indexOf('x64') > 0);
      doesDownloadExist(computedUrls.ie, done);
    });
  });

  describe('edge', function() {
    before(function(){
      Object.defineProperty(process, 'platform', {
        value: 'win32'
      });

    });

    var releases = require('../lib/microsoft-edge-releases')

    Object.keys(releases).forEach(function (version) {
      it('version `' + version + '` download exists', function(done) {
          opts = merge(opts, {
            drivers: {
              edge: {
                version: version
              }
            }
          });

        computedUrls = computeDownloadUrls(opts);

        assert.equal(computedUrls.edge, releases[version].url);
        doesDownloadExist(computedUrls.edge, done);
      });
    });
  });

  describe('chrome', function() {
    describe('linux', function() {
      before(function(){
        Object.defineProperty(process, 'platform', {
          value: 'linux'
        });
      });

      // No x32 for latest chromedriver on linux

      it('x64 download exists', function(done) {
        opts = merge(opts, {
          drivers: {
            chrome: {
              arch: 'x64'
            }
          }
        });

        computedUrls = computeDownloadUrls(opts);

        assert(computedUrls.chrome.indexOf('linux64') > 0);
        doesDownloadExist(computedUrls.chrome, done);
      });
    });

    describe('mac', function() {
      before(function(){
        Object.defineProperty(process, 'platform', {
          value: 'darwin'
        });
      });

      // No x32 for latest chromedriver on mac

      it('x64 download exists', function(done) {
        computedUrls = computeDownloadUrls(opts);

        assert(computedUrls.chrome.indexOf('mac64') > 0);
        doesDownloadExist(computedUrls.chrome, done);
      });
    });

    describe('win', function() {
      before(function(){
        Object.defineProperty(process, 'platform', {
          value: 'win32'
        });
      });

      // No x64 for latest chromedriver on win

      it('x32 download exists', function(done) {
        computedUrls = computeDownloadUrls(opts);

        assert(computedUrls.chrome.indexOf('win32') > 0);
        doesDownloadExist(computedUrls.chrome, done);
      });
    });
  });

  describe('firefox', function() {
    describe('arm', function() {
      before(function(){
        this.originalArch = Object.getOwnPropertyDescriptor(process, 'arch');

        Object.defineProperty(process, 'arch', {
          value: 'arm'
        });
      });

      after(function() {
        Object.defineProperty(process, 'arch', this.originalArch);
      });

      it('arm download exists', function(done) {
        computedUrls = computeDownloadUrls(opts);

        assert(computedUrls.firefox.indexOf('arm') > 0);
        doesDownloadExist(computedUrls.firefox, done);
      });
    });

    describe('linux', function() {
      before(function(){
        Object.defineProperty(process, 'platform', {
          value: 'linux'
        });
      });

      it('x64 download exists', function(done) {
        opts = merge(opts, {
          drivers: {
            firefox: {
              arch: 'x64'
            }
          }
        });

        computedUrls = computeDownloadUrls(opts);

        assert(computedUrls.firefox.indexOf('linux64') > 0);
        doesDownloadExist(computedUrls.firefox, done);
      });
    });

    describe('mac', function() {
      before(function(){
        Object.defineProperty(process, 'platform', {
          value: 'darwin'
        });
      });

      // No difference between arch for latest firefox driver on mac
      it('download exists', function(done) {
        computedUrls = computeDownloadUrls(opts);

        assert(computedUrls.firefox.indexOf('mac') > 0);
        doesDownloadExist(computedUrls.firefox, done);
      });
    });

    describe('win', function() {
      before(function(){
        Object.defineProperty(process, 'platform', {
          value: 'win32'
        });
      });

      it('ia32 download exists', function(done) {
        opts = merge(opts, {
          drivers: {
            firefox: {
              arch: 'ia32'
            }
          }
        });

        computedUrls = computeDownloadUrls(opts);

        assert(computedUrls.firefox.indexOf('win32') > 0);
        doesDownloadExist(computedUrls.firefox, done);
      });

      it('x64 download exists', function(done) {
        opts = merge(opts, {
          drivers: {
            firefox: {
              arch: 'x64'
            }
          }
        });

        computedUrls = computeDownloadUrls(opts);

        assert(computedUrls.firefox.indexOf('win64') > 0);
        doesDownloadExist(computedUrls.firefox, done);
      });
    });
  });
});
