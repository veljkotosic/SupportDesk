import http from 'k6/http';
import { check } from 'k6';
import { apiUrl } from '../config/environment.js';

const JSON_HEADERS = { 'Content-Type': 'application/json' };

export class MessageService {
    static sendMessage(ticketId, text) {
        const payload = JSON.stringify({ ticketId, text });
        const res = http.post(`${apiUrl}/Message`, payload, { headers: JSON_HEADERS });
        
        check(res, {
            'sendMessage status is 201': (r) => r.status === 201,
        });
        
        return res.status === 201 ? res.json() : null;
    }

    static getMessage(messageId) {
        const res = http.get(`${apiUrl}/Message/${messageId}`);
        
        check(res, {
            'getMessage status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? res.json() : null;
    }
}
