using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using EduLog.Core.Entities;
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

    public interface IAIQuestionService
    {
        Task<List<AIGeneratedQuestion>> GenerateQuestionsAsync(string topic, string? notes, string? examples, int questionCount = 5);
        Task<AIGeneratedCodeTask> GenerateCodeTaskAsync(string topic, string? notes, string? examples);
        Task<AICodeReview> ReviewCodeSubmissionAsync(string topic, string taskDescription, string studentCode, int maxScore);
    }

    public interface IAnthropicService : IAIQuestionService { }

    public class AnthropicService : IAnthropicService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        private const string PromptTemplate =
            "Sen bir yazılım eğitmenisin. Aşağıdaki ders konusuna göre {3} adet çoktan seçmeli soru üret.\n" +
            "Konu: {0}\n" +
            "Notlar: {1}\n" +
            "Örnekler: {2}\n\n" +
            "Yanıtı YALNIZCA şu JSON formatında ver, başka hiçbir şey ekleme:\n" +
            "[{{\"questionText\":\"...\",\"optionA\":\"...\",\"optionB\":\"...\",\"optionC\":\"...\",\"optionD\":\"...\",\"correctAnswer\":\"A\"}}]";

        private const string CodeTaskPromptTemplate =
            "Sen bir yazılım eğitmenisin. Aşağıdaki konuya uygun bir kod yazma ödevi oluştur.\n" +
            "Konu: {0}\n" +
            "Notlar: {1}\n" +
            "Örnekler: {2}\n\n" +
            "YALNIZCA şu JSON formatında yanıt ver, başka hiçbir şey ekleme:\n" +
            "{{\"title\":\"...\",\"description\":\"...\",\"expectedBehavior\":\"...\",\"starterCode\":\"...\"}}\n\n" +
            "starterCode alanı opsiyoneldir, boş bırakılabilir. description öğrenciye ne yapması gerektiğini net anlatsın. Türkçe yanıt ver.";

        private const string CodeReviewPromptTemplate =
            "Sen bir yazılım eğitmenisin ve öğrenci kodunu değerlendiriyorsun.\n" +
            "Ders konusu: {0}\n" +
            "Ödev: {1}\n" +
            "Maksimum puan: {2}\n\n" +
            "Öğrencinin kodu:\n{3}\n\n" +
            "Değerlendirmeyi YALNIZCA şu JSON formatında ver, başka hiçbir şey ekleme:\n" +
            "{{\"suggestedScore\":85,\"summary\":\"...\",\"strengths\":\"...\",\"improvements\":\"...\",\"missingParts\":\"...\"}}\n\n" +
            "suggestedScore 0 ile {2} arasında olmalı. Türkçe yanıt ver.";

        public AnthropicService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["AnthropicApi:ApiKey"] ?? throw new InvalidOperationException("AnthropicApi:ApiKey is not configured.");
            _model = configuration["AnthropicApi:Model"] ?? "claude-sonnet-4-6";
        }

        public async Task<List<AIGeneratedQuestion>> GenerateQuestionsAsync(string topic, string? notes, string? examples, int questionCount = 5)
        {
            var prompt = string.Format(PromptTemplate, topic, notes ?? "Yok", examples ?? "Yok", questionCount);
            var responseText = await SendAnthropicRequestAsync(prompt);

            try
            {
                var cleaned = CleanJsonResponse(responseText);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var questions = JsonSerializer.Deserialize<List<AIGeneratedQuestion>>(cleaned, options);
                return questions ?? new List<AIGeneratedQuestion>();
            }
            catch (Exception)
            {
                return new List<AIGeneratedQuestion>();
            }
        }

        public async Task<AIGeneratedCodeTask> GenerateCodeTaskAsync(string topic, string? notes, string? examples)
        {
            var prompt = string.Format(CodeTaskPromptTemplate, topic, notes ?? "Yok", examples ?? "Yok");
            var responseText = await SendAnthropicRequestAsync(prompt);

            try
            {
                var cleaned = CleanJsonResponse(responseText);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var codeTask = JsonSerializer.Deserialize<AIGeneratedCodeTask>(cleaned, options);
                return codeTask ?? new AIGeneratedCodeTask();
            }
            catch (Exception)
            {
                return new AIGeneratedCodeTask();
            }
        }

        public async Task<AICodeReview> ReviewCodeSubmissionAsync(string topic, string taskDescription, string studentCode, int maxScore)
        {
            var prompt = string.Format(CodeReviewPromptTemplate, topic, taskDescription, maxScore, studentCode);
            var responseText = await SendAnthropicRequestAsync(prompt);

            try
            {
                var cleaned = CleanJsonResponse(responseText);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var review = JsonSerializer.Deserialize<AICodeReview>(cleaned, options);
                return review ?? new AICodeReview();
            }
            catch (Exception)
            {
                return new AICodeReview();
            }
        }

        private async Task<string> SendAnthropicRequestAsync(string prompt)
        {
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

            using var doc = JsonDocument.Parse(responseJson);
            var contentArray = doc.RootElement.GetProperty("content");
            return contentArray[0].GetProperty("text").GetString() ?? "[]";
        }

        private static string CleanJsonResponse(string text)
        {
            text = text.Trim();
            if (text.StartsWith("```json"))
                text = text.Substring(7);
            else if (text.StartsWith("```"))
                text = text.Substring(3);
            if (text.EndsWith("```"))
                text = text.Substring(0, text.Length - 3);
            return text.Trim();
        }
    }
}
