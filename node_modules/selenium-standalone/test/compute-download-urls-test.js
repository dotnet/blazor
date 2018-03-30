var assert = require('assert');

var computeDownloadUrls;

/**
 * Tests for the `computeDownloadUrls` module.
 *
 * NOTE: This does not verify that the module is returning valid URLs that will respond with
 * the desired binary files. Just that the logic contained in the module, specifically for
 * handling when paths formats differ between versions of the same driver.
 */
describe('compute-download-urls', function() {
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

  var opts;

  describe('selenium-jar', function() {
    it('basic version', function() {
      var actual = computeDownloadUrls({
        seleniumVersion: '1.0',
        seleniumBaseURL: 'https://localhost',
        drivers: {}
      });

      assert.equal(actual.selenium, 'https://localhost/1.0/selenium-server-standalone-1.0.jar');
    });

    it('version with patch', function() {
      var actual = computeDownloadUrls({
        seleniumVersion: '1.0.1',
        seleniumBaseURL: 'https://localhost',
        drivers: {}
      });

      assert.equal(actual.selenium, 'https://localhost/1.0/selenium-server-standalone-1.0.1.jar');
    });

    it('version with beta string', function() {
      var actual = computeDownloadUrls({
        seleniumVersion: '3.0.0-beta2',
        seleniumBaseURL: 'https://localhost',
        drivers: {}
      });

      assert.equal(actual.selenium, 'https://localhost/3.0-beta2/selenium-server-standalone-3.0.0-beta2.jar');
    });
  });

  describe('chrome', function() {
    beforeEach(function() {
      opts = {
        seleniumVersion: '1.0',
        seleniumBaseURL: 'https://localhost',
        drivers: {
          chrome: {}
        }
      };
    });

    describe('linux', function() {
      before(function() {
        Object.defineProperty(process, 'platform', {
          value: 'linux'
        });
      });

      it('x32 for versions < 2.34', function() {
        opts.drivers.chrome = {
          baseURL: 'https://localhost',
          version: '2.0',
          arch: 'x32'
        }

        var actual = computeDownloadUrls(opts);
        assert.equal(actual.chrome, 'https://localhost/2.0/chromedriver_linux32.zip');
      });

      it('x64', function() {
        opts.drivers.chrome = {
          baseURL: 'https://localhost',
          version: '2.0',
          arch: 'x64'
        }

        var actual = computeDownloadUrls(opts);
        assert.equal(actual.chrome, 'https://localhost/2.0/chromedriver_linux64.zip');
      });
    });

    describe('mac', function() {
      before(function() {
        Object.defineProperty(process, 'platform', {
          value: 'darwin'
        });
      });

      it('Use `mac32` for versions < 2.23', function() {
        opts.drivers.chrome = {
          baseURL: 'https://localhost',
          version: '2.22',
          arch: ''
        }

        var actual = computeDownloadUrls(opts);
        assert.equal(actual.chrome, 'https://localhost/2.22/chromedriver_mac32.zip');
      })

      it('Use `mac64` for versions >= 2.23', function() {
        opts.drivers.chrome = {
          baseURL: 'https://localhost',
          version: '2.23',
          arch: ''
        }

        var actual = computeDownloadUrls(opts);
        assert.equal(actual.chrome, 'https://localhost/2.23/chromedriver_mac64.zip');
      })
    });

    describe('win', function() {
      before(function() {
        Object.defineProperty(process, 'platform', {
          value: 'win32'
        });
      });

      it('basic version', function() {
        opts.drivers.chrome = {
          baseURL: 'https://localhost',
          version: '2.0',
          arch: ''
        }

        var actual = computeDownloadUrls(opts);
        assert.equal(actual.chrome, 'https://localhost/2.0/chromedriver_win32.zip');
      });
    });
  });

  describe('firefox', function() {
    beforeEach(function() {
      opts = {
        seleniumVersion: '1.0',
        seleniumBaseURL: 'https://localhost',
        drivers: {
          firefox: {}
        }
      };
    });

    describe('linux', function() {
      before(function() {
        Object.defineProperty(process, 'platform', {
          value: 'linux'
        });
      });

      it('uses `wires` name for versions < 0.8.0', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.7.0',
          arch: ''
        }

        var actual = computeDownloadUrls(opts);
        assert.equal(actual.firefox, 'https://localhost/v0.7.0/wires-0.7.0-linux64.gz');
      });

      it('uses `geckodriver` name for versions >= 0.8.0', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.8.0',
          arch: ''
        }

        var actual = computeDownloadUrls(opts);
        assert.equal(actual.firefox, 'https://localhost/v0.8.0/geckodriver-0.8.0-linux64.gz');
      });

      it('uses correct directory for 0.3.0', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.3.0',
          arch: ''
        }

        var actual = computeDownloadUrls(opts);
        assert.equal(actual.firefox, 'https://localhost/0.3.0/wires-0.3.0-linux64.gz');
      });

      it('uses leading `v` in version string when >= 0.9.0', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.9.0',
          arch: ''
        }

        var actual = computeDownloadUrls(opts);
        assert.equal(actual.firefox, 'https://localhost/v0.9.0/geckodriver-v0.9.0-linux64.tar.gz');
      });

      it('uses plain version string when < 0.9.0', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.7.0',
          arch: ''
        }

        var actual = computeDownloadUrls(opts);
        assert.equal(actual.firefox, 'https://localhost/v0.7.0/wires-0.7.0-linux64.gz');
      });

      it('uses `.gz` file extension for versions < 0.9.0', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.8.0',
          arch: ''
        }

        var actual = computeDownloadUrls(opts);
        assert(actual.firefox.indexOf('.gz') > 0);
        assert(actual.firefox.indexOf('.tar.gz') === -1);
      });

      it('uses `.tar.gz` file extension for versions >= 0.9.0', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.9.0',
          arch: ''
        }

        var actual = computeDownloadUrls(opts);
        assert(actual.firefox.indexOf('.tar.gz') > 0);
      });

      it('throws if asking a < 0.11.0 version and an arch which is not x64', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.9.0',
          arch: 'x86'
        };

        try {
          computeDownloadUrls(opts);
          throw new Error('Error not thrown');
        } catch (err) {
          if (err && err.message === 'Only x64 architecture is available for Firefox < 0.11.0') {
            return;
          }
          throw err;
        }
      });

      it('gets the right arch when arch is x86', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.11.0',
          arch: 'x86'
        };

        var actual = computeDownloadUrls(opts);
        assert(actual.firefox.indexOf('-linux32.tar.gz') > 0);
      });

      it('gets the right arch when arch is x64', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.11.0',
          arch: 'x64'
        };

        var actual = computeDownloadUrls(opts);
        assert(actual.firefox.indexOf('-linux64.tar.gz') > 0);
      });

      it('gets the right arch when arch is x32', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.11.0',
          arch: 'x32'
        };

        var actual = computeDownloadUrls(opts);
        assert(actual.firefox.indexOf('-linux32.tar.gz') > 0);
      });
    });

    describe('mac', function() {
      before(function() {
        Object.defineProperty(process, 'platform', {
          value: 'darwin'
        });
      });

      it('uses `OSX` platform for versions < 0.9.0', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.8.0',
          arch: ''
        }

        var actual = computeDownloadUrls(opts);
        assert(actual.firefox.indexOf('OSX') > 0);
      });

      it('uses `mac` platform for versions == 0.9.0', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.9.0',
          arch: ''
        }

        var actual = computeDownloadUrls(opts);
        assert(actual.firefox.indexOf('mac') > 0);
        assert(actual.firefox.indexOf('macos') === -1);
      });

      it('uses `macos` platform for versions >= 0.10.0', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.10.0',
          arch: ''
        }

        var actual = computeDownloadUrls(opts);
        assert(actual.firefox.indexOf('macos') > 0);
      });

      it('uses `osx` platform for versions <= 0.6.0', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.5.0',
          arch: ''
        }

        var actual = computeDownloadUrls(opts);
        assert(actual.firefox.indexOf('osx') > 0);
      });
    });

    describe('win', function() {
      before(function() {
        Object.defineProperty(process, 'platform', {
          value: 'win32'
        });
      });

      it('uses leading `v` in version string when == 0.5.0', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.5.0',
          arch: ''
        }

        var actual = computeDownloadUrls(opts);
        assert.equal(actual.firefox, 'https://localhost/v0.5.0/wires-v0.5.0-win.zip');
      });

      it('gets the right arch when arch is x32 and version >= 0.11.0', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.11.1',
          arch: 'x32'
        };

        var actual = computeDownloadUrls(opts);
        assert(actual.firefox.indexOf('win32.zip') >= 0);
      });

      it('gets the right arch when arch is x64 and version >= 0.11.0', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.11.0',
          arch: 'x64'
        };

        var actual = computeDownloadUrls(opts);
        assert(actual.firefox.indexOf('win64.zip') >= 0);
      });

      it('gets the 32 bits version when no arch specified version >= 0.11.0', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.11.0',
          arch: ''
        };

        var actual = computeDownloadUrls(opts);
        assert(actual.firefox.indexOf('win32.zip') >= 0);
      });

      it('throws if asking the 32bit version for 0.9.0/0.10.0', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.10.0',
          arch: 'x32'
        };

        try {
          computeDownloadUrls(opts);
          throw new Error('Error not thrown');
        } catch (err) {
          if (err && err.message === 'Only x64 architecture is available for Firefox 0.9.0 and 0.10.0') {
            return;
          }
          throw err;
        }
      });

      it('throws if asking the 64bit version for < 0.9.0', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.8.0',
          arch: 'x64'
        };

        try {
          computeDownloadUrls(opts);
          throw new Error('Error not thrown');
        } catch (err) {
          if (err && err.message === 'Only 32 bits architectures are available for Firefox <= 0.8.0') {
            return;
          }
          throw err;
        }
      });

      it('uses `win32` name for versions 0.8.0 & 0.7.1', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.7.1',
          arch: ''
        }

        var actual = computeDownloadUrls(opts);
        assert(actual.firefox.indexOf('-win32.zip') > 0);
      });

      it('uses `windows` name for version 0.3.0', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.3.0',
          arch: ''
        }

        var actual = computeDownloadUrls(opts);
        assert(actual.firefox.indexOf('-windows.zip') > 0);
      });

      it('uses `win` name for versions other versions', function() {
        opts.drivers.firefox = {
          baseURL: 'https://localhost',
          version: '0.5.0',
          arch: ''
        }

        var actual = computeDownloadUrls(opts);
        assert(actual.firefox.indexOf('-win.zip') > 0);
      });

    });
  });

  describe('ie', function() {
    before(function() {
      Object.defineProperty(process, 'platform', {
        value: 'win32'
      });
    });

    beforeEach(function() {
      opts = {
        seleniumVersion: '1.0',
        seleniumBaseURL: 'https://localhost',
        drivers: {
          ie: {}
        }
      };
    });

    it('uses `Win32` platform when arch == ia32', function() {
      opts.drivers.ie = {
        baseURL: 'https://localhost',
        version: '2.20.0',
        arch: 'ia32'
      }

      var actual = computeDownloadUrls(opts);
      assert(actual.ie.indexOf('IEDriverServer_Win32') > 0);
    });

    it('uses `x64` platform when arch == x64', function() {
      opts.drivers.ie = {
        baseURL: 'https://localhost',
        version: '2.20.0',
        arch: 'x64'
      }

      var actual = computeDownloadUrls(opts);
      assert(actual.ie.indexOf('IEDriverServer_x64') > 0);
    });

    it('uses `major.minor` folder for `major.minor.patch` version', function() {
      opts.drivers.ie = {
        baseURL: 'https://localhost',
        version: '2.20.1',
        arch: 'x64'
      }

      var actual = computeDownloadUrls(opts);
      assert(actual.ie.indexOf('/2.20/') > 0);
      assert(actual.ie.indexOf('2.20.1.zip') > 0);
    });
  });

  describe('edge', function() {
    before(function() {
      Object.defineProperty(process, 'platform', {
        value: 'win32'
      });

    });

    beforeEach(function() {
      opts = {
        seleniumVersion: '1.0',
        seleniumBaseURL: 'https://localhost',
        drivers: {
          edge: {}
        }
      };
    });

    var releases = require('../lib/microsoft-edge-releases');

    Object.keys(releases).forEach(function (version) {
      it('uses version `' + version + '` correct url', function() {
        opts.drivers.edge = { version: version };

        var actual = computeDownloadUrls(opts);
        assert.equal(actual.edge, releases[version].url);
      })
    });

    Object.keys(releases).forEach(function (version) {
      it('uses version `' + version + '` correct extension', function() {
        opts.drivers.edge = { version: version };

        var actual = computeDownloadUrls(opts);
        assert(actual.edge.indexOf(releases[version].extension) > 0);
      })
    });

    it('throws for unknown releases', function() {
      ['1.0', '2.3', '10'].forEach(function (version) {
        opts.drivers.edge = { version: version };

        assert.throws(function () { computeDownloadUrls(opts) });
      })
    });
  });
});
