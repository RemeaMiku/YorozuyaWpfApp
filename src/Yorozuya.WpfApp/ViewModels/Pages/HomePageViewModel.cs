using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Yorozuya.WpfApp.Common;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;
using Yorozuya.WpfApp.Views.Pages;

namespace Yorozuya.WpfApp.ViewModels.Pages;

public partial class HomePageViewModel : BaseValidatorViewModel
{
    public static class UiStatus
    {
        public const string StartSearch = nameof(StartSearch);
        public const string EndSearch = nameof(EndSearch);
        public const string StartPost = nameof(StartPost);
        public const string EndPost = nameof(EndPost);
    }

    [ObservableProperty] private string _nowSelectedField;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartSearchCommand))]
    private string _searchInput = string.Empty;
    [ObservableProperty] private List<Post>? _searchResult;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PostNewPostCommand))]
    private string _newPostTitle = string.Empty;
    [ObservableProperty] private string _newPostContent = string.Empty;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PostNewPostCommand))]
    private string _selectedNewPostField = string.Empty;
    [ObservableProperty] private List<string> _fieldSource = [];
    public ObservableCollection<Post> PostSource { get; } = [];

    partial void OnNowSelectedFieldChanged(string value)
    {
        RefreshPostsCommand.Execute(default);
    }

    private bool PostNewPostCommandCanExe =>
        !string.IsNullOrEmpty(NewPostTitle) && !string.IsNullOrEmpty(SelectedNewPostField);
    [RelayCommand(CanExecute = nameof(PostNewPostCommandCanExe))]
    private async Task PostNewPost()
    {
        await _postService.PublishPostAsync(_userService.Token!, NewPostTitle, NewPostContent, SelectedNewPostField);
        EndPostNewPost();
    }

    private bool StartPostNewPostCommandCanExe => !string.IsNullOrEmpty(_userService.Token);
    [RelayCommand(CanExecute = nameof(StartPostNewPostCommandCanExe))]
    private void StartPostNewPost()
    {
        NewPostTitle = string.Empty;
        NewPostContent = string.Empty;
        SelectedNewPostField = NowSelectedField;
        _messenger.Send(UiStatus.StartPost, nameof(HomePage));
    }

    [RelayCommand]
    private void EndPostNewPost()
    {
        _messenger.Send(UiStatus.EndPost, nameof(HomePage));
    }


    [RelayCommand]
    void OpenPost(Post post)
    {
        _messenger.Send(post);
    }

    [RelayCommand]
    private async Task RefreshPosts()
    {
        IsBusy = true;
        try
        {
            var source = await _postService.GetPostsByFieldAsync(NowSelectedField);
            PostSource.Clear();
            if (source is null) return;
            foreach (Post post in source)
            {
                PostSource.Add(post);
            }
        }
        catch (Exception e)
        {
            //TODO: ErrorDialog
            Console.WriteLine(e);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool StartSearchCommandCanExe => !string.IsNullOrEmpty(SearchInput);
    [RelayCommand(CanExecute = nameof(StartSearchCommandCanExe))]
    private async Task StartSearch()
    {
        SearchResult = null;
        var postsTask = _postService.GetPostByTitle(SearchInput);
        _messenger.Send(UiStatus.StartSearch, nameof(HomePage));
        var posts = await postsTask;
        SearchResult = posts?.ToList() ?? [];
        //SearchResult = new List<Post>
        //{
        //    new()
        //    {
        //        Id = 1, Content = "wfwf", Field = "ww", Views = 500, AskerId = 1, Title = "www", CreateTime = "11",
        //        DelTag = 0, UpdateTime = "11"
        //    }
        //};
    }

    [RelayCommand]
    private void EndSearch()
    {
        _messenger.Send(UiStatus.EndSearch, nameof(HomePage));
    }

    private readonly IPostService _postService;
    private readonly IMessenger _messenger;
    private readonly IUserService _userService;

    public HomePageViewModel(IPostService nowPostService, IUserService userService, IMessenger messenger)
    {
        _postService = nowPostService;
        _messenger = messenger;
        _userService = userService;
        messenger.Register<HomePageViewModel, string>(this, (viewModel, message) =>
        {
            switch (message)
            {
                case StringMessages.UserLogined:
                    viewModel.StartPostNewPostCommand.NotifyCanExecuteChanged();
                    break;
                case StringMessages.UserLogouted:
                    EndPostNewPost();
                    viewModel.StartPostNewPostCommand.NotifyCanExecuteChanged();
                    break;
            }
        });
        FieldSource = Field.Fields.ToList();
        NowSelectedField = FieldSource[0];
    }
}
