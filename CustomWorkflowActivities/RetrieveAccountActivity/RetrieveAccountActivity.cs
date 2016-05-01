using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;

namespace RetrieveAccountActivity {
    public sealed partial class RetrieveAccountActivity : CodeActivity {

        protected override void Execute(CodeActivityContext executionContext) {
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService organizationService = serviceFactory.CreateOrganizationService(context.UserId);

            var id = AccountReference.Get(executionContext).Id;
            var account = organizationService.Retrieve("account", id, new ColumnSet(new[] {"name"}));
            
            OutputAccount.Set(executionContext, account);
            // TODO-osy: try if this works at all
            OutputAccountReference.Set(executionContext, new EntityReference("account", new KeyAttributeCollection() {new KeyValuePair<string, object>("Id", id), new KeyValuePair<string, object>("Name", account.LogicalName)} ));
        }

        [Input("Input Account Reference")]
        public InArgument<EntityReference> AccountReference {
            get;
            set;
        }

        // TODO-osy: this does not work -- cannot register plugin (activity)
        // the type outargument is not supported -- cannot be an Entity.
        // https://social.microsoft.com/Forums/en-US/e3eb5cf8-3457-4a39-8a62-90ae2addf09d/implementation-of-list-variable-in-output-parameter-of-a-custom-workflow?forum=crm
        // https://technet.microsoft.com/en-us/library/gg327984.aspx
        // But can be an EntityReference -- will this work?
        [Output("Output Account")]
        public OutArgument<Entity> OutputAccount {
            get;
            set;
        }

        [Output("Output Account Reference")]
        public OutArgument<EntityReference> OutputAccountReference {
            get;
            set;
        }

    }
}
