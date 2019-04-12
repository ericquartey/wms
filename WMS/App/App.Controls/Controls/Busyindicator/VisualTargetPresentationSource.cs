using System;
using System.Windows;
using System.Windows.Media;

namespace Ferretto.WMS.App.Controls
{
    public class VisualTargetPresentationSource : PresentationSource, IDisposable
    {
        #region Fields

        private readonly VisualTarget visualTarget;

        private bool isDisposed;

        #endregion

        #region Constructors

        public VisualTargetPresentationSource(HostVisual hostVisual)
        {
            this.visualTarget = new VisualTarget(hostVisual);
            this.AddSource();
        }

        #endregion

        #region Properties

        public Size DesiredSize { get; private set; }

        public override bool IsDisposed => this.isDisposed;

        public override Visual RootVisual
        {
            get => this.visualTarget.RootVisual;
            set
            {
                var oldRoot = this.visualTarget.RootVisual;

                this.visualTarget.RootVisual = value;

                this.RootChanged(oldRoot, value);

                var rootElement = value as UIElement;
                if (rootElement != null)
                {
                    rootElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    rootElement.Arrange(new Rect(rootElement.DesiredSize));

                    this.DesiredSize = rootElement.DesiredSize;
                }
                else
                {
                    this.DesiredSize = new Size(0, 0);
                }
            }
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.visualTarget?.Dispose();
                    this.RemoveSource();
                }

                this.isDisposed = true;
            }
        }

        protected override CompositionTarget GetCompositionTargetCore()
        {
            return this.visualTarget;
        }

        #endregion
    }
}
