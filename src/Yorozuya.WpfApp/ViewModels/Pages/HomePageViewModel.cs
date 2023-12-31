﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Mvvm.Contracts;
using Yorozuya.WpfApp.Common;
using Yorozuya.WpfApp.Extensions;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;
using Yorozuya.WpfApp.ViewModels.Windows;
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
        var newPost = await _postService.PublishPostAsync(_userService.Token!, NewPostTitle, NewPostContent, SelectedNewPostField);
        _messenger.Send(newPost);
        _messenger.Send(StringMessages.UserPostChanged);
        _snackbarService.ShowSuccessMessage("发布成功！", "帖子已发布");
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
            _snackbarService.ShowErrorMessage("获取错误，请重试", $"{e.Message}");
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
    private readonly ISnackbarService _snackbarService;

    public HomePageViewModel(IPostService nowPostService, IUserService userService, IMessenger messenger, [FromKeyedServices(nameof(MainWindowViewModel))]ISnackbarService snackbarService)
    {
        _postService = nowPostService;
        _messenger = messenger;
        _userService = userService;
        _snackbarService = snackbarService;
        messenger.Register<HomePageViewModel, string>(this, (viewModel, message) =>
        {
            switch (message)
            {
                case StringMessages.UserLogined:
                    viewModel.StartPostNewPostCommand.NotifyCanExecuteChanged();
                    var fields = Field.Fields.ToList();
                    var idx = fields.IndexOf(_userService.UserInfo!.Field);
                    if(idx == -1) break;
                    fields.RemoveAt(idx);
                    fields.Insert(0, _userService.UserInfo.Field);
                    FieldSource = fields;
                    break;
                case StringMessages.UserLogouted:
                    EndPostNewPost();
                    viewModel.StartPostNewPostCommand.NotifyCanExecuteChanged();
                    FieldSource = Field.Fields.ToList();
                    break;
            }
        });
        FieldSource = Field.Fields.ToList();
        NowSelectedField = FieldSource[0];
    }
}
