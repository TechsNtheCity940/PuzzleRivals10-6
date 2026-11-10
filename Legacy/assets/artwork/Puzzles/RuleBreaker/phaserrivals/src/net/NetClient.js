export default class NetClient {
  constructor(scene) {
    this.scene = scene;
    this.socket = null;
    this.connected = false;
    this.roomId = null;
    this.side = null;
  }

  connect() {
    if (!window.io) return false;
    try {
      this.socket = window.io('http://localhost:3000', { transports: ['websocket', 'polling'] });
      this.socket.on('connect', () => {
        this.connected = true;
        this.socket.emit('queue_join');
      });
      this.socket.on('match_ready', payload => {
        this.roomId = payload.roomId;
        this.side = payload.side;
        this.scene.onNetReady(payload);
      });
      this.socket.on('match_state', state => this.scene.applyNetState(state));
      this.socket.on('disconnect', () => { this.connected = false; this.scene.onNetDisconnected(); });
      return true;
    } catch (e) {
      console.warn('Socket connect failed', e);
      return false;
    }
  }

  tap(row, col) {
    if (!this.connected) return;
    this.socket.emit('tile_tap', { row, col });
  }
}
