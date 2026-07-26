public interface IPaymentService
{
    Task<bool> ProcessPaymentAsync(Guid userId, decimal amount, string paymentMethodToken);
}