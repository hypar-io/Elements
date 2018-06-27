using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;

namespace Hypar
{
    //https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/cognito-authentication-extension.html

    public class Cognito 
    {

        public static CognitoUser User{get;set;}

        private static string ClientId = "4lk1efk1vp923fnenc89itsihj";
        public static string UserPoolId = "us-west-2_DHGhR25BU";

        public static string IdentityPoolId = "us-west-2:d0cea890-84af-4af7-9398-fff1bf5268ee";

        public static void GetCredsAsync(string username, string password)
        {
            var provider = new AmazonCognitoIdentityProviderClient(new Amazon.Runtime.AnonymousAWSCredentials(), RegionEndpoint.USWest2);
            var userPool = new CognitoUserPool(UserPoolId, ClientId, provider);
            var user = new CognitoUser(username, ClientId, userPool, provider);
            var authRequest = new InitiateSrpAuthRequest()
            {
                Password = password
            };

            var authResponse = Task.Run(()=>user.StartWithSrpAuthAsync(authRequest)).Result;

            string accessToken;
            while (authResponse.AuthenticationResult == null)
            {
                Console.WriteLine("waiting...");
                if (authResponse.ChallengeName == ChallengeNameType.NEW_PASSWORD_REQUIRED)
                {
                    Console.WriteLine("Enter your desired new password:");
                    string newPassword = Console.ReadLine();

                    authResponse = Task.Run(()=>user.RespondToNewPasswordRequiredAsync(new RespondToNewPasswordRequiredRequest()
                    {
                        SessionID = authResponse.SessionID,
                        NewPassword = newPassword
                    })).Result;
                    accessToken = authResponse.AuthenticationResult.AccessToken;
                }
                else if (authResponse.ChallengeName == ChallengeNameType.SMS_MFA)
                {
                    Console.WriteLine("Enter the MFA Code sent to your device:");
                    string mfaCode = Console.ReadLine();

                    AuthFlowResponse mfaResponse = Task.Run(()=>user.RespondToSmsMfaAuthAsync(new RespondToSmsMfaRequest()
                    {
                        SessionID = authResponse.SessionID,
                        MfaCode = mfaCode

                    })).Result;
                    accessToken = authResponse.AuthenticationResult.AccessToken;
                }
                else
                {
                    Console.WriteLine("Unrecognized authentication challenge.");
                    accessToken = "";
                    break;
                }
            }

            if (authResponse.AuthenticationResult != null)
            {
                Console.WriteLine("User successfully authenticated.");

                User = user;
                // var userDetails = Task.Run(()=>user.GetUserDetailsAsync()).Result;

                // foreach(var kvp in userDetails.UserAttributes)
                // {
                //     Console.WriteLine(kvp.Name + ":" + kvp.Value);
                // }

                
                //TODO: Experiment with caching credentials
                // credentials.CacheCredentials()
            }
            else
            {
                Console.WriteLine("Error in authentication process.");
            }
        }
    }
}