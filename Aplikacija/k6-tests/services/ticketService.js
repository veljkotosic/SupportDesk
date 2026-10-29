import http from 'k6/http';
import { check } from 'k6';
import { apiUrl } from '../config/environment.js';
import { TicketPriority, TicketFeedback, TicketStatus, TicketSortOption } from '../utils/enums.js';

const JSON_HEADERS = { 'Content-Type': 'application/json' };

function normalizeSearchResponse(data) {
    if (!data) return null;
    if (data.hits && Array.isArray(data.hits.hits)) {
        const tickets = data.hits.hits.map(h => h._source ?? h);
        const totalCount = typeof data.hits.total === 'object' ? data.hits.total.value : (data.hits.total ?? 0);
        let openCount = Number(data.openCount ?? 0);
        let assignedCount = Number(data.assignedCount ?? 0);
        let closedCount = Number(data.closedCount ?? 0);

        const aggs = data.aggregations ?? data.aggs;
        if (aggs) {
            if (aggs.status_counts) {
                const buckets = aggs.status_counts.buckets;
                const bucketList = Array.isArray(buckets) ? buckets : (buckets && typeof buckets === 'object' ? Object.values(buckets) : []);
                for (const b of bucketList) {
                    const key = String(b.key ?? b.key_as_string ?? '').toLowerCase();
                    const count = Number(b.doc_count ?? b.docCount ?? 0);
                    if (key === '0' || key === 'open') openCount = count;
                    else if (key === '1' || key === 'assigned') assignedCount = count;
                    else if (key === '2' || key === 'closed' || key === 'resolved') closedCount = count;
                }
            }

            if (aggs.open_tickets || aggs.open_count || aggs.openTickets || aggs.openCount) {
                openCount = Number(aggs.open_tickets?.doc_count ?? aggs.open_count?.doc_count ?? aggs.openTickets?.doc_count ?? aggs.openCount?.doc_count ?? openCount);
            }
            if (aggs.assigned_tickets || aggs.assigned_count || aggs.assignedTickets || aggs.assignedCount) {
                assignedCount = Number(aggs.assigned_tickets?.doc_count ?? aggs.assigned_count?.doc_count ?? aggs.assignedTickets?.doc_count ?? aggs.assignedCount?.doc_count ?? assignedCount);
            }
            if (aggs.closed_tickets || aggs.closed_count || aggs.closedTickets || aggs.closedCount || aggs.resolved_tickets || aggs.resolved_count) {
                closedCount = Number(aggs.closed_tickets?.doc_count ?? aggs.closed_count?.doc_count ?? aggs.closedTickets?.doc_count ?? aggs.closedCount?.doc_count ?? aggs.resolved_tickets?.doc_count ?? aggs.resolved_count?.doc_count ?? closedCount);
            }
        }

        const allCount = (data.allCount !== undefined && data.allCount !== null)
            ? Number(data.allCount)
            : (openCount + assignedCount + closedCount);

        return {
            tickets,
            totalCount,
            allCount,
            openCount,
            assignedCount,
            closedCount,
            raw: data
        };
    }
    return data;
}

export class TicketService {
    static openTicket(organizationId, categoryId, priority, subject, initialMessage) {
        const payload = JSON.stringify({ organizationId, categoryId, priority, subject, initialMessage });
        const res = http.post(`${apiUrl}/Ticket`, payload, {
            headers: JSON_HEADERS,
            tags: { name: 'POST /Ticket' },
        });
        
        check(res, {
            'openTicket status is 201': (r) => r.status === 201,
        });
        
        return res.status === 201 ? res.json() : null;
    }

    static assignTicket(ticketId) {
        const res = http.patch(http.url`${apiUrl}/Ticket/${ticketId}/assign`, null, {
            headers: JSON_HEADERS,
            tags: { name: 'PATCH /Ticket/:id/assign' },
        });
        
        check(res, {
            'assignTicket status is 204': (r) => r.status === 204,
        });
        
        return res;
    }

    static closeTicket(ticketId) {
        const res = http.patch(http.url`${apiUrl}/Ticket/${ticketId}/close`, null, {
            headers: JSON_HEADERS,
            tags: { name: 'PATCH /Ticket/:id/close' },
        });
        
        check(res, {
            'closeTicket status is 204': (r) => r.status === 204,
        });
        
        return res;
    }

    static giveFeedback(ticketId, feedback) {
        const payload = JSON.stringify({ feedback });
        const res = http.patch(http.url`${apiUrl}/Ticket/${ticketId}/giveFeedback`, payload, {
            headers: JSON_HEADERS,
            tags: { name: 'PATCH /Ticket/:id/giveFeedback' },
        });
        
        check(res, {
            'giveFeedback status is 204': (r) => r.status === 204,
        });
        
        return res;
    }

    static readAllNotifications(ticketId) {
        const res = http.patch(http.url`${apiUrl}/Ticket/${ticketId}/readAllNotifications`, null, {
            headers: JSON_HEADERS,
            tags: { name: 'PATCH /Ticket/:id/readAllNotifications' },
        });
        
        check(res, {
            'readAllNotifications status is 204': (r) => r.status === 204,
        });
        
        return res;
    }

    static getCustomerTickets(skip = 0, take = 10, searchTerm = '', status = null) {
        let query = `skip=${skip}&take=${take}`;
        if (searchTerm) query += `&searchTerm=${encodeURIComponent(searchTerm)}`;
        if (status !== null) query += `&status=${status}`;
        
        const res = http.get(http.url`${apiUrl}/Ticket/customer?${query}`, {
            tags: { name: 'GET /Ticket/customer' },
        });
        
        check(res, {
            'getCustomerTickets status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? normalizeSearchResponse(res.json()) : null;
    }

    static getOrganizationTickets(skip = 0, take = 10, searchTerm = '', status = null, priority = null, sortBy = TicketSortOption.Latest) {
        let query = `skip=${skip}&take=${take}&sortBy=${sortBy}`;
        if (searchTerm) query += `&searchTerm=${encodeURIComponent(searchTerm)}`;
        if (status !== null) query += `&status=${status}`;
        if (priority !== null) query += `&priority=${priority}`;
        
        const res = http.get(http.url`${apiUrl}/Ticket/organization?${query}`, {
            tags: { name: 'GET /Ticket/organization' },
        });
        
        check(res, {
            'getOrganizationTickets status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? normalizeSearchResponse(res.json()) : null;
    }

    static getTicket(ticketId) {
        const res = http.get(http.url`${apiUrl}/Ticket/${ticketId}`, {
            tags: { name: 'GET /Ticket/:id' },
        });
        
        check(res, {
            'getTicket status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? res.json() : null;
    }

    static getTicketInfo(ticketId) {
        const res = http.get(http.url`${apiUrl}/Ticket/${ticketId}/info`, {
            tags: { name: 'GET /Ticket/:id/info' },
        });
        
        check(res, {
            'getTicketInfo status is 200': (r) => r.status === 200,
        });
        
        return res.status === 200 ? res.json() : null;
    }
}
