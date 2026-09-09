using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.TicketNotification.Options;

namespace SupportDesk.Domain.Models.TicketNotification.ValueObjects;

public sealed record TicketNotificationText : AbstractValueObject
{
    public string TextValue { get; init; }
    
    public TicketNotificationText(string TextValue)
    {
        this.TextValue = TextValue;
        ValidateValueObject();
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [
            new MaxLengthRule(TextValue, TicketNotificationOptionsDefaults.TextMaximumLength)
        ];
    }
}