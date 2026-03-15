namespace EduLog.Services
{
    public enum AIErrorType
    {
        JsonParseError,
        ApiLimitExceeded,
        Timeout,
        AuthenticationError,
        NetworkError,
        Unknown
    }

    public class AIServiceException : Exception
    {
        public AIErrorType ErrorType { get; }

        public AIServiceException(AIErrorType errorType, string? message = null, Exception? innerException = null)
            : base(message ?? GetDefaultMessage(errorType), innerException)
        {
            ErrorType = errorType;
        }

        public string UserFriendlyMessage => GetUserFriendlyMessage(ErrorType);

        public static string GetUserFriendlyMessage(AIErrorType errorType)
        {
            return errorType switch
            {
                AIErrorType.JsonParseError => "AI yanıtı beklenmeyen bir formatta döndü. Lütfen tekrar deneyin.",
                AIErrorType.ApiLimitExceeded => "AI servisinin kullanım limiti aşıldı. Lütfen birkaç dakika bekleyip tekrar deneyin.",
                AIErrorType.Timeout => "AI servisinden yanıt alınamadı (zaman aşımı). Lütfen tekrar deneyin.",
                AIErrorType.AuthenticationError => "AI servisine bağlanılamadı. API anahtarınızı kontrol edin.",
                AIErrorType.NetworkError => "AI servisine bağlantı kurulamadı. İnternet bağlantınızı kontrol edin.",
                _ => "AI servisinde beklenmeyen bir hata oluştu. Lütfen tekrar deneyin."
            };
        }

        private static string GetDefaultMessage(AIErrorType errorType)
        {
            return errorType switch
            {
                AIErrorType.JsonParseError => "Failed to parse AI response JSON.",
                AIErrorType.ApiLimitExceeded => "AI API rate limit exceeded.",
                AIErrorType.Timeout => "AI API request timed out.",
                AIErrorType.AuthenticationError => "AI API authentication failed.",
                AIErrorType.NetworkError => "Network error while calling AI API.",
                _ => "An unknown AI service error occurred."
            };
        }
    }
}
