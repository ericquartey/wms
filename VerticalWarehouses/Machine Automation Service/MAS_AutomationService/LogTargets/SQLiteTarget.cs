using NLog;
using NLog.Config;
using NLog.Targets;

namespace Ferretto.VW.MAS_AutomationService.LogTargets
{
    namespace Ferretto.VW.MAS_AutomationService.LogTargets
    {
        [Target("MyFirst")]
        public sealed class SQliteTarget : TargetWithLayout
        {
            #region Constructors

            public SQliteTarget()
            {
            }

            #endregion

            #region Properties

            [RequiredParameter]
            public string Database { get; set; }

            #endregion

            #region Methods

            protected override void Write(LogEventInfo logEvent)
            {
                var logMessage = this.Layout.Render(logEvent);
            }

            #endregion
        }
    }
}
