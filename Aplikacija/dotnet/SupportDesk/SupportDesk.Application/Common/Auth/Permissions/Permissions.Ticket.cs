using SupportDesk.Application.Abstract.Auth.Permission;

namespace SupportDesk.Application.Common.Auth.Permissions;

public static partial class Permissions
{
    public static class Tickets
    {
        public static readonly Permission View = new("Tickets.View", "You don't have permission to view a ticket.");
        public static readonly Permission GetCustomerTickets = new("Tickets.GetCustomerTickets", "You don't have permission to get customer tickets.");
        public static readonly Permission GetOrganizationTickets = new("Tickets.GetOrganizationTickets", "You don't have permission to get organization tickets.");
        public static readonly Permission Open = new("Tickets.Open", "You don't have permission to open a ticket.");  
        public static readonly Permission Assign = new("Tickets.Assign", "You don't have permission to assign a ticket.");
        public static readonly Permission Close = new("Tickets.Close", "You don't have permission to close a ticket."); 
        public static readonly Permission GiveFeedback = new("Tickets.GiveFeedback", "You don't have permission to give feedback on a ticket.");
        public static readonly Permission ReadNotifications = new("Tickets.ReadNotifications", "You don't have permission to read notifications on a ticket.");
    }    
}