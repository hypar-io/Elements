using System;
using System.Text.RegularExpressions;
using System.Text;

using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;

namespace Hypar.Commands
{
    //https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/cognito-authentication-extension.html

    public class Cognito 
    {

        public static CognitoUser User{get;internal set;}
        public static string IdToken{get;internal set;}
        private static bool GetCredsAsync(string username, string password)
        {
            var provider = new AmazonCognitoIdentityProviderClient(new Amazon.Runtime.AnonymousAWSCredentials(), RegionEndpoint.USWest2);
            var userPool = new CognitoUserPool(Program.Configuration["cognito_user_pool_id"], Program.Configuration["cognito_client_id"], provider);
            var user = new CognitoUser(username, Program.Configuration["cognito_client_id"], userPool, provider);
            var authRequest = new InitiateSrpAuthRequest()
            {
                Password = password
            };

            var authResponse = Task.Run(()=>user.StartWithSrpAuthAsync(authRequest)).Result;

            string accessToken;
            while (authResponse.AuthenticationResult == null)
            {
                Logger.LogInfo("waiting...");
                if (authResponse.ChallengeName == ChallengeNameType.NEW_PASSWORD_REQUIRED)
                {
                    Logger.LogInfo("Enter your desired new password:");
                    var newPassword = GetConsolePassword();

                    if(!ValidatePassword(newPassword))
                    {
                        Logger.LogError("Passwords must be a minimum of 8 characters, and contain upper and lower case letters, a number, and a special character.");
                        return false;
                    }

                    authResponse = Task.Run(()=>user.RespondToNewPasswordRequiredAsync(new RespondToNewPasswordRequiredRequest()
                    {
                        SessionID = authResponse.SessionID,
                        NewPassword = newPassword
                    })).Result;
                    accessToken = authResponse.AuthenticationResult.AccessToken;
                }
                else if (authResponse.ChallengeName == ChallengeNameType.SMS_MFA)
                {
                    Logger.LogInfo("Enter the MFA Code sent to your device:");
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
                    Logger.LogError("Unrecognized authentication challenge.");
                    accessToken = "";
                    break;
                }
            }

            if (authResponse.AuthenticationResult != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Logger.LogSuccess($"{user.Username} successfully authenticated.");

                User = user;
                IdToken = authResponse.AuthenticationResult.IdToken;
                return true;

                //TODO: Experiment with caching credentials
                // credentials.CacheCredentials()
            }
            else
            {
                Logger.LogError("Error in authentication process.");
                return false;
            }
        }

        // Minimum eight characters, at least one uppercase letter, one lowercase letter, one number and one special character:
        public static bool ValidatePassword(string password)
        {
            var r = new Regex("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[$@$!%*?&])[A-Za-z\\d$@$!%*?&]{8,}");
            return r.Match(password).Success;
        }

        public static bool Login()
        {
            Logger.LogSuccess("Enter your user name:");
            var username = Console.ReadLine();
            
            Logger.LogSuccess("Enter your password:");
            var password = GetConsolePassword();

            bool response = false;
            try
            {
                response = Task.Run(()=>Cognito.GetCredsAsync(username, password)).Result;
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Logger.LogError("Login to Hypar failed.");
            }
            return response;
        }

        private static string GetConsolePassword( )
        {
            var sb = new StringBuilder( );
            while ( true )
            {
                ConsoleKeyInfo cki = Console.ReadKey( true );
                if ( cki.Key == ConsoleKey.Enter )
                {
                    Console.WriteLine( );
                    break;
                }

                if ( cki.Key == ConsoleKey.Backspace || cki.Key == ConsoleKey.Delete )
                {
                    if ( sb.Length > 0 )
                    {
                        Console.Write( "\b \b" );
                        sb.Length--;
                    }
                    continue;
                }
                else
                {
                    Console.Write( '*' );
                    sb.Append( cki.KeyChar );
                }
            }

            return sb.ToString();
        }
    }
}