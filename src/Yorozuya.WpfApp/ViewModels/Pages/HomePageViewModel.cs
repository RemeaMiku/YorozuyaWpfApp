using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.ViewModels.Pages;

public partial class HomePageViewModel : BaseViewModel
{
    private readonly List<string> _fields = new() { "文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏", "aaa", "bbb"};

    [ObservableProperty] private string _nowSelectedField;

    public CollectionViewSource FieldSource { get; } = new();
    public ObservableCollection<PostButton> PostSource { get; } = new();

    partial void OnNowSelectedFieldChanged(string value)
    { 
        RefreshPosts();
    }

    public class PostButton(Post post, IRelayCommand openPostCommand)
    {
        public Post Post { get; } = post;
        public IRelayCommand OpenPostCommand { get; } = openPostCommand;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshPostsCommand))]
    private bool _isNotRefreshing = true;

    [RelayCommand(CanExecute = nameof(IsNotRefreshing))]
    private async Task RefreshPosts()
    {
        IsNotRefreshing = false;
        var source = await _nowPostService.GetPostsByFieldAsync(NowSelectedField);
        //List<PostButton> postButtons = new();
        PostSource.Clear();
        if (source is null) return;
        foreach (Post post in source)
        {
            PostSource.Add(new(post, new RelayCommand(() =>
            {
                _messenger.Send(post);
            })));
        }

        IsNotRefreshing = true;
    }

    private readonly IPostService _nowPostService;
    private readonly IMessenger _messenger;
    public HomePageViewModel(IPostService nowPostService, IMessenger messenger)
    {
        _nowPostService = nowPostService;
        _messenger = messenger;
        FieldSource.Source = _fields;
        FieldSource.View.Refresh();
        NowSelectedField = _fields[0];
    }
}
