using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DevExpress.Mvvm;

namespace Ferretto.VW.App.Controls.Controls
{
    public class PpcPressAndReleaseButton : PpcButton
    {
        #region Fields

        public static readonly DependencyProperty PressCommandProperty =
            DependencyProperty.Register(nameof(PressCommand), typeof(ICommand), typeof(PpcPressAndReleaseButton), new PropertyMetadata(null));

        public static readonly DependencyProperty ReleaseCommandProperty =
            DependencyProperty.Register(nameof(ReleaseCommand), typeof(ICommand), typeof(PpcPressAndReleaseButton), new PropertyMetadata(null));

        #endregion

        //private readonly DelegateCommand command;

        #region Constructors

        public PpcPressAndReleaseButton()
        {
            this.PreviewMouseLeftButtonUp += (o, a) =>
            {
                this.OnButtonUp();
            };
            this.TouchLeave += (o, a) =>
            {
                this.OnButtonUp();
            };

            this.PreviewMouseLeftButtonDown += (o, a) =>
            {
                this.OnButtonDown();
            };
            this.TouchEnter += (o, a) =>
            {
                this.OnButtonDown();
            };

            //this.Command = new DelegateCommand(() => { }, this.CanExecuteCommand);
        }

        #endregion

        #region Properties

        public ICommand PressCommand
        {
            get => (ICommand)this.GetValue(PressCommandProperty);
            set => this.SetValue(PressCommandProperty, value);
        }

        public ICommand ReleaseCommand
        {
            get => (ICommand)this.GetValue(ReleaseCommandProperty);
            set => this.SetValue(ReleaseCommandProperty, value);
        }

        #endregion

        //private bool CanExecuteCommand
        //{
        //    return this.PressCommand.CanExecute(null);
        //}

        //protected override bool IsEnabledCore => this.PressCommand.CanExecute(null);

        #region Methods

        private void OnButtonDown()
        {
            if (this.PressCommand?.CanExecute(null) == true)
            {
                this.PressCommand.Execute(null);
            }
        }

        private void OnButtonUp()
        {
            if (this.ReleaseCommand?.CanExecute(null) == true)
            {
                this.ReleaseCommand.Execute(null);
            }
        }

        #endregion
    }
}
