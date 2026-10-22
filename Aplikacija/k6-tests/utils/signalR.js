export const RECORD_SEPARATOR = String.fromCharCode(0x1e);
export const HANDSHAKE_REQUEST = '';

const noopSocket = {
    send: () => {},
    close: () => {},
    on: () => {},
};

const dummyStats = {
    incomingCount: 0,
    connected: false,
};

/**
 * SignalR disabled for performance load testing to eliminate TCP socket churn and port exhaustion.
 */
export function withSignalRHub(hubUrlOrPath, actionCallback) {
    if (typeof actionCallback === 'function') {
        actionCallback(noopSocket, dummyStats);
    }
    return null;
}
