using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Threading.Tasks;

public class EmailService
{
    private readonly string _apiKey = "38ef7db582c93bcbcdd650266ef2d5d5-3724298e-dcdace87";
    private readonly string _domain = "sandbox70ca21150c044e47bc12116b70b0fcab.mailgun.org";

    public async Task<RestResponse> SendVerificationEmail(string email, string verificationLink)
    {
        var client = new RestClient(new RestClientOptions
        {
            BaseUrl = new Uri($"https://api.mailgun.net/v3/{_domain}/messages"),
            Authenticator = new HttpBasicAuthenticator("api", _apiKey)

        });

        var request = new RestRequest
        {
            Method = Method.Post
        };

        request.AddParameter("from", $"PALIT Email Verification <mailgun@{_domain}>");
        request.AddParameter("to", email);
        request.AddParameter("subject", "Please verify your email address");
        request.AddParameter("text", $"Thank you for registering! Please verify your email by clicking on the link: {verificationLink}");

        var response = await client.ExecuteAsync(request);

        if (!response.IsSuccessful)
        {
            throw new Exception($"Error sending email: {response.Content}");
        }

        return response;
    }
}
