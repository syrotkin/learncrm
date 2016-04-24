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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Crm.Sdk.Samples.HelperCode
{
    /// <summary>
    /// Produces a populated exception from an error message in the content of an HTTP response. 
    /// </summary>
    public class CrmHttpResponseException : System.Exception
    {
        #region Properties
        private static string _stackTrace;
        
        /// <summary>
        /// Gets a string representation of the immediate frames on the call stack.
        /// </summary>
        public override string StackTrace
        {
            get { return _stackTrace; }
        }
        #endregion Properties

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the CrmHttpResponseException class.
        /// </summary>
        /// <param name="content">The populated HTTP content in Json format.</param>
        public CrmHttpResponseException(HttpContent content)
            : base(ExtractMessageFromContent(content)) { }

        /// <summary>
        /// Initializes a new instance of the CrmHttpResponseException class.
        /// </summary>
        /// <param name="content">The populated HTTP content in Json format.</param>
        /// <param name="innerexception">The exception that is the cause of the current exception, or a null reference
        /// if no inner exception is specified.</param>
        public CrmHttpResponseException(HttpContent content, Exception innerexception)
            : base(ExtractMessageFromContent(content), innerexception) { }

        #endregion Constructors

        #region Methods
        /// <summary>
        /// Extracts the CRM specific error message and stack trace from an HTTP content. 
        /// </summary>
        /// <param name="content">The HTTP content in Json format.</param>
        /// <returns>The error message.</returns>
        private static string ExtractMessageFromContent(HttpContent content)
        {
            string message = String.Empty;
            string downloadedContent = content.ReadAsStringAsync().Result;

            JObject jcontent = (JObject)JsonConvert.DeserializeObject(downloadedContent);
            IDictionary<string, JToken> d = jcontent;

            if (d.ContainsKey("error"))
            {
                JObject error = (JObject)jcontent.Property("error").Value;
                message = (String)error.Property("message").Value;
            }
            else if (d.ContainsKey("Message"))
                message = (String)jcontent.Property("Message").Value;

            if(d.ContainsKey("StackTrace"))
                _stackTrace = (String)jcontent.Property("StackTrace").Value;

            return message;
        #endregion Methods
        }
    }
}
