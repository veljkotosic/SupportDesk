import http from 'k6/http';
import { group } from 'k6';
import { AuthService } from '../../services/authService.js';
import { workload } from '../../config/workload.js';
import { baseUrl } from '../../config/environment.js';

export const SESSION_MAX_ITERATIONS = 30;

let currentSession = null;
let currentEmail = null;
let currentAccessToken = null;
let currentRefreshToken = null;
let sessionIterationCount = 0;

function extractCookiesFromResponse(res) {
    if (res && res.cookies) {
        if (res.cookies.accessToken && res.cookies.accessToken.length > 0) {
            currentAccessToken = res.cookies.accessToken[0].value;
        }
        if (res.cookies.refreshToken && res.cookies.refreshToken.length > 0) {
            currentRefreshToken = res.cookies.refreshToken[0].value;
        }
    }
    if (!currentAccessToken) {
        try {
            const jar = http.cookieJar();
            const cookies = jar.cookiesForURL(baseUrl);
            if (cookies && cookies.accessToken && cookies.accessToken.length > 0) {
                currentAccessToken = cookies.accessToken[0];
            }
            if (cookies && cookies.refreshToken && cookies.refreshToken.length > 0) {
                currentRefreshToken = cookies.refreshToken[0];
            }
        } catch (_) {}
    }
}

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
            extractCookiesFromResponse(res);
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
            extractCookiesFromResponse(res);
            profile = AuthService.getMe();
        }
    });
    return profile;
}

/**
 * Initializes a session. If a session for the given credentials is already active
 * and has not exceeded 30 iterations, reuses the session without making any auth requests.
 * Otherwise, establishes a new session for the 30-iteration window.
 * @param {Object} credentials - { email, password, refreshToken }
 * @param {string} [roleName] - Optional role prefix for the group name
 * @returns {Object} User profile from getMe()
 */
export function initSession(credentials, roleName) {
    if (currentSession && currentEmail === credentials.email && sessionIterationCount < SESSION_MAX_ITERATIONS) {
        sessionIterationCount++;
        // k6 wipes the VU's cookie jar at the end of each iteration by default.
        // Re-inject the persistent cookies into this iteration's jar!
        const jar = http.cookieJar();
        if (currentAccessToken) {
            jar.set(baseUrl, 'accessToken', currentAccessToken);
        }
        if (currentRefreshToken) {
            jar.set(baseUrl, 'refreshToken', currentRefreshToken);
        }
        return currentSession;
    }

    // Begin new 30-iteration session window
    const jar = http.cookieJar();
    jar.clear(baseUrl);
    currentSession = null;
    currentEmail = credentials.email;
    currentAccessToken = null;
    currentRefreshToken = null;
    sessionIterationCount = 1;

    if (Math.random() < 0.95) {
        currentSession = restartSession(credentials, roleName);
    } else {
        currentSession = startSession(credentials, roleName);
    }

    return currentSession;
}

/**
 * Concludes an iteration. When the 30-iteration session limit is reached,
 * ends the session (optional logout and cookie clearing) so the next iteration
 * starts a fresh session.
 */
export function finishSession() {
    if (sessionIterationCount >= SESSION_MAX_ITERATIONS) {
        if (Math.random() < workload.logoutProbability) {
            AuthService.logout();
        }
        const jar = http.cookieJar();
        jar.clear(baseUrl);
        currentSession = null;
        currentEmail = null;
        currentAccessToken = null;
        currentRefreshToken = null;
        sessionIterationCount = 0;
    }
}
