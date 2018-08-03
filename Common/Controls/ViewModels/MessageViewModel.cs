using System.Collections.Generic;
using Ferretto.Common.Controls.Interfaces;
using Prism.Commands;

namespace Ferretto.Common.Controls.ViewModels
{
  public class MessageViewModel : BaseNavigationViewModel
  {
    #region Properties
    private string message;
    public string Message
    {
      get { return this.message; }
      set { this.SetProperty(ref this.message, value); }
    }

    private string title;
    public string Title
    {
      get { return this.title; }
      set { this.SetProperty(ref this.title, value); }
    }

    public DialogResult Result
    {
      get;
      private set;
    }

    private DialogButton buttons;
    public DialogButton Buttons
    {
      get { return this.buttons; }
      set { this.SetProperty(ref this.buttons, value); }
    }

    private DialogType type;
    public DialogType Type
    {
      get { return this.type; }
      set { this.SetProperty(ref this.type, value); }
    }
    #endregion

    #region Commands
    private DelegateCommand<DialogResult> cmdChooseOption;
    public DelegateCommand<DialogResult> CmdChooseOption => this.cmdChooseOption ?? (this.cmdChooseOption = new DelegateCommand<DialogResult>(this.ChooseOption));
    #endregion

    #region Ctor
    public MessageViewModel()
    {
      this.Appear();
    }
    #endregion

    #region Public Methods 
    private void ChooseOption(DialogResult result)
    {
      this.Result = result;
      this.Disappear();
    }
    #endregion
  }
}
