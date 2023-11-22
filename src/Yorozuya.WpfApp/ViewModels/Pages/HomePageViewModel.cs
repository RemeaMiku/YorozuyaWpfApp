using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.ViewModels.Pages;

public partial class HomePageViewModel : BaseValidatorViewModel
{
    private readonly List<string> _fields = ["文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏", "aaa", "bbb"];

    [ObservableProperty] private string _nowSelectedField;

    public CollectionViewSource FieldSource { get; } = new();
    public ObservableCollection<Post> PostSource { get; } = [];

    partial void OnNowSelectedFieldChanged(string value)
    {
        RefreshPostsCommand.Execute(default);
    }

    [RelayCommand]
    void OpenPost(Post post)
    {
        _messenger.Send(post);
    }

    [RelayCommand]
    private async Task RefreshPosts()
    {
        IsBusy = true;
        try
        {
            var source = await _postService.GetPostsByFieldAsync(NowSelectedField);
            PostSource.Clear();
            if (source is null) return;
            foreach (Post post in source)
            {
                PostSource.Add(post);
            }
        }
        catch (Exception e)
        {
            //TODO: ErrorDialog
            Console.WriteLine(e);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private readonly IPostService _postService;
    private readonly IMessenger _messenger;

    //[RelayCommand]
    //async Task Test()
    //{
    //    var posts = await _postService.GetPostsByFieldAsync("Test");
    //    foreach (var post in posts!)
    //    {
    //        _messenger.Send(post);
    //        await Task.Delay(3000);
    //    }
    //}

    public HomePageViewModel(IPostService nowPostService, IMessenger messenger)
    {
        _postService = nowPostService;
        _messenger = messenger;
        FieldSource.Source = _fields;
        FieldSource.View.Refresh();
        NowSelectedField = _fields[0];
        //TestCommand.Execute(default);
    }
}
