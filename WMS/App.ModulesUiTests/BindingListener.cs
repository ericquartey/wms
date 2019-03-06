using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Ferretto.WMS.App.ModulesUITests
{
    internal class BindingListener : TraceListener
    {
        #region Fields

        public static readonly BindingListener Current = new BindingListener();

        private const int Initialised = 1;

        private const int Uninitialised = 0;

        private readonly StringBuilder errors = new StringBuilder();

        private int initialised = Uninitialised;

        #endregion

        #region Properties

        public string Errors => this.errors.ToString();

        #endregion

        #region Methods

        public void Initialise()
        {
            if (Interlocked.CompareExchange(ref this.initialised, Initialised, Uninitialised) == Uninitialised)
            {
                PresentationTraceSources.DataBindingSource.Listeners.Add(this);
            }
        }

        public override void Write(string message)
        {
        }

        public override void WriteLine(string message)
        {
            this.errors.AppendLine(message);
        }

        #endregion
    }
}
