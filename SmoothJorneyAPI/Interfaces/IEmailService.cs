namespace SmoothJorneyAPI.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string messageBody);
    }
}
