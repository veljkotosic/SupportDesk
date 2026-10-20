import http from 'k6/http';
import { check } from 'k6';
import { apiUrl } from '../config/environment.js';

const JSON_HEADERS = { 'Content-Type': 'application/json' };

export class TemplateAnswerService {
    static addTemplateAnswer(title, text) {
        const payload = JSON.stringify({ title, text });
        const res = http.post(`${apiUrl}/TemplateAnswer`, payload, { headers: JSON_HEADERS });
        
        check(res, {
            'addTemplateAnswer status is 201': (r) => r.status === 201,
        });
        
        return res.status === 201 ? res.json() : null;
    }

    static deleteTemplateAnswer(templateAnswerId) {
        const res = http.del(`${apiUrl}/TemplateAnswer/${templateAnswerId}`);
        
        check(res, {
            'deleteTemplateAnswer status is 204': (r) => r.status === 204,
        });
        
        return res;
    }

    static updateTemplateAnswerDetails(templateAnswerId, title, text) {
        const payload = JSON.stringify({ title, text });
        const res = http.patch(`${apiUrl}/TemplateAnswer/${templateAnswerId}`, payload, { headers: JSON_HEADERS });
        
        check(res, {
            'updateTemplateAnswerDetails status is 204': (r) => r.status === 204,
        });
        
        return res;
    }
}
