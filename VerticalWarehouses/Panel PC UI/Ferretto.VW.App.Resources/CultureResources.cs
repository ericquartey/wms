using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Resources;
using System.Windows;
using System.Windows.Markup;
using System.Xaml;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Ferretto.VW.App.Resources
{
    public class LocalizedResources : INotifyPropertyChanged

    {
        #region Fields

        private readonly Dictionary<string, ResourceManager> resourceManagerDictionary = new Dictionary<string, ResourceManager>();

        private CultureInfo currentCulture = CultureInfo.InstalledUICulture;

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        public static LocalizedResources Instance { get; } = new LocalizedResources();

        public CultureInfo CurrentCulture

        {
            get { return this.currentCulture; }

            set

            {
                if (this.currentCulture != value)

                {
                    this.currentCulture = value;

                    // string.Empty/null indicates that all properties have changed
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
                }
            }
        }

        #endregion

        #region Indexers

        public string this[string key]

        {
            get

            {
                var (baseName, stringName) = SplitName(key);

                string translation = null;

                if (this.resourceManagerDictionary.ContainsKey(baseName))
                {
                    translation = this.resourceManagerDictionary[baseName].GetString(stringName, this.currentCulture);
                }
                return translation ?? key;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Return Localized value for the resource path
        /// Path must be "ResFileName.ResKey" (X Ex: InstallationApp.ActualValue)
        /// </summary>
        public static string Get(string key)
        {
            try
            {
                var (baseName, stringName) = SplitName(key);

                string translation = null;

                if (Instance.resourceManagerDictionary.ContainsKey(baseName))
                {
                    translation = Instance.resourceManagerDictionary[baseName].GetString(stringName, Instance.currentCulture);
                }
                return translation ?? key;
            }
            catch (Exception ex)
            {
                return key;
            }
        }

        public static (string baseName, string stringName) SplitName(string name)

        {
            int idx = name.LastIndexOf('.');

            return (name.Substring(0, idx), name.Substring(idx + 1));
        }

        // WPF bindings register PropertyChanged event if the object supports it and update themselves when it is raised
        public void AddResourceManager(ResourceManager resourceManager)
        {
            try
            {
                string name = resourceManager.BaseName.Split('.').Last();

                if (!this.resourceManagerDictionary.ContainsKey(name))

                {
                    this.resourceManagerDictionary.Add(name, resourceManager);
                }
            }
            catch (Exception ex)
            {
            }
        }

        #endregion
    }

    public class LocExtension : MarkupExtension

    {
        #region Constructors

        public LocExtension(string stringName)

        {
            this.StringName = stringName;
        }

        #endregion

        #region Properties

        public string StringName { get; }

        #endregion

        #region Methods

        public override object ProvideValue(IServiceProvider serviceProvider)

        {
            try
            {
                // targetObject is the control that is using the LocExtension

                object targetObject = (serviceProvider as IProvideValueTarget)?.TargetObject;

                if (targetObject?.GetType().Name == "SharedDp") // is extension used in a control template?
                {
                    return targetObject; // required for template re-binding
                }
                string baseName = this.GetResourceManager(targetObject)?.BaseName ?? string.Empty;

                if (string.IsNullOrEmpty(baseName))

                {
                    // rootObject is the root control of the visual tree (the top parent of targetObject)

                    object rootObject = (serviceProvider as IRootObjectProvider)?.RootObject;

                    baseName = this.GetResourceManager(rootObject)?.BaseName ?? string.Empty;
                }

                if (string.IsNullOrEmpty(baseName)) // template re-binding

                {
                    if (targetObject is FrameworkElement frameworkElement)

                    {
                        baseName = this.GetResourceManager(frameworkElement.TemplatedParent)?.BaseName ?? string.Empty;
                    }
                }

                Binding binding = new Binding

                {
                    Mode = BindingMode.OneWay,

                    Path = new PropertyPath($"[{baseName}{this.StringName}]"),

                    Source = LocalizedResources.Instance,

                    FallbackValue = StringName
                };

                return binding.ProvideValue(serviceProvider);
            }
            catch (Exception exc)
            {
                return new object();
            }
        }

        private ResourceManager GetResourceManager(object control)

        {
            if (control is DependencyObject dependencyObject)

            {
                object localValue = dependencyObject.ReadLocalValue(Translation.ResourceManagerProperty);

                // does this control have a "Translation.ResourceManager" attached property with a set value?

                if (localValue != DependencyProperty.UnsetValue)

                {
                    if (localValue is ResourceManager resourceManager)

                    {
                        LocalizedResources.Instance.AddResourceManager(resourceManager);

                        return resourceManager;
                    }
                }
            }

            return null;
        }

        #endregion
    }

    public class Translation : DependencyObject

    {
        #region Fields

        public static readonly DependencyProperty ResourceManagerProperty =

            DependencyProperty.RegisterAttached("ResourceManager", typeof(ResourceManager), typeof(Translation));

        #endregion

        #region Methods

        public static ResourceManager GetResourceManager(DependencyObject dependencyObject)

        {
            return (ResourceManager)dependencyObject.GetValue(ResourceManagerProperty);
        }

        public static void SetResourceManager(DependencyObject dependencyObject, ResourceManager value)

        {
            dependencyObject.SetValue(ResourceManagerProperty, value);
        }

        #endregion
    }
}
