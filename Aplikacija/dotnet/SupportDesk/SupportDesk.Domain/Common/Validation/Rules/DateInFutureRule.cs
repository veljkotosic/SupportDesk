using SupportDesk.Domain.Abstract.Validation.Rule;

namespace SupportDesk.Domain.Common.Validation.Rules;

public sealed class DateInFutureRule : AbstractRule, IRuleMetadata
{
    private readonly DateTime? _date;
    private readonly DateTime _now;
    
    public DateInFutureRule(DateTime? date) 
    {
        _date = date;
        _now = DateTime.Now;
    }

    public static string ErrorCodeString => "date_in_future";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => "Date must be in the future";
    
    public override bool Validate()
    {
        if (_date is not null)
        {
            return _date > _now;
        }
        
        return true;
    }
}