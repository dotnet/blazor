suite('App Startup', () => {
  bench('Time to first UI', async function (deferred) {
    await delay(100);
    deferred.resolve();
  }, { defer: true });
});

function delay(duration) {
  return new Promise(resolve => {
    setTimeout(resolve, duration);
  });
}
