using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Yorozuya.WpfApp.Common;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.ViewModels.Pages;

public partial class PersonPageViewModel : BaseViewModel
{
    private readonly IUserService _userService;
    private readonly IPostService _postService;
    private readonly IMessenger _messenger;

    [ObservableProperty] private UserInfo? _nowUserInfo;

    [ObservableProperty] private List<Post> _postSource = new();

    [ObservableProperty] private List<Reply> _replySource = new();

    [RelayCommand]
    private void OpenPost(Post post)
    {
        _messenger.Send(post);
    }

    [RelayCommand]
    private async Task OpenReply(Reply reply)
    {
        var post = await _postService.GetPostById(reply.PostId);
        if (post is not null && post.Any())
        {
            _messenger.Send(Tuple.Create(post.First(), reply.Id));
        }
    }

    [RelayCommand]
    private async Task SetActionCard()
    {
        List<Task> taskList = new()
        {
            Task.Run(RefreshUserPosts),
            Task.Run(RefreshUserReplies)
        };
        await Task.WhenAll(taskList);
    }

    private async void RefreshUserPosts()
    {
        if (string.IsNullOrEmpty(_userService.Token)) return;
        var posts = await _postService.GetUserPostsAsync(_userService.Token);
        PostSource = posts?.ToList() ?? new();
    }

    private async void RefreshUserReplies()
    {
        if (string.IsNullOrEmpty(_userService.Token)) return;
        var replies = await _postService.GetUserRepliesAsync(_userService.Token);
        ReplySource = replies?.ToList() ?? new();
    }

    public PersonPageViewModel(IUserService userService, IPostService postService, IMessenger messenger)
    {
        _userService = userService;
        _postService = postService;
        _messenger = messenger;
        _messenger.Register<PersonPageViewModel, string>(this, (viewModel, message) =>
        {
            switch (message)
            {
                case StringMessages.UserLogined:
                    NowUserInfo = userService.UserInfo;
                    SetActionCardCommand.Execute(default);
                    break;
                case StringMessages.UserLogouted:
                    NowUserInfo = null;
                    break;
                case StringMessages.UserPostChanged:
                    RefreshUserPosts();
                    break;
                case StringMessages.UserReplyChanged:
                    RefreshUserReplies();
                    break;
            }
        });
        NowUserInfo = userService.UserInfo;
    }
}
