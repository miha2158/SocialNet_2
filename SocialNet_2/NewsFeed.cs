using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SocialNet
{
    public class NewsFeed
    {
        public ObservableCollection<NewsItem> UserPosts = new ObservableCollection<NewsItem>();

        public ObservableCollection<NewsItem> Feed = new ObservableCollection<NewsItem>();

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