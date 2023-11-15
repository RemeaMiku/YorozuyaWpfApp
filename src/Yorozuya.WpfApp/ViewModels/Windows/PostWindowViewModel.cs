using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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

public partial class PostWindowViewModel : BaseValidatorViewModel
{
    readonly Stack<Post> _backStack = new();
    readonly Stack<Post> _forwardStack = new();

    public PostWindowViewModel(ILeftRightButtonDialogService dialogService, ISnackbarService snackbarService, IUserService userService, IPostService postService)
    {
        _dialogService = dialogService;
        _snackbarService = snackbarService;
        _userService = userService;
        _postService = postService;
    }

    readonly ISnackbarService _snackbarService;

    public ISnackbarService GetSnackbarService() => _snackbarService;

    [ObservableProperty]
    bool _isReplying = false;

    public bool IsOrderByLikes
    {
        get => _isOrderByLikes;
        set
        {
            if (value == _isOrderByLikes)
                return;
            OnPropertyChanging(nameof(IsOrderByLikes));
            _isOrderByLikes = value;
            OnPropertyChanged(nameof(IsOrderByLikes));
            if (RepliesViewSource.View is null)
                return;
            RepliesViewSource.View.SortDescriptions.Clear();
            if (_isOrderByLikes)
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
    }

    [ObservableProperty]
    string _newReplyContent = string.Empty;

    public int ReplyMaxLength { get; } = 10000;

    [RelayCommand]
    void OpenReplyPanel()
    {
        IsReplying = true;
    }

    [RelayCommand]
    void CancelReply()
    {
        NewReplyContent = string.Empty;
        IsReplying = false;
    }

    public CollectionViewSource RepliesViewSource { get; } = new();

    public Reply? CurrentReply
    {
        get => _currentReply;

        set
        {
            if (value == _currentReply)
                return;
            OnPropertyChanging(nameof(CurrentReply));
            _currentReply = value;
            OnPropertyChanged(nameof(CurrentReply));
            OnPropertyChanged(nameof(IsCurrentReplyMostLiked));
            OnPropertyChanged(nameof(CurrentReplyState));
            AcceptReplyCommand.NotifyCanExecuteChanged();
            if (_currentReply is null)
                return;
            UpdateIsLikedAndIsUserReplyCommand.Execute(default);
        }
    }

    public int CurrentReplyIndex => SelectedIndex + 1;

    public bool IsNotFirstReply => CurrentReplyIndex > 1;

    public bool IsNotLastReply => CurrentReplyIndex < RepliesCount;

    public bool IsCurrentReplyMostLiked => CurrentReply == _mostLikedReply;

    public ReplyState CurrentReplyState
    {
        get
        {
            if (CurrentReply is null)
                return ReplyState.Default;
            if (CurrentReply.IsAccepted && IsCurrentReplyMostLiked)
                return ReplyState.IsAcceptedAndMostLiked;
            if (CurrentReply.IsAccepted)
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

    private bool _isOrderByLikes = true;

    private Post? _post;

    public Post? Post
    {
        get => _post;
        set
        {
            if (value == _post)
                return;
            OnPropertyChanging(nameof(Post));
            _post = value;
            OnPropertyChanged(nameof(Post));
            BackCommand.NotifyCanExecuteChanged();
            ForwardCommand.NotifyCanExecuteChanged();
            LoadPostRepliesCommand.Execute(default);
        }
    }

    bool CanBack => _backStack.Any();

    [RelayCommand(CanExecute = nameof(CanBack))]
    void Back()
    {
        if (Post is not null)
            _forwardStack.Push(Post);
        Post = _backStack.Pop();
    }

    bool CanForward => _forwardStack.Any();

    [RelayCommand(CanExecute = nameof(CanForward))]
    void Forward()
    {
        if (Post is not null)
            _backStack.Push(Post);
        Post = _forwardStack.Pop();
    }

    public void CheckForward()
    {
        if (!CanForward)
            return;
        if (Post != _forwardStack.Pop())
        {
            _forwardStack.Clear();
            ForwardCommand.NotifyCanExecuteChanged();
        }
    }

    public void PushBackward()
    {
        if (Post is null)
            return;
        _backStack.Push(Post);
        BackCommand.NotifyCanExecuteChanged();
    }

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

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptReplyCommand))]
    private bool _isUserPost = false;

    [ObservableProperty]
    private bool _isUserReply = false;

    public bool CanAcceptReply => CurrentReply is not null && IsUserPost && !CurrentReply.IsAccepted;

    const string _postIsNullErrorMessage = "问题不存在或已被删除";

    const string _replyIsNullErrorMessage = "回答不存在或已被删除";

    const string _userNotLoggedInErrorMessage = "登录失效，请重新登录";

    [RelayCommand(CanExecute = nameof(CanAcceptReply))]
    private async Task AcceptReplyAsync()
    {
        if (_snackbarService.ShowErrorMessageIfAny("采纳回答失败",
            (() => Post is null, _postIsNullErrorMessage),
            (() => CurrentReply is null, _replyIsNullErrorMessage),
            (() => !_userService.IsUserLoggedIn(), _userNotLoggedInErrorMessage),
            (() => CurrentReply!.IsAccepted, "该回答已被采纳，请勿重复操作"),
            (() => !IsUserPost, "你不是提问者，无法接受该回答")))
            return;
        try
        {
            await _dialogService.ShowDialogAsync("确定要采纳该回答吗？采纳后不可更改！", "警告", "取消", "确认");
            if (!_dialogService.GetIsRightButtonClicked())
                return;
            IsBusy = true;
            await _postService.AcceptReplyAsync(Post!, CurrentReply!);
            await LoadPostRepliesAsync();
            OnPropertyChanged(nameof(CurrentReply));
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
             (() => !_userService.IsUserLoggedIn(), _userNotLoggedInErrorMessage),
             (() => CurrentReply!.DelTag != 0, "该回答已被删除，请勿重复操作"),
             (() => !IsUserReply, "你不是此回答的作者，无法删除该回答")))
            return;
        try
        {
            await _dialogService.ShowDialogAsync("确定要删除该回答吗？删除后不可恢复！", "警告", "取消", "确认");
            if (!_dialogService.GetIsRightButtonClicked())
                return;
            IsBusy = true;
            await _postService.DeleteReplyAsync(CurrentReply!);
            await LoadPostRepliesAsync();
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
           (() => !_userService.IsUserLoggedIn(), _userNotLoggedInErrorMessage),
           (() => !IsUserPost, "你不是提问者，无法删除该问题"),
           (() => Post!.DelTag != 0, "该问题已被删除，请勿重复操作")))
            return;
        try
        {
            await _dialogService.ShowDialogAsync("确定要删除该问题吗？删除后不可恢复！", "警告", "取消", "确认");
            if (!_dialogService.GetIsRightButtonClicked())
                return;
            IsBusy = true;
            await _postService.DeletePostAsync(Post!);
            Post = default;
            await LoadPostRepliesAsync();
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



    [RelayCommand]
    private async Task LoadPostRepliesAsync()
    {
        if (Post is null)
            return;
        if (Post.DelTag != 0)
        {
            Post = default;
            return;
        }
        try
        {
            if (_snackbarService.ShowErrorMessageIf("加载失败", () => !_userService.IsUserLoggedIn(), _userNotLoggedInErrorMessage))
                return;
            IsBusy = true;
            IsUserPost = _postService.GetIsUserPost(Post);
            var replies = await _postService.GetPostRepliesAsync(Post);
            ArgumentNullException.ThrowIfNull(replies);
            if (replies.Any())
            {
                UserReply = replies.SingleOrDefault(r => r.UserId == _userService.UserInfo!.Id);
                _mostLikedReply = replies.MaxBy(r => r.Likes);
                RepliesViewSource.Source = replies;
                RepliesCount = replies.Count();
                IsOrderByLikes = true;
                RepliesViewSource.View.Refresh();
                CurrentReply = _mostLikedReply;
            }
            else
            {
                UserReply = default;
                RepliesViewSource.Source = default;
                _mostLikedReply = default;
                RepliesCount = 0;
                CurrentReply = default;
                SelectedIndex = -1;
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowDialogAsync(ex.Message, "加载失败", null, "重试");
            if (_dialogService.GetIsRightButtonClicked())
                LoadPostRepliesCommand.Execute(default);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void MoveToUserReply()
    {
        ArgumentNullException.ThrowIfNull(UserReply);
        CurrentReply = UserReply;
    }

    [RelayCommand(CanExecute = nameof(IsNotLastReply))]
    private void MoveToNextReply()
    {
        SelectedIndex++;
    }

    [RelayCommand(CanExecute = nameof(IsNotFirstReply))]
    private void MoveToPreviousReply()
    {
        SelectedIndex--;
    }

    [RelayCommand]
    private async Task UpdateIsLikedAndIsUserReplyAsync()
    {
        try
        {
            if (CurrentReply is null)
            {
                IsCurrentReplyLiked = false;
                IsUserReply = false;
                return;
            }
            IsBusy = true;
            var isUserReply = _postService.GetIsUserReply(CurrentReply);
            var isLiked = await _postService.GetIsLikedAsync(CurrentReply);
            IsCurrentReplyLiked = isLiked;
            IsUserReply = isUserReply;
        }
        catch (Exception ex)
        {
            await _dialogService.ShowDialogAsync(ex.Message, "加载失败", null, "重试");
            if (_dialogService.GetIsRightButtonClicked())
                UpdateIsLikedAndIsUserReplyCommand.Execute(default);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    async Task ReplyPostAsync()
    {
        if (_snackbarService.ShowErrorMessageIfAny("提交回答失败",
            (() => Post is null, _postIsNullErrorMessage),
            (() => !_userService.IsUserLoggedIn(), _userNotLoggedInErrorMessage),
            (() => string.IsNullOrEmpty(NewReplyContent), "回答不能为空"),
            (() => NewReplyContent.Length > ReplyMaxLength, $"回答字数不能超过 {ReplyMaxLength} 个字")))
            return;
        try
        {
            IsBusy = true;
            var newReply = await _postService.ReplyPostAsync(Post!, NewReplyContent);
            NewReplyContent = string.Empty;
            IsReplying = false;
            await LoadPostRepliesAsync();
            CurrentReply = newReply;
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
           (() => !_userService.IsUserLoggedIn(), _userNotLoggedInErrorMessage),
           (() => CurrentReply is null, _replyIsNullErrorMessage)))
            return;
        try
        {
            IsBusy = true;
            if (IsCurrentReplyLiked)
                await _postService.CancelLikeAsync(CurrentReply!);
            else
                await _postService.LikeAsync(CurrentReply!);
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