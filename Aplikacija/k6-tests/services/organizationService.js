import http from 'k6/http';
import { check } from 'k6';
import { apiUrl } from '../config/environment.js';

export class OrganizationService {
    static getDashboard() {
        const res = http.get(`${apiUrl}/Organization/dashboard`);
        
        check(res, {
            'getDashboard status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? res.json() : null;
    }

    static getKnowledgeBase() {
        const res = http.get(`${apiUrl}/Organization/knowledgeBase`);
        
        check(res, {
            'getKnowledgeBase status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? res.json() : null;
    }

    static getSupportAgentsSummary() {
        const res = http.get(`${apiUrl}/Organization/supportAgentsSummary`);
        
        check(res, {
            'getSupportAgentsSummary status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? res.json() : null;
    }

    static getAllOrganizations() {
        const res = http.get(`${apiUrl}/Organization/all`);
        
        check(res, {
            'getAllOrganizations status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? res.json() : null;
    }

    static getOrganizationCategories(organizationId) {
        const res = http.get(`${apiUrl}/Organization/${organizationId}/categories`);
        
        check(res, {
            'getOrganizationCategories status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? res.json() : null;
    }

    static getOrganizationFaqs(organizationId) {
        const res = http.get(`${apiUrl}/Organization/${organizationId}/faqs`);
        
        check(res, {
            'getOrganizationFaqs status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? res.json() : null;
    }

    static getOrganizationTemplateAnswers() {
        const res = http.get(`${apiUrl}/Organization/templateAnswers`);
        
        check(res, {
            'getOrganizationTemplateAnswers status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? res.json() : null;
    }
}
