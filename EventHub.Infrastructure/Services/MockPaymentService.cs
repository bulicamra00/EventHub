public class MockPaymentService : IPaymentService
{
    public async Task<bool> ProcessPaymentAsync(Guid userId, decimal amount, string paymentMethodToken)
    {
        await Task.Delay(1000); 
        
        return true; 
    }
}