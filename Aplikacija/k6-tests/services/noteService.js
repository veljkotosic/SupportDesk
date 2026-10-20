import http from 'k6/http';
import { check } from 'k6';
import { apiUrl } from '../config/environment.js';

const JSON_HEADERS = { 'Content-Type': 'application/json' };

export class NoteService {
    static addNote(ticketId, text) {
        const payload = JSON.stringify({ ticketId, text });
        const res = http.post(`${apiUrl}/Note`, payload, { headers: JSON_HEADERS });
        
        check(res, {
            'addNote status is 201': (r) => r.status === 201,
        });
        
        return res.status === 201 ? res.json() : null;
    }
}
