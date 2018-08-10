suite('A suite', () => {
  bench('String#match', () => {
    !! 'Hello world'.match(/o/);
  });

  bench('RegExp#test', () => {
    !! /o/.test('Hello world');
  });
});

suite('B suite', function (suite) {
  setup(function () {
    suite.text = 'Hello world';
  });

  bench('Benchmark with error', function () {
    !!text.match(/o/);
  });

  bench('Deferred benchmark', function (deferred) {
    !! /o/.test(suite.text);

    setTimeout(function () {
      deferred.resolve();
    }, 100);
  }, { defer: true });
});
