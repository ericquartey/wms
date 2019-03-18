using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using CommonServiceLocator;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prism.Unity.Ioc;
using Unity;

namespace Ferretto.WMS.App.Tests
{
    public class MockUI
    {
        #region Fields

        private const string Model = "Model";

#pragma warning disable SA1311 // Static readonly fields must begin with upper-case letter
#pragma warning disable S1144 // Unused private types or members should be removed
#pragma warning disable CA1823 // Unused field application
        private static readonly Application application = new Application { ShutdownMode = ShutdownMode.OnMainWindowClose };

#pragma warning restore S1144 // Unused private types or members should be removed

#pragma warning restore SA1311 // Static readonly fields must begin with upper-case letter

        private static readonly Queue<ViewInfo> ViewsToProcess = new Queue<ViewInfo>();

        private IUnityContainer container;

        private INavigationService navigationService;

        #endregion

        #region Constructors

        public MockUI()
        {
            this.InitializeContext();
        }

        #endregion

        protected IUnityContainer Container => this.container;

        protected INavigationService NavigationService => this.navigationService;

        #region Methods

        public static void AppearViews(Type type)
        {
            if (type == null)
            {
                return;
            }

            var moduleName = type.Name;
            foreach (var viewName in GetAllPublicConstantValues<string>(type))
            {
                ViewsToProcess.Enqueue(new ViewInfo(moduleName, viewName));
            }
        }

        public void WaitUiComplete()
        {
            application.MainWindow.Dispatcher.BeginInvoke(new Action(() => this.AppearOnLoaded(ViewsToProcess)), DispatcherPriority.ContextIdle);
            application.MainWindow.ShowDialog();
        }

        public virtual void CheckViewModel(INavigableViewModel viewModel)
        {
            SetDefaultValue(viewModel, Model);
        }

        private static List<T> GetAllPublicConstantValues<T>(Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
                .Select(x => (T)x.GetRawConstantValue())
                .ToList();
        }

        private static void SetDefaultValue(object propObject, string p_propertyName)
        {
            var property = propObject.GetType().GetProperty(p_propertyName);
            if (property != null)
            {
                var newtype = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                var valueObject = Activator.CreateInstance(newtype);
                property.SetValue(propObject, valueObject, null);
            }
        }

        private void AppearOnLoaded(Queue<ViewInfo> viewsToProcess)
        {
            if (viewsToProcess.Count == 0)
            {
                Application.Current.MainWindow.Close();
                var bindingErrors = BindingListener.Current.Errors;
                Assert.IsTrue(string.IsNullOrEmpty(bindingErrors), bindingErrors);
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
                var viewModel = view.DataContext as INavigableViewModel;
                Assert.IsTrue(viewModel != null, $"Failed to initialize ViewModel om v   iew {view.MapId}");
                this.CheckViewModel(viewModel);
                view.Disappear();
            }

            this.AppearOnLoaded(viewsToProcess);
        }

        private void InitializeContext()
        {
            BindingListener.Current.Initialise();

            var wmsAppTest = new WmsApplicationTest();
            wmsAppTest.InitializeTest();
            this.container = ((UnityContainerExtension)wmsAppTest.Container).Instance;

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
}
