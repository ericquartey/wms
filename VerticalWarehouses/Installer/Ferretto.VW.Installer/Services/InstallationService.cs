using System.Collections.Generic;
using System.Linq;

namespace Ferretto.VW.Installer
{
    internal sealed class InstallationService
    {
        #region Properties

        public IStep ActiveStep => this.Steps.FirstOrDefault(s => s.Status == StepStatus.InProgress || s.Status == StepStatus.ToDo);

        public IEnumerable<IStep> Steps { get; } = new List<IStep>();

        #endregion
    }
}
