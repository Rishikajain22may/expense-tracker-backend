namespace ExpensesWebApp_BE.DTOs
{
    public class ExpenseDTO
    {
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int CategoryId { get; set; }
    }
}
