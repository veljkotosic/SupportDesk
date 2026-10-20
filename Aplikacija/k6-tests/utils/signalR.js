import ws from 'k6/ws';
import { check } from 'k6';
import { baseUrl } from '../config/environment.js';

export const RECORD_SEPARATOR = String.fromCharCode(0x1e);
export const HANDSHAKE_REQUEST = JSON.stringify({ protocol: 'json', version: 1 }) + RECORD_SEPARATOR;

export function withSignalRHub(hubUrlOrPath, actionCallback) {
    const wsUrl = hubUrlOrPath.startsWith('ws://') || hubUrlOrPath.startsWith('wss://')
        ? hubUrlOrPath
        : baseUrl.replace(/^http/, 'ws') + hubUrlOrPath;

    const res = ws.connect(wsUrl, null, function (socket) {
        const stats = {
            incomingCount: 0,
            connected: false,
        };

        socket.on('open', function () {
            socket.send(HANDSHAKE_REQUEST);
        });

        socket.on('message', function (data) {
            if (!stats.connected) {
                stats.connected = true;
                return;
            }

            if (typeof data === 'string' && data.includes('"target"')) {
                stats.incomingCount++;
            }
        });

        socket.on('error', function (e) {

        });

        try {
            actionCallback(socket, stats);
        } finally {
            socket.close();
        }
    });

    check(res, {
        'SignalR connection established (101)': (r) => r && r.status === 101,
    });

    return res;
}
