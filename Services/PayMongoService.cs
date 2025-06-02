using cce106_palit.Entity;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

public class PayMongoService
{
    private readonly HttpClient _client;
    private readonly string _secretKey;

    public PayMongoService(string secretKey)
    {
        _secretKey = secretKey;
        _client = new HttpClient();
    }

    public async Task<(string? checkoutUrl, string? checkoutSessionId)> CreateCheckoutSession(
    string name,
    string email,
    string phone,
    string orderDescription,
    List<Cart> cartItems,
    string paymentMethod,
    string successUrl,
    string cancelUrl,
    string currency = "PHP"
)
    {
        var lineItems = cartItems.Select(item => new
        {
            currency,
            amount = (int)(item.Product.Price * 100),
            description = item.Product.Description,
            name = item.Product.Name,
            quantity = item.Quantity
        }).ToList();

        var totalAmount = lineItems.Sum(item => item.amount);
        if (totalAmount < 2000)
        {
            return (null, null); 
        }

        var payload = new
        {
            data = new
            {
                attributes = new
                {
                    billing = new
                    {
                        name,
                        email,
                        phone
                    },
                    send_email_receipt = true,
                    show_description = true,
                    show_line_items = true,
                    description = orderDescription,
                    line_items = lineItems,
                    payment_method_types = new[]
                    {
                    "gcash", "paymaya", "grab_pay", "card", "qrph", "billease",
                    "dob", "dob_ubp", "brankas_bdo", "brankas_metrobank"
                },
                    success_url = successUrl,
                    cancel_url = cancelUrl
                },
            }
        };

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("https://api.paymongo.com/v1/checkout_sessions"),
            Content = new StringContent(JsonConvert.SerializeObject(payload))
            {
                Headers =
            {
                ContentType = new MediaTypeHeaderValue("application/json")
            }
            }
        };

        var apiKey = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_secretKey}:"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", apiKey);

        using (var response = await _client.SendAsync(request))
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Request failed with status {response.StatusCode}: {responseContent}");
            }

            dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
            string checkoutUrl = jsonResponse?.data?.attributes?.checkout_url;
            string checkoutSessionId = jsonResponse?.data?.id;

            return (checkoutUrl, checkoutSessionId);
        }
    }

    public async Task<dynamic> RetrieveCheckoutSession(string checkoutSessionId)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://api.paymongo.com/v1/checkout_sessions/{checkoutSessionId}"),
            Headers =
        {
            { "accept", "application/json" },
        },
        };

        var apiKey = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_secretKey}:"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", apiKey);

        using (var response = await _client.SendAsync(request))
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Request failed with status {response.StatusCode}: {responseContent}");
            }

            return JsonConvert.DeserializeObject(responseContent);
        }
    }

    internal async Task RetrieveCheckoutSession(object checkoutSessionId)
    {
        throw new NotImplementedException();
    }
}
