using System.Runtime.Serialization;

namespace SB.Shared.Models.Actions
{
    [DataContract]
    public class ActionResponse
    {
        public ActionResponse()
        {
            Status = Status.Success;
            Value = "No response";
        }

        [DataMember] public Status Status { get; set; }
        [DataMember] public string Value { get; set; }
    }

    [DataContract]
    public enum Status
    {
        [EnumMemberAttribute] Success = 1,
        [EnumMemberAttribute] Error = 0
    }
}