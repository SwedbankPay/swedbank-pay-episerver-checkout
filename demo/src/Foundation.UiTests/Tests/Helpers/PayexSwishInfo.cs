namespace Foundation.UiTests.Tests.Helpers
{
    public class PayexSwishInfo : PayexInfo
    {
        public PayexSwishInfo(string swishNumber)
        {
            SwishNumber = swishNumber;
        }

        public string SwishNumber { get; }
    }
}
