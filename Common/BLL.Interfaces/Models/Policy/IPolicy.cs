namespace Ferretto.Common.BLL.Interfaces.Models
{
    public interface IPolicy
    {
        #region Properties

        bool IsAllowed { get; }

        string Name { get; }

        string Reason { get; }

        PolicyType Type { get; }

        #endregion
    }
}
