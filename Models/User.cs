namespace ExpensesWebApp_BE.Models
{
    public class User
    {
        public int UserID { get; set; } // Primary Key
        public string UserName { get; set; } = null!; // Unique
        public string PasswordHash { get; set; } = null!;  

        public DateTime CreatedAt { get; set; }

        //Navigation Properties
        public ICollection<Expense>? MappedExpenses { get; set; }
        public ICollection<Target>? MappedTargets { get; set; }
    }
}
