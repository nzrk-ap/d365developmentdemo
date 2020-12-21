using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace SB.Shared.Extensions
{
    public static class DateTimeExtensions
    {
        public static int? RetrieveCurrentUsersTimeZoneSettings(IOrganizationService service)
        {
            var currentUserSettings = service.RetrieveMultiple(
                new QueryExpression("usersettings")
                {
                    ColumnSet = new ColumnSet("localeid", "timezonecode"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("systemuserid", ConditionOperator.EqualUserId)
                        }
                    }
                }).Entities[0].ToEntity<Entity>();

            return (int?)currentUserSettings.Attributes["timezonecode"];
        }

        public static DateTime RetrieveLocalTimeFromUtcTime(this DateTime utcTime, IOrganizationService service)
        {
            {
                var timeZone = RetrieveCurrentUsersTimeZoneSettings(service);

                if (!timeZone.HasValue)
                {
                    return DateTime.MinValue;
                }

                var request = new LocalTimeFromUtcTimeRequest
                {
                    TimeZoneCode = timeZone.Value,
                    UtcTime = utcTime.ToUniversalTime()
                };

                var response = (LocalTimeFromUtcTimeResponse)service.Execute(request);

                return response.LocalTime;
            }
        }
        public static DateTime ToLocalDateTime(this DateTime datetime)
        {
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(datetime.ToUniversalTime(), cstZone);
        }
    }
}
