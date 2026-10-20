export const baseUrl = __ENV.BASE_URL || 'http://localhost:5011';
export const apiUrl = `${baseUrl}/api/v1`;

const baseWsUrl = baseUrl.replace(/^http:/i, 'ws:').replace(/^https:/i, 'wss:');
export const customerDashboardHubUrl = `${baseWsUrl}/hubs/customerDashboardHub`;
export const organizationDashboardHubUrl = `${baseWsUrl}/hubs/organizationDashboardHub`;
export const ticketHubUrl = `${baseWsUrl}/hubs/ticketHub`;