using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.ViewModels.Pages;

public partial class PersonPageViewModel : BaseViewModel
{
    private readonly IUserService _userService;
    private readonly IPostService _postService;
    private readonly IMessenger _messenger;

    [ObservableProperty] private UserInfo? _nowUserInfo;

    [ObservableProperty] private List<HomePageViewModel.PostButton> _postSource = new();

    [ObservableProperty] private List<ReplyButton> _replySource = new();

    public class ReplyButton(Reply reply, IRelayCommand openReplyCommand)
    {
        public Reply Reply { get; } = reply;
        public IRelayCommand OpenReplyCommand { get; } = openReplyCommand;
    }

    private async void SetActionCard()
    {
        var posts = await _postService.GetUserPostsAsync(_userService.Token);
        List<HomePageViewModel.PostButton> postSource = new();
        if (posts is not null)
        {
            postSource.AddRange(posts.Select(post => new HomePageViewModel.PostButton(post, new RelayCommand(() => { _messenger.Send(post); }))));
            PostSource = postSource;
        }

        var replies = await _postService.GetUserRepliesAsync(_userService.Token);
        List<ReplyButton> replySource = new();
        if (replies is not null)
        {
            replySource.AddRange(replies.Select(reply => new ReplyButton(reply, new RelayCommand(() => { Console.WriteLine(reply.Id); }))));
            ReplySource = replySource;
        }
    }

    public PersonPageViewModel(IUserService userService, IPostService postService, IMessenger messenger)
    {
        _userService = userService;
        _postService = postService;
        _messenger = messenger;
        NowUserInfo = userService.UserInfo;
        SetActionCard();
    }
}
