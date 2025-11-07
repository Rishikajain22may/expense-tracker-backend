namespace ExpensesWebApp_BE.Models
{
    public class Category
    {
        public int CategoryId { get; set; } // Primary Key
        public string CategoryName { get; set; } = string.Empty;

        // Navigation Properties
        public ICollection<Expense>? MappedExpenses { get; set; }
    }
}
