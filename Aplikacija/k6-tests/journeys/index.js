// Customer Journeys
export { customerOpenTicketJourney } from './customer/customerOpenTicketJourney.js';
export { customerBrowseDashboardJourney } from './customer/customerBrowseDashboardJourney.js';
export { customerChatJourney } from './customer/customerChatJourney.js';
export { customerFeedbackJourney } from './customer/customerFeedbackJourney.js';
export { customerRegistrationJourney } from './customer/customerRegistrationJourney.js';

// Support Agent Journeys
export { agentBrowseTicketsJourney } from './supportAgent/agentBrowseTicketsJourney.js';
export { agentClaimTicketJourney } from './supportAgent/agentClaimTicketJourney.js';
export { agentResolveTicketJourney } from './supportAgent/agentResolveTicketJourney.js';
export { agentChatJourney } from './supportAgent/agentChatJourney.js';

// Organization Admin Journeys
export { orgAdminDashboardJourney } from './orgAdminDashboardJourney.js';
export { adminKnowledgeBaseJourney } from './organizationAdmin/adminKnowledgeBaseJourney.js';
export { adminManageAgentsJourney } from './organizationAdmin/adminManageAgentsJourney.js';
export { adminBrowseTicketsJourney } from './organizationAdmin/adminBrowseTicketsJourney.js';
export { adminViewTicketJourney } from './organizationAdmin/adminViewTicketJourney.js';
export { orgRegistrationJourney } from './organizationAdmin/orgRegistrationJourney.js';

// Shared Utilities
export { startSession, finishSession } from './shared/session.js';

