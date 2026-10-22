import http from 'k6/http';
import { check } from 'k6';
import { apiUrl } from '../config/environment.js';

export class OrganizationService {
    static getDashboard() {
        const res = http.get(`${apiUrl}/Organization/dashboard`, {
            tags: { name: 'GET /Organization/dashboard' },
        });
        
        check(res, {
            'getDashboard status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? res.json() : null;
    }

    static getKnowledgeBase() {
        const res = http.get(`${apiUrl}/Organization/knowledgeBase`, {
            tags: { name: 'GET /Organization/knowledgeBase' },
        });
        
        check(res, {
            'getKnowledgeBase status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? res.json() : null;
    }

    static getSupportAgentsSummary() {
        const res = http.get(`${apiUrl}/Organization/supportAgentsSummary`, {
            tags: { name: 'GET /Organization/supportAgentsSummary' },
        });
        
        check(res, {
            'getSupportAgentsSummary status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? res.json() : null;
    }

    static getAllOrganizations() {
        const res = http.get(`${apiUrl}/Organization/all`, {
            tags: { name: 'GET /Organization/all' },
        });
        
        check(res, {
            'getAllOrganizations status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? res.json() : null;
    }

    static getOrganizationCategories(organizationId) {
        const res = http.get(http.url`${apiUrl}/Organization/${organizationId}/categories`, {
            tags: { name: 'GET /Organization/:id/categories' },
        });
        
        check(res, {
            'getOrganizationCategories status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? res.json() : null;
    }

    static getOrganizationFaqs(organizationId) {
        const res = http.get(http.url`${apiUrl}/Organization/${organizationId}/faqs`, {
            tags: { name: 'GET /Organization/:id/faqs' },
        });
        
        check(res, {
            'getOrganizationFaqs status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? res.json() : null;
    }

    static getOrganizationTemplateAnswers() {
        const res = http.get(`${apiUrl}/Organization/templateAnswers`, {
            tags: { name: 'GET /Organization/templateAnswers' },
        });
        
        check(res, {
            'getOrganizationTemplateAnswers status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? res.json() : null;
    }
}
