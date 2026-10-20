import http from 'k6/http';
import { check } from 'k6';
import { apiUrl } from '../config/environment.js';

const JSON_HEADERS = { 'Content-Type': 'application/json' };

export class FaqService {
    static addFaq(question, answer) {
        const payload = JSON.stringify({ question, answer });
        const res = http.post(`${apiUrl}/Faq`, payload, { headers: JSON_HEADERS });
        
        check(res, {
            'addFaq status is 201': (r) => r.status === 201,
        });
        
        return res.status === 201 ? res.json() : null;
    }

    static deleteFaq(faqId) {
        const res = http.del(`${apiUrl}/Faq/${faqId}`);
        
        check(res, {
            'deleteFaq status is 204': (r) => r.status === 204,
        });
        
        return res;
    }

    static updateFaqDetails(faqId, question, answer) {
        const payload = JSON.stringify({ question, answer });
        const res = http.patch(`${apiUrl}/Faq/${faqId}`, payload, { headers: JSON_HEADERS });
        
        check(res, {
            'updateFaqDetails status is 204': (r) => r.status === 204,
        });
        
        return res;
    }
}
