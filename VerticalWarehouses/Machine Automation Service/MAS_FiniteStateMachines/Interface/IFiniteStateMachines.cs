namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public interface IFiniteStateMachines
    {
        #region Methods

        void Destroy();

        /// <summary>
        /// Execute complete homing.
        /// </summary>
        void DoHoming();

        /// <summary>
        /// Execute vertical homing.
        /// </summary>
        void DoVerticalHoming();

        #endregion
    }
}
