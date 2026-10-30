import http from 'k6/http';
import { group } from 'k6';
import { AuthService } from '../../services/authService.js';
import { workload } from '../../config/workload.js';
import { baseUrl } from '../../config/environment.js';

/**
 * Starts a user session by logging in with credentials and fetching the current user profile.
 * @param {Object} credentials - { email, password, refreshToken }
 * @param {string} [roleName] - Optional role prefix for the group name (e.g. 'Customer', 'Agent', 'Admin', 'Org Admin')
 * @returns {Object} Response from AuthService.getMe()
 */
export function startSession(credentials, roleName) {
    const groupName = roleName ? `${roleName} - Login` : 'Login';
    let profile = null;
    group(groupName, () => {
        const jar = http.cookieJar();
        jar.clear(baseUrl);
        const res = AuthService.login(credentials.email, credentials.password);
        if (res && res.status === 204) {
            profile = AuthService.getMe();
        }
    });
    return profile;
}

/**
 * Restarts a user session by injecting the seeded refresh token and calling refreshLogin + getMe.
 * @param {Object} credentials - { email, password, refreshToken }
 * @param {string} [roleName] - Optional role prefix for the group name (e.g. 'Customer', 'Agent', 'Admin', 'Org Admin')
 * @returns {Object} Response from AuthService.getMe()
 */
export function restartSession(credentials, roleName) {
    const groupName = roleName ? `${roleName} - Restart Session` : 'Restart Session';
    let profile = null;
    group(groupName, () => {
        const jar = http.cookieJar();
        jar.clear(baseUrl);
        const token = credentials.refreshToken || `rt_${credentials.email}`;
        jar.set(baseUrl, 'refreshToken', token);
        const res = AuthService.refreshLogin();
        if (res && res.status === 204) {
            profile = AuthService.getMe();
        }
    });
    return profile;
}

/**
 * Initializes a session with 95% odds of choosing restartSession and 5% odds of choosing startSession.
 * @param {Object} credentials - { email, password, refreshToken }
 * @param {string} [roleName] - Optional role prefix for the group name
 * @returns {Object} User profile from getMe()
 */
export function initSession(credentials, roleName) {
    if (Math.random() < 0.95) {
        return restartSession(credentials, roleName);
    }
    return startSession(credentials, roleName);
}

export function finishSession() {
    if (Math.random() < workload.logoutProbability) {
        AuthService.logout();
    } else {
        const jar = http.cookieJar();
        jar.clear(baseUrl);
    }
}
