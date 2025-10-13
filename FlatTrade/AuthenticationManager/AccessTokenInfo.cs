namespace FlatTrade.AuthenticationManager
{
    public class AccessTokenInfo
    {
        public string AccessToken { get; set; } = string.Empty;
        public string ClientCode { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; } // When the token expires (UTC)
        public DateTime LastGeneratedUtc { get; set; } // When the token was last generated (UTC)
                                                       // You might also store RefreshToken here if the API provides it

    }

}
