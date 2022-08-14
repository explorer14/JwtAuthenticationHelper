namespace JwtHelper.Core.Types
{
    public class JwtValidationResult
    {
        private JwtValidationResult(bool isTokenValid, string reason)
        {
            IsTokenValid = isTokenValid;
            Reason = reason;
        }

        public bool IsTokenValid { get; private set; }
        public string Reason { get; private set; } = string.Empty;

        internal static JwtValidationResult Failure(string reason) =>
            new JwtValidationResult(false, reason);

        internal static JwtValidationResult Success() =>
            new JwtValidationResult(true, string.Empty);
    }
}