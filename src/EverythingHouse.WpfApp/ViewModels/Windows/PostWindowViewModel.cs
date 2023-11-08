using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EverythingHouse.WpfApp.Models;

namespace EverythingHouse.WpfApp.ViewModels.Windows;

public partial class PostWindowViewModel : BaseViewModel
{
    public PostWindowViewModel()
    {

    }

    [ObservableProperty]
    Post? _post;

    public CollectionViewSource RepliesViewSource { get; } = new();

    [ObservableProperty]
    Reply? _currentReply;

    [ObservableProperty]
    bool _isCurrentReplyLiked = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPostNotReplied))]
    bool _isPostReplied = false;

    [ObservableProperty]
    bool _isCurrentReplyMostLiked = false;

    [ObservableProperty]
    bool _isCurrentReplyDeletable = false;

    [ObservableProperty]
    bool _isPostDeletable = false;

    public bool IsPostNotReplied => !IsPostReplied;
}
