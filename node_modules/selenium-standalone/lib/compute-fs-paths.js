module.exports = computeFsPaths;

var path = require('path');

var basePath = path.join(__dirname, '..', '.selenium');

function computeFsPaths(opts) {
  var fsPaths = {};
  opts.basePath = opts.basePath || basePath;
  if (opts.drivers.chrome) {
    fsPaths.chrome = {
      installPath: path.join(opts.basePath, 'chromedriver', opts.drivers.chrome.version + '-' + opts.drivers.chrome.arch + '-chromedriver')
    };
  }

  if (opts.drivers.ie) {
    fsPaths.ie = {
      installPath: path.join(opts.basePath, 'iedriver', opts.drivers.ie.version + '-' + opts.drivers.ie.arch + '-IEDriverServer.exe')
    };
  }

  if (opts.drivers.edge) {
    fsPaths.edge = {
      installPath: path.join(opts.basePath, 'edgedriver', opts.drivers.edge.version + '-MicrosoftEdgeDriver.exe')
    };
  }

  if (opts.drivers.firefox) {
    fsPaths.firefox = {
      installPath: path.join(opts.basePath, 'geckodriver', opts.drivers.firefox.version + '-' + opts.drivers.firefox.arch + '-geckodriver')
    };
  }

  fsPaths.selenium = {
    installPath: path.join(opts.basePath, 'selenium-server', opts.seleniumVersion + '-server.jar')
  };

  fsPaths = Object.keys(fsPaths).reduce(function computeDownloadPath(newFsPaths, name) {
    var downloadPath;

    if (name === 'selenium' || name === 'edge') {
      downloadPath = newFsPaths[name].installPath;
    } else if (name === 'firefox' && process.platform !== 'win32') {
      downloadPath = newFsPaths[name].installPath + '.gz';
    } else {
      downloadPath = newFsPaths[name].installPath + '.zip';
    }

    newFsPaths[name].downloadPath = downloadPath;
    return newFsPaths;
  }, fsPaths);

  return fsPaths;
}
