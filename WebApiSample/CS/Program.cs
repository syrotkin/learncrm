// =====================================================================
//  This file is part of the Microsoft Dynamics CRM SDK code samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  This source code is intended only as a supplement to Microsoft
//  Development Tools and/or on-line documentation.  See these other
//  materials for detailed information regarding Microsoft code samples.
//
//  THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//  PARTICULAR PURPOSE.
// =====================================================================
using Microsoft.Crm.Sdk.Samples.HelperCode;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Linq;

namespace Microsoft.Crm.Sdk.Samples
{
    /// <summary>
    /// This program performs basic create, retrieve, update, and delete (CRUD) operations
    /// against a CRM Online 2015 Update 1 or later organization using the Web API.
    /// </summary>
    /// <remarks>Before building this application, you must first modify the app.config file to replace
    /// the app settings with the correct values for your Azure app registration. Also, provide connection
    /// string service URL's for your organizations. See the provided app.config file for more information</remarks>
    class BasicOperations
    {
        static public void Main(string[] args)
        {
            try
            { 

                BasicOperations app = new BasicOperations();

                // The first argument on the command line is the optional connection string name.
                String[] arguments = Environment.GetCommandLineArgs();

                // Create a configuration object to store the service URL and app registration settings.
                Configuration config = null;
                if (arguments.Length > 1)
                    config = new Configuration(arguments[1], arguments[0] + ".config");
                else
                    config = new Configuration();

                // Authenticate the user.
                Authentication auth = new Authentication(config);

                Task.WaitAll( Task.Run(async () => await app.Run(auth, config)));
            }
 
            catch (System.Exception ex) { DisplayException(ex); }

            finally
            {
                Console.WriteLine("Press <Enter> to exit the program.");
                Console.ReadLine();
            }
        }

        public async Task Run(Authentication auth, Configuration config)
        {
            try
            {
                // Use an HttpClient object to communicate with the Web services.
                using (HttpClient httpClient = new HttpClient())
                {
                    // Define the Web API address of the service and the period of time each request has to execute.
                    httpClient.BaseAddress = new Uri(config.ServiceUrl);
                    httpClient.Timeout = new TimeSpan(0, 2, 0);  // 2 minutes
                    httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                    httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");

                    // Set the type of payload that will be accepted.
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    #region Create a entity
                    // Create an in-memory account using the early-bound Account class.
                    Account account = new Account();
                    account.name = "Contoso";
                    account.telephone1 = "555-5555";

                    // It is a best practice to refresh the access token before every message request is sent. Doing so
                    // avoids having to check the expiration date/time of the token. This operation is quick.
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);

                    // Send the request, and then check the response for success.
                    // POST api/data/accounts
                    HttpResponseMessage response =
                        await HttpClientExtensions.SendAsJsonAsync<Account>(httpClient, HttpMethod.Post, "api/data/v8.0/accounts", account);

                    if (response.IsSuccessStatusCode)
                        Console.WriteLine("Account '{0}' created.", account.name);
                    else
                        throw new Exception(String.Format("Failed to create account '{0}', reason is '{1}'.",
                                            account.name, response.ReasonPhrase), new CrmHttpResponseException(response.Content));
                    #endregion Create a entity

                    #region Retrieve a entity
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);

                    // Retrieve the previously created entity using its Uri. Limit returned properties using a select
                    // statement for improved performance.
                    // GET api/data/accounts(<guid>)?$select=name,telephone1

                    string accountUri = response.Headers.GetValues("OData-EntityId").FirstOrDefault();
                    var retrieveResponse = await httpClient.GetAsync(accountUri + "?$select=name,telephone1");

                    Account retrievedAccount = null;
                    if (retrieveResponse.IsSuccessStatusCode)
                    {
                        // Deserialize the content into an Account object.
                        retrievedAccount = JsonConvert.DeserializeObject<Account>(await retrieveResponse.Content.ReadAsStringAsync());
                        Console.WriteLine("Account '{0}' retrieved.", retrievedAccount.name);
                    }
                    else
                        throw new Exception(String.Format("Failed to retrieve the account, reason is '{0}'.",
                                            retrieveResponse.ReasonPhrase), new CrmHttpResponseException(response.Content));
                    #endregion retrieve a entity

                    // TODO: retrieve all accounts
                    #region Retrieve All accounts
                    // https://phillyhillel.crm.dynamics.com/api/data/v8.0/accounts?$select=name&$top=10
                    // GET
                    #endregion


                    #region Update a entity
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);

                    // Change just the account name.
                    // You should only specify properties you intend to update.
                    JObject accountToUpdate = new JObject();
                    accountToUpdate.Add("name", retrievedAccount.name + " Incorporated");

                    // Send the update request, and then check the response for success. 
                    // Note that accountUri includes the unique identifier for the entity so we do not have to set the accountid property in the updatedAccount object.
                    // PATCH api/data/acounts(<guid>)
                    response = await HttpClientExtensions.SendAsJsonAsync<JObject>(httpClient, new HttpMethod("PATCH"), accountUri, accountToUpdate);

                    if (response.IsSuccessStatusCode)
                        Console.WriteLine("Account '{0}' updated.", accountToUpdate["name"]);
                    else
                        throw new Exception(
                            String.Format("Failed to update account '{0}', reason is '{1}'.", retrievedAccount.name,
                            response.ReasonPhrase), new CrmHttpResponseException(response.Content));
                    #endregion Update a entity

                    #region Delete a entity
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);

                    // DELETE api/data/accounts(<guid>)
                    // Send the request, and then check the response for success.
                    response = await httpClient.DeleteAsync(accountUri);

                    if (response.IsSuccessStatusCode)
                        Console.WriteLine("Account '{0}' deleted.", retrievedAccount.name);
                    else
                        throw new Exception(
                            String.Format("Failed to delete account '{0}', reason is '{1}'.", retrievedAccount.name,
                            response.ReasonPhrase), new CrmHttpResponseException(response.Content));
                    #endregion Delete a entity
                }
            }
            catch (TimeoutException ex) { DisplayException(ex); }

            catch (HttpRequestException ex) { DisplayException(ex); }
        }

        private static void DisplayException(Exception ex)
        {
            Console.WriteLine("The application terminated with an error.");
            Console.WriteLine(ex.Message);
            while (ex.InnerException != null)
            {
                Console.WriteLine("\t* {0}", ex.InnerException.Message);
                ex = ex.InnerException;
            }
        }
    }
}
