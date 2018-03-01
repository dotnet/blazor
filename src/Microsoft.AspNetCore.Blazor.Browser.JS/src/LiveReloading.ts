const reconnectionPollIntervalMs = 250;

export function enableLiveReloading(eventSourceUrl: string) {
  listenForReloadEvent(eventSourceUrl, /* reloadOnConnection */ false);
}

function listenForReloadEvent(eventSourceUrl: string, reloadOnConnection: boolean) {
  const EventSource = window['EventSource'];
  if (!EventSource) {
    console.log('Browser does not support EventSource, so live reloading will be disabled.');
    return;
  }

  // First, connect to the endpoint. If the connection itself is meant to signal a
  // reload, then do that.
  const source = new EventSource(resolveAgainstBaseUri(eventSourceUrl));
  let sourceDidOpen;
  source.addEventListener('open', e => {
    sourceDidOpen = true;
    if (reloadOnConnection) {
      location.reload();
    }
  });

  // If we're directly notified that we should reload, then do so.
  source.addEventListener('message', e => {
    if (e.data === 'reload') {
      location.reload();
    }
  });

  // If the server disconnects (e.g., because the app is being recycled), then
  // we want to wait until it reappears then reload. Implement that by polling
  // the event source endpoint.
  // We *don't* want to rely on any built-in browser logic for reconnecting
  // eventsource, because browsers are inconsistent.
  source.addEventListener('error', e => {
    if (source.readyState === 0) {
      if (sourceDidOpen || reloadOnConnection) {
        source.close();
        setTimeout(() => {
          listenForReloadEvent(eventSourceUrl, /* reloadOnConnection */ true);
        }, reconnectionPollIntervalMs);
      }
    }
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
