// The global Module object is created in Blazor.WebAssembly.js just before it
// inserts the mono.js script. We want to hook into its preRun/postRun events,
// so wait until we know the Module object is created.

function whenInsertingMonoScript(callback) {
  const observer = new MutationObserver(mutations => {
    for (const mutation of mutations) {
      for (const node of mutation.addedNodes) {
        if (node.tagName === 'SCRIPT' && node.getAttribute('src').endsWith('/mono.js')) {
          observer.disconnect();
          callback();
        }
      }
    }
  });

  observer.observe(document.body, { childList: true });
}

whenInsertingMonoScript(() => {
  console.log('Inserting mono.js');
  Module.preRun.push(() => {
    console.log('In preRun');
  });
  Module.postRun.push(() => {
    console.log('In postRun');
  });
});
