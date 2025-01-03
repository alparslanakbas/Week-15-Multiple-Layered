namespace Multiple_Layered_DataAccess.Library.Models
{
    public class User : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty!;
        public string LastName { get; set; } = string.Empty!;
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
