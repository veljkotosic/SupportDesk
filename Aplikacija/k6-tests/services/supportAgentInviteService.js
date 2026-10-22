import http from 'k6/http';
import { check } from 'k6';
import { apiUrl } from '../config/environment.js';

const JSON_HEADERS = { 'Content-Type': 'application/json' };

export class SupportAgentInviteService {
    static createSupportAgentInvite(email) {
        const payload = JSON.stringify({ email });
        const res = http.post(`${apiUrl}/SupportAgentInvite`, payload, {
            headers: JSON_HEADERS,
            tags: { name: 'POST /SupportAgentInvite' },
        });
        
        check(res, {
            'createSupportAgentInvite status is 201': (r) => r.status === 201,
        });
        
        return res.status === 201 ? res.json() : null;
    }

    static revokeSupportAgentInvite(supportAgentInviteId) {
        const res = http.del(http.url`${apiUrl}/SupportAgentInvite/${supportAgentInviteId}`, null, {
            tags: { name: 'DELETE /SupportAgentInvite/:id' },
        });
        
        check(res, {
            'revokeSupportAgentInvite status is 204': (r) => r.status === 204,
        });
        
        return res;
    }
}
