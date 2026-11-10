import express from 'express';
import http from 'http';
import { Server } from 'socket.io';

const app = express();
const server = http.createServer(app);
const io = new Server(server, { cors: { origin: '*' } });
let queue = null;
let roomCount = 1;

io.on('connection', socket => {
  socket.on('queue_join', () => {
    if (!queue) {
      queue = socket;
      socket.emit('match_ready', { roomId: 'queue-wait', side: 'left' });
      return;
    }
    const roomId = `room-${roomCount++}`;
    queue.join(roomId);
    socket.join(roomId);
    queue.emit('match_ready', { roomId, side: 'left' });
    socket.emit('match_ready', { roomId, side: 'right' });
    io.to(roomId).emit('match_state', { roomId, started: true });
    queue = null;
  });

  socket.on('tile_tap', payload => {
    const rooms = [...socket.rooms].filter(r => r !== socket.id);
    rooms.forEach(roomId => socket.to(roomId).emit('match_state', { roomId, remoteTap: payload }));
  });

  socket.on('disconnect', () => {
    if (queue && queue.id === socket.id) queue = null;
  });
});

server.listen(3000, () => console.log('Neon Rivals socket server listening on http://localhost:3000'));
