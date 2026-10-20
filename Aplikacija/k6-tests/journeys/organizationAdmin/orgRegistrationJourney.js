import { group } from 'k6';
import { AuthService } from '../../services/authService.js';
import { finishSession } from '../shared/session.js';

/**
 * Organization Registration Journey (mapped from RegisterView.vue - Register Organization tab):
 * 1. Register a new Organization + Org Admin account.
 * 2. Fetch profile to confirm authenticated session.
 * 3. Conclude session.
 */
export function orgRegistrationJourney() {
    const vuId = typeof __VU !== 'undefined' ? __VU : 1;
    const iter = typeof __ITER !== 'undefined' ? __ITER : 0;
    const rand = Math.random().toString(36).substring(2, 8);
    // Note: Domain enforces Username min 8, max 20 chars; Org name min 3, max 40 chars
    const userName = `a_${(vuId % 1000)}_${rand}`.padEnd(8, '0').substring(0, 20);
    const organizationName = `Org_${vuId}_${iter}_${rand}`.substring(0, 40);
    const email = `a_${vuId}_${iter}_${Date.now()}_${rand}@testorg.com`;
    const password = 'TestPassword123!';

    group('Org Admin - Register Organization', () => {
        AuthService.registerOrganization(userName, organizationName, email, password);
        AuthService.getMe();
    });

    group('Org Admin - Session End', () => {
        finishSession();
    });
}
