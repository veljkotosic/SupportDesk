namespace SupportDeskWebApi.Data.Entities.User;

public record UserRole
{
    public string Name { get; init; }

    private UserRole(string Name)
    {
        this.Name = Name;
    }
    
    public static UserRole Customer => new("Customer");
    public static UserRole OrganizationAdmin => new("OrganizationAdmin");
    public static UserRole SupportAgent => new("SupportAgent");
    
    public static IEnumerable<UserRole> All() => [Customer, OrganizationAdmin, SupportAgent];

    public static UserRole? FromString(string name)
    {
        return All().FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}