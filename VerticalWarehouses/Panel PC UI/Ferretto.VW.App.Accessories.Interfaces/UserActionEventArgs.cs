using System.Collections.Generic;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Accessories.Interfaces
{
    public class UserActionEventArgs : ActionEventArgs
    {
        #region Fields

        private readonly BarcodeRule rule;

        #endregion

        #region Constructors

        public UserActionEventArgs(string code, bool isReset)
            : base(code)
        {
            this.IsReset = isReset;
        }

        public UserActionEventArgs(string code, BarcodeRule rule)
            : base(code)
        {
            if (rule is null)
            {
                throw new System.ArgumentNullException(nameof(rule));
            }

            this.rule = rule;
            var match = System.Text.RegularExpressions.Regex.Match(code, rule.Pattern);
            System.Diagnostics.Debug.Assert(match.Success);

            for (var i = 0; i < match.Groups.Count; i++)
            {
                var group = match.Groups[i];
                this.Parameters.Add(group.Name, group.Value);
            }

            if (System.Enum.TryParse<UserAction>(rule.Action, out var userAction))
            {
                this.UserAction = userAction;
            }
        }

        #endregion

        #region Properties

        public bool HasMismatch { get; set; }

        /// <summary>
        /// When set to <c>True</c> it indicates that the barcode was discarded because not valid for the active chain.
        /// </summary>
        public bool IsReset { get; }

        public Dictionary<string, string> Parameters { get; } = new Dictionary<string, string>();

        public bool RestartOnMismatch => (this.rule?.NextRuleId != null && this.rule?.RestartOnMismatch == true);

        public UserAction UserAction { get; } = UserAction.NotSpecified;

        #endregion
    }
}
