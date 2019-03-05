using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Utils;
using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.App.ModulesUITests
{
    [TestClass]
    public class MockUI
    {
        #region Fields

        private static readonly Application application = new Application() { ShutdownMode = ShutdownMode.OnMainWindowClose };

        private static readonly Queue<ViewInfo> viewsToProcess = new Queue<ViewInfo>();

        private INavigationService navigationService;

        #endregion

        #region Constructors

        public MockUI()
        {
            this.InitializeContext();
        }

        #endregion

        #region Methods

        public void AppearViews(Type type)
        {
            var moduleName = type.Name;
            foreach (var viewName in GetAllPublicConstantValues<string>(type))
            {
                viewsToProcess.Enqueue(new ViewInfo(moduleName, viewName));
            }
        }

        public void WaitUIComplete()
        {
            Application.Current.MainWindow.Dispatcher.BeginInvoke(new Action(() => this.AppearOnLoaed(viewsToProcess)), DispatcherPriority.ContextIdle);
            Application.Current.MainWindow.ShowDialog();
        }

        private static List<T> GetAllPublicConstantValues<T>(Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
                .Select(x => (T)x.GetRawConstantValue())
                .ToList();
        }

        private void AppearOnLoaed(Queue<ViewInfo> viewsToProcess)
        {
            if (viewsToProcess.Count == 0)
            {
                Application.Current.MainWindow.Close();
                return;
            }

            var viewInfo = viewsToProcess.Dequeue();
            if (MvvmNaming.IsViewModelNameValid(viewInfo.ViewName))
            {
                var view = this.navigationService.Appear(viewInfo.ModuleName, viewInfo.ViewName);
                Assert.IsTrue(view != null, $"Failed to load view {viewInfo.ToString()}");
                ((FrameworkElement)view).Dispatcher.BeginInvoke(new Action(() => this.CheckDataContext(viewsToProcess, view)), DispatcherPriority.ContextIdle);
            }
            else
            {
                this.CheckDataContext(viewsToProcess, null);
            }
        }

        private void CheckDataContext(Queue<ViewInfo> viewsToProcess, INavigableView view)
        {
            if (view != null)
            {
                Assert.IsTrue(view.DataContext != null, $"Failed to initialize ViewModel om view {view.MapId}");
                view.Disappear();
            }

            this.AppearOnLoaed(viewsToProcess);
        }

        private void InitializeContext()
        {
            new Bootstrapper().Run();

            DevExpress.Xpf.Core.ApplicationThemeHelper.ApplicationThemeName = Common.Utils.Common.THEMECONTROLSNAME;

            var dictionary = new ResourceDictionary();
            var resourceUri = $"pack://application:,,,/{Common.Utils.Common.ASSEMBLY_THEMENAME};Component/Themes/{Common.Utils.Common.THEMERESOURCEDICTIONARY}.xaml";
            dictionary.Source = new Uri(resourceUri, UriKind.Absolute);
            Application.Current.Resources.MergedDictionaries.Add(dictionary);

            this.navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            this.navigationService.IsUnitTest = true;
        }

        #endregion
    }

    public class ViewInfo
    {
        #region Constructors

        public ViewInfo(string moduleName, string viewName)
        {
            this.ModuleName = moduleName;
            this.ViewName = viewName;
        }

        #endregion

        #region Properties

        public string ModuleName { get; set; }

        public string ViewName { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"{this.ModuleName}.{this.ViewName}";
        }

        #endregion
    }
}
