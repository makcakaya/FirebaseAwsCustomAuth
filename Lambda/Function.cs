
using Amazon.Lambda.Core;
using Lambda.Models;
using Lambda.Models.Auth;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json;
using System;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Lambda
{
    public class Function
    {
        public const string ProjectId = "markarar-android";
        private JwtValidator Validator { get; } = new JwtValidator();

        /// <summary>
        /// A simple function that takes the token authorizer and returns a policy based on the authentication token included.
        /// </summary>
        /// <param name="input">token authorization received by api-gateway event sources</param>
        /// <param name="context"></param>
        /// <returns>IAM Auth Policy</returns>
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public AuthPolicy FunctionHandler(TokenAuthorizerContext input, ILambdaContext context)
        {
            try
            {
                IdentityModelEventSource.ShowPII = true;

                context.Logger.LogLine($"{nameof(input.AuthorizationToken)}: {input.AuthorizationToken}");
                context.Logger.LogLine($"{nameof(input.MethodArn)}: {input.MethodArn}");
                var result = Validator.Validate(input.AuthorizationToken, ProjectId);
                var principalId = result.IsValid ? result.Token.Payload.Sub : null;
                context.Logger.LogLine($"Is sub={principalId} valid: {result.IsValid}");
                var methodArn = ApiGatewayArn.Parse(input.MethodArn);
                var apiOptions = new ApiOptions(methodArn.Region, methodArn.RestApiId, methodArn.Stage);
                var policyBuilder = new AuthPolicyBuilder(principalId, methodArn.AwsAccountId, apiOptions);
                if (principalId != null)
                {
                    policyBuilder.AllowAllMethods();
                }
                else
                {
                    policyBuilder.AllowMethod(HttpVerb.Post, "/api/scrape/scrape");
                }

                var authResponse = policyBuilder.Build();
                return authResponse;
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"Exception caught: {JsonConvert.SerializeObject(ex)}");
                throw ex;
            }
        }
    }
}
