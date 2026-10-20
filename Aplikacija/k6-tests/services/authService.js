import http from 'k6/http';
import { check } from 'k6';
import { apiUrl } from '../config/environment.js';

const JSON_HEADERS = { 'Content-Type': 'application/json' };

export class AuthService {
    static login(email, password) {
        const payload = JSON.stringify({ email, password });
        const res = http.post(`${apiUrl}/Auth/login`, payload, { headers: JSON_HEADERS });

        check(res, {
            'login succeeded (204)': (r) => r.status === 204,
        });

        return res;
    }

    static getMe() {
        const res = http.get(`${apiUrl}/Auth/me`);
        check(res, {
            'getMe status is 200': (r) => r.status === 200,
        });
        return res.status === 200 ? res.json() : null;
    }

    static refreshLogin() {
        const res = http.post(`${apiUrl}/Auth/refreshLogin`, null);
        check(res, {
            'refreshLogin status is 204': (r) => r.status === 204,
        });
        return res;
    }

    static logout() {
        const res = http.del(`${apiUrl}/Auth/logout`);
        check(res, {
            'logout status is 204': (r) => r.status === 204,
        });
        return res;
    }

    static logoutAll() {
        const res = http.del(`${apiUrl}/Auth/logoutAll`);
        check(res, {
            'logoutAll status is 204': (r) => r.status === 204,
        });
        return res;
    }

    static registerCustomer(userName, email, password) {
        const payload = JSON.stringify({ userName, email, password });
        const res = http.post(`${apiUrl}/Auth/registerCustomer`, payload, { headers: JSON_HEADERS });
        check(res, {
            'registerCustomer status is 204': (r) => r.status === 204,
        });
        return res;
    }

    static registerSupportAgent(userName, email, password, code) {
        const payload = JSON.stringify({ userName, email, password, code });
        const res = http.post(`${apiUrl}/Auth/registerSupportAgent`, payload, { headers: JSON_HEADERS });
        check(res, {
            'registerSupportAgent status is 204': (r) => r.status === 204,
        });
        return res;
    }

    static registerOrganization(userName, organizationName, email, password) {
        const payload = JSON.stringify({ userName, organizationName, email, password });
        const res = http.post(`${apiUrl}/Auth/registerOrganization`, payload, { headers: JSON_HEADERS });
        check(res, {
            'registerOrganization status is 204': (r) => r.status === 204,
        });
        return res;
    }
}