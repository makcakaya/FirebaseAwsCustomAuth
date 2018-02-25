using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Lambda.Tests
{
    public sealed class JwtValidatorIntegrationTests
    {
        private const string TOKEN = "YOUR_FIREBASE_JWT_TOKEN_HERE";
        private const string PROJECT_ID = "YOUR_FIREBASE_PROJECT_ID_HERE";

        [Theory]
        [InlineData(TOKEN, PROJECT_ID)]
        public void CanValidateValidToken(string token, string projectId)
        {
            var validator = new JwtValidator();
            var result = validator.Validate(token, projectId);

            Assert.True(result.IsValid, result.ErrorMesage);
            Assert.NotNull(result.Token);
        }

        [Theory]
        [InlineData(TOKEN, PROJECT_ID)]
        public void PerformanceTestValidation(string token, string projectId)
        {
            var tasks = new List<Task>();
            for (var i = 0; i < Environment.ProcessorCount; i++)
            {
                tasks.Add(Task.Run(() => RunValidationFor(1000, token, projectId)));
            }

            Task.WaitAll(tasks.ToArray());
        }

        private void RunValidationFor(int count, string token, string projectId)
        {
            var validator = new JwtValidator();
            for (var i = 0; i < count; i++)
            {
                validator.Validate(token, projectId);
            }
        }
    }
}
