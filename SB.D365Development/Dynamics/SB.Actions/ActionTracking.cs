using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using SB.Shared;
using SB.Shared.Models.Actions;

namespace SB.Actions
{
    public class ActionTracking : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.Get<IPluginExecutionContext>();
            var factory = serviceProvider.Get<IOrganizationServiceFactory>();
            var notificationService = serviceProvider.Get<IServiceEndpointNotificationService>();
            var service = factory.CreateOrganizationService(context.UserId);
            var adminService = factory.CreateOrganizationService(null);
            var tracer = serviceProvider.Get<ITracingService>();

            try
            {

                var actionName = (string)context.InputParameters["ActionName"];
                var parameters = context.InputParameters.Contains("Parameters") ? (string)context.InputParameters["Parameters"] : string.Empty;

                var response = new ActionResponse();

                foreach (var parameter in context.InputParameters)
                {
                    tracer?.Trace($"{parameter.Key} = {parameter.Value}");
                }

                switch (actionName)
                {

                }

                context.OutputParameters["Response"] = JsonSerializer.Serialize(response);

                tracer?.Trace($"{nameof(context.OutputParameters)}:");
                foreach (var parameter in context.OutputParameters)
                {
                    tracer?.Trace($"{parameter.Key} = {parameter.Value}");
                }
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }
    }
}