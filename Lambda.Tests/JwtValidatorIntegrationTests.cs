using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Lambda.Tests
{
    public sealed class JwtValidatorIntegrationTests
    {
        //private const string TOKEN = "YOUR_FIREBASE_JWT_TOKEN_HERE";
        //private const string PROJECT_ID = "YOUR_FIREBASE_PROJECT_ID_HERE";

        private const string TOKEN = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImUwZDU1NjQ5Yzk4MWI1ZWE2YTZkNzBhYTIyMDhiYWMxNjRkYTViMmMifQ.eyJpc3MiOiJodHRwczovL3NlY3VyZXRva2VuLmdvb2dsZS5jb20vbWFya2FyYXItYW5kcm9pZCIsIm5hbWUiOiJWdXJhbCBNZWNidXIiLCJhdWQiOiJtYXJrYXJhci1hbmRyb2lkIiwiYXV0aF90aW1lIjoxNTE5MDUwODYzLCJ1c2VyX2lkIjoiZkVURFRYR2NxeVFDUTRVTFdBeXdqVXFtdFBNMiIsInN1YiI6ImZFVERUWEdjcXlRQ1E0VUxXQXl3alVxbXRQTTIiLCJpYXQiOjE1MTk1OTUwODQsImV4cCI6MTUxOTU5ODY4NCwiZW1haWwiOiJ2dXJhbG1lY2J1ckBnbWFpbC5jb20iLCJlbWFpbF92ZXJpZmllZCI6dHJ1ZSwiZmlyZWJhc2UiOnsiaWRlbnRpdGllcyI6eyJlbWFpbCI6WyJ2dXJhbG1lY2J1ckBnbWFpbC5jb20iXX0sInNpZ25faW5fcHJvdmlkZXIiOiJwYXNzd29yZCJ9fQ.BXWi3noBaejVz0VNR5A57mwjQmbp4aK2Hpb5lTJM1A2sHCh7VizCtEfVRerxTH2-Le814pWUj-nXrZwLLr2Dr78GVKiNMj82hYwTRwVh9IdJib-NSYPsK8IJTDR_VxepupbuqOxz-3zD6_YMqgauWYFBReax29ePjUWs287eS-jceuiuDpt8lGh-2yfI0LRxBIErbwiteBP--arAAV48fo9S7oxlrDM-3jMdZYdOLZ5wxS5S-Tb8PSbgROnZFsN9ywoIaLgTg9wdeL-eJXpDMHeX-IY8uXlbHFLH-XsUsL6c9E0zm8JCX-xq8CVZhwYl29xOnuCGuqehU7uGre81dA";
        private const string PROJECT_ID = "markarar-android";

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
