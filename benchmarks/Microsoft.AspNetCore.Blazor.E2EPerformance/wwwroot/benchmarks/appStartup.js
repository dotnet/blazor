import { BlazorApp } from './Util/BlazorApp.js';

suite('App Startup', () => {

  bench('Time to first UI', async function (deferred) {
    const app = new BlazorApp();

    try {
      await app.start();
      deferred.resolve();
    } finally {
      app.dispose();
    }
  }, { defer: true });

});
