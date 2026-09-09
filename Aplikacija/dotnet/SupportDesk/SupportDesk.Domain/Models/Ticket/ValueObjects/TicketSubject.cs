using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Ticket.Options;

namespace SupportDesk.Domain.Models.Ticket.ValueObjects;

public sealed record TicketSubject : AbstractValueObject
{
    public string SubjectValue { get; init; }
    
    public TicketSubject(string SubjectValue)
    {
        this.SubjectValue = SubjectValue;
        ValidateValueObject();
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [
            new MinLengthRule(SubjectValue, TicketOptionsDefaults.SubjectMinimumLength),
            new MaxLengthRule(SubjectValue, TicketOptionsDefaults.SubjectMaximumLength)
        ];
    }
    
    public static implicit operator string(TicketSubject subject) => subject.SubjectValue;   
}