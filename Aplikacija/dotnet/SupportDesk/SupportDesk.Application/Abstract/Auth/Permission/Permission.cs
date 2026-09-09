namespace SupportDesk.Application.Abstract.Auth.Permission;

public readonly record struct Permission(string Value, string ErrorMessage)
{
    public override string ToString() => Value;
}