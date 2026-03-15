using System.Text.Json;
using EduLog.Core.Entities;
using Google.GenAI;
using Microsoft.Extensions.Configuration;

namespace EduLog.Services
{
    public class GeminiService : IAIQuestionService
    {
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

        public GeminiService(IConfiguration configuration)
        {
            _apiKey = configuration["GeminiApi:ApiKey"]
                ?? throw new InvalidOperationException("Gemini API anahtarı yapılandırılmamış. appsettings.json dosyasında 'GeminiApi:ApiKey' ayarını kontrol edin.");
            _model = configuration["GeminiApi:Model"] ?? "gemini-2.0-flash";
        }

        public async Task<List<AIGeneratedQuestion>> GenerateQuestionsAsync(string topic, string? notes, string? examples, int questionCount = 5)
        {
            var prompt = string.Format(PromptTemplate, topic, notes ?? "Yok", examples ?? "Yok", questionCount);
            var text = await SendGeminiRequestAsync(prompt);

            try
            {
                var cleaned = CleanJsonResponse(text);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var questions = JsonSerializer.Deserialize<List<AIGeneratedQuestion>>(cleaned, options);
                return questions ?? new List<AIGeneratedQuestion>();
            }
            catch (JsonException ex)
            {
                throw new AIServiceException(AIErrorType.JsonParseError, null, ex);
            }
        }

        public async Task<AIGeneratedCodeTask> GenerateCodeTaskAsync(string topic, string? notes, string? examples)
        {
            var prompt = string.Format(CodeTaskPromptTemplate, topic, notes ?? "Yok", examples ?? "Yok");
            var text = await SendGeminiRequestAsync(prompt);

            try
            {
                var cleaned = CleanJsonResponse(text);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var codeTask = JsonSerializer.Deserialize<AIGeneratedCodeTask>(cleaned, options);
                return codeTask ?? new AIGeneratedCodeTask();
            }
            catch (JsonException ex)
            {
                throw new AIServiceException(AIErrorType.JsonParseError, null, ex);
            }
        }

        public async Task<AICodeReview> ReviewCodeSubmissionAsync(string topic, string taskDescription, string studentCode, int maxScore)
        {
            var prompt = string.Format(CodeReviewPromptTemplate, topic, taskDescription, maxScore, studentCode);
            var text = await SendGeminiRequestAsync(prompt);

            try
            {
                var cleaned = CleanJsonResponse(text);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var review = JsonSerializer.Deserialize<AICodeReview>(cleaned, options);
                return review ?? new AICodeReview();
            }
            catch (JsonException ex)
            {
                throw new AIServiceException(AIErrorType.JsonParseError, null, ex);
            }
        }

        private async Task<string> SendGeminiRequestAsync(string prompt)
        {
            try
            {
                var client = new Client(apiKey: _apiKey);
                var response = await client.Models.GenerateContentAsync(
                    model: _model,
                    contents: prompt
                );
                return response.Candidates?[0]?.Content?.Parts?[0]?.Text ?? "[]";
            }
            catch (TaskCanceledException ex)
            {
                throw new AIServiceException(AIErrorType.Timeout, null, ex);
            }
            catch (HttpRequestException ex)
            {
                throw new AIServiceException(AIErrorType.NetworkError, null, ex);
            }
            catch (AIServiceException)
            {
                throw;
            }
            catch (Exception ex) when (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
            {
                throw new AIServiceException(AIErrorType.AuthenticationError, null, ex);
            }
            catch (Exception ex) when (ex.Message.Contains("429") || ex.Message.Contains("rate", StringComparison.OrdinalIgnoreCase))
            {
                throw new AIServiceException(AIErrorType.ApiLimitExceeded, null, ex);
            }
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
