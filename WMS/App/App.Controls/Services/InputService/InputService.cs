using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core;
using Ferretto.WMS.App.Controls.Interfaces;

namespace Ferretto.WMS.App.Controls.Services
{
    public class InputService : IInputService
    {
        #region Fields

        private readonly Dictionary<object, Action<ShortKeyInfo>> keyNotifiers = new Dictionary<object, Action<ShortKeyInfo>>();

        private readonly Dictionary<object, Action<MouseDownInfo>> mouseNotifiers = new Dictionary<object, Action<MouseDownInfo>>();

        private readonly INavigationService navigationService;

        private readonly object syncKeyRoot = new object();

        private readonly object syncMouseRoot = new object();

        private FrameworkElement currentElement;

        private FrameworkElement currentHost;

        #endregion

        #region Constructors

        public InputService(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        #endregion

        #region Properties

        public IInputElement FocusedElement => this.currentElement;

        #endregion

        #region Methods

        public static string GetStringFromKey(Key keyCode)
        {
            var key = new KeyConverter().ConvertToString(keyCode);

            if (key.Contains("NumPad"))
            {
                key = key.Replace("NumPad", string.Empty);
            }

            if (key.Equals("Space", StringComparison.Ordinal))
            {
                key = " ";
            }

            return key;
        }

        public void BeginMouseNotify(object instance, Action<MouseDownInfo> callback)
        {
            lock (this.syncMouseRoot)
            {
                if (this.mouseNotifiers.ContainsKey(instance) == false)
                {
                    this.mouseNotifiers.Add(instance, callback);
                }
            }
        }

        public void BeginShortKeyNotify(object instance, Action<ShortKeyInfo> callback)
        {
            lock (this.syncKeyRoot)
            {
                if (this.keyNotifiers.ContainsKey(instance) == false)
                {
                    this.keyNotifiers.Add(instance, callback);
                }
            }
        }

        public void EndMouseNotify(object instance)
        {
            lock (this.syncMouseRoot)
            {
                if (this.mouseNotifiers.ContainsKey(instance))
                {
                    this.mouseNotifiers.Remove(instance);
                }
            }
        }

        public void EndShortKeyNotify(object instance)
        {
            lock (this.syncKeyRoot)
            {
                if (this.keyNotifiers.ContainsKey(instance))
                {
                    this.keyNotifiers.Remove(instance);
                }
            }
        }

        public void Start()
        {
            ShortKeys.Initialize();

            EventManager.RegisterClassHandler(
                            typeof(UIElement),
                            Keyboard.PreviewGotKeyboardFocusEvent,
                            (KeyboardFocusChangedEventHandler)this.OnPreviewGotKeyboardFocus);
        }

        private static ShortKey GetNewShortKey(Key key, bool isControl, bool isShift, bool isAlt)
        {
            ShortKey shortKey;
            if (isControl && isShift)
            {
                shortKey = new ShortKey(key, false, ModifierKeys.Control, ModifierKeys.Shift);
            }
            else if (isControl)
            {
                shortKey = new ShortKey(key, false, ModifierKeys.Control);
            }
            else if (isShift)
            {
                shortKey = new ShortKey(key, false, ModifierKeys.Shift);
            }
            else if (isAlt)
            {
                shortKey = new ShortKey(key, false, ModifierKeys.Alt);
            }
            else
            {
                shortKey = new ShortKey(key, false);
            }

            return shortKey;
        }

        private static string GetSensitiveKey(Key key)
        {
            var keyString = GetStringFromKey(key);
            var isCapsLockOn = (Keyboard.GetKeyStates(Key.CapsLock) & KeyStates.Toggled) == KeyStates.Toggled;
            var onlyShiftKeyIsPressed = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

            if (!isCapsLockOn
                && !onlyShiftKeyIsPressed
                && key >= Key.A
                && key <= Key.Z)
            {
                keyString = keyString.ToLower();
            }

            return keyString;
        }

        private void ControlFocusHandleKeyPress(object sender, KeyEventArgs e)
        {
            var viewModel = this.navigationService.GetViewModelFromActiveWindow();
            if ((viewModel is INavigableViewModel) == false)
            {
                return;
            }

            var isControl = false;
            var isShift = false;
            var isAlt = false;

            var origKey = (e.SystemKey != Key.None) ? e.SystemKey : e.Key;

            var keySt = origKey;

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                isControl = true;
                isShift = true;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                isControl = true;
            }

            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                isShift = true;
            }

            if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                isAlt = true;
            }

            var shortKey = GetNewShortKey(keySt, isControl, isShift, isAlt);
            if (shortKey == null)
            {
                return;
            }

            shortKey.KeyString = GetSensitiveKey(origKey);

            this.NotifyShortKey(shortKey, viewModel, e);
        }

        private void DxWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            this.ControlFocusHandleKeyPress(sender, e);
        }

        private void DxWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var mouseButton = MouseButtonPressed.None;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                mouseButton = MouseButtonPressed.Left;
            }
            else if (e.MiddleButton == MouseButtonState.Pressed)
            {
                mouseButton = MouseButtonPressed.Middle;
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                mouseButton = MouseButtonPressed.Right;
            }

            // Check to notify mouse down
            foreach (var mouseDownKey in this.mouseNotifiers.Keys)
            {
                var mouseInfo = new MouseDownInfo(mouseDownKey, e.OriginalSource, mouseButton, this.currentElement, this.currentElement.DataContext);
                this.mouseNotifiers[mouseDownKey](mouseInfo);
            }
        }

        private INavigableViewModel GetMainMenuViewModel()
        {
            var menuViewModelName = $"{nameof(Common.Utils.Modules.Layout)}.{Common.Utils.Modules.Layout.REGION_MENU}";
            var view = this.navigationService.GetRegisteredView(menuViewModelName);

            return view?.DataContext as INavigableViewModel;
        }

        private void NotifyShortKey(ShortKey shortKey, INavigableViewModel viewModel, RoutedEventArgs e)
        {
            var shortKeyInfo = new ShortKeyInfo(shortKey, this.currentElement, this.currentElement.DataContext);

            // Check to notify Shortkey is changed
            foreach (var shortKeyAction in this.keyNotifiers.Values)
            {
                shortKeyAction(shortKeyInfo);
            }

            (var handledShortKey, var isMain) = ShortKeys.GetShortKey(viewModel.GetType(), shortKey);
            e.Handled = this.TryExecuteShortkey(viewModel, shortKeyInfo, handledShortKey, isMain);
        }

        private void OnPreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.currentElement = e.NewFocus as FrameworkElement;
            if (this.currentElement == null)
            {
                return;
            }

            var dialogFound = LayoutTreeHelper.GetVisualParents(this.currentElement)
                    .OfType<DXWindow>()
                    .FirstOrDefault();

            Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        new Action(() =>
                        {
                            FrameworkElement mainHost = dialogFound;
                            if (dialogFound == null)
                            {
                                mainHost = Application.Current.MainWindow;
                            }

                            if (mainHost != this.currentHost)
                            {
                                if (this.currentHost != null)
                                {
                                    this.currentHost.PreviewMouseDown -= this.DxWindow_PreviewMouseDown;
                                    this.currentHost.PreviewKeyDown -= this.DxWindow_PreviewKeyDown;
                                }

                                if (mainHost != null)
                                {
                                    this.currentHost = mainHost;
                                    this.currentHost.PreviewMouseDown += this.DxWindow_PreviewMouseDown;
                                    this.currentHost.PreviewKeyDown += this.DxWindow_PreviewKeyDown;
                                }
                            }
                        }));
        }

        private bool TryExecuteShortkey(INavigableViewModel viewModel, ShortKeyInfo shortKeyInfo, ShortKey handledShortKey, bool isMain)
        {
            if (isMain && viewModel == null)
            {
                return false;
            }

            var actualViewModel = isMain ? this.GetMainMenuViewModel() : viewModel;
            var isHandled = false;
            if (handledShortKey != null &&
                handledShortKey.DoAction != null)
            {
                var shortAction = new ShortKeyAction(actualViewModel, this.currentElement, shortKeyInfo.ShortKey);
                handledShortKey.DoAction.Invoke(shortAction);
                isHandled = shortAction.IsHandled;
            }

            var isLastHandled = ((IShortKey)actualViewModel).KeyPress(shortKeyInfo);

            return isHandled || isLastHandled;
        }

        #endregion
    }
}
