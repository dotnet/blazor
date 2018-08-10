# Astrobench

[![NPM version](https://badge.fury.io/js/astrobench.svg)](http://badge.fury.io/js/astrobench)
[![Bower version](https://badge.fury.io/bo/astrobench.svg)](http://badge.fury.io/bo/astrobench)

> Make the Web Faster

JavaScript library for running in the browser performance benchmarks, based on Benchmark.js. [Live Demo](http://astrobench.js.org/)

Easy way to create test cases. Provides rich and pretty UI.

[![astro.png][2]][1]

## Installation

Install with [bower](http://bower.io/)

```
$ bower install astrobench --save
```

Or with [npm](https://www.npmjs.org/)

```
$ npm install astrobench --save
```

## Use it

```html
$ bower install astrobench --save
$ $EDITOR tests.html

<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <title>Performance tests</title>
  <link rel="stylesheet" href="bower_components/astrobench/src/style.css">
</head>
<body>
  <!-- Wrapper for tests -->
  <div id="astrobench"></div>

  <script src="bower_components/astrobench/dist/astrobench.js"></script>
  <script>
    // A test suite begins with a call to the global function `suite` with two parameters:
    // a string and a function.
    // The string is a name or title for a spec suite – usually what is being tested.
    // The function is a block of code that implements the suite.
    suite('String matching', function(suite) {
      var text;

      // To help a test suite DRY up any duplicated setup code, provides
      // the global `setup` functions.
      // As the name implies the `setup` function is called once.
      // You can store data in `suite` Object, or define necessary variables.
      // Code from body of the function will be presented in UI.
      setup(function() {
        suite.text = 'Hello world';
        text = 'Hello world';
      });

      // Benchmark are defined by calling the global function `bench`,
      // which, like `suite` takes a string and a function.
      // The string is the title of the test and the function is the test
      bench('String#match', function() {
        !! text.match(/o/);
      });

      bench('RegExp#test', function() {
        !! /o/.test(suite.text);
      });

      // Async benchmark.
      // Calls to `bench` can take an optional single argument, Deferred object,
      // method .resolve() should be called when the async work is complete.
      bench('Async#test', function(deferred) {
        setTimeout(function() {
          deferred.resolve();
        }, 100);
      },
      // Options for current benchmark
      // See more on http://benchmarkjs.com/docs#options
      {
        defer: true
      });
    });
  </script>
</body>
</html>
```

And enjoy the result.

[![sample.png][3]][1]

[1]: http://astrobench.js.org
[2]: https://cdn.rawgit.com/kupriyanenko/astrobench/gh-pages/astro.png
[3]: https://cdn.rawgit.com/kupriyanenko/astrobench/gh-pages/sample.png
