using System.Windows.Controls;

namespace Ferretto.VW.App.Scaffolding.Behaviors
{
    public abstract class TextBoxValidationBehavior : TogglableBehavior<TextBox>
    {
        public abstract bool Validate(string text);
    }
}
