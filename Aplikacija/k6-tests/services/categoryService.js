import http from 'k6/http';
import { check } from 'k6';
import { apiUrl } from '../config/environment.js';

const JSON_HEADERS = { 'Content-Type': 'application/json' };

export class CategoryService {
    static addCategory(name, description) {
        const payload = JSON.stringify({ name, description });
        const res = http.post(`${apiUrl}/Category`, payload, { headers: JSON_HEADERS });
        
        check(res, {
            'addCategory status is 201': (r) => r.status === 201,
        });
        
        return res.status === 201 ? res.json() : null;
    }

    static deleteCategory(categoryId) {
        const res = http.del(`${apiUrl}/Category/${categoryId}`);
        
        check(res, {
            'deleteCategory status is 204': (r) => r.status === 204,
        });
        
        return res;
    }

    static updateCategoryDetails(categoryId, name, description) {
        const payload = JSON.stringify({ name, description });
        const res = http.patch(`${apiUrl}/Category/${categoryId}`, payload, { headers: JSON_HEADERS });
        
        check(res, {
            'updateCategoryDetails status is 204': (r) => r.status === 204,
        });
        
        return res;
    }
}
