import http from 'k6/http';
import { check } from 'k6';
import { baseOptions } from '../../config/options.js';
import {baseUrl} from "../../config/environment.js";

export const options = {
    ...baseOptions,
    vus: 1,
    iterations: 10,
};

export function handleSummary(data) {
    return {
        'results/summary.json': JSON.stringify(data, null, 2),
    };
}

export default function () {
    const response = http.get(`${baseUrl}/health`, {
        tags: { name: 'GET /health' },
    });

    check(response, {
        'status is 200': (r) => r.status === 200,
    });
}
