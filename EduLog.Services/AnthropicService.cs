using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace EduLog.Services
{
    public class AIGeneratedQuestion
    {
        public string QuestionText { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
    }

    public interface IAnthropicService
    {
        Task<List<AIGeneratedQuestion>> GenerateQuestionsAsync(string topic, string? notes, string? examples);
    }

    public class AnthropicService : IAnthropicService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        private const string PromptTemplate =
            "Sen bir yazılım eğitmenisin. Aşağıdaki ders konusuna göre 5 adet çoktan seçmeli soru üret.\n" +
            "Konu: {0}\n" +
            "Notlar: {1}\n" +
            "Örnekler: {2}\n\n" +
            "Yanıtı YALNIZCA şu JSON formatında ver, başka hiçbir şey ekleme:\n" +
            "[{{\"questionText\":\"...\",\"optionA\":\"...\",\"optionB\":\"...\",\"optionC\":\"...\",\"optionD\":\"...\",\"correctAnswer\":\"A\"}}]";

        public AnthropicService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["AnthropicApi:ApiKey"] ?? throw new InvalidOperationException("AnthropicApi:ApiKey is not configured.");
            _model = configuration["AnthropicApi:Model"] ?? "claude-sonnet-4-6";
        }

        public async Task<List<AIGeneratedQuestion>> GenerateQuestionsAsync(string topic, string? notes, string? examples)
        {
            var prompt = string.Format(PromptTemplate, topic, notes ?? "Yok", examples ?? "Yok");

            var requestBody = new
            {
                model = _model,
                max_tokens = 2048,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("x-api-key", _apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(responseJson);
                var contentArray = doc.RootElement.GetProperty("content");
                var textBlock = contentArray[0].GetProperty("text").GetString() ?? "[]";

                // Clean up: remove markdown code fences if present
                textBlock = textBlock.Trim();
                if (textBlock.StartsWith("```json"))
                    textBlock = textBlock.Substring(7);
                else if (textBlock.StartsWith("```"))
                    textBlock = textBlock.Substring(3);
                if (textBlock.EndsWith("```"))
                    textBlock = textBlock.Substring(0, textBlock.Length - 3);
                textBlock = textBlock.Trim();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var questions = JsonSerializer.Deserialize<List<AIGeneratedQuestion>>(textBlock, options);
                return questions ?? new List<AIGeneratedQuestion>();
            }
            catch (Exception)
            {
                return new List<AIGeneratedQuestion>();
            }
        }
    }
}
