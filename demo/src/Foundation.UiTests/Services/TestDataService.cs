using System;

namespace Foundation.UiTests.Services
{
    public static class TestDataService
    {
        public static string FirstName => "John";
        public static string LastName => "Doe";
        public static string Street => "Hornsgatan 123";
        public static string ZipCode => "12345";
        public static string City => "Stockholm";
        public static string PhoneNumber => "0706050403";
        public static string SwedishPhoneNumber => "+46739000001";
        public static string PersonalNumber => "19971020-2392";
        public static string PersonalNumberShort => "971020-2392";
        public static string Email => "leia.ahlstrom@payex.com";


        public static string ClearingNumber = "1234";
        public static string AccountNumber => "1234567890";
        public static string CreditCardNumber => "4761739001010416";
        public static string CreditCardCvc => "210";
        public static string CreditCardExpiratioDate => DateTime.Now.AddMonths(3).AddYears(1).ToString("MMyy");
        public static string SwishPhoneNumber => "0739000001";

        public static string ManagerUsername = "admin@example.com";
        public static string ManagerPassword = "Swedbankstore3#";


        public static string LoremIpsum => "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Praesent convallis facilisis neque ut scelerisque. Morbi arcu purus, gravida sed velit nec, interdum egestas ante. Pellentesque dapibus nisl ultrices dolor placerat, eu lobortis mauris elementum. Curabitur placerat ante est. Fusce et massa est. Etiam quis lacus justo. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Phasellus nulla enim, ornare in facilisis quis, ornare nec erat. Nullam sit amet mi augue. Proin dignissim risus urna, sed pulvinar turpis sollicitudin quis. Proin pretium lacinia ullamcorper.";
        public static string Description(int length = 30) => LoremIpsum.Substring(0, length);
    }
}
