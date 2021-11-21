namespace XeroIntergration.Models.XeroBearerToken
{
    public interface IXeroBearerToken
    {
        string LoginUrl();
        string BearerToken(string code);
    }
}