using System.Threading.Tasks;
using Wpf.Ui.Controls.Interfaces;

namespace EverythingHouse.WpfApp.Servcies.Contracts;

public interface ICancelConfirmDialogService
{
    public bool GetIsConfirmed();

    public Task ShowDialogAsync(string message, string? title = null, string? leftButtonText = null, string? rightButtonText = null);

    public void Initialize(IDialogControl dialogControl);
}
