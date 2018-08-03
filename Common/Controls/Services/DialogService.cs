using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.ViewModels;

namespace Ferretto.Common.Controls.Services
{
  public class DialogService : IDialogService
  {
    #region Ctor
    public DialogService()
    {
    }
    #endregion

    #region Methods
    public DialogResult ShowMessage(
      string message,
      string title = null,
      DialogType type = DialogType.Information,
      DialogButton buttons = DialogButton.OK)
    {
      return this.ShowMessageDialog(message, title, type, buttons);
    }
    
    private DialogResult ShowMessageDialog(
        string message,
        string title,
        DialogType type,
        DialogButton buttons)
    {
      var textMessageViewModel = new MessageViewModel()
      {        
        Message = message,
        Title = title,
        Type = type,
        Buttons = buttons,
      };
      textMessageViewModel.Appear();
      return textMessageViewModel.Result;
    }
    #endregion
  }
}
