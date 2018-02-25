
using Amazon.Lambda.Core;
using Lambda.Models;
using Lambda.Models.Auth;
using Newtonsoft.Json;
using System;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Lambda
{
    public class Function
    {
        public const string ProjectId = "markarar-android";
        private JwtValidator JwtService { get; } = new JwtValidator();

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
                context.Logger.LogLine($"{nameof(input.AuthorizationToken)}: {input.AuthorizationToken}");
                context.Logger.LogLine($"{nameof(input.MethodArn)}: {input.MethodArn}");

                // validate the incoming token
                // and produce the principal user identifier associated with the token

                // this could be accomplished in a number of ways:
                // 1. Call out to OAuth provider
                // 2. Decode a JWT token inline
                // 3. Lookup in a self-managed DB
                var result = JwtService.Validate(input.AuthorizationToken, ProjectId);
                if (!result.IsValid)
                {
                    throw new Exception(result.ErrorMesage);
                }
                var principalId = result.Token.Payload.Sub;
                context.Logger.LogLine($"sub: {principalId} is validated.");

                // you can send a 401 Unauthorized response to the client by failing like so:
                // throw new Exception("Unauthorized");

                // if the token is valid, a policy must be generated which will allow or deny access to the client

                // if access is denied, the client will receive a 403 Access Denied response
                // if access is allowed, API Gateway will proceed with the backend integration configured on the method that was called

                // build apiOptions for the AuthPolicy
                var methodArn = ApiGatewayArn.Parse(input.MethodArn);
                var apiOptions = new ApiOptions(methodArn.Region, methodArn.RestApiId, methodArn.Stage);

                // this function must generate a policy that is associated with the recognized principal user identifier.
                // depending on your use case, you might store policies in a DB, or generate them on the fly

                // keep in mind, the policy is cached for 5 minutes by default (TTL is configurable in the authorizer)
                // and will apply to subsequent calls to any method/resource in the RestApi
                // made with the same token

                // the example policy below denies access to all resources in the RestApi
                var policyBuilder = new AuthPolicyBuilder(principalId, methodArn.AwsAccountId, apiOptions);
                policyBuilder.AllowAllMethods();
                // policyBuilder.AllowMethod(HttpVerb.GET, "/users/username");

                // finally, build the policy
                var authResponse = policyBuilder.Build();

                //// new! -- add additional key-value pairs
                //// these are made available by APIGW like so: $context.authorizer.<key>
                //// additional context is cached
                //authResponse.Context.Add("key", "value"); // $context.authorizer.key -> value
                //authResponse.Context.Add("number", 1);
                //authResponse.Context.Add("bool", true);

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
