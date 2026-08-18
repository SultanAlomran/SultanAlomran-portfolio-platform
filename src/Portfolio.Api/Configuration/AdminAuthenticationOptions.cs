namespace Portfolio.Api.Configuration;

public sealed class AdminAuthenticationOptions
{
    public const string SectionName = "Authentication";
    public string AdminBaseUrl { get; set; } = "http://localhost:4300";
    public CookieOptions Cookie { get; set; } = new();
    public GoogleOptions Google { get; set; } = new();

    public sealed class CookieOptions
    {
        public string Name { get; set; } = ".Portfolio.Admin.Auth";
        public int SessionHours { get; set; } = 8;
        public int PersistentDays { get; set; } = 14;
        public string SecurePolicy { get; set; } = "Always";
        public string SameSite { get; set; } = "Lax";
    }

    public sealed class GoogleOptions
    {
        public bool Enabled { get; set; }
        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";
        public bool TestMode { get; set; }
        public string TestSubject { get; set; } = "";
    }
}

public sealed class AdminBootstrapOptions
{
    public const string SectionName = "AdminBootstrap";
    public string Email { get; set; } = "";
    public string UserName { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Password { get; set; } = "";
    public string? GoogleSubject { get; set; }
    public string? GoogleEmail { get; set; }
}

public static class AdminAuthenticationSchemes
{
    public const string ApplicationCookie = "PortfolioAdmin";
    public const string ExternalCookie = "PortfolioExternal";
    public const string Google = "Google";
}
