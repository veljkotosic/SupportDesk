import { group } from 'k6';
import { OrganizationService } from '../../services/organizationService.js';
import { FaqService } from '../../services/faqService.js';
import { TemplateAnswerService } from '../../services/templateAnswerService.js';
import { waitBetween } from '../../utils/wait.js';
import { initSession, finishSession } from '../shared/session.js';

/**
 * Organization Admin Knowledge Base Journey (mapped from OrganizationAdminSettingsView.vue):
 * 1. Log in.
 * 2. Fetch Knowledge Base settings (FAQs, Templates, Categories).
 * 3. Add a new FAQ entry.
 * 4. Add a new Template Answer entry.
 * 5. Conclude session.
 */
export function adminKnowledgeBaseJourney(adminCredentials) {
    initSession(adminCredentials, 'Admin');

    const vuId = typeof __VU !== 'undefined' ? __VU : 1;
    const iter = typeof __ITER !== 'undefined' ? __ITER : 0;

    group('Admin - Load Knowledge Base', () => {
        OrganizationService.getKnowledgeBase();
    });

    group('Admin - Manage FAQ', () => {
        waitBetween(2, 4);
        const salt = `${vuId}_${iter}_${Date.now() % 1000000}`;
        FaqService.addFaq(
            `FAQ #${salt}?`,
            'Here is the detailed solution to this frequently encountered question.'
        );
    });

    group('Admin - Manage Template Answers', () => {
        waitBetween(2, 4);
        const salt = `${vuId}_${iter}_${Date.now() % 1000000}`;
        // Note: Domain enforces TemplateAnswer title max 40 chars
        const title = `Reply #${salt}`.substring(0, 40);
        TemplateAnswerService.addTemplateAnswer(
            title,
            'Thank you for contacting our customer support team. We are reviewing your issue.'
        );
    });

    group('Admin - Session End', () => {
        finishSession();
    });
}
