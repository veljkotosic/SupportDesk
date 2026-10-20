export const standardThresholds = {
    http_req_failed: ['rate<0.01'],
    http_req_duration: [
        'p(95)<500',
        'p(99)<1000',
    ],
};
