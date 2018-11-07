using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core;
using Ferretto.Common.Controls.Interfaces;

namespace Ferretto.Common.Controls.Services
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

        #endregion Fields

        #region Constructors

        public InputService(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        #endregion Constructors

        #region Properties

        public IInputElement FocusedElement => this.currentElement as IInputElement;

        #endregion Properties

        #region Methods

        public static string GetStringFromKey(System.Windows.Input.Key keyCode)
        {
            var key = new KeyConverter().ConvertToString(keyCode);

            if (key.Contains("NumPad"))
            {
                key = key.Replace("NumPad", "");
            }

            if (key.Equals("Space"))
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

        public void End()
        {
            if (this.currentHost != null)
            {
                this.currentHost.PreviewMouseDown -= this.DxWindow_PreviewMouseDown;
                this.currentHost.PreviewKeyDown -= this.DxWindow_PreviewKeyDown;
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

        public void RaiseEvent(System.Windows.Input.Key key)
        {
            if (Keyboard.PrimaryDevice.ActiveSource != null)
            {
                InputManager.Current.ProcessInput(
                    new KeyEventArgs(
                        Keyboard.PrimaryDevice,
                        Keyboard.PrimaryDevice.ActiveSource,
                        0, (System.Windows.Input.Key)key)
                    {
                        RoutedEvent = Keyboard.KeyDownEvent
                    });
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

        private static bool IsValidKey(System.Windows.Input.Key key)
        {
            return ((key >= System.Windows.Input.Key.A && key <= System.Windows.Input.Key.Z) ||
                    (key >= System.Windows.Input.Key.D0 && key <= System.Windows.Input.Key.D9) ||
                    (key >= System.Windows.Input.Key.F1 && key <= System.Windows.Input.Key.F24) ||
                    (key >= System.Windows.Input.Key.NumPad0 && key <= System.Windows.Input.Key.NumPad9));
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
            var isTab = false;

            var origKey = (e.SystemKey != System.Windows.Input.Key.None) ? e.SystemKey : e.Key;

            var keySt = (Key)origKey;

            if (Keyboard.Modifiers == (System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Shift))
            {
                isControl = true;
                isShift = true;
            }
            if (Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                isControl = true;
            }
            if (Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Shift)
            {
                isShift = true;
            }
            if (Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Alt)
            {
                isTab = true;
            }

            var shortKey = this.getNewShortKey(keySt, isControl, isShift, isTab);
            if (shortKey == null)
            {
                return;
            }
            shortKey.KeyString = this.GetSensitiveKey(origKey);

            this.NotifyShortKey(shortKey, viewModel, e);
        }

        private void DxWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            this.ControlFocusHandleKeyPress(sender, e);
        }

        private void DxWindow_PreviewMouseDown(Object sender, MouseButtonEventArgs e)
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
            if (view != null)
            {
                return ((FrameworkElement)view).DataContext as INavigableViewModel;
            }
            return null;
        }

        private ShortKey getNewShortKey(Key key, bool isControl, bool isShift, bool isTab)
        {
            ShortKey shortKey = null;
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
            else if (isTab)
            {
                shortKey = new ShortKey(key, false, ModifierKeys.Alt);
            }
            else
            {
                shortKey = new ShortKey(key, false);
            }
            return shortKey;
        }

        private string GetSensitiveKey(System.Windows.Input.Key key)
        {
            var keySt = GetStringFromKey(key);
            var isCapsLockOn = System.Windows.Forms.Control
                                .IsKeyLocked(System.Windows.Forms.Keys.CapsLock);
            var isShiftKeyPressed = (Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Shift) == System.Windows.Input.ModifierKeys.Shift;
            if (!isCapsLockOn && !isShiftKeyPressed)
            {
                if (key >= System.Windows.Input.Key.A && key <= System.Windows.Input.Key.Z)
                {
                    keySt = keySt.ToLower();
                }
            }

            return keySt;
        }

        private void NotifyShortKey(ShortKey shortKey, INavigableViewModel viewModel, KeyEventArgs e)
        {
            var shortKeyInfo = new ShortKeyInfo(shortKey, this.currentElement, this.currentElement.DataContext);

            // Check to notify Shortkey is changed
            foreach (var ShortKeyToExecute in this.keyNotifiers.Values)
            {
                ShortKeyToExecute(shortKeyInfo);
            }

            var handledShortKey = ShortKeys.GetShortKey(viewModel.GetType(), shortKey, out var isMain);
            e.Handled = this.TryExecuteShortkey(viewModel, shortKeyInfo, handledShortKey, isMain);
        }

        private void OnPreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.currentElement = e.NewFocus as FrameworkElement;
            if (this.currentElement == null)
            {
                return;
            }

            var dialogFound = LayoutTreeHelper.GetVisualParents(this.currentElement as DependencyObject)
                    .OfType<DXWindow>()
                    .FirstOrDefault();

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
        }

        private bool TryExecuteShortkey(INavigableViewModel viewModel, ShortKeyInfo shortKeyInfo, ShortKey handledShortKey, bool isMain)
        {
            var isHandled = false;
            if (isMain)
            {
                viewModel = this.GetMainMenuViewModel();
            }
            if (handledShortKey != null &&
                handledShortKey.DoAction != null)
            {
                var shortAction = new ShortKeyAction(viewModel, this.currentElement, shortKeyInfo.ShortKey);
                handledShortKey.DoAction.Invoke(shortAction);
                isHandled = (isHandled == false) ? shortAction.IsHandled : true;
            }
            var isLastHandled = ((IShortKey)viewModel).KeyPress(shortKeyInfo);
            isHandled = (isHandled == false) ? isLastHandled : true;
            return isHandled;
        }

        #endregion Methods
    }
}
