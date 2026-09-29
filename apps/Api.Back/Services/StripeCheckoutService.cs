using Stripe.Checkout;

namespace Api.Back.Services
{
    public interface IStripeCheckoutService
    {
        Task<string> CreateCheckoutSessionAsync(string projectName, string? description, string? colorHex, Guid userId);
    }

    public class StripeCheckoutService : IStripeCheckoutService
    {
        private readonly IConfiguration _configuration;

        public StripeCheckoutService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> CreateCheckoutSessionAsync(string projectName, string? description, string? colorHex, Guid userId)
        {
            var frontendBaseUrl = _configuration["Frontend:BaseUrl"]
                ?? throw new InvalidOperationException("Frontend:BaseUrl is not configured.");

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card", "link" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "eur",
                            UnitAmount = 5_000,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Déblocage du projet : {projectName}",
                                Description = "Paiement unique pour débloquer un projet supplémentaire."
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = $"{frontendBaseUrl}/projects?payment=success",
                CancelUrl = $"{frontendBaseUrl}/projects?payment=cancel",
                Metadata = new Dictionary<string, string>
                {
                    { "UserId", userId.ToString() },
                    { "ProjectName", projectName },
                    { "ProjectDesc", description ?? "" },
                    { "ProjectColor", colorHex ?? "" }
                }
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);
            return session.Url;
        }
    }
}