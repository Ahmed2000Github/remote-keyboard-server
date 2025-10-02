const WebSocket = require('ws');

const wss = new WebSocket.Server({ port: 8080 });

wss.on('connection', function connection(ws) {
    console.log('Client connected');

    ws.on('message', function incoming(message) {
        console.log('received: %s', message);
        // Echo the message back to the client
        ws.send(`Server received: ${message}`);
    });

    ws.on('close', function close() {
        console.log('Client disconnected');
    });

    ws.send('Welcome to the WebSocket server!');
});

console.log('WebSocket server is running on ws://localhost:8080');