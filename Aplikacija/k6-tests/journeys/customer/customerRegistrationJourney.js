import { group } from 'k6';
import { AuthService } from '../../services/authService.js';
import { finishSession } from '../shared/session.js';

/**
 * Customer Registration Journey (mapped from RegisterView.vue):
 * 1. Register a new customer account.
 * 2. Fetch profile to confirm session is active.
 * 3. Conclude session (logout).
 */
export function customerRegistrationJourney() {
    const vuId = typeof __VU !== 'undefined' ? __VU : 1;
    const iter = typeof __ITER !== 'undefined' ? __ITER : 0;
    const rand = Math.random().toString(36).substring(2, 8);
    // Note: Domain enforces Username min 8, max 20 chars
    const userName = `c_${(vuId % 1000)}_${rand}`.padEnd(8, '0').substring(0, 20);
    const email = `c_${vuId}_${iter}_${Date.now()}_${rand}@testreg.com`;
    const password = 'TestPassword123!';

    group('Customer - Register', () => {
        AuthService.registerCustomer(userName, email, password);
        AuthService.getMe();
    });

    group('Customer - Session End', () => {
        finishSession();
    });
}
