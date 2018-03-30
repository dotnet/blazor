describe('when files are installed', function () {

    this.timeout(10000);

    it('should not reinstall them', function (done) {

        var fs = require('fs');
        var async = require('async');
        var path = require('path');
        var target = path.join(__dirname, '..', '.selenium');
        var selenium = require('..');

        // Recursively find files in the given directory
        function walk(dirname, files) {
            files = files || [];
            fs.readdirSync(dirname).forEach(function (name) {
                var filepath = path.join(dirname, name);
                if (fs.statSync(filepath).isDirectory()) {
                    walk(filepath, files);
                } else {
                    files.push(filepath);
                }
            });
            return files;
        }

        // Get last modified time of files that should already be installed in
        // the .selenium directory.
        var mtimes = walk(target).reduce(function (results, filepath) {
            results[filepath] = fs.statSync(filepath).mtime.getTime();
            return results;
        }, {});

        // Compare last modified time of files after running the installation
        // again. It shouldn't download any files, otherwise it fails.
        selenium.install(function () {

            var isModified = !walk(target).every(function (filepath) {
                return mtimes[filepath] === fs.statSync(filepath).mtime.getTime();
            });

            if (isModified) {
                done(new Error('It should not have reinstalled files'));
            } else {
                done();
            }
        });
    });
});
