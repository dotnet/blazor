module.exports = checkPathsExistence;

var async = require('async');
var fs = require('fs');

function checkPathsExistence(paths, cb) {
  paths = Object.keys(paths).map(function(key) {
    return paths[key];
  });

  async.parallel(paths.map(function(path) {
    return function(existsCb) {
      fs.exists(path, function(res) {
        if (res === false) {
          existsCb(new Error('Missing ' + path));
          return;
        }

        existsCb();
      });
    };
  }), cb);
}
