const reconnectionPollIntervalMs = 250;

export function enableLiveReloading(eventSourceUrl: string) {
  listenForReloadEvent(eventSourceUrl);
}

function listenForReloadEvent(eventSourceUrl: string) {
  const EventSource = window['EventSource'];
  if (!EventSource) {
    console.log('Browser does not support EventSource, so live reloading will be disabled.');
    return;
  }

  // First, connect to the endpoint
  const source = new EventSource(resolveAgainstBaseUri(eventSourceUrl));
  let sourceDidOpen;
  source.addEventListener('open', e => {
    sourceDidOpen = true;
  });

  // If we're notified that we should reload, then do so.
  source.addEventListener('message', e => {
    if (e.data === 'reload') {
      location.reload();
    }
  });

  // If the server disconnects (e.g., because the app is being recycled), then
  // stop listening.
  source.addEventListener('error', e => {
    if (source.readyState === 0 && sourceDidOpen) {
      source.close();
    }
  });

  // Needed for some versions of Firefox
  window.addEventListener('beforeunload', () => {
    source.close();
  });
}

function resolveAgainstBaseUri(uri: string) {
  const baseUri = document.baseURI;
  if (baseUri) {
    const lastSlashPos = baseUri.lastIndexOf('/');
    const prefix = baseUri.substr(0, lastSlashPos);
    return prefix + uri;
  } else {
    return uri;
  }
}
