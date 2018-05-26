using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Lambda
{
    public sealed class JwtValidator
    {
        private const string GOOGLE_URL = "https://www.googleapis.com/robot/v1/metadata/x509/securetoken@system.gserviceaccount.com";
        private const string ISSUER_PREFIX = "https://securetoken.google.com/";
        private static Dictionary<string, string> IssuerSiginingKeysCache;
        private static object IssuerSigningKeyCacheLock = new object();
        private JwtSecurityTokenHandler _jwtHandler;

        public JwtSecurityTokenHandler JwtHandler
        {
            get
            {
                if (_jwtHandler == null)
                {
                    _jwtHandler = new JwtSecurityTokenHandler();
                }

                return _jwtHandler;
            }
        }

        public static async Task<Dictionary<string, string>> GetGoogleSigningKeys()
        {
            var client = new HttpClient();
            var response = await client.GetAsync(GOOGLE_URL);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            else
            {
                throw new HttpRequestException("Request failed with a status code: " + response.StatusCode);
            }
        }

        public JwtValidationResult Validate(string token, string projectId, IDictionary<string, string> issuerSigningKeys = null)
        {
            if (string.IsNullOrEmpty(token))
            {
                return JwtValidationResult.Invalid("Token not supplied.");
            }

            if (token.StartsWith("Bearer "))
            {
                token = token.Substring(7);
            }

            // Validate expiration and issued-at-time.
            var jwt = JwtHandler.ReadJwtToken(token ?? throw new ArgumentNullException(nameof(token)));
            var expOffset = DateTimeOffset.FromUnixTimeSeconds(jwt.Payload.Exp.Value);
            var iatOffset = DateTimeOffset.FromUnixTimeSeconds(jwt.Payload.Iat.Value);
            if (!ValidateLifetime(iatOffset.DateTime, expOffset.DateTime, jwt, null))
            {
                return JwtValidationResult.Invalid("Lifetime is invalid.");
            }

            // Validate audience.
            if (!jwt.Payload.Aud.Contains(projectId))
            {
                return JwtValidationResult.Invalid("Audience is invalid.");
            }

            // Validate issuer.
            if (jwt.Payload.Iss != ISSUER_PREFIX + projectId)
            {
                return JwtValidationResult.Invalid("Issuer is invalid.");
            }

            // Ensure sub(uid) exists.
            if (String.IsNullOrEmpty(jwt.Payload.Sub))
            {
                return JwtValidationResult.Invalid("Sub is invalid.");
            }

            string signingKey = null;
            if (issuerSigningKeys != null)
            {
                if (!issuerSigningKeys.TryGetValue(jwt.Header.Kid, out signingKey))
                {
                    return JwtValidationResult.Invalid("Signing keys does not contain kid.");
                }
            }
            else
            {
                lock (IssuerSigningKeyCacheLock)
                {
                    if (IssuerSiginingKeysCache == null || !IssuerSiginingKeysCache.ContainsKey(jwt.Header.Kid))
                    {
                        IssuerSiginingKeysCache = GetGoogleSigningKeys().Result;
                    }
                    if (!IssuerSiginingKeysCache.TryGetValue(jwt.Header.Kid, out signingKey))
                    {
                        return JwtValidationResult.Invalid("Signing keys does not contain kid.");
                    }
                }
            }

            JwtHandler.ValidateToken(token,
                new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudience = projectId,

                    ValidateIssuer = true,
                    ValidIssuer = ISSUER_PREFIX + projectId,

                    ValidateLifetime = true,
                    LifetimeValidator = new LifetimeValidator(ValidateLifetime),

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new X509SecurityKey(new X509Certificate2(Encoding.UTF8.GetBytes(signingKey)))

                },
                out SecurityToken securityToken);

            return JwtValidationResult.Valid(securityToken as JwtSecurityToken);
        }

        public bool ValidateLifetime(DateTime? iat, DateTime? exp, SecurityToken token, TokenValidationParameters param)
        {
            var offset = new DateTimeOffset(DateTime.Now.ToUniversalTime());
            if (iat.HasValue && iat.Value.Ticks > offset.Ticks)
            {
                return false;
            }

            if (exp.HasValue && exp.Value.Ticks < offset.Ticks)
            {
                return false;
            }

            return true;
        }
    }
}