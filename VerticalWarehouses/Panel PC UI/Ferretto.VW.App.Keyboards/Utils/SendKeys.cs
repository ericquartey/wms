namespace Ferretto.VW.App.Keyboards
{
    internal static class SendKeys
    {
        #region Methods

        /// <summary>
        ///   Sends the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public static void Send(System.Windows.Input.Key key)
        {
            var target = System.Windows.Input.Keyboard.FocusedElement;    // Target element
            var routedEvent = System.Windows.Input.Keyboard.KeyDownEvent; // Event to send

            if (target is System.Windows.Media.Visual visual)
            {
                target.RaiseEvent(
                  new System.Windows.Input.KeyEventArgs(
                    System.Windows.Input.Keyboard.PrimaryDevice,
                    System.Windows.PresentationSource.FromVisual(visual),
                    0,
                    key)
                  { RoutedEvent = routedEvent }
                );
            }
        }

        public static void Send(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var target = System.Windows.Input.Keyboard.FocusedElement;

                if (target != null)
                {
                    var routedEvent = System.Windows.Input.TextCompositionManager.TextInputEvent;

                    target.RaiseEvent(
                      new System.Windows.Input.TextCompositionEventArgs(
                        System.Windows.Input.InputManager.Current.PrimaryKeyboardDevice,
                        new System.Windows.Input.TextComposition(System.Windows.Input.InputManager.Current, target, text))
                      { RoutedEvent = routedEvent }
                    );
                }
            }
        }

        #endregion
    }
}
