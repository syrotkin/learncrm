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
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;

namespace Microsoft.Crm.Sdk.Samples.HelperCode
{
    /// <summary>
    /// Manages user authentication with the Dynamics CRM Web API (OData v4) services. This class uses Microsoft Azure
    /// Active Directory Authentication Library (ADAL) to handle the OAuth 2.0 protocol. 
    /// </summary>
    public class Authentication
    {
        #region Constructors
        private string _clientId = null;
        private string _service = null;
        private string _redirectUrl = null;
        private Configuration _config;

        /// <summary>
        /// Establishes an authentication session for the service.
        /// </summary>
        /// <param name="config">A populated configuration object.</param>
        public Authentication(Configuration config)
            : this(config.ClientId, config.ServiceUrl, config.RedirectUrl)
        {
            _config = config;
        }

        /// <summary>
        /// Establishes an authentication session for the service.
        /// </summary>
        /// <param name="clientId">The client ID of an Azure Active Directory registered app.</param>
        /// <param name="service">The URL of the CRM server root address. For example: https://mydomain.crm.dynamics.com</param>
        /// <param name="redirect">The redirect URL of an Azure Active Directory registered app.</param>
        public Authentication(String clientId, String service, String redirect)
        {
            _clientId = clientId;
            _service = service;
            _redirectUrl = redirect;

            _context = new AuthenticationContext(Authority, false);
        }
        #endregion Constructors

        #region Properties
        private AuthenticationContext _context = null;
        private string _authority = null;

        /// <summary>
        /// The authentication context.
        /// </summary>
        public AuthenticationContext Context
        { get { return _context; } }

        /// <summary>
        /// The URL of the authority to be used for authentication.
        /// </summary>
        public string Authority
        {
            get
            {
                if (_authority == null)
                    DiscoverAuthority(_service);

                return _authority;
            }

            set { _authority = value; }
        }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Returns the authentication result for the configured authentication context.
        /// </summary>
        /// <returns>The refreshed access token.</returns>
        /// <remarks>Refresh the access token before every service call to avoid having to manage token expiration.</remarks>
        public AuthenticationResult AcquireToken()
        {
            if (_config != null && (!string.IsNullOrEmpty(_config.Username) && _config.Password != null))
            {
                UserCredential cred = new UserCredential(_config.Username, _config.Password);
                return _context.AcquireToken(_service, _clientId, cred);
            }
            return _context.AcquireToken(_service, _clientId, new Uri(_redirectUrl));
        }

        /// <summary>
        /// Returns the authentication result for the configured authentication context.
        /// </summary>
        /// <param name="username">The username of a CRM system user in the target organization. </param>
        /// <param name="password">The password of a CRM system user in the target organization.</param>
        /// <returns>The authentication result.</returns>
        /// <remarks>Setting the username or password parameters to null results in the user being prompted to
        /// enter log-on credentials. Refresh the access token before every service call to avoid having to manage
        /// token expiration.</remarks>
        public AuthenticationResult AcquireToken(string username, SecureString password)
        {

            try
            {
                if (!string.IsNullOrEmpty(username) && password != null)
                {
                    UserCredential cred = new UserCredential(username, password);
                    return _context.AcquireToken(_service, _clientId, cred);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Authentication failed. Verify the configuration values are correct.", e);
            }
            return null;
        }


        /// <summary>
        /// Discover the authentication authority.
        /// </summary>
        /// <param name="serviceUrl">The URL of the CRM server root address. For example: https://mydomain.crm.dynamics.com</param>
        public void DiscoverAuthority(String serviceUrl)
        {
            try
            {
                AuthenticationParameters ap = AuthenticationParameters.CreateFromResourceUrlAsync(new Uri(serviceUrl + "/api/data/")).Result;

                if (!String.IsNullOrEmpty(ap.Resource))
                {
                    _service = ap.Resource;
                }
                _authority = ap.Authority;
            }
            catch (HttpRequestException e)
            {
                throw new Exception("An HTTP request exception occurred during authority discovery.", e);
            }
        }
        #endregion Methods
    }
}
