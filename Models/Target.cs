namespace ExpensesWebApp_BE.Models
{
    public class Target
    {
        public int TargetID { get; set; } // Primary Key
        public int UserID { get; set; } // Foreign Key
        public decimal MonthlyIncome { get; set; }
        public decimal MonthlySavingsTarget { get; set; }
        public decimal MonthlyBudget { get; set; }
        public DateTime Timestamp { get; set; }
        public string MonthYearOfTarget { get; set; } = null!;

        //Navigation Properties
        public User? User { get; set; }
    }

}
