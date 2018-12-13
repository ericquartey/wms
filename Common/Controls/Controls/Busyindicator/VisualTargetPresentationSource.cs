using System;
using System.Windows;
using System.Windows.Media;

namespace Ferretto.Common.Controls
{
    public class VisualTargetPresentationSource : PresentationSource
    {
        #region Fields

        private readonly VisualTarget visualTarget;
        private bool isDisposed = false;

        #endregion Fields

        #region Constructors

        public VisualTargetPresentationSource(HostVisual hostVisual)
        {
            this.visualTarget = new VisualTarget(hostVisual);
            this.AddSource();
        }

        #endregion Constructors

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
                    rootElement.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    rootElement.Arrange(new Rect(rootElement.DesiredSize));

                    this.DesiredSize = rootElement.DesiredSize;
                }
                else
                {
                    this.DesiredSize = new Size(0, 0);
                }
            }
        }

        #endregion Properties

        #region Methods

        internal void Dispose()
        {
            this.RemoveSource();
            this.isDisposed = true;
        }

        protected override CompositionTarget GetCompositionTargetCore()
        {
            return this.visualTarget;
        }

        #endregion Methods
    }
}
