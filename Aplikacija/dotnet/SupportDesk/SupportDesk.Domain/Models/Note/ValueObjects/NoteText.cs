using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Note.Options;

namespace SupportDesk.Domain.Models.Note.ValueObjects;

public sealed record NoteText : AbstractValueObject
{
    public string TextValue { get; init; }
    
    public NoteText(string TextValue)
    {
        this.TextValue = TextValue;
        ValidateValueObject();
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [
            new MinLengthRule(TextValue, NoteOptionsDefaults.TextMinimumLength),
            new MaxLengthRule(TextValue, NoteOptionsDefaults.TextMaximumLength)
        ];
    }
}