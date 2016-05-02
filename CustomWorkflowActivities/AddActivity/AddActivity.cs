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

//<snippetAddActivity>

using System;
using System.Activities;

// These namespaces are found in the Microsoft.Xrm.Sdk.dll assembly
// located in the SDK\bin folder of the SDK download.
using System.Timers;

using Microsoft.Xrm.Sdk;

// These namespaces are found in the Microsoft.Xrm.Sdk.Workflow.dll assembly
// located in the SDK\bin folder of the SDK download.
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;

namespace Microsoft.Crm.Sdk.Samples
{
    /// <summary>
    /// Performs the addition of two summands and returns the result.
    /// Input arguments:
    ///   "First summand". Type: Int. Is the first summand of the addition.
    ///   "Second summand". Type: Int. Is the second summand of the addition.
    /// Output argument:
    ///   "Result". Type: Int. Is the result of the addition.
    /// </summary>
    public sealed partial class AddActivity : CodeActivity
    {
        /// <summary>
        /// Performs the addition of two summands
        /// </summary>
        protected override void Execute(CodeActivityContext executionContext)
        {
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory =
                executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service =
                serviceFactory.CreateOrganizationService(context.UserId);

            // Retrieve the summands and perform addition
            this.result.Set(executionContext, 
                this.firstSummand.Get(executionContext) + 
                this.secondSummand.Get(executionContext));

            //var account = service.Retrieve("account", testEntityReference.Get(executionContext).Id, new ColumnSet(new[] {"name"}));
            //retrievedAccount.Set(executionContext, account);
            // b3e2e3de-713e-4e1d-9107-59ee04e8bbaf -- friendly name
            
        }

        //[Input("Test Entity Reference to retrieve Account By ID")]
        //public InArgument<EntityReference> testEntityReference {
        //    get;
        //    set;
        //}
        //[Output("Retrieved Account")]
        //public OutArgument<Entity> retrievedAccount {
        //    get;
        //    set;
        //}


        // Define Input/Output Arguments
        [Input("First summand")]
        public InArgument<int> firstSummand { get; set; }

        [Input("Second summand")]
        public InArgument<int> secondSummand { get; set; }

        [Output("Result")]
        public OutArgument<int> result { get; set; }
    }
}
//</snippetAddActivity>