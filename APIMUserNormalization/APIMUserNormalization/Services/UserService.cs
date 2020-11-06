﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using APIMUserNormalization.Models;
using Microsoft.Graph;
using Newtonsoft.Json;
using log4net;

namespace APIMUserNormalization.Services
{
    class UserService
    {
        ILog log;
        public UserService(ILog injectedLogger)
        {
            log = injectedLogger;
        }
        public static async Task ListUsers(GraphServiceClient graphClient)
        {
            Console.WriteLine("Getting list of users...");

            // Get all users (one page)
            var result = await graphClient.Users
                .Request()
                .Select(e => new
                {
                    e.DisplayName,
                    e.Id,
                    e.GivenName,
                    e.CompanyName,
                    e.Identities,
                    e.UserPrincipalName,
                    e.UserType
                })
                .GetAsync();

            int e = 1;
            foreach (var user in result.CurrentPage)
            {
                foreach (var identity in user.Identities)
                {
                    Console.WriteLine(e + ".- [DisplayName: " + user.DisplayName + "] | [UserType: " + user.UserType + "] | [GivenName: " + user.GivenName + " ] | [CompanyName: " + user.CompanyName + " ] | [Identities Issuer: " + identity.Issuer + " ] | [Identities SignIn Type: " + identity.SignInType + "] | [Identities Issuer: " + identity.IssuerAssignedId + " ] | [Id: " + user.Id + "] | [UserPrincipal Name: " + user.UserPrincipalName + " ]");

                }
                e++;
            }
        }

        public static async Task<IList<User>> GetAADB2CUsers(GraphServiceClient graphClient)
        {
            // Get all users (one page)
            var result = await graphClient.Users
                .Request()
                .Select(e => new
                {
                    e.DisplayName,
                    e.Id,
                    e.GivenName,
                    e.CompanyName,
                    e.Identities,
                    e.UserPrincipalName,
                    e.UserType
                })
                .GetAsync();

            return result.CurrentPage;
        }

        public static async Task ListUsersWithCustomAttribute(GraphServiceClient graphClient, string b2cExtensionAppClientId)
        {
            if (string.IsNullOrWhiteSpace(b2cExtensionAppClientId))
            {
                throw new ArgumentException("B2cExtensionAppClientId (its Application ID) is missing from appsettings.json. Find it in the App registrations pane in the Azure portal. The app registration has the name 'b2c-extensions-app. Do not modify. Used by AADB2C for storing user data.'.", nameof(b2cExtensionAppClientId));
            }

            // Declare the names of the custom attributes
            const string customAttributeName1 = "FavouriteSeason";
            const string customAttributeName2 = "LovesPets";

            // Get the complete name of the custom attribute (Azure AD extension)
            Helpers.B2cCustomAttributeHelper helper = new Helpers.B2cCustomAttributeHelper(b2cExtensionAppClientId);
            string favouriteSeasonAttributeName = helper.GetCompleteAttributeName(customAttributeName1);
            string lovesPetsAttributeName = helper.GetCompleteAttributeName(customAttributeName2);

            Console.WriteLine($"Getting list of users with the custom attributes '{customAttributeName1}' (string) and '{customAttributeName2}' (boolean)");
            Console.WriteLine();

            // Get all users (one page)
            var result = await graphClient.Users
                .Request()
                .Select($"id,displayName,identities,{favouriteSeasonAttributeName},{lovesPetsAttributeName}")
                .GetAsync();

            foreach (var user in result.CurrentPage)
            {
                Console.WriteLine(JsonConvert.SerializeObject(user));

                // Only output the custom attributes...
                //Console.WriteLine(JsonConvert.SerializeObject(user.AdditionalData));
            }
        }

        public static async Task<User> GetUserById(GraphServiceClient graphClient, string userId, string userName = "Null")
        {

            try
            {
                // Get user by object ID
                var result = await graphClient.Users[userId]
                    .Request()
                    .Select(e => new
                    {
                        e.DisplayName,
                        e.Id,
                        e.GivenName,
                        e.CompanyName,
                        e.Identities,
                        e.UserPrincipalName,
                        e.UserType

                    })
                    .GetAsync();

                return result;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                if (ex.Message.Contains("NotFound") || ex.Message.Contains("Not Found"))
                {
                    Console.WriteLine("User Name: (" + userName + ") User Id: (" + userId + ") Not Found in Azure AD B2C");
                    Console.WriteLine("Exception="  + ex.Message);
                }
                else
                {
                    Console.WriteLine(ex.Message);

                }
                Console.ResetColor();

            }
            return null;
        }

        public static async Task GetUserById(GraphServiceClient graphClient)
        {
            Console.Write("Enter user object ID: ");
            string userId = Console.ReadLine();

            Console.WriteLine($"Looking for user with object ID '{userId}'...");

            try
            {
                // Get user by object ID
                var result = await graphClient.Users[userId]
                    .Request()
                    .Select(e => new
                    {
                        e.DisplayName,
                        e.Id,
                        e.Identities
                    })
                    .GetAsync();

                if (result != null)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(result));
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        public static async Task GetUserBySignInName(AppSettings config, GraphServiceClient graphClient)
        {
            Console.Write("Enter user sign-in name (username or email address): ");
            string userId = Console.ReadLine();

            Console.WriteLine($"Looking for user with sign-in name '{userId}'...");

            try
            {
                // Get user by sign-in name
                var result = await graphClient.Users
                    .Request()
                    .Filter($"identities/any(c:c/issuerAssignedId eq '{userId}' and c/issuer eq '{config.AADB2CTenantId}')")
                    .Select(e => new
                    {
                        e.DisplayName,
                        e.Id,
                        e.Identities
                    })
                    .GetAsync();

                if (result != null)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(result));
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        public static async Task DeleteUserById(GraphServiceClient graphClient)
        {
            Console.Write("Enter user object ID: ");
            string userId = Console.ReadLine();

            Console.WriteLine($"Looking for user with object ID '{userId}'...");

            try
            {
                // Delete user by object ID
                await graphClient.Users[userId]
                   .Request()
                   .DeleteAsync();

                Console.WriteLine($"User with object ID '{userId}' successfully deleted.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        public static async Task SetPasswordByUserId(GraphServiceClient graphClient)
        {
            Console.Write("Enter user object ID: ");
            string userId = Console.ReadLine();

            Console.Write("Enter new password: ");
            string password = Console.ReadLine();

            Console.WriteLine($"Looking for user with object ID '{userId}'...");

            var user = new User
            {
                PasswordPolicies = "DisablePasswordExpiration,DisableStrongPassword",
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = false,
                    Password = password,
                }
            };

            try
            {
                // Update user by object ID
                await graphClient.Users[userId]
                   .Request()
                   .UpdateAsync(user);

                Console.WriteLine($"User with object ID '{userId}' successfully updated.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        public static async Task BulkCreate(AppSettings config, GraphServiceClient graphClient)
        {
            // Get the users to import
            string appDirectoryPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string dataFilePath = Path.Combine(appDirectoryPath, "Pending to Define");

            // Verify and notify on file existence
            if (!System.IO.File.Exists(dataFilePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"File '{dataFilePath}' not found.");
                Console.ResetColor();
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Starting bulk create operation...");

            // Read the data file and convert to object
            UsersModel users = UsersModel.Parse(System.IO.File.ReadAllText(dataFilePath));

            foreach (var user in users.Users)
            {
                user.SetB2CProfile(config.AADB2CTenantId);

                try
                {
                    // Create the user account in the directory
                    User user1 = await graphClient.Users
                                    .Request()
                                    .AddAsync(user);

                    Console.WriteLine($"User '{user.DisplayName}' successfully created.");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                }
            }
        }


        public static async Task<User> CreateUserFromAPIMToAADB2C(GraphServiceClient graphClient, string b2cExtensionAppClientId, string tenantId, UserContract user, bool migrationEnabled, string tableConnection)
        {
            string defaultPassword = "DevCenter2020!";
            if (string.IsNullOrWhiteSpace(b2cExtensionAppClientId))
            {
                throw new ArgumentException("B2C Extension App ClientId (ApplicationId) is missing in the appsettings.json. Get it from the App Registrations blade in the Azure portal. The app registration has the name 'b2c-extensions-app. Do not modify. Used by AADB2C for storing user data.'.", nameof(b2cExtensionAppClientId));
            }

            // Declare the names of the custom attributes
            //const string companyNameAttribute = "";


            // Get the complete name of the custom attribute (Azure AD extension)
            Helpers.B2cCustomAttributeHelper helper = new Helpers.B2cCustomAttributeHelper(b2cExtensionAppClientId);
            //string companyAttributeName = helper.GetCompleteAttributeName(companyNameAttribute);

            //Console.WriteLine($"Create a user with the custom attributes '{companyNameAttribute}' (string)");

            // Fill custom attributes
            //IDictionary<string, object> extensionInstance = new Dictionary<string, object>();
            //extensionInstance.Add(companyAttributeName, "ValueToBeAdded");

            Microsoft.Graph.User result = null;

            try
            {
                if (migrationEnabled)
                {
                    // Create user
                    result = await graphClient.Users
                    .Request()
                    .AddAsync(new User
                    {
                        GivenName = user.Properties.FirstName,
                        Surname = user.Properties.LastName,
                        DisplayName = user.Properties.FirstName + " " + user.Properties.LastName,
                        Identities = new List<ObjectIdentity>
                        {
                            new ObjectIdentity()
                            {
                                SignInType = "emailAddress",
                                Issuer = tenantId,
                                IssuerAssignedId = user.Properties.Email
                            }
                        },
                        PasswordProfile = new PasswordProfile()
                        {
                            //Password = Helpers.PasswordHelper.GenerateNewPassword(4, 8, 4)
                            Password = defaultPassword
                        },
                        PasswordPolicies = "DisablePasswordExpiration" //,
                        //AdditionalData = extensionInstance
                    });

                    string userId = result.Id;

                    // Get created user by object ID
                    result = await graphClient.Users[userId]
                        .Request()
                        //.Select($"id,givenName,surName,displayName,identities,{companyAttributeName}")
                        .Select($"id,givenName,surName,displayName,identities")
                        .GetAsync();
                }

                if (result != null)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"DisplayName: {result.DisplayName}");
                    Console.WriteLine();
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.White;
                    //Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
                }

                //ALB:  If we skipped the results because this is a test then add to log
                //      If we got the results then add to log
                if (result != null || !migrationEnabled)
                {
                    string jsonProps = JsonConvert.SerializeObject(user.Properties, Formatting.Indented);

                    var ats = new AzureTableService(tableConnection, "accountsLog");
                    ats.WriteSuccessEnablement(user.sourceAPIM, user.Properties.Email, jsonProps, defaultPassword, migrationEnabled);
                    //ALB:Done
                }

                return result;
            }
            catch (ServiceException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    //Console.WriteLine($"Have you created the custom attributes '{companyNameAttribute}' (string) in your tenant?");
                    Console.WriteLine();
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
            return null;
        }


        public static async Task CreateUserWithCustomAttribute(GraphServiceClient graphClient, string b2cExtensionAppClientId, string tenantId)
        {
            if (string.IsNullOrWhiteSpace(b2cExtensionAppClientId))
            {
                throw new ArgumentException("B2C Extension App ClientId (ApplicationId) is missing in the appsettings.json. Get it from the App Registrations blade in the Azure portal. The app registration has the name 'b2c-extensions-app. Do not modify. Used by AADB2C for storing user data.'.", nameof(b2cExtensionAppClientId));
            }

            // Declare the names of the custom attributes
            const string customAttributeName1 = "FavouriteSeason";
            const string customAttributeName2 = "LovesPets";

            // Get the complete name of the custom attribute (Azure AD extension)
            Helpers.B2cCustomAttributeHelper helper = new Helpers.B2cCustomAttributeHelper(b2cExtensionAppClientId);
            string favouriteSeasonAttributeName = helper.GetCompleteAttributeName(customAttributeName1);
            string lovesPetsAttributeName = helper.GetCompleteAttributeName(customAttributeName2);

            Console.WriteLine($"Create a user with the custom attributes '{customAttributeName1}' (string) and '{customAttributeName2}' (boolean)");

            // Fill custom attributes
            IDictionary<string, object> extensionInstance = new Dictionary<string, object>();
            extensionInstance.Add(favouriteSeasonAttributeName, "summer");
            extensionInstance.Add(lovesPetsAttributeName, true);

            try
            {
                // Create user
                var result = await graphClient.Users
                .Request()
                .AddAsync(new User
                {
                    GivenName = "Casey",
                    Surname = "Jensen",
                    DisplayName = "Casey Jensen",
                    Identities = new List<ObjectIdentity>
                    {
                        new ObjectIdentity()
                        {
                            SignInType = "emailAddress",
                            Issuer = tenantId,
                            IssuerAssignedId = "casey.jensen@example.com"
                        }
                    },
                    PasswordProfile = new PasswordProfile()
                    {
                        Password = Helpers.PasswordHelper.GenerateNewPassword(4, 8, 4)
                    },
                    PasswordPolicies = "DisablePasswordExpiration",
                    AdditionalData = extensionInstance
                });

                string userId = result.Id;

                Console.WriteLine($"Created the new user. Now get the created user with object ID '{userId}'...");

                // Get created user by object ID
                result = await graphClient.Users[userId]
                    .Request()
                    .Select($"id,givenName,surName,displayName,identities,{favouriteSeasonAttributeName},{lovesPetsAttributeName}")
                    .GetAsync();

                if (result != null)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"DisplayName: {result.DisplayName}");
                    Console.WriteLine($"{customAttributeName1}: {result.AdditionalData[favouriteSeasonAttributeName].ToString()}");
                    Console.WriteLine($"{customAttributeName2}: {result.AdditionalData[lovesPetsAttributeName].ToString()}");
                    Console.WriteLine();
                    Console.ResetColor();
                    Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
                }
            }
            catch (ServiceException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Have you created the custom attributes '{customAttributeName1}' (string) and '{customAttributeName2}' (boolean) in your tenant?");
                    Console.WriteLine();
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }
    }
}
