namespace PayEx.Net.Api
{
    using PayEx.Net.Api.Controllers;

    public class PayExApi : Communication
    {
        public ConsumersController Consumers => GetRegisteredController<ConsumersController>();
        public PaymentOrdersController PaymentOrders => GetRegisteredController<PaymentOrdersController>();


        public PayExApi(string apiAddress, string token) : base(apiAddress, token)
        {
            RegisterControllerLoader(() => new ConsumersController(Client));
            RegisterControllerLoader(() => new PaymentOrdersController(Client));
        }
    }
}
