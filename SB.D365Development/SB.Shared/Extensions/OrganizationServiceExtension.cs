using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace SB.Shared.Extensions
{
    public static class OrganizationServiceExtension
    {
        public static EntityCollection RetrieveMultipleAll(this IOrganizationService service, QueryExpression query)
        {
            var entitiesList = new List<Entity>();
            query.PageInfo.PageNumber = 1;
            while (true)
            {
                var collRecords = service.RetrieveMultiple(query);
                entitiesList.AddRange(collRecords.Entities);

                if (!collRecords.MoreRecords)
                {
                    break;
                }

                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = collRecords.PagingCookie;
            }
            return new EntityCollection(entitiesList);
        }

        public static EntityCollection RetrieveMultipleAll(this IOrganizationService service, FetchExpression fetchXml)
        {
            var conversionRequest = new FetchXmlToQueryExpressionRequest
            {
                FetchXml = fetchXml.Query
            };

            var conversionResponse =
                (FetchXmlToQueryExpressionResponse)service.Execute(conversionRequest);

            return service.RetrieveMultipleAll(conversionResponse.Query);
        }

        public static T Retrieve<T>(this IOrganizationService service, string entityName, Guid entityId, ColumnSet columnSet)
            where T : EntityBase
        {
            return service.Retrieve(entityName, entityId, columnSet).ToEntity<T>(service);
        }
    }
}