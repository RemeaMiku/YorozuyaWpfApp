using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EverythingHouse.WpfApp.Models;

namespace EverythingHouse.WpfApp.Servcies;

public interface IPostService
{
    public Task<IEnumerable<Reply>?> GetPostRepliesAsync(Post post);

    public Task<bool?> GetIsLikedAsync(Reply reply);

    public bool GetIsUserPost(Post post);

    public bool GetIsUserReply(Reply reply);

    public Task AcceptReplyAsync(Post post, Reply reply);

    public Task ReplyPostAsync(Post post, Reply reply);

    public Task LikeReplyAsync(Reply reply);

    public Task CancelLikeAsync(Reply reply);

    public Task DeletePostAsync(Post post);

    public Task DeleteReplyAsync(Reply reply);

}
