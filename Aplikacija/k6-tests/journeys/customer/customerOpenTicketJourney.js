import { group } from 'k6';
import { TicketService } from '../../services/ticketService.js';
import { OrganizationService } from '../../services/organizationService.js';
import { withSignalRHub } from '../../utils/signalR.js';
import { waitBetween } from '../../utils/wait.js';
import { startSession, finishSession } from '../shared/session.js';
import { TicketPriority } from '../../utils/enums.js';
import { customerDashboardHubUrl } from '../../config/environment.js';

/**
 * Customer Open Ticket Journey (mapped from OpenTicketView.vue):
 * 1. Log in and load profile.
 * 2. Connect to customerDashboardHubUrl.
 * 3. Fetch organizations, categories, and FAQs for selected org.
 * 4. Think time (user enters subject and initial message).
 * 5. Open ticket via POST /Ticket.
 * 6. Conclude session.
 */
export function customerOpenTicketJourney(customerCredentials, targetOrgIndex = null) {
    startSession(customerCredentials, 'Customer');

    const vuId = typeof __VU !== 'undefined' ? __VU : 1;
    const iter = typeof __ITER !== 'undefined' ? __ITER : 0;
    const orgSelector = targetOrgIndex !== null ? targetOrgIndex : (vuId + iter);

    withSignalRHub(customerDashboardHubUrl, () => {
        let selectedOrgId = null;
        let selectedCategoryId = null;

        group('Customer - Open Ticket Form Preparation', () => {
            const organizations = OrganizationService.getAllOrganizations();
            if (organizations && organizations.length > 0) {
                const org = organizations[orgSelector % organizations.length];
                selectedOrgId = org.id;

                const categories = OrganizationService.getOrganizationCategories(selectedOrgId);
                if (categories && categories.length > 0) {
                    const catIdx = (vuId + iter) % categories.length;
                    selectedCategoryId = categories[catIdx].id;
                }

                OrganizationService.getOrganizationFaqs(selectedOrgId);
            }
        });

        if (selectedOrgId && selectedCategoryId) {
            group('Customer - Submit Ticket', () => {
                waitBetween(2, 4);

                // Note: Domain enforces Subject max 40 chars
                const subject = `Issue VU${vuId}-IT${iter} #${(Date.now() % 10000000)}`.substring(0, 40);
                const initialMessage = 'I am experiencing problems accessing my account dashboard.';

                TicketService.openTicket(
                    selectedOrgId,
                    selectedCategoryId,
                    TicketPriority.Medium,
                    subject,
                    initialMessage
                );
            });
        }
    });

    group('Customer - Session End', () => {
        finishSession();
    });
}
