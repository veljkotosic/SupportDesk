import http from 'k6/http';
import { check } from 'k6';
import { apiUrl } from '../config/environment.js';
import { TicketPriority, TicketFeedback, TicketStatus, TicketSortOption } from '../utils/enums.js';

const JSON_HEADERS = { 'Content-Type': 'application/json' };

export class TicketService {
    static openTicket(organizationId, categoryId, priority, subject, initialMessage) {
        const payload = JSON.stringify({ organizationId, categoryId, priority, subject, initialMessage });
        const res = http.post(`${apiUrl}/Ticket`, payload, { headers: JSON_HEADERS });
        
        check(res, {
            'openTicket status is 201': (r) => r.status === 201,
        });
        
        return res.status === 201 ? res.json() : null;
    }

    static assignTicket(ticketId) {
        const res = http.patch(`${apiUrl}/Ticket/${ticketId}/assign`, null, { headers: JSON_HEADERS });
        
        check(res, {
            'assignTicket status is 204': (r) => r.status === 204,
        });
        
        return res;
    }

    static closeTicket(ticketId) {
        const res = http.patch(`${apiUrl}/Ticket/${ticketId}/close`, null, { headers: JSON_HEADERS });
        
        check(res, {
            'closeTicket status is 204': (r) => r.status === 204,
        });
        
        return res;
    }

    static giveFeedback(ticketId, feedback) {
        const payload = JSON.stringify({ feedback });
        const res = http.patch(`${apiUrl}/Ticket/${ticketId}/giveFeedback`, payload, { headers: JSON_HEADERS });
        
        check(res, {
            'giveFeedback status is 204': (r) => r.status === 204,
        });
        
        return res;
    }

    static readAllNotifications(ticketId) {
        const res = http.patch(`${apiUrl}/Ticket/${ticketId}/readAllNotifications`, null, { headers: JSON_HEADERS });
        
        check(res, {
            'readAllNotifications status is 204': (r) => r.status === 204,
        });
        
        return res;
    }

    static getCustomerTickets(skip = 0, take = 10, searchTerm = '', status = null) {
        let query = `skip=${skip}&take=${take}`;
        if (searchTerm) query += `&searchTerm=${encodeURIComponent(searchTerm)}`;
        if (status !== null) query += `&status=${status}`;
        
        const res = http.get(`${apiUrl}/Ticket/customer?${query}`);
        
        check(res, {
            'getCustomerTickets status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? res.json() : null;
    }

    static getOrganizationTickets(skip = 0, take = 10, searchTerm = '', status = null, priority = null, sortBy = TicketSortOption.Latest) {
        let query = `skip=${skip}&take=${take}&sortBy=${sortBy}`;
        if (searchTerm) query += `&searchTerm=${encodeURIComponent(searchTerm)}`;
        if (status !== null) query += `&status=${status}`;
        if (priority !== null) query += `&priority=${priority}`;
        
        const res = http.get(`${apiUrl}/Ticket/organization?${query}`);
        
        check(res, {
            'getOrganizationTickets status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? res.json() : null;
    }

    static getTicket(ticketId) {
        const res = http.get(`${apiUrl}/Ticket/${ticketId}`);
        
        check(res, {
            'getTicket status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? res.json() : null;
    }

    static getTicketInfo(ticketId) {
        const res = http.get(`${apiUrl}/Ticket/${ticketId}/info`);
        
        check(res, {
            'getTicketInfo status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? res.json() : null;
    }
}
