using System;
using System.Collections.Generic;

namespace SocialNet
{
    public class NewsFeed
    {
        public SortedSet<NewsItem> UserPosts = new SortedSet<NewsItem>();

        public SortedSet<NewsItem> Feed = new SortedSet<NewsItem>();

        public event EventHandler<NewsItem> AddNewItem;
        public event EventHandler<NewsItem> RemoveNewsItem;

        public void InvokeAdd(User sender, NewsItem item)
        {
            AddNewItem(sender, item);
        }
        public void InvokeRemove(User sender, NewsItem item)
        {
            RemoveNewsItem(sender, item);
        }

        public NewsFeed() { }
        public NewsFeed(User u)
        {
            AddNewItem += u.AddNewPost;
        }
    }
}