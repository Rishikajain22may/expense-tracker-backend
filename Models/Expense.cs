namespace ExpensesWebApp_BE.Models
{
    public class Expense
    {
        public int ExpenseID { get; set; } // Primary Key
        public int UserID { get; set; }    // Foreign Key
        public decimal Amount { get; set; }
        public int CategoryId { get; set; }  // Foreign Key
        public DateTime ExpenseDate { get; set; }
        public string? Description { get; set; }

        //Navigation Properties
        public User? User { get; set; }
        public Category? Categories { get; set; }
    }
}
