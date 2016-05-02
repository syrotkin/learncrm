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

//<snippetSimplifiedConnection>
using System;
using System.ComponentModel.Design;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;

// These namespaces are found in the Microsoft.Crm.Sdk.Proxy.dll assembly
// located in the SDK\bin folder of the SDK download.
using Elca;

using Microsoft.Crm.Sdk.Messages;

// These namespaces are found in the Microsoft.Xrm.Sdk.dll assembly
// located in the SDK\bin folder of the SDK download.
using Microsoft.IdentityModel.Protocols.WSIdentity;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;

// These namespaces are found in the Microsoft.Xrm.Client.dll assembly
// located in the SDK\bin folder of the SDK download.
using System.Collections.Generic;

namespace Microsoft.Crm.Sdk.Samples
{
    /// <summary>
    /// This sample uses the CrmConnection class found in the Microsoft.Xrm.Client
    /// namespace to connect to and authenticate with the organization web service.
    /// 
    /// Next, the sample demonstrates how to do basic entity operations like create,
    /// retrieve, update, and delete.</summary>
    /// <remarks>
    /// At run-time, you will be given the option to delete all the database
    /// records created by this program.
    /// 
    /// No helper code from CrmServiceHelpers.cs is used in this sample.</remarks>
    /// <see cref="http://msdn.microsoft.com/en-us/library/gg695810.aspx"/>
    public class SimplifiedConnection
    {
        #region Class Level Members

        private Guid _accountId;
        private IOrganizationService _organizationService;
        private Guid _newContactId;

        #endregion Class Level Members

        /// <summary>
        /// The Run() method first connects to the Organization service. Afterwards,
        /// basic create, retrieve, update, and delete entity operations are performed.
        /// </summary>
        /// <param name="connectionString">Provides service connection information.</param>
        /// <param name="promptforDelete">When True, the user will be prompted to delete all
        /// created entities.</param>
        public void Run(String connectionString, bool promptforDelete)
        {
            try
            {
                // Connect to the CRM web service using a connection string.
                CrmServiceClient conn = new Xrm.Tooling.Connector.CrmServiceClient(connectionString);

                // Cast the proxy client to the IOrganizationService interface.
                _organizationService = (IOrganizationService)conn.OrganizationWebProxyClient != null ? (IOrganizationService)conn.OrganizationWebProxyClient : (IOrganizationService)conn.OrganizationServiceProxy;

                //Create any entity records this sample requires.
                CreateRequiredRecords();

                DisplayLoggedOnUserInformation();

                DisplayDynamicsCrmVersion();

                //TryWebApi();


                var searchString = GetAccountSearchString();
                RetrieveMultipleAccountsUsingLinq(searchString);


                string contactSearchString = "Holmes";
                RetrieveAndUpdateAccountsByName(contactSearchString);

                const string contactLastName = "Holmes 3";
                Console.WriteLine("Creating contact: " + contactLastName);
                var contact = CreateContact(contactLastName);
                var retrievedContact = RetrieveContactById();
                Console.WriteLine("created contact retrieved: " + retrievedContact.FullName + ", ELCA: " + retrievedContact.GetAttributeValue<bool>("new_iselcaemployee"));

                // create a new animal (Late bound):
                var entity = new Entity("new_animal");
                entity["new_name"] = "Pluto";
                var _animalId = _organizationService.Create(entity);
                Console.WriteLine("Animal created: " + entity.LogicalName + ", name: " + entity.Attributes["new_name"]);


                string accountName = GetAccountName();
                var account = CreateAccount(accountName);
                Console.Write("{0} {1} created, ", account.LogicalName, account.Name);

                var retrievedAccount = RetrieveAccount();
                Console.Write("retrieved, ");
                Console.WriteLine();
                // displaying the retrieved attributes of the account:
                Console.WriteLine("Account information: "
                                  + Environment.NewLine
                                  + "name: {0}, address1_postalcode: {1}, lastusedincampaign: {2}", retrievedAccount.Name,
                                  retrievedAccount.Address1_PostalCode, retrievedAccount.LastUsedInCampaign);


                UpdateAccount(retrievedAccount);
                Console.WriteLine("and updated.");


                //ExecuteActions(retrievedAccount);
                ExecuteCallAddActivityAction();

                // Delete any entity records this sample created.
                DeleteRequiredRecords(promptforDelete);
            }

            // Catch any service fault exceptions that Microsoft Dynamics CRM throws.
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>)
            {
                // You can handle an exception here or pass it back to the calling method.
                throw;
            }
        }

        // 1. Create Action
        // 2. Go to a solution, Processes, Add Existing
        // 3. run crmSvcUtil with all the parameters + /generateActions
        // 4. Use the generated .cs file with the correct namespace, correct arguments
        private void ExecuteActions(Account retrievedAccount) {
            new_DelegateRequest request = new new_DelegateRequest();
            request.EntityCollection = new EntityCollection(new List<Entity> {
                retrievedAccount
            });
            request.Subject = "dummy action request";
            request.Target = new EntityReference("account", retrievedAccount.Id);
            var response = _organizationService.Execute(request);
        }

        private void ExecuteCallAddActivityAction() {
            new_CallAddActivityRequest request = new new_CallAddActivityRequest();
            request.firstSummand = 2;
            request.secondSummand = 3;
            //request.Target =  // no target, it is a global action
            var response = _organizationService.Execute(request);
        }

        /*
         E:\Downloads\Installers\dev\CRM\CRMSDK\SDK\Bin>CrmSvcUtil.exe /url:http://phillyhillel.crm.dynamics.com/XRMServices/2011/Organization.svc /out:OsyTypes.cs /username:bobmeierus
                er@phillyhillel.onmicrosoft.com /password:xxxxxxx /namespace:Elca /serviceContextName:ServiceContext /generateActions
         */

        private async void TryWebApi() {
            using (var httpClient = new HttpClient()) {
                // Define the Web API address of the service and the period of time each request has to execute.
                httpClient.BaseAddress = new Uri("https://phillyhillel.crm.dynamics.com");
                httpClient.Timeout = new TimeSpan(0, 2, 0);  // 2 minutes
                httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");

                // Set the type of payload that will be accepted.
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                #region Create a entity
                // Create an in-memory account using the early-bound Account class.
                Account account = new Account {
                    Name = "Fourth Coffee"
                };
                //account.name = "Contoso";
                //account.telephone1 = "555-5555";

                // It is a best practice to refresh the access token before every message request is sent. Doing so
                // avoids having to check the expiration date/time of the token. This operation is quick.
                // OSY: commented this out.... because I don't have "auth", because I don't have a ClientId
                //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);


                // Send the request, and then check the response for success.
                // POST api/data/accounts
                //HttpResponseMessage response =
                //    await HttpClientExtensions.SendAsJsonAsync<Account>(httpClient, HttpMethod.Post, "api/data/accounts", account);

                HttpResponseMessage response =
                    await HttpClientExtensions.SendAsJsonAsync<Account>(httpClient, HttpMethod.Get, "api/data/v8.0/accounts?$select=name&$top=10", null);

                if (response.IsSuccessStatusCode)
                    Console.WriteLine("Account '{0}' created.", account.Name);
                else
                    throw new Exception(String.Format("Failed to create account '{0}', reason is '{1}'.",
                                        account.Name, response.ReasonPhrase), new CrmHttpResponseException(response.Content));
                #endregion Create a entity
            }
        }

        private Contact CreateContact(string contactName) {
            var contact = new Contact {
                FirstName = "Sherlock",
                LastName = contactName,
                AccountRoleCode = new OptionSetValue((int)ContactAccountRoleCode.Employee),
                CustomerTypeCode = new OptionSetValue((int)ContactCustomerTypeCode.DefaultValue)
            };
            _newContactId = _organizationService.Create(contact);
            return contact;
        }

        private Account CreateAccount(string accountName) {
            // Instantiate an account object. Note the use of option set enumerations defined in OptionSets.cs.
            // Refer to the Entity Metadata topic in the SDK documentation to determine which attributes must
            // be set for each entity.

            var account = new Account {
                Name = accountName,
                AccountCategoryCode = new OptionSetValue((int)AccountAccountCategoryCode.PreferredCustomer),
                CustomerTypeCode = new OptionSetValue((int)AccountCustomerTypeCode.Investor)
            };

            // Create an account record named Fourth Coffee.
            _accountId = _organizationService.Create(account);
            return account;


        }

        private string GetAccountSearchString() {
            Console.Write("Enter a search string to search for accounts: ");
            string searchString = Console.ReadLine();
            if (string.IsNullOrEmpty(searchString)) {
                return "Coffee";
            }
            return searchString;
        }

        private string GetAccountName() {
            Console.Write("Enter account name to create: ");
            string name = Console.ReadLine();
            if (string.IsNullOrEmpty(name)) {
                return "Sixth Coffee";
            }
            return name;
        }

        private void DisplayLoggedOnUserInformation() {
            // Obtain information about the logged on user from the web service.
            Guid userid = ((WhoAmIResponse)_organizationService.Execute(new WhoAmIRequest())).UserId;
            SystemUser systemUser = (SystemUser)_organizationService.Retrieve("systemuser", userid,
                    new ColumnSet(new string[] { "firstname", "lastname" }));
            Console.WriteLine("Logged on user is {0} {1}.", systemUser.FirstName, systemUser.LastName);
        }

        private void CreateWorkflow()
        {
            // Create a real-time workflow. 
            // The workflow should execute after a new opportunity is created
            // and run in the context of the logged on user.
            Workflow workflow = new Workflow()
            {
                // These properties map to the New Process form settings in the web application.
                Name = "Set closeprobability on opportunity create (real-time)",
                Type = new OptionSetValue((int)WorkflowType.Definition),
                Category = new OptionSetValue((int)WorkflowCategory.Workflow),
                PrimaryEntity = Opportunity.EntityLogicalName,
                Mode = new OptionSetValue((int)WorkflowMode.Realtime),

                // Additional settings from the second New Process form.
                Description = @"When an opportunity is created, this workflow" +
                    " sets the closeprobability field of the opportunity record to 40%.",
                OnDemand = false,
                Subprocess = false,
                Scope = new OptionSetValue((int)WorkflowScope.User),
                RunAs = new OptionSetValue((int)workflow_runas.CallingUser),
                SyncWorkflowLogOnFailure = true,
                TriggerOnCreate = true,
                CreateStage = new OptionSetValue((int)workflow_stage.Postoperation),
                // TODO-osy: figure out what to do with XAML
                //Xaml = xamlWF,

                // Other properties not in the web forms.
                LanguageCode = 1033,  // U.S. English
            };
            Guid _workflowId = _organizationService.Create(workflow);
        }

        private void DisplayDynamicsCrmVersion() {
            // Retrieve the version of Microsoft Dynamics CRM.
            RetrieveVersionRequest versionRequest = new RetrieveVersionRequest();
            RetrieveVersionResponse versionResponse =
                    (RetrieveVersionResponse)_organizationService.Execute(versionRequest);
            Console.WriteLine("Microsoft Dynamics CRM version {0}.", versionResponse.Version);
        }

        private Account RetrieveAccount() {
            // Retrieve the several attributes from the new account.
            ColumnSet columnSet = new ColumnSet(
                    new[] {"name", "address1_postalcode", "lastusedincampaign", "accountid"});
            Account retrievedAccount = (Account)_organizationService.Retrieve("account", _accountId, columnSet);
            return retrievedAccount;
        }

        private Contact RetrieveContactById() {
            ColumnSet columnSet = new ColumnSet(new[] {"fullname", "firstname", "lastname", "new_iselcaemployee"});
            var retrievedContact = (Contact)_organizationService.Retrieve("contact", _newContactId, columnSet);
            return retrievedContact;
        }

        private void RetrieveAndUpdateAccountsByName(string contactSearchString) {
            IList<Contact> contactList;
            using (var serviceContext = new ServiceContext(_organizationService)) {
                var contacts = serviceContext.ContactSet
                        .Where(contact => contact.FullName.Contains(contactSearchString))
                        .OrderBy(contact => contact.FullName);
                Console.WriteLine("retrieved contacts: ");
                contactList = contacts.ToList();

                foreach (var contact in contactList) {
                    Console.WriteLine(contact.FullName + ", ELCA: " + contact.GetAttributeValue<bool>("new_iselcaemployee"));
                }
                foreach (var contact in contactList) {
                    contact.Attributes["new_iselcaemployee"] = true;
                    serviceContext.UpdateObject(contact);
                    _organizationService.Update(contact);
                }
            }
        }

        private void RetrieveMultipleAccountsUsingLinq(string searchString) {
            using (var serviceContext = new ServiceContext(_organizationService)) {
                var accounts = serviceContext.AccountSet
                        .Where(account => account.Name.Contains(searchString))
                        .OrderBy(account => account.Name)
                        .Select(account => new {
                            account.Name,
                            account.AccountId
                        });
                Console.WriteLine("Retrieved accounts: ");
                foreach (var account in accounts) {
                    Console.WriteLine(account.Name + ": " + account.AccountId);
                }
            }
        }
        
        private void UpdateAccount(Account account) {
            // Update the postal code attribute.
            account.Address1_PostalCode = "98052";

            // The address 2 postal code was set accidentally, so set it to null.
            account.Address2_PostalCode = null;

            // Shows use of a Money value.
            account.Revenue = new Money(5000000);

            // Shows use of a Boolean value.
            account.CreditOnHold = false;

            // Update the account record.
            _organizationService.Update(account);
        }

        #region Public Methods
        /// <summary>
        /// Creates any entity records this sample requires.
        /// </summary>
        public void CreateRequiredRecords()
        {
            // For this sample, all required entities are created in the Run() method.
        }

        /// <summary>
        /// Deletes any entity records that were created for this sample.
        /// <param name="prompt">Indicates whether to prompt the user 
        /// to delete the records created in this sample.</param>
        /// </summary>
        public void DeleteRequiredRecords(bool prompt)
        {
            bool deleteRecords = true;

            if (prompt)
            {
                Console.Write("\nDo you want these entity records deleted? (y/n) [y]: ");
                String answer = Console.ReadLine();

                deleteRecords = (answer.StartsWith("y") || answer.StartsWith("Y") || answer == String.Empty);
            }

            if (deleteRecords)
            {
                _organizationService.Delete(Account.EntityLogicalName, _accountId);
                Console.WriteLine("Entity records have been deleted.");
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Gets web service connection information from the app.config file.
        /// If there is more than one available, the user is prompted to select
        /// the desired connection configuration by name.
        /// </summary>
        /// <returns>A string containing web service connection configuration information.</returns>
        private static String GetServiceConfiguration()
        {
            // Get available connection strings from app.config.
            int count = ConfigurationManager.ConnectionStrings.Count;

            // Create a filter list of connection strings so that we have a list of valid
            // connection strings for Microsoft Dynamics CRM only.
            List<KeyValuePair<String, String>> filteredConnectionStrings = 
                new List<KeyValuePair<String, String>>();

            for (int a = 0; a < count; a++)
            {
                if (isValidConnectionString(ConfigurationManager.ConnectionStrings[a].ConnectionString))
                    filteredConnectionStrings.Add
                        (new KeyValuePair<string, string>
                            (ConfigurationManager.ConnectionStrings[a].Name,
                            ConfigurationManager.ConnectionStrings[a].ConnectionString));
            }

            // No valid connections strings found. Write out and error message.
            if (filteredConnectionStrings.Count == 0)
            {
                Console.WriteLine("An app.config file containing at least one valid Microsoft Dynamics CRM " +
                    "connection string configuration must exist in the run-time folder.");
                Console.WriteLine("\nThere are several commented out example connection strings in " +
                    "the provided app.config file. Uncomment one of them and modify the string according " +
                    "to your Microsoft Dynamics CRM installation. Then re-run the sample.");
                return null;
            }

            // If one valid connection string is found, use that.
            if (filteredConnectionStrings.Count == 1)
            {
                return filteredConnectionStrings[0].Value;
            }

            // If more than one valid connection string is found, let the user decide which to use.
            if (filteredConnectionStrings.Count > 1)
            {
                Console.WriteLine("The following connections are available:");
                Console.WriteLine("------------------------------------------------");

                for (int i = 0; i < filteredConnectionStrings.Count; i++)
                {
                    Console.Write("\n({0}) {1}\t",
                    i + 1, filteredConnectionStrings[i].Key);
                }

                Console.WriteLine();

                Console.Write("\nType the number of the connection to use (1-{0}) [{0}] : ", 
                    filteredConnectionStrings.Count);
                String input = Console.ReadLine();
                int configNumber;
                if (input == String.Empty) input = filteredConnectionStrings.Count.ToString();
                if (!Int32.TryParse(input, out configNumber) || configNumber > count || 
                    configNumber == 0)
                {
                    Console.WriteLine("Option not valid.");
                    return null;
                }

                return filteredConnectionStrings[configNumber - 1].Value;

            }
            return null;
            
        }


        /// <summary>
        /// Verifies if a connection string is valid for Microsoft Dynamics CRM.
        /// </summary>
        /// <returns>True for a valid string, otherwise False.</returns>
        private static Boolean isValidConnectionString(String connectionString)
        {
            // At a minimum, a connection string must contain one of these arguments.
            if (connectionString.Contains("Url=") ||
                connectionString.Contains("Server=") ||
                connectionString.Contains("ServiceUri="))
                return true;

            return false;
        }
        
        #endregion Private Methods

        #region Main method

        /// <summary>
        /// Standard Main() method used by most SDK samples.
        /// </summary>
        /// <param name="args"></param>
        static public void Main(string[] args)
        {
            try
            {
                // Obtain connection configuration information for the Microsoft Dynamics
                // CRM organization web service.
                String connectionString = GetServiceConfiguration();

                if (connectionString != null)
                {
                    SimplifiedConnection app = new SimplifiedConnection();
                    app.Run(connectionString, true);
                }
            }

            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                Console.WriteLine("The application terminated with an error.");
                Console.WriteLine("Timestamp: {0}", ex.Detail.Timestamp);
                Console.WriteLine("Code: {0}", ex.Detail.ErrorCode);
                Console.WriteLine("Message: {0}", ex.Detail.Message);
                Console.WriteLine("Trace: {0}", ex.Detail.TraceText);
                Console.WriteLine("Inner Fault: {0}",
                    null == ex.Detail.InnerFault ? "No Inner Fault" : "Has Inner Fault");
            }
            catch (System.TimeoutException ex)
            {
                Console.WriteLine("The application terminated with an error.");
                Console.WriteLine("Message: {0}", ex.Message);
                Console.WriteLine("Stack Trace: {0}", ex.StackTrace);
                Console.WriteLine("Inner Fault: {0}",
                    null == ex.InnerException.Message ? "No Inner Fault" : ex.InnerException.Message);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("The application terminated with an error.");
                Console.WriteLine(ex.Message);

                // Display the details of the inner exception.
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);

                    FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> fe = ex.InnerException
                        as FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>;
                    if (fe != null)
                    {
                        Console.WriteLine("Timestamp: {0}", fe.Detail.Timestamp);
                        Console.WriteLine("Code: {0}", fe.Detail.ErrorCode);
                        Console.WriteLine("Message: {0}", fe.Detail.Message);
                        Console.WriteLine("Trace: {0}", fe.Detail.TraceText);
                        Console.WriteLine("Inner Fault: {0}",
                            null == fe.Detail.InnerFault ? "No Inner Fault" : "Has Inner Fault");
                    }
                }
            }
            
            // Additional exceptions to catch: SecurityTokenValidationException, ExpiredSecurityTokenException,
            // SecurityAccessDeniedException, MessageSecurityException, and SecurityNegotiationException.

            finally
            {
                Console.WriteLine("Press <Enter> to exit.");
                Console.ReadLine();
            }
        }
        #endregion Main method
    }
}
//</snippetSimplifiedConnection>
