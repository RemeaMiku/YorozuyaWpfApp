using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EverythingHouse.WpfApp.Common;
using EverythingHouse.WpfApp.Models;
using EverythingHouse.WpfApp.Servcies;
using EverythingHouse.WpfApp.Servcies.DesignTime;

namespace EverythingHouse.WpfApp.ViewModels.Windows;

public partial class PostWindowViewModel : BaseViewModel
{

    readonly IPostService _postService;

    //TODO ：测试
    readonly Post _localPost = new() { AskerId = new Random().Next(100), Id = 114514, Title = "初音未来是第一个虚拟歌姬吗？", Content = "初音未来是第一个虚拟歌姬吗？", CreateTime = "2023.08.31 11:14:51", UpdateTime = "2023.08.31 11:14:51", Field = "VOCALOID", Views = 831 };

    public PostWindowViewModel(IPostService postService)
    {
        _postService = postService;
        Test();
    }

    void Test()
    {
        // TODO：测试
        Post = _localPost;
        LoadPostRepliesCommand.Execute(Post);
    }

    bool _isOrderByLikes = true;

    readonly SortDescription _likesSortDescription = new(nameof(Reply.Likes), ListSortDirection.Descending);
    readonly SortDescription _creatTimeSortDescription = new(nameof(Reply.CreateTime), ListSortDirection.Descending);

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
    async Task LoadPostRepliesAsync(Post post)
    {
        try
        {
            IsBusy = true;
            IsUserPost = _postService.GetIsUserPost(post);
            var replies = await _postService.GetPostRepliesAsync(post);
            ArgumentNullException.ThrowIfNull(replies);
            _mostLikedReply = replies.MaxBy(r => r.Likes);
            RepliesViewSource.Source = replies;
            RepliesViewSource.View.SortDescriptions.Add(new(nameof(Reply.Likes), ListSortDirection.Descending));
            RepliesCount = replies.Count();
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
        RepliesViewSource.View.MoveCurrentToNext();
    }

    [RelayCommand(CanExecute = nameof(IsNotFirstReply))]
    void MoveToPreviousReply()
    {
        RepliesViewSource.View.MoveCurrentToPrevious();
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
            var isLiked = await _postService.GetIsLikedAsync(CurrentReply!);
            ArgumentNullException.ThrowIfNull(isLiked);
            var isUserReply = _postService.GetIsUserReply(CurrentReply!);
            IsCurrentReplyLiked = isLiked.Value;
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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPostNotReplied))]
    bool _isPostReplied = false;

    public bool IsCurrentReplyMostLiked => CurrentReply == _mostLikedReply;

    Reply? _mostLikedReply;

    [ObservableProperty]
    bool _isUserPost = false;

    [ObservableProperty]
    bool _isUserReply = false;

    public bool IsPostNotReplied => !IsPostReplied;

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
