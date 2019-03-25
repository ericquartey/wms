namespace Ferretto.Common.BLL.Interfaces.Models
{
    public interface IPolicy
    {
        #region Properties

        bool IsAllowed { get; set; }

        string Name { get; set; }

        string Reason { get; set; }

        PolicyType Type { get; set; }

        #endregion
    }
}
