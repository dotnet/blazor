describe('when files are missing', function () {
  it('should fail', function(done) {
    var fs = require('fs');
    var path = require('path');
    var from = path.join(__dirname, '..', '.selenium');
    var to = path.join(__dirname, '..', '.selenium-tmp');

    fs.renameSync(from, to);

    var selenium = require('../');
    selenium.start(function(err) {
      fs.renameSync(to, from);
      if (err) {
        done();
        return;
      }

      done(new Error('We should have got an error'));
    });
  });
});
