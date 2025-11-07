namespace ExpensesWebApp_BE.DTOs
{
    public class UserDTO
    {
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }
}
