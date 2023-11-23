using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Wpf.Ui.Mvvm.Contracts;
using Yorozuya.WpfApp.Common;
using Yorozuya.WpfApp.Extensions;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.ViewModels.Windows;

public partial class PostWindowViewModel : BaseViewModel
{
    readonly Stack<Post> _backStack = new();
    readonly Stack<Post> _forwardStack = new();
    readonly ISnackbarService _snackbarService;

    public EventHandler? WindowOpened;

    public PostWindowViewModel(ILeftRightButtonDialogService dialogService, ISnackbarService snackbarService, IUserService userService, IPostService postService, IMessenger messenger)
    {
        _dialogService = dialogService;
        _snackbarService = snackbarService;
        _userService = userService;
        _postService = postService;
        messenger.Register<PostWindowViewModel, Post>(this, async (r, m) => await r.OpenPostAsync(m));
    }

    async Task OpenPostAsync(Post post)
    {
        WindowOpened?.Invoke(this, EventArgs.Empty);
        if (_snackbarService.ShowErrorMessageIf("加载失败", () => IsBusy, "正在忙碌，请稍后重试"))
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
        await GetAndUpdatePostRepliesAsync(post);
    }

    partial void OnPostChanging(Post? value)
    {
        IsReplying = false;
    }

    public ISnackbarService GetSnackbarService() => _snackbarService;

    [ObservableProperty]
    bool _isReplying = false;

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

    [ObservableProperty]
    string _newReplyContent = string.Empty;

    public int ReplyMaxLength { get; } = 10000;

    [RelayCommand]
    void OpenReplyPanel()
    {
        if (_snackbarService.ShowErrorMessageIf("新建回答失败", () => !_userService.IsUserLoggedIn, _userNotLoggedInErrorMessage))
            return;
        IsReplying = true;
    }

    [RelayCommand]
    void CloseReplyPanel()
    {
        IsReplying = false;
    }

    public CollectionViewSource RepliesViewSource { get; } = new();

    [RelayCommand]
    async Task SelectReplyAsync(Reply reply)
    {
        try
        {
            IsBusy = true;
            CurrentReply = reply;
            IsCurrentReplyLiked = _userService.IsUserLoggedIn && await _postService.GetIsLikedAsync(_userService.Token, reply.Id);
        }
        catch (Exception)
        {
            //TODO:异常处理
        }
        finally
        {
            IsBusy = false;
        }
    }

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

    public ILeftRightButtonDialogService GetCancelConfirmDialogService() => _dialogService;

    private static readonly SortDescription _likesSortDescription = new(nameof(Reply.Likes), ListSortDirection.Descending);

    private static readonly SortDescription _creatTimeSortDescription = new(nameof(Reply.CreateTime), ListSortDirection.Descending);

    private readonly ILeftRightButtonDialogService _dialogService;

    private readonly IUserService _userService;

    private readonly IPostService _postService;

    [ObservableProperty]
    private bool _isOrderByLikes = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUserPost))]
    [NotifyCanExecuteChangedFor(nameof(AcceptReplyCommand))]
    private Post? _post;

    bool _canForward;

    bool CanForward
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

    bool CanBack
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

    bool _canBack;

    async Task GetAndUpdatePostRepliesAsync(Post post, Func<Reply, bool>? prediction = default)
    {
        try
        {
            IsBusy = true;
            Post = post;
            var replies = await _postService.GetPostRepliesAsync(post.Id);
            await UpdateRepliesAndSelectReplyAsync(replies, prediction);
        }
        catch (Exception)
        {
            //TODO:异常处理 
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanBack))]
    async Task BackAsync()
    {
        if (Post is not null)
        {
            _forwardStack.Push(Post);
            CanForward = true;
        }
        await GetAndUpdatePostRepliesAsync(_backStack.Pop());
        if (_backStack.Count == 0)
            CanBack = false;
    }

    [RelayCommand(CanExecute = nameof(CanForward))]
    async Task ForwardAsync()
    {
        if (Post is not null)
        {
            _backStack.Push(Post);
            CanBack = true;
        }
        await GetAndUpdatePostRepliesAsync(_forwardStack.Pop());
        if (_forwardStack.Count == 0)
            CanForward = false;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCurrentReplyMostLiked), nameof(CurrentReplyState), nameof(IsCurrentReplyAccepted))]
    [NotifyCanExecuteChangedFor(nameof(AcceptReplyCommand))]
    private Reply? _currentReply;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentReplyIndex))]
    [NotifyCanExecuteChangedFor(nameof(MoveToNextReplyCommand), nameof(MoveToPreviousReplyCommand))]
    private int _selectedIndex = 0;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MoveToNextReplyCommand))]
    private int _repliesCount;

    [ObservableProperty]
    private bool _isCurrentReplyLiked = false;

    [ObservableProperty]
    private Reply? _userReply;

    private Reply? _mostLikedReply;

    public bool IsUserPost => Post is not null && _userService.IsUserLoggedIn && Post.Id == _userService.UserInfo!.Id;

    public bool IsUserReply => CurrentReply is not null && _userService.IsUserLoggedIn && CurrentReply.UserId == _userService.UserInfo!.Id;

    public bool CanAcceptReply => IsUserPost && !IsCurrentReplyAccepted;

    public bool IsCurrentReplyAccepted => CurrentReply is not null && CurrentReply.IsAccepted == 1;

    const string _postIsNullErrorMessage = "问题不存在或已被删除";

    const string _replyIsNullErrorMessage = "回答不存在或已被删除";

    const string _userNotLoggedInErrorMessage = "登录失效，请重新登录";

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
        try
        {
            await _dialogService.ShowDialogAsync("确定要采纳该回答吗？采纳后不可更改！", "警告", "取消", "确认");
            if (!_dialogService.GetIsRightButtonClicked())
                return;
            IsBusy = true;
            await _postService.AcceptReplyAsync(_userService.Token, CurrentReply!.Id);
            var replies = await _postService.GetPostRepliesAsync(Post!.Id);
            var targetId = CurrentReply!.Id;
            await UpdateRepliesAndSelectReplyAsync(replies, r => r.Id == targetId);
            // 仅在本地时保留
            OnPropertyChanged(nameof(IsCurrentReplyAccepted));
            OnPropertyChanged(nameof(CurrentReplyState));
        }
        catch (Exception ex)
        {
            _snackbarService.ShowErrorMessage("采纳回答失败", ex.Message);
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
        try
        {
            await _dialogService.ShowDialogAsync("确定要删除该回答吗？删除后不可恢复！", "警告", "取消", "确认");
            if (!_dialogService.GetIsRightButtonClicked())
                return;
            IsBusy = true;
            await _postService.DeleteReplyAsync(_userService.Token, CurrentReply!.Id);
            var replies = await _postService.GetPostRepliesAsync(Post!.Id);
            await UpdateRepliesAndSelectReplyAsync(replies);
        }
        catch (Exception ex)
        {
            _snackbarService.ShowErrorMessage("删除回答失败", ex.Message);
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
        try
        {
            await _dialogService.ShowDialogAsync("确定要删除该问题吗？删除后不可恢复！", "警告", "取消", "确认");
            if (!_dialogService.GetIsRightButtonClicked())
                return;
            IsBusy = true;
            await _postService.DeletePostAsync(_userService.Token, Post!.Id);
            Post = default;
            IsBusy = false;
        }
        catch (Exception ex)
        {
            _snackbarService.ShowErrorMessage("删除问题失败", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    async Task UpdateRepliesAndSelectReplyAsync(IEnumerable<Reply>? replies, Func<Reply, bool>? prediction = default)
    {
        if (replies is not null)
        {
            RepliesViewSource.Source = replies;
            RepliesViewSource.View.Refresh();
            RepliesCount = replies.Count();
            UserReply = _userService.IsUserLoggedIn ? replies.SingleOrDefault(r => r.UserId == _userService.UserInfo!.Id) : default;
            _mostLikedReply = replies.MaxBy(r => r.Likes);
            CurrentReply = prediction is null ? _mostLikedReply : replies.SingleOrDefault(prediction);
            IsCurrentReplyLiked = CurrentReply is not null && _userService.IsUserLoggedIn && await _postService.GetIsLikedAsync(_userService.Token, CurrentReply.Id);
        }
        else
        {
            RepliesViewSource.Source = default;
            RepliesCount = 0;
            UserReply = default;
            _mostLikedReply = default;
            CurrentReply = default;
            SelectedIndex = -1;
        }
    }

    [RelayCommand]
    private async Task MoveToUserReply()
    {
        ArgumentNullException.ThrowIfNull(UserReply);
        await SelectReplyAsync(UserReply);
    }

    [RelayCommand(CanExecute = nameof(IsNotLastReply))]
    private async Task MoveToNextReply()
    {
        SelectedIndex++;
        await SelectReplyAsync(CurrentReply!);
    }

    [RelayCommand(CanExecute = nameof(IsNotFirstReply))]
    private async Task MoveToPreviousReply()
    {
        SelectedIndex--;
        await SelectReplyAsync(CurrentReply!);
    }

    partial void OnIsReplyingChanged(bool value)
    {
        if (!IsReplying)
            NewReplyContent = string.Empty;
    }

    [RelayCommand]
    async Task ReplyPostAsync()
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
            var newReply = await _postService.PublishReplyAsync(_userService.Token, Post!.Id, NewReplyContent);
            var replies = await _postService.GetPostRepliesAsync(Post!.Id);
            await UpdateRepliesAndSelectReplyAsync(replies, r => r.Id == newReply.Id);
            IsReplying = false;
        }
        catch (Exception ex)
        {
            _snackbarService.ShowErrorMessage("提交回答失败", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    async Task LikeOrCancelLikeAsync()
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
                await _postService.CancelLikeAsync(_userService.Token, CurrentReply!.Id);
                CurrentReply!.Likes--;
            }
            else
            {
                await _postService.LikeAsync(_userService.Token, CurrentReply!.Id);
                CurrentReply!.Likes++;
            }
            IsBusy = false;
            IsCurrentReplyLiked = !IsCurrentReplyLiked;
            OnPropertyChanged(nameof(CurrentReply));
        }
        catch (Exception ex)
        {
            _snackbarService.ShowErrorMessage("点赞或取消点赞失败", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

}