﻿// Author : RemeaMiku (Wuhan University) E-mail : remeamiku@whu.edu.cn
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
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
using Yorozuya.WpfApp.Common.Exceptions;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;
using System.Diagnostics;

namespace Yorozuya.WpfApp.ViewModels.Windows;

public partial class PostWindowViewModel : BaseViewModel
{
    public EventHandler? OpenPostRequested;

    public PostWindowViewModel([FromKeyedServices(nameof(PostWindowViewModel))] ILeftRightButtonDialogService dialogService, [FromKeyedServices(nameof(PostWindowViewModel))] ISnackbarService snackbarService, IUserService userService, IPostService postService, IMessenger messenger)
    {
        _dialogService = dialogService;
        _snackbarService = snackbarService;
        _userService = userService;
        _postService = postService;
        _messenger = messenger;
        messenger.Register<PostWindowViewModel, Post>(this, async (r, m) => await r.ReplyOpenPostRequestAsync(m));
        messenger.Register<PostWindowViewModel, Tuple<Post, long>>(this, async (r, m) => await r.ReplyOpenPostRequestAsync(m.Item1, m.Item2));
        messenger.Register<PostWindowViewModel, string>(this, (r, m) =>
        {
            if (m == StringMessages.UserLogined || m == StringMessages.UserLogouted)
                RefreshPostCommand.Execute(default);
        });
    }

    public int ReplyMaxLength { get; } = 10000;

    public CollectionViewSource RepliesViewSource { get; } = new();

    public int CurrentReplyIndex => SelectedIndex + 1;

    public bool IsNotFirstReply => CurrentReplyIndex > 1;

    public bool IsNotLastReply => CurrentReplyIndex < RepliesCount;

    public bool IsCurrentReplyMostLiked => CurrentReply is not null && _mostLikedReply is not null && CurrentReply.Id == _mostLikedReply.Id;

    public ReplyState CurrentReplyState
    {
        get
        {
            if (CurrentReply is null)
                return ReplyState.Default;
            if (CurrentReply.IsAccepted == 1 && IsCurrentReplyMostLiked)
                return ReplyState.IsAcceptedAndMostLiked;
            if (CurrentReply.IsAccepted == 1)
                return ReplyState.IsAccepted;
            if (IsCurrentReplyMostLiked)
                return ReplyState.IsMostLiked;
            return ReplyState.Default;
        }
    }

    public bool IsUserPost => Post is not null && _userService.IsUserLoggedIn && Post.AskerId == _userService.UserInfo!.Id;

    public bool IsUserReply => CurrentReply is not null && _userService.IsUserLoggedIn && CurrentReply.UserId == _userService.UserInfo!.Id;

    public bool CanAddNewReply => Post is not null && Post.DelTag == 0 && _userService.IsUserLoggedIn && UserReply is null;

    public bool CanAcceptReply => IsUserPost && CurrentReply is not null && !IsCurrentReplyAccepted;

    public bool IsCurrentReplyAccepted => CurrentReply is not null && CurrentReply.IsAccepted == 1;

    private const string _postIsNullErrorMessage = "问题不存在或已被删除";

    private const string _replyIsNullErrorMessage = "回答不存在或已被删除";

    private const string _userNotLoggedInErrorMessage = "未登录或登录失效，请登录账号并重试";

    private static readonly SortDescription _likesSortDescription = new(nameof(Reply.Likes), ListSortDirection.Descending);

    private static readonly SortDescription _creatTimeSortDescription = new(nameof(Reply.CreateTime), ListSortDirection.Descending);

    private readonly Stack<Post> _backStack = new();

    private readonly Stack<Post> _forwardStack = new();

    private readonly ISnackbarService _snackbarService;

    private readonly ILeftRightButtonDialogService _dialogService;

    private readonly IUserService _userService;

    private readonly IPostService _postService;

    private readonly IMessenger _messenger;

    [ObservableProperty]
    private bool _isReplying = false;

    [ObservableProperty]
    private string _newReplyContent = string.Empty;

    [ObservableProperty]
    private bool _isOrderByLikes = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUserPost), nameof(CanAddNewReply))]
    [NotifyCanExecuteChangedFor(nameof(AcceptReplyCommand))]
    private Post? _post;

    private bool _canForward;

    private bool _canBack;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCurrentReplyMostLiked), nameof(CurrentReplyState), nameof(IsCurrentReplyAccepted), nameof(IsUserReply))]
    [NotifyCanExecuteChangedFor(nameof(AcceptReplyCommand))]
    private Reply? _currentReply;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentReplyIndex))]
    [NotifyCanExecuteChangedFor(nameof(MoveToNextReplyCommand), nameof(MoveToPreviousReplyCommand))]
    private int _selectedIndex = -1;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MoveToNextReplyCommand))]
    private int _repliesCount;

    [ObservableProperty]
    private bool _isCurrentReplyLiked = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAddNewReply))]
    private Reply? _userReply;

    private Reply? _mostLikedReply;

    private bool CanForward
    {
        get => _canForward;
        set
        {
            if (value == _canForward)
                return;
            _canForward = value;
            ForwardCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanBack
    {
        get => _canBack;
        set
        {
            if (value == _canBack)
                return;
            _canBack = value;
            BackCommand.NotifyCanExecuteChanged();
        }
    }

    async Task UpdateCurrentReplyIsUserLiked()
    {
        if (CurrentReply is null || !_userService.IsUserLoggedIn)
        {
            IsCurrentReplyLiked = false;
            return;
        }
        IsCurrentReplyLiked = await _postService.GetIsLikedAsync(_userService.Token!, CurrentReply.Id);
    }

    async Task UpdatePostAndReplies(long? replyId = default)
    {
        if (Post is null)
            return;
        if (Post.DelTag != 0)
        {
            Post = default;
            return;
        }
        Post = await _postService.GetPostById(Post.Id);
        var replies = await _postService.GetPostRepliesAsync(Post.Id);
        if (replies is null || !replies.Any())
        {
            RepliesViewSource.Source = default;
            RepliesCount = 0;
            UserReply = default;
            _mostLikedReply = default;
            CurrentReply = default;
            SelectedIndex = -1;
            return;
        }
        UserReply = _userService.IsUserLoggedIn ? replies.SingleOrDefault(r => r.UserId == _userService.UserInfo!.Id) : default;
        _mostLikedReply = replies.MaxBy(r => r.Likes);
        RepliesViewSource.Source = replies;
        RepliesViewSource.View.Refresh();
        RepliesCount = replies.Count();
        if (replyId is null)
        {
            CurrentReply = replies.First();
            return;
        }
        CurrentReply = replies.SingleOrDefault(r => r.Id == replyId && r.DelTag == 0);
        if (CurrentReply is null)
        {
            _snackbarService.ShowErrorMessage("加载回答失败", "回答不存在或已被删除");
            CurrentReply = replies.First();
        }
    }

    private async Task HandleExceptions(string title, Exception exception)
    {
        var leftButtonContent = "取消";
        var rightButtonContent = "重试";
        switch (exception)
        {
            case ApiResponseException apiResponseException:
                await _dialogService.ShowDialogAsync(apiResponseException.Message, title, leftButtonContent, rightButtonContent);
                break;
            case HttpRequestException httpRequestException:
                await _dialogService.ShowDialogAsync($"请检查网络设置：{httpRequestException.Message}", title, leftButtonContent, rightButtonContent);
                break;
            default:
                await _dialogService.ShowDialogAsync($"出现了一些问题：{exception.Message}", title, leftButtonContent, rightButtonContent);
                break;
        }
    }

    [RelayCommand]
    private async Task RefreshPostAsync()
    {
        if (Post is null)
            return;
        try
        {
            IsBusy = true;
            await UpdatePostAndReplies(CurrentReply?.Id);
            await UpdateCurrentReplyIsUserLiked();
            OnPropertyChanged(string.Empty);
        }
        catch (Exception ex)
        {
            await HandleExceptions("加载失败", ex);
            if (_dialogService.GetIsRightButtonClicked())
                await RefreshPostAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ReplyOpenPostRequestAsync(Post post, long? replyId = default)
    {
        OpenPostRequested?.Invoke(this, EventArgs.Empty);
        if (_snackbarService.ShowErrorMessageIf($"打开问题：{post.Id} 失败", () => IsBusy, "正在忙碌，请稍后重试"))
            return;
        if (Post is not null && Post.Id != post.Id)
        {
            _backStack.Push(Post);
            CanBack = true;
        }
        if (CanForward && post.Id != _forwardStack.Pop().Id)
        {
            _forwardStack.Clear();
            CanForward = false;
        }
        if (_forwardStack.Count == 0)
            CanForward = false;
        Post = post;
        try
        {
            IsBusy = true;
            await UpdatePostAndReplies(replyId);
            await UpdateCurrentReplyIsUserLiked();
        }
        catch (Exception ex)
        {
            await HandleExceptions("加载失败", ex);
            if (_dialogService.GetIsRightButtonClicked())
                await RefreshPostAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnPostChanging(Post? value)
    {
        IsReplying = false;
        NewReplyContent = string.Empty;
    }

    partial void OnCurrentReplyChanging(Reply? value)
    {
        IsReplying = false;
    }

    partial void OnIsOrderByLikesChanged(bool value)
    {
        if (RepliesViewSource.View is null)
            return;
        RepliesViewSource.View.SortDescriptions.Clear();
        if (IsOrderByLikes)
        {
            RepliesViewSource.View.SortDescriptions.Add(_likesSortDescription);
            RepliesViewSource.View.SortDescriptions.Add(_creatTimeSortDescription);
        }
        else
        {
            RepliesViewSource.View.SortDescriptions.Add(_creatTimeSortDescription);
            RepliesViewSource.View.SortDescriptions.Add(_likesSortDescription);
        }
    }

    [RelayCommand]
    private void OpenReplyPanel()
    {
        if (_snackbarService.ShowErrorMessageIf("回答问题失败", () => !_userService.IsUserLoggedIn, _userNotLoggedInErrorMessage))
            return;
        IsReplying = true;
    }

    [RelayCommand]
    private void CloseReplyPanel()
    {
        IsReplying = false;
    }

    [RelayCommand]
    private async Task SelectReplyAsync(Reply reply)
    {
        CurrentReply = reply;
        try
        {
            IsBusy = true;
            await UpdateCurrentReplyIsUserLiked();
        }
        catch (Exception ex)
        {
            await HandleExceptions("加载失败", ex);
            if (_dialogService.GetIsRightButtonClicked())
                await SelectReplyAsync(reply);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanBack))]
    private async Task BackAsync()
    {
        if (Post is not null)
        {
            _forwardStack.Push(Post);
            CanForward = true;
        }
        Post = _backStack.Pop();
        if (_backStack.Count == 0)
            CanBack = false;
        try
        {
            IsBusy = true;
            await UpdatePostAndReplies();
            await UpdateCurrentReplyIsUserLiked();
        }
        catch (Exception ex)
        {
            await HandleExceptions("加载失败", ex);
            if (_dialogService.GetIsRightButtonClicked())
                await RefreshPostAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanForward))]
    private async Task ForwardAsync()
    {
        if (Post is not null)
        {
            _backStack.Push(Post);
            CanBack = true;
        }
        Post = _forwardStack.Pop();
        if (_forwardStack.Count == 0)
            CanForward = false;
        try
        {
            IsBusy = true;
            await UpdatePostAndReplies();
            await UpdateCurrentReplyIsUserLiked();
        }
        catch (Exception ex)
        {
            await HandleExceptions("加载失败", ex);
            if (_dialogService.GetIsRightButtonClicked())
                await RefreshPostAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanAcceptReply))]
    private async Task AcceptReplyAsync()
    {
        if (_snackbarService.ShowErrorMessageIfAny("采纳回答失败",
            (() => Post is null, _postIsNullErrorMessage),
            (() => CurrentReply is null, _replyIsNullErrorMessage),
            (() => !_userService.IsUserLoggedIn, _userNotLoggedInErrorMessage),
            (() => CurrentReply!.IsAccepted == 1, "该回答已被采纳，请勿重复操作"),
            (() => !IsUserPost, "你不是提问者，无法接受该回答")))
            return;
        await _dialogService.ShowDialogAsync("确定要采纳该回答吗？采纳后不可更改！", "警告", "取消", "确认");
        if (!_dialogService.GetIsRightButtonClicked())
            return;
        try
        {
            IsBusy = true;
            await _postService.AcceptReplyAsync(_userService.Token!, CurrentReply!.Id);
            await UpdatePostAndReplies(CurrentReply!.Id);
            await UpdateCurrentReplyIsUserLiked();
        }
        catch (Exception ex)
        {
            await HandleExceptions("采纳回答失败", ex);
            if (_dialogService.GetIsRightButtonClicked())
                await AcceptReplyAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteReplyAsync()
    {
        if (_snackbarService.ShowErrorMessageIfAny("删除回答失败",
             (() => Post is null, _postIsNullErrorMessage),
             (() => CurrentReply is null, _replyIsNullErrorMessage),
             (() => !_userService.IsUserLoggedIn, _userNotLoggedInErrorMessage),
             (() => CurrentReply!.DelTag != 0, "该回答已被删除，请勿重复操作"),
             (() => !IsUserReply, "你不是此回答的作者，无法删除该回答")))
            return;
        await _dialogService.ShowDialogAsync("确定要删除该回答吗？删除后不可恢复！", "警告", "取消", "确认");
        if (!_dialogService.GetIsRightButtonClicked())
            return;
        try
        {
            IsBusy = true;
            await _postService.DeleteReplyAsync(_userService.Token!, CurrentReply!.Id);
            await UpdatePostAndReplies();
            await UpdateCurrentReplyIsUserLiked();
            _messenger.Send(StringMessages.UserReplyChanged);
        }
        catch (Exception ex)
        {
            await HandleExceptions("删除回答失败", ex);
            if (_dialogService.GetIsRightButtonClicked())
                await DeleteReplyAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeletePostAsync()
    {
        if (_snackbarService.ShowErrorMessageIfAny("删除问题失败",
           (() => Post is null, _postIsNullErrorMessage),
           (() => !_userService.IsUserLoggedIn, _userNotLoggedInErrorMessage),
           (() => !IsUserPost, "你不是提问者，无法删除该问题"),
           (() => Post!.DelTag != 0, "该问题已被删除，请勿重复操作")))
            return;
        await _dialogService.ShowDialogAsync("确定要删除该问题吗？删除后不可恢复！", "警告", "取消", "确认");
        if (!_dialogService.GetIsRightButtonClicked())
            return;
        try
        {
            IsBusy = true;
            await _postService.DeletePostAsync(_userService.Token!, Post!.Id);
            Post = default;
            _messenger.Send(StringMessages.UserPostChanged);
        }
        catch (Exception ex)
        {
            await HandleExceptions("删除问题失败", ex);
            if (_dialogService.GetIsRightButtonClicked())
                await DeletePostAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void MoveToUserReply()
    {
        if (_snackbarService.ShowErrorMessageIf("查看回答失败", () => !_userService.IsUserLoggedIn, _userNotLoggedInErrorMessage))
            return;
        ArgumentNullException.ThrowIfNull(UserReply);
        SelectReplyCommand.Execute(UserReply);
    }

    [RelayCommand(CanExecute = nameof(IsNotLastReply))]
    private async Task MoveToNextReply()
    {
        SelectedIndex++;
        try
        {
            IsBusy = true;
            await UpdateCurrentReplyIsUserLiked();
        }
        catch (Exception ex)
        {
            await HandleExceptions("加载失败", ex);
            if (_dialogService.GetIsRightButtonClicked())
                await UpdateCurrentReplyIsUserLiked();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(IsNotFirstReply))]
    private async Task MoveToPreviousReply()
    {
        SelectedIndex--;
        try
        {
            IsBusy = true;
            await UpdateCurrentReplyIsUserLiked();
        }
        catch (Exception ex)
        {
            await HandleExceptions("加载失败", ex);
            if (_dialogService.GetIsRightButtonClicked())
                await UpdateCurrentReplyIsUserLiked();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ReplyPostAsync()
    {
        if (_snackbarService.ShowErrorMessageIfAny("提交回答失败",
            (() => Post is null, _postIsNullErrorMessage),
            (() => !_userService.IsUserLoggedIn, _userNotLoggedInErrorMessage),
            (() => string.IsNullOrEmpty(NewReplyContent), "回答不能为空"),
            (() => NewReplyContent.Length > ReplyMaxLength, $"回答字数不能超过 {ReplyMaxLength} 个字")))
            return;
        try
        {
            IsBusy = true;
            var newReply = await _postService.PublishReplyAsync(_userService.Token!, Post!.Id, NewReplyContent);
            IsReplying = false;
            NewReplyContent = string.Empty;
            _messenger.Send(StringMessages.UserReplyChanged);
            await UpdatePostAndReplies(newReply.Id);
            await UpdateCurrentReplyIsUserLiked();
        }
        catch (Exception ex)
        {
            await HandleExceptions("提交回答失败", ex);
            if (_dialogService.GetIsRightButtonClicked())
                await ReplyPostAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LikeOrCancelLikeAsync()
    {
        if (_snackbarService.ShowErrorMessageIfAny("点赞或取消点赞失败",
           (() => Post is null, _postIsNullErrorMessage),
           (() => !_userService.IsUserLoggedIn, _userNotLoggedInErrorMessage),
           (() => CurrentReply is null, _replyIsNullErrorMessage)))
            return;
        try
        {
            IsBusy = true;
            if (IsCurrentReplyLiked)
            {
                await _postService.CancelLikeAsync(_userService.Token!, CurrentReply!.Id);
                CurrentReply.Likes--;
            }
            else
            {
                await _postService.LikeAsync(_userService.Token!, CurrentReply!.Id);
                CurrentReply.Likes++;
            }
            await UpdateCurrentReplyIsUserLiked();
            OnPropertyChanged(nameof(CurrentReply));
        }
        catch (Exception ex)
        {
            await HandleExceptions("点赞/取消点赞失败", ex);
            if (_dialogService.GetIsRightButtonClicked())
                await LikeOrCancelLikeAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }
}