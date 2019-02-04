using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZZ_AutomationServiceTESTPROJECT
{
    public partial class App : System.Windows.Application
    {
        #region Fields

        private bool _contentLoaded;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public static void Main()
        {
            ZZ_AutomationServiceTESTPROJECT.App app = new ZZ_AutomationServiceTESTPROJECT.App();
            app.InitializeComponent();
            app.Run();
        }

        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent()
        {
            if (_contentLoaded)
            {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("ZZ_AutomationServiceTESTPROJECT;component/app.xaml", System.UriKind.Relative);

#line 1 "..\..\App.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);

#line default
#line hidden
        }

        #endregion Methods
    }
}
