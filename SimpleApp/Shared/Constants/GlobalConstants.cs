namespace SimpleApp.Shared.Constants;

public static class GlobalConstants
{
    //Vietnam phone regex
    public const string PhoneRegex = @"^(\+84|84|0)(3[2-9]|5[2689]|7[06789]|8[1-9]|9[0-9])([0-9]{7})$";
    internal static class Claims
    {
        public const string Name = "name";
        public const string ClientId = "client_id";
        public const string UserId = "user_id";
    }
}