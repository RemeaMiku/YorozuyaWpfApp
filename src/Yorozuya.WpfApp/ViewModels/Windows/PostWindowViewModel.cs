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
using Yorozuya.WpfApp.Common;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.ViewModels.Windows;

public partial class PostWindowViewModel : BaseValidatorViewModel
{
    readonly Stack<Post> _backStack = new();
    readonly Stack<Post> _forwardStack = new();

    public PostWindowViewModel(ICancelConfirmDialogService dialogService, IUserService userService, IPostService postService)
    {
        _dialogService = dialogService;
        _userService = userService;
        _postService = postService;
    }

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
    string _newReply = string.Empty;

    public int ReplyMaxLength { get; } = 10000;

    [RelayCommand]
    void OpenReplyPanel()
    {
        IsReplying = true;
    }

    [RelayCommand]
    void CancelReply()
    {
        NewReply = string.Empty;
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

    public ICancelConfirmDialogService GetCancelConfirmDialogService() => _dialogService;

    private static readonly SortDescription _likesSortDescription = new(nameof(Reply.Likes), ListSortDirection.Descending);

    private static readonly SortDescription _creatTimeSortDescription = new(nameof(Reply.CreateTime), ListSortDirection.Descending);

    private readonly ICancelConfirmDialogService _dialogService;

    private readonly IUserService _userService;

    private readonly IPostService _postService;

    //private readonly Post _localPost = new() { AskerId = 0, Id = 114514, Title = "初音未来是第一个虚拟歌姬吗？", Content = "初音未来是第一个虚拟歌姬吗？", CreateTime = "2023.08.31 11:14:51", UpdateTime = "2023.08.31 11:14:51", Field = "VOCALOID", Views = 831 };

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

    [RelayCommand(CanExecute = nameof(CanAcceptReply))]
    private async Task AcceptReplyAsync()
    {
        ArgumentNullException.ThrowIfNull(Post);
        ArgumentNullException.ThrowIfNull(CurrentReply);
        ArgumentNullException.ThrowIfNull(_userService.UserInfo);
        if (CurrentReply.IsAccepted)
        {
            await _dialogService.ShowDialogAsync("请勿重复操作", "错误");
            return;
        }
        if (!IsUserPost)
        {
            await _dialogService.ShowDialogAsync("你不是提问者，无法接受该回答", "无权限");
            return;
        }
        try
        {
            await _dialogService.ShowDialogAsync("确定要采纳该回答吗？采纳后不可更改！", "接受回答");
            if (!_dialogService.GetIsConfirmed())
                return;
            IsBusy = true;
            await _postService.AcceptReplyAsync(Post, CurrentReply);
            await LoadPostRepliesAsync();
            OnPropertyChanged(nameof(CurrentReply));
            OnPropertyChanged(nameof(CurrentReplyState));
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteReplyAsync()
    {
        ArgumentNullException.ThrowIfNull(Post);
        ArgumentNullException.ThrowIfNull(CurrentReply);
        ArgumentNullException.ThrowIfNull(_userService.UserInfo);
        if (CurrentReply.DelTag == 1)
        {
            await _dialogService.ShowDialogAsync("请勿重复操作", "错误");
            return;
        }
        if (!IsUserReply)
        {
            await _dialogService.ShowDialogAsync("你不是此回答的作者，无法删除该回答", "无权限");
            return;
        }
        try
        {
            await _dialogService.ShowDialogAsync("确定要删除该回答吗？删除后不可恢复！", "删除回答");
            if (!_dialogService.GetIsConfirmed())
                return;
            IsBusy = true;
            await _postService.DeleteReplyAsync(CurrentReply);
            await LoadPostRepliesAsync();
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeletePostAsync()
    {
        ArgumentNullException.ThrowIfNull(Post);
        if (Post.DelTag == 1)
        {
            await _dialogService.ShowDialogAsync("请勿重复操作", "错误");
            return;
        }
        if (!IsUserPost)
        {
            await _dialogService.ShowDialogAsync("你不是提问者，无法删除该问题", "无权限");
            return;
        }
        try
        {
            await _dialogService.ShowDialogAsync("确定要删除该问题吗？删除后不可恢复！", "警告");
            if (!_dialogService.GetIsConfirmed())
                return;
            IsBusy = true;
            await _postService.DeletePostAsync(Post);
            Post = default;
            await LoadPostRepliesAsync();
        }
        catch (Exception)
        {
            throw;
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
            ArgumentNullException.ThrowIfNull(_userService.UserInfo);
            IsBusy = true;
            IsUserPost = _postService.GetIsUserPost(Post);
            var replies = await _postService.GetPostRepliesAsync(Post);
            ArgumentNullException.ThrowIfNull(replies);
            if (replies.Any())
            {
                UserReply = replies.SingleOrDefault(r => r.UserId == _userService.UserInfo.Id);
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
        catch (Exception)
        {
            throw;
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
        catch (Exception)
        {
            throw;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    async Task ReplyPostAsync()
    {
        ArgumentNullException.ThrowIfNull(Post);
        ArgumentNullException.ThrowIfNull(_userService.UserInfo);
        if (string.IsNullOrEmpty(NewReply))
        {
            await _dialogService.ShowDialogAsync("回答不能为空", "错误");
            return;
        }
        try
        {
            IsBusy = true;
            var newReply = new Reply() { Content = NewReply, UserId = _userService.UserInfo.Id, PostId = Post.Id };
            await _postService.ReplyPostAsync(Post, newReply);
            NewReply = string.Empty;
            IsReplying = false;
            await LoadPostRepliesAsync();
            CurrentReply = newReply;
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    async Task LikeOrCancelLikeAsync()
    {
        ArgumentNullException.ThrowIfNull(Post);
        ArgumentNullException.ThrowIfNull(CurrentReply);
        ArgumentNullException.ThrowIfNull(_userService.UserInfo);
        try
        {
            IsBusy = true;
            if (IsCurrentReplyLiked)
            {
                await _postService.CancelLikeAsync(CurrentReply);
            }
            else
            {
                await _postService.LikeAsync(CurrentReply);
            }
            IsCurrentReplyLiked = !IsCurrentReplyLiked;
            OnPropertyChanged(nameof(CurrentReply));
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            IsBusy = false;
        }
    }
}