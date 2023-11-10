using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EverythingHouse.WpfApp.Common;
using EverythingHouse.WpfApp.Models;
using EverythingHouse.WpfApp.Servcies.Contracts;

namespace EverythingHouse.WpfApp.ViewModels.Windows;

public partial class PostWindowViewModel : BaseViewModel
{
    readonly ICancelConfirmDialogService _dialogService;
    readonly IUserService _userService;
    readonly IPostService _postService;

    //TODO ：测试
    readonly Post _localPost = new() { AskerId = 0, Id = 114514, Title = "初音未来是第一个虚拟歌姬吗？", Content = "初音未来是第一个虚拟歌姬吗？", CreateTime = "2023.08.31 11:14:51", UpdateTime = "2023.08.31 11:14:51", Field = "VOCALOID", Views = 831 };

    public PostWindowViewModel(ICancelConfirmDialogService dialogService, IUserService userService, IPostService postService)
    {
        _dialogService = dialogService;
        _userService = userService;
        _postService = postService;
        //TODO ：测试
        Test();
    }

    [ObservableProperty]
    bool _isPostDeleted = false;


    public ICancelConfirmDialogService GetCancelConfirmDialogService() => _dialogService;

    void Test()
    {
        // TODO：测试
        Post = _localPost;
        LoadPostRepliesCommand.Execute(Post);
    }

    bool _isOrderByLikes = true;

    readonly static SortDescription _likesSortDescription = new(nameof(Reply.Likes), ListSortDirection.Descending);
    readonly static SortDescription _creatTimeSortDescription = new(nameof(Reply.CreateTime), ListSortDirection.Descending);

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

    [RelayCommand]
    async Task DeletePostAsync()
    {
        if (Post is null)
        {
            await _dialogService.ShowDialogAsync("当前问题不存在", "错误");
            return;
        }
        if (!IsUserPost)
        {
            await _dialogService.ShowDialogAsync("你不是提问者，无法删除该问题", "无权限");
            return;
        }
        try
        {
            await _dialogService.ShowDialogAsync("确定要删除该问题吗？删除后不可恢复！", "删除问题");
            if (!_dialogService.GetIsConfirmed())
                return;
            IsBusy = true;
            await _postService.DeletePostAsync(Post);
            await LoadPostRepliesAsync(Post);
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
    async Task LoadPostRepliesAsync(Post post)
    {
        if (post.DelTag == 1)
        {
            IsPostDeleted = true;
            return;
        }
        IsPostDeleted = false;
        try
        {
            ArgumentNullException.ThrowIfNull(_userService.UserInfo);
            IsBusy = true;
            IsUserPost = _postService.GetIsUserPost(post);
            var replies = await _postService.GetPostRepliesAsync(post);
            ArgumentNullException.ThrowIfNull(replies);
            if (replies.Any())
            {
                UserReply = replies.SingleOrDefault(r => r.UserId == _userService.UserInfo.Id);
                _mostLikedReply = replies.MaxBy(r => r.Likes);
                RepliesViewSource.Source = replies;
                RepliesCount = replies.Count();
                IsOrderByLikes = true;
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

    [ObservableProperty]
    Post? _post;

    public CollectionViewSource RepliesViewSource { get; } = new();

    Reply? _currentReply;

    //public bool IsCurrentReplyNull => CurrentReply is null;

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
            UpdateIsLikedAndIsUserReplyCommand.Execute(default);
        }
    }

    [RelayCommand]
    void MoveToUserReply()
    {
        ArgumentNullException.ThrowIfNull(UserReply);
        CurrentReply = UserReply;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentReplyIndex))]
    [NotifyCanExecuteChangedFor(nameof(MoveToNextReplyCommand), nameof(MoveToPreviousReplyCommand))]
    int _selectedIndex = 0;

    public int CurrentReplyIndex => SelectedIndex + 1;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MoveToNextReplyCommand))]
    int _repliesCount;

    public bool IsNotFirstReply => CurrentReplyIndex > 1;

    public bool IsNotLastReply => CurrentReplyIndex < RepliesCount;


    [RelayCommand(CanExecute = nameof(IsNotLastReply))]
    void MoveToNextReply()
    {
        SelectedIndex++;
    }

    [RelayCommand(CanExecute = nameof(IsNotFirstReply))]
    void MoveToPreviousReply()
    {
        SelectedIndex--;
    }

    [RelayCommand]
    async Task UpdateIsLikedAndIsUserReplyAsync()
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
            var isLiked = await _postService.GetIsLikedAsync(CurrentReply!);
            var isUserReply = _postService.GetIsUserReply(CurrentReply!);
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

    [ObservableProperty]
    bool _isCurrentReplyLiked = false;

    public bool IsCurrentReplyMostLiked => CurrentReply == _mostLikedReply;

    [ObservableProperty]
    Reply? _userReply;

    Reply? _mostLikedReply;

    [ObservableProperty]
    bool _isUserPost = false;

    [ObservableProperty]
    bool _isUserReply = false;

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
}
