using System;
using System.IdentityModel.Tokens.Jwt;

namespace Lambda
{
    public sealed class JwtValidationResult
    {
        private readonly bool _isValid;
        private readonly JwtSecurityToken _token;
        private readonly string _errorMessage;

        public bool IsValid { get { return _isValid; } }

        public JwtSecurityToken Token { get { return _token; } }

        public string ErrorMesage { get { return _errorMessage ?? String.Empty; } }

        private JwtValidationResult(bool isValid, JwtSecurityToken token, string errorMessage)
        {
            _isValid = isValid;
            _token = token;
            _errorMessage = errorMessage;
        }

        public static JwtValidationResult Valid(JwtSecurityToken token)
        {
            return new JwtValidationResult(true, token ?? throw new ArgumentNullException(nameof(token)), null);
        }

        public static JwtValidationResult Invalid(string errorMessage)
        {
            return new JwtValidationResult(false, null, errorMessage);
        }
    }
}
