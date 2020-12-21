using SB.Shared.Models.Actions;

namespace SB.Actions
{
    internal interface IActionTracking
    {
        void Execute(string parameters, ref ActionResponse actionResponse);
    }
}