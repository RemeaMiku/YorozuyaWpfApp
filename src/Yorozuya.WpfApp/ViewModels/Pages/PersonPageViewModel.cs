﻿using System;
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

    [ObservableProperty] private List<Post> _postSource = [];

    [ObservableProperty] private List<Reply> _replySource = [];

    [RelayCommand]
    private void OpenPost(Post post)
    {
        _messenger.Send(post);
    }

    [RelayCommand]
    private async Task OpenReply(Reply reply)
    {
        var post = await _postService.GetPostById(reply.PostId);
        if (post is not null)
            _messenger.Send(Tuple.Create(post, reply.Id));
    }

    [RelayCommand]
    private async Task SetActionCard()
    {
        if (_userService.Token is null) return;
        var posts = await _postService.GetUserPostsAsync(_userService.Token);
        var replies = await _postService.GetUserRepliesAsync(_userService.Token);
        PostSource = posts?.ToList() ?? [];
        ReplySource = replies?.ToList() ?? [];
    }

    public PersonPageViewModel(IUserService userService, IPostService postService, IMessenger messenger)
    {
        _userService = userService;
        _postService = postService;
        _messenger = messenger;
        _messenger.Register<PersonPageViewModel, string>(this, (viewModel, message) =>
        {
            if (message == StringMessages.UserLogined)
            {
                NowUserInfo = userService.UserInfo;
                SetActionCardCommand.Execute(default);
            }
            else if (message == StringMessages.UserLogouted)
                NowUserInfo = null;
        });
        NowUserInfo = userService.UserInfo;
    }
}
