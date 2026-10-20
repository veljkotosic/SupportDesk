import { group } from 'k6';
import { organizationDashboardHubUrl } from '../config/environment.js';
import { OrganizationService } from '../services/organizationService.js';
import { withSignalRHub } from '../utils/signalR.js';
import { waitBetween } from '../utils/wait.js';
import { startSession, finishSession } from './shared/session.js';

export function orgAdminDashboardJourney(credentials) {
    startSession(credentials, 'Org Admin');

    withSignalRHub(organizationDashboardHubUrl, () => {
        group('Org Admin - View Dashboard', () => {
            OrganizationService.getDashboard();
            waitBetween(3, 6);
        });
    });

    group('Org Admin - Session End', () => {
        finishSession();
    });
}
