using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace VirtualScrap_25069_24169.Data
{
    public class BrevoEmailSender : IEmailSender
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public BrevoEmailSender(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Busca as configurações do teu appsettings.json
            var apiKey = _configuration["BrevoSettings:ApiKey"];
            var senderEmail = _configuration["BrevoSettings:SenderEmail"];
            var senderName = _configuration["BrevoSettings:SenderName"];

            System.Diagnostics.Debug.WriteLine($"---> A API KEY LIDA É: {apiKey}");

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("Erro Crítico: A API Key da Brevo não foi carregada do appsettings.json!");
            }

            // Monta o JSON exato que a API da Brevo exige
            var payload = new
            {
                sender = new { name = senderName, email = senderEmail },
                to = new[] { new { email = email } },
                subject = subject,
                htmlContent = htmlMessage // O Identity já passa o link HTML pronto aqui
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
            request.Headers.Add("api-key", apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro da Brevo ({response.StatusCode}): {errorContent}");
            }
        }
    }
}