using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.ViewModels.Pages;

public partial class HomePageViewModel(IPostService postService, IMessenger messenger) : BaseViewModel
{
    readonly IMessenger _messenger = messenger;
    readonly IPostService _postService = postService;

    [RelayCommand]
    public async Task Test()
    {
        var posts = await _postService.GetPostsByFieldAsync("Test");
        foreach (var post in posts!)
            _messenger.Send(post);
    }
}
