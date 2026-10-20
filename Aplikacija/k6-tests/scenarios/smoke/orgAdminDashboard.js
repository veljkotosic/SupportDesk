import { orgAdminDashboardJourney } from '../../journeys/orgAdminDashboardJourney.js';
import { baseOptions } from '../../config/options.js';

export const options = {
    ...baseOptions,
    vus: 1,
    iterations: 10,
};

export default function () {
    const adminUser = {
        email: __ENV.ADMIN_EMAIL || 'admin1@org1.com',
        password: __ENV.ADMIN_PASSWORD || 'Password123!',
    };

    orgAdminDashboardJourney(adminUser);
}
