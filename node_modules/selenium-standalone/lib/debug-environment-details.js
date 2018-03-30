var os = require('os');

var debug = require('debug')('selenium-standalone:env-details');

debug('Platform:', os.platform(), os.release());
debug('Architecture:', process.arch);
debug('Node.js:', process.version);
debug('CWD:', process.cwd());
debug('Module Path:', __dirname);
debug('Package Version:', require('../package.json').version);
