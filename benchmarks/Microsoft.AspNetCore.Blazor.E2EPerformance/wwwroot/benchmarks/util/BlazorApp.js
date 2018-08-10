import { receiveEvent } from './BenchmarkEvents.js';

export class BlazorApp {
  constructor() {
    this._frame = document.createElement('iframe');
    document.body.appendChild(this._frame);
  }

  async start() {
    this._frame.src = 'blazor-frame.html';
    await receiveEvent('Rendered index.cshtml');
  }

  dispose() {
    document.body.removeChild(this._frame);
  }
}
