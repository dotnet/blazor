const reconnectionPollIntervalMs = 250;

export function enableLiveReloading(endpointUri: string) {
  listenForReloadEvent(endpointUri);
}

function listenForReloadEvent(endpointUri: string) {
  if (!WebSocket) {
    console.log('Browser does not support WebSocket, so live reloading will be disabled.');
    return;
  }

  // First, connect to the endpoint
  const source = new WebSocket(toAbsoluteWebSocketUri(endpointUri));

  // If we're notified that we should reload, then do so
  source.onmessage = e => {
    if (e.data === 'reload') {
      location.reload();
    }
  };
}

function toAbsoluteWebSocketUri(uri: string) {
  const baseUri = document.baseURI;
  if (baseUri) {
    const lastSlashPos = baseUri.lastIndexOf('/');
    const prefix = baseUri.substr(0, lastSlashPos);
    uri = prefix + uri;
  }

  // Scheme must be ws: or wss:
  return uri.replace(/^http/, 'ws');
}
