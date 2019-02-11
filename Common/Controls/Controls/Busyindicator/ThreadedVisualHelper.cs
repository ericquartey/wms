using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Ferretto.Common.Controls
{
    public class ThreadedVisualHelper
    {
        #region Fields

        private const string backgroundVisualHostName = "BackgroundVisualHostThread";

        private readonly BackgroundVisualHost.CreateVisualContent createContent;
        private readonly HostVisual hostVisual = new HostVisual();
        private readonly Action invalidateMeasure;

        private readonly AutoResetEvent sync = new AutoResetEvent(false);

        #endregion

        #region Constructors

        public ThreadedVisualHelper(
            BackgroundVisualHost.CreateVisualContent createContent,
            Action invalidateMeasure)
        {
            this.createContent = createContent;
            this.invalidateMeasure = invalidateMeasure;

            var backgroundUi = new Thread(this.CreateAndShowContent);
            backgroundUi.SetApartmentState(ApartmentState.STA);
            backgroundUi.Name = backgroundVisualHostName;
            backgroundUi.IsBackground = true;
            backgroundUi.Start();

            this.sync.WaitOne();
        }

        #endregion

        #region Properties

        public Size DesiredSize { get; private set; }

        public HostVisual HostVisual => this.hostVisual;

        private Dispatcher Dispatcher { get; set; }

        #endregion

        #region Methods

        public void Exit()
        {
            this.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
        }

        private void CreateAndShowContent()
        {
            this.Dispatcher = Dispatcher.CurrentDispatcher;
            var source = new VisualTargetPresentationSource(this.hostVisual);
            this.sync.Set();
            source.RootVisual = this.createContent();
            this.DesiredSize = source.DesiredSize;
            this.invalidateMeasure();

            Dispatcher.Run();
            source.Dispose();
        }

        #endregion
    }
}
