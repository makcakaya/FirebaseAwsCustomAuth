using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Lambda.Tests
{
    public sealed class JwtValidatorIntegrationTests
    {
        private const string TOKEN = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImFhNzE5ZDE4MjQ2OTAyN2ZkYWQ5YzVlMjVmNTA0NWUzZjRhZTBjMTAifQ.eyJpc3MiOiJodHRwczovL3NlY3VyZXRva2VuLmdvb2dsZS5jb20vbWFya2FyYXItYW5kcm9pZCIsIm5hbWUiOiJkZXJ5YSBha2Nha2F5YSIsInBpY3R1cmUiOiJodHRwczovL3BsdXMuZ29vZ2xlLmNvbS9fL2ZvY3VzL3Bob3Rvcy9wcml2YXRlL0FJYkVpQUlBQUFCRENQbkl4NkNEOFBEU01TSUxkbU5oY21SZmNHaHZkRzhxS0dFeU9XRTJPREZrWWpBeVlqRmpZV1JsT1Raak5qa3lORFV5T1dFNE1qWTRPV1l6TVROaU5EZ3dBVFIwa0FyTzk1QnpZakFGNkpFZlp1TUpPX0xsIiwiYXVkIjoibWFya2FyYXItYW5kcm9pZCIsImF1dGhfdGltZSI6MTUyNzAwNjQxMCwidXNlcl9pZCI6Ijg1RDZKSXRxeE1WY3FzSDJUWUljbnNSNFBLNjIiLCJzdWIiOiI4NUQ2Skl0cXhNVmNxc0gyVFlJY25zUjRQSzYyIiwiaWF0IjoxNTI3MzM0MTEzLCJleHAiOjE1MjczMzc3MTMsImVtYWlsIjoiZGVyeWEuYWtjYWtheWFAZ21haWwuY29tIiwiZW1haWxfdmVyaWZpZWQiOnRydWUsImZpcmViYXNlIjp7ImlkZW50aXRpZXMiOnsiZW1haWwiOlsiZGVyeWEuYWtjYWtheWFAZ21haWwuY29tIl19LCJzaWduX2luX3Byb3ZpZGVyIjoicGFzc3dvcmQifX0.q7k0yKJVqIZJSbDu-M9J8OXznDJq_Xlbh_INV5a7EO4U_U8QXvMoioJz80-Nbbv3mWJ961bJzp3OX1Y9djk0MGuxJN-JhkjJcz3kgkj5o3PeKxEZnfIPXUF0bxH3favTBTYvwhrmt6_xC56jt-ZJRS5b3Z_-Mu0vCZZ43RIUdXKuXkENR4DozPhVTLeVTAXxwPDp9FEFA86n-RMjCwquieEgihdLeSX4MG8dusypYQFciJ50k6MW_zm0duT7ntY-usS56BuLqX8pNLfye_T9q9OsHE6B_fO3WHBeU0a5KogdkdtCLtDTBx5e6LPQBARtilarf7NvfIruFkSZScQ_pA";
        private const string BEARER_TOKEN = "Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6ImFhNzE5ZDE4MjQ2OTAyN2ZkYWQ5YzVlMjVmNTA0NWUzZjRhZTBjMTAifQ.eyJpc3MiOiJodHRwczovL3NlY3VyZXRva2VuLmdvb2dsZS5jb20vbWFya2FyYXItYW5kcm9pZCIsIm5hbWUiOiJkZXJ5YSBha2Nha2F5YSIsInBpY3R1cmUiOiJodHRwczovL3BsdXMuZ29vZ2xlLmNvbS9fL2ZvY3VzL3Bob3Rvcy9wcml2YXRlL0FJYkVpQUlBQUFCRENQbkl4NkNEOFBEU01TSUxkbU5oY21SZmNHaHZkRzhxS0dFeU9XRTJPREZrWWpBeVlqRmpZV1JsT1Raak5qa3lORFV5T1dFNE1qWTRPV1l6TVROaU5EZ3dBVFIwa0FyTzk1QnpZakFGNkpFZlp1TUpPX0xsIiwiYXVkIjoibWFya2FyYXItYW5kcm9pZCIsImF1dGhfdGltZSI6MTUyNzAwNjQxMCwidXNlcl9pZCI6Ijg1RDZKSXRxeE1WY3FzSDJUWUljbnNSNFBLNjIiLCJzdWIiOiI4NUQ2Skl0cXhNVmNxc0gyVFlJY25zUjRQSzYyIiwiaWF0IjoxNTI3MzM0MTEzLCJleHAiOjE1MjczMzc3MTMsImVtYWlsIjoiZGVyeWEuYWtjYWtheWFAZ21haWwuY29tIiwiZW1haWxfdmVyaWZpZWQiOnRydWUsImZpcmViYXNlIjp7ImlkZW50aXRpZXMiOnsiZW1haWwiOlsiZGVyeWEuYWtjYWtheWFAZ21haWwuY29tIl19LCJzaWduX2luX3Byb3ZpZGVyIjoicGFzc3dvcmQifX0.q7k0yKJVqIZJSbDu-M9J8OXznDJq_Xlbh_INV5a7EO4U_U8QXvMoioJz80-Nbbv3mWJ961bJzp3OX1Y9djk0MGuxJN-JhkjJcz3kgkj5o3PeKxEZnfIPXUF0bxH3favTBTYvwhrmt6_xC56jt-ZJRS5b3Z_-Mu0vCZZ43RIUdXKuXkENR4DozPhVTLeVTAXxwPDp9FEFA86n-RMjCwquieEgihdLeSX4MG8dusypYQFciJ50k6MW_zm0duT7ntY-usS56BuLqX8pNLfye_T9q9OsHE6B_fO3WHBeU0a5KogdkdtCLtDTBx5e6LPQBARtilarf7NvfIruFkSZScQ_pA";
        private const string PROJECT_ID = "markarar-android";

        [Theory]
        [InlineData(TOKEN, PROJECT_ID)]
        [InlineData(BEARER_TOKEN, PROJECT_ID)]
        public void CanValidateValidToken(string token, string projectId)
        {
            var validator = new JwtValidator();
            var result = validator.Validate(token, projectId);

            Assert.True(result.IsValid, result.ErrorMesage);
            Assert.NotNull(result.Token);
        }

        [Theory]
        [InlineData(TOKEN, PROJECT_ID)]
        [InlineData(BEARER_TOKEN, PROJECT_ID)]
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
