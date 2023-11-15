using System.Threading.Tasks;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;

namespace Yorozuya.WpfApp.Servcies.Contracts;

public interface ILeftRightButtonDialogService
{
    public bool GetIsRightButtonClicked();

    public Task ShowDialogAsync(string message, string? title = null, string? leftButtonText = null, string? rightButtonText = null);

    public void SetDialogControl(Dialog dialog);
}
