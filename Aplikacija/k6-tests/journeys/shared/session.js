import http from 'k6/http';
import { group } from 'k6';
import { AuthService } from '../../services/authService.js';
import { workload } from '../../config/workload.js';
import { baseUrl } from '../../config/environment.js';

/**
 * Starts a user session by logging in and fetching the current user profile.
 * @param {Object} credentials - { email, password }
 * @param {string} [roleName] - Optional role prefix for the group name (e.g. 'Customer', 'Agent', 'Admin', 'Org Admin')
 * @returns {Object} Response from AuthService.getMe()
 */
export function startSession(credentials, roleName) {
    const groupName = roleName ? `${roleName} - Login` : 'Login';
    let profile = null;
    group(groupName, () => {
        const res = AuthService.login(credentials.email, credentials.password);
        if (res && res.status === 204) {
            profile = AuthService.getMe();
        }
    });
    return profile;
}

export function finishSession() {
    if (Math.random() < workload.logoutProbability) {
        AuthService.logout();
    } else {
        const jar = http.cookieJar();
        jar.clear(baseUrl);
    }
}

