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
using System;
using System.Security;
using System.Collections;
using System.Configuration;

namespace Microsoft.Crm.Sdk.Samples.HelperCode
{
    /// <summary>
    /// An application configuration. This configuration contains the service
    /// and application settings required for CRM Web API sample code to run.
    /// </summary>
    public class Configuration
    {
        #region Properties
        private string _serviceUrl = String.Empty;
        private string _redirectUrl = String.Empty;
        private string _clientId = String.Empty;
        private string _path = String.Empty;
        private string _connectionName = String.Empty;
        private string _username = String.Empty;
        private SecureString _password = null;

        /// <summary>
        /// The root address of the Dynamics CRM service.
        /// </summary>
        /// <example>https://myorg.crm.dynamics.com</example>
        public string ServiceUrl
        {
            get { return _serviceUrl; }
            set { _serviceUrl = value; } 
        }

        /// <summary>
        /// The redirect address provided when the app was registered in Microsoft Azure Active Directory.
        /// </summary>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/dn531010(v=crm.7).aspx#bkmk_redirect"/>
        public string RedirectUrl
        {
            get { return _redirectUrl; }
            set { _redirectUrl = value; }
        }

        /// <summary>
        /// The client ID that was generated when the app was registered in Microsoft Azure
        /// Active Directory.
        /// </summary>
        public string ClientId
        {
            get { return _clientId; }
            set { _clientId = value; }
        }

        /// <summary>
        /// The full or relative path to the application's configuration file.
        /// </summary>
        /// <remarks>The file name is in the format <appname>.exe.config.</appname></remarks>
        public string PathToConfig
        {
            get { return _path; }
            set { _path = value; }
        }

        /// <summary>
        /// The user name of the logged on user or null.
        /// </summary>
        public string Username
        {
            get { return _username; }
            set { _username  = value; }
        }

        /// <summary>
        ///  The password of the logged on user or null.
        /// </summary>
        public SecureString Password
        {
            get { return _password; }
            set { _password = value; }
        }
        #endregion Properties

        #region Constructors
        /// <summary>
        /// Loads the parameters of the first connection string and application settings from the configuration file.
        /// </summary>
        /// <remarks>The app.config file must exist in the run-time folder and have the name <appname>.exe.config.</remarks>
        public Configuration()
        {
            var path = System.IO.Path.Combine(Environment.CurrentDirectory, Environment.GetCommandLineArgs()[0]);

            ReadConfigFile(null, path + ".config");
        }

        /// <summary>
        /// Loads a named connection string and application settings from the configuration file.
        /// </summary>
        /// <param name="name">The name of the target connection string.</param>
        /// <param name="path">The full or relative pathname of the configuration file.</param>
        /// <remarks>The app.config file must exist in the run-time folder and have the name <appname>.exe.config.</remarks>
        public Configuration(string name, string path) 
        {
            ReadConfigFile(name, path);
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Loads server connection information and application settings from the configuration file.
        /// </summary>
        /// <remarks>A setting named OverrideConfig can optionally be added. If a config file that this setting
        /// refers to exists, that config file is read instead of the config file specified in the path parameter.
        /// This allows for an alternate config file, for example a global config file shared by multiple applications.
        /// </summary>
        /// <param name="connectionName">The name of the connection string in the configuration file to use. 
        /// Each CRM organization can have its own connection string. A value of null or String.Empty results
        /// in the first (top most) connection string being used.</param>
        /// <param name="path">The full or relative pathname of the configuration file.</param>
        public void ReadConfigFile(string connectionName, string path)
        {
            // Check passed parameters.
            if ( string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
                throw new ArgumentException("The specified app.config file path is invalid.", this.ToString());
            else
                _path = path;

            try
            {
                // Read the app.config file and obtain the app settings.
                System.Configuration.Configuration config = null;
                ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
                configFileMap.ExeConfigFilename = _path;
                config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

                var appSettings = config.AppSettings.Settings;

                // If an alternate config file exists, load that configuration instead. Note the test
                // for redirectTo.Equals(path) to avoid an infinite loop.
                if (appSettings["AlternateConfig"] != null)
                {
                    var redirectTo = appSettings["AlternateConfig"].Value;
                    if (redirectTo != null && !redirectTo.Equals(path) && System.IO.File.Exists(redirectTo))
                    {
                        ReadConfigFile(connectionName, redirectTo);
                        return;
                    }
                }

                // Get the connection string.
                ConnectionStringSettings connection;
                if (string.IsNullOrEmpty(connectionName))
                {
                    // No connection string name specified, so use the first one in the file.
                    connection = config.ConnectionStrings.ConnectionStrings[0];
                    _connectionName = connection.Name;
                }
                else
                {
                    connection = config.ConnectionStrings.ConnectionStrings[connectionName];
                    _connectionName = connectionName;
                }

                // Get the connection string parameter values.
                if (connection != null)
                {
                    var parameters = connection.ConnectionString.Split(';');
                    foreach (string parameter in parameters)
                    {
                        var trimmedParameter = parameter.Trim();
                        if (trimmedParameter.StartsWith("Url="))
                            _serviceUrl = parameter.Replace("Url=", String.Empty);

                        if (trimmedParameter.StartsWith("Username="))
                            _username = parameters[1].Replace("Username=", String.Empty);

                        if (trimmedParameter.StartsWith("Password="))
                        {
                            var password = parameters[2].Replace("Password=", String.Empty);

                            _password = new SecureString();
                            foreach (char c in password) _password.AppendChar(c);
                        }
                    }
                }
                else
                    throw new Exception("The specified connection string could not be found.");

                // Get the Azure Active Directory application registration settings.
                _redirectUrl = appSettings["RedirectUrl"].Value;
                _clientId = appSettings["ClientId"].Value;
            }
            catch (InvalidOperationException e)
            {
                throw new Exception("Required setting in app.config does not exist or is of the wrong type.", e);
            }
        }
        #endregion Methods
    }
}
