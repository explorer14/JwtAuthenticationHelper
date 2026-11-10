namespace JwtHelper.Core.Types
{
    public class JwtValidationResult
    {
        private JwtValidationResult(bool isTokenValid, string reason)
        {
            IsTokenValid = isTokenValid;
            Reason = reason;
        }

        public bool IsTokenValid { get; }
        public string Reason { get; }

        internal static JwtValidationResult Failure(string reason) => new(isTokenValid: false, reason);

        internal static JwtValidationResult Success() => new(isTokenValid: true, reason: string.Empty);
    }
}