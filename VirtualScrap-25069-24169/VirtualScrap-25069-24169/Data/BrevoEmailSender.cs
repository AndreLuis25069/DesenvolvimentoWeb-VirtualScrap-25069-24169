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

            string emailDesign = $@"<div style='background-color: #f8f9fa; padding: 40px 10px; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif;'>
                <div style='max-width: 500px; margin: 0 auto; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.05); border: 1px solid #eaeaea;'>
                    
                    <div style='background-color: #ffffff; padding: 30px 20px 10px 20px; text-align: center; border-bottom: 1px solid #f1f3f5;'>
                        <span style='font-size: 26px; font-weight: bold; color: #212529; letter-spacing: -0.5px; text-decoration: none;'>
                            ♻️ Virtual<span style='color: #ffc107;'>Scrap</span>
                        </span>
                    </div>

                    <div style='padding: 30px 30px 40px 30px; text-align: center;'>
                        <h2 style='font-size: 20px; font-weight: 700; color: #212529; margin-top: 0; margin-bottom: 15px;'>{subject}</h2>
                        
                        <div style='font-size: 15px; color: #495057; line-height: 1.6; margin-bottom: 30px;'>
                            {htmlMessage}
                        </div>

                        <hr style='border: 0; border-top: 1px solid #f1f3f5; margin: 30px 0;' />
                        
                        <p style='font-size: 12px; color: #868e96; margin: 0;'>
                            Este é um e-mail automático enviado pelo mercado VirtualScrap. Se não solicitaste esta ação, podes ignorar esta mensagem de forma segura.
                        </p>
                    </div>

                </div>
            </div>";

            // Monta o JSON exato que a API da Brevo exige
            var payload = new
            {
                sender = new { name = senderName, email = senderEmail },
                to = new[] { new { email = email } },
                subject = subject,
                htmlContent = emailDesign // O Identity já passa o link HTML pronto aqui
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