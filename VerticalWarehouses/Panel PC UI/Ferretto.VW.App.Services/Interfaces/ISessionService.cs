using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public interface ISessionService
    {
        #region Properties

        bool IsLogged { get; set; }

        MachineIdentity MachineIdentity { get; set; }

        UserAccessLevel UserAccessLevel { get; }

        #endregion

        #region Methods

        void SetUserAccessLevel(UserAccessLevel userAccessLevel);

        /// <summary>
        /// Shuts down the current machine.
        /// </summary>
        /// <returns>Returns True if the shutdown process is successfully initiated, False otherwise.</returns>
        bool Shutdown();

        #endregion
    }
}
