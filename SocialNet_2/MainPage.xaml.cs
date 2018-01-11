using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Pickers.Provider;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace SocialNet
{
    internal enum DisplayMode
    {
        ActiveUserFeed,
        ActiveUserPosts,
        ActiveUserInfo,
        SubscriptionPosts,
        UserInfo
    }

    public sealed partial class MainPage : Page
    {
        private void Breakpoint(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        public MainPage()
        {
            InitializeComponent();
        }
        private async void MainGrid_OnLoaded(object sender, RoutedEventArgs e)
        {
            var UsersAdded = 6;

            for (var i = 0; i < UsersAdded; i++)
                AllUsers.Add(new User(await Person.MakeNew()));

            AllUsers[1].Subscribe(AllUsers[5]);
            AllUsers[0].Subscribe(AllUsers[1]);
            AllUsers[2].Subscribe(AllUsers[4]);
            AllUsers[4].Subscribe(AllUsers[5]);
            AllUsers[1].Subscribe(AllUsers[2]);
            AllUsers[0].Subscribe(AllUsers[3]);
            AllUsers[0].Subscribe(AllUsers[5]);
            AllUsers[2].Subscribe(AllUsers[3]);



            
            ChooseUser.SelectedIndex = 0;


            //TODO: add a testing thingy
            UpdateCollections();
        }

        public ObservableCollection<User> AllUsers = new ObservableCollection<User>();

        public User ActiveUser;
        public User DisplayUser;
        public ObservableCollection<User> ActiveUserSubscriptions;

        public ObservableCollection<NewsItem> AllPosts;
        private DisplayMode displayMode = DisplayMode.ActiveUserInfo;

        public NewsItem NewNewsItem = new NewsItem();

        private void UpdateCollections()
        {
            ActiveUserSubscriptions = new ObservableCollection<User>(DisplayUser.Subscriptions);
            FriendsList.ItemsSource = ActiveUserSubscriptions;
            FriendsList.UpdateLayout();

            if (displayMode == DisplayMode.ActiveUserPosts || displayMode == DisplayMode.ActiveUserFeed)
                AllPosts = new ObservableCollection<NewsItem>(displayMode == DisplayMode.ActiveUserPosts
                                                                  ? DisplayUser.News.UserPosts
                                                                  : DisplayUser.News.Feed);

            NewsFeed.ItemsSource = AllPosts;
            NewsFeed.UpdateLayout();
            foreach (var element in UserInfo.Children)
                element.UpdateLayout();
        }
        
        private void AddFriendButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayUser = ActiveUser;
            //TODO: new friends picker

            User r;

            do
            {
                r = AllUsers[Generate.Int(AllUsers.Count)];
            } while (ActiveUserSubscriptions.Contains(r) || DisplayUser == r);

            DisplayUser.Subscribe(r);
            UpdateCollections();
        }
        private async void NewUser_Click(object sender, RoutedEventArgs e)
        {
            AllUsers.Add(new User(await Person.MakeNew()));

            ChooseUser.SelectedIndex = AllUsers.Count;
        }
        private void ChooseUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActiveUser = (User)e.AddedItems.Last();
            DisplayUser = ActiveUser;
            UserName.Content = ActiveUser.ToString();

            UpdateCollections();
        }

        private void FriendItemName_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            DisplayUser = ActiveUserSubscriptions[FriendsList.SelectedIndex];
            AllPosts = new ObservableCollection<NewsItem>(DisplayUser.News.UserPosts);
            NewsFeedLabel.Content = DisplayUser.FullName + "'s Posts";

            NewsFeed.ItemsSource = AllPosts;
            NewsFeed.UpdateLayout();
        }
        private async void FriendItemName_OnPointerPressed(object sender, DoubleTappedRoutedEventArgs e)
        {
            DisplayUser = ActiveUser;
            var dialog = new MessageDialog($"Do you really wish to remove {ActiveUserSubscriptions[FriendsList.SelectedIndex]} from your friends?", "Are you sure");
            dialog.Commands.Add(new UICommand("Yes"));
            dialog.Commands.Add(new UICommand("No"));

            if ((await dialog.ShowAsync()).Label == "No")
                return;
            DisplayUser.UnSubscribe(ActiveUserSubscriptions[FriendsList.SelectedIndex]);
            UpdateCollections();
        }
        private void UserName_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (ActiveUser == DisplayUser)
            {
                if (displayMode == DisplayMode.ActiveUserInfo)
                {
                    displayMode = DisplayMode.ActiveUserFeed;
                    NewsFeedLabel.Visibility = Visibility.Collapsed;
                    NewPost.Visibility = Visibility.Collapsed;
                    NewsFeed.Visibility = Visibility.Collapsed;
                    UserInfo.Visibility = Visibility.Visible;
                }
                else
                {
                    displayMode = DisplayMode.ActiveUserInfo;
                    NewsFeedLabel.Visibility = Visibility.Visible;
                    NewPost.Visibility = Visibility.Visible;
                    NewsFeed.Visibility = Visibility.Visible;
                    UserInfo.Visibility = Visibility.Collapsed;
                }
            }
            else if (displayMode == DisplayMode.UserInfo)
            {
                displayMode = DisplayMode.SubscriptionPosts;
                NewsFeedLabel.Visibility = Visibility.Collapsed;
                NewPost.Visibility = Visibility.Collapsed;
                NewsFeed.Visibility = Visibility.Collapsed;
                UserInfo.Visibility = Visibility.Visible;
            }
            else
            {
                displayMode = DisplayMode.UserInfo;
                NewsFeedLabel.Visibility = Visibility.Visible;
                NewPost.Visibility = Visibility.Visible;
                NewsFeed.Visibility = Visibility.Visible;
                UserInfo.Visibility = Visibility.Collapsed;
            }

            UpdateCollections();
        }
        
        private void PostNewNewsItem_Click(object sender, RoutedEventArgs e)
        {
            DisplayUser = ActiveUser;
            NewNewsItem.publisher = DisplayUser;
            NewNewsItem.Content = NewNewsText.Text;
            NewNewsItem.publishTime = DateTime.UtcNow;
            DisplayUser.AddPost(NewNewsItem);
            UpdateCollections();

            NewNewsText.Text = string.Empty;
            AddImageToNewNewsItem.Content = "+";
            
            NewNewsItem = new NewsItem();
        }
        private async void AddImageToNewNewsItem_Click(object sender, RoutedEventArgs e)
        {
            if (NewNewsItem.Image != null)
            {
                NewNewsItem.Image = null;
                AddImageToNewNewsItem.Content = "+";
                return;
            }
            var p = new FileOpenPicker();

            p.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            p.ViewMode = PickerViewMode.Thumbnail;
            p.FileTypeFilter.Add(".jpg");
            p.FileTypeFilter.Add(".jpeg");
            p.FileTypeFilter.Add(".png");
            p.FileTypeFilter.Add(".tiff");
            p.FileTypeFilter.Add(".gif");

            var p1 = await p.PickSingleFileAsync();
            if (p1 == null)
                return;
            NewNewsItem.Image = new BitmapImage(new Uri(p1.Path, UriKind.Absolute));
            AddImageToNewNewsItem.Content = "-";
        }
        private async void NewsPost_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            DisplayUser = ActiveUser;
            if (AllPosts[NewsFeed.SelectedIndex].publisher == DisplayUser)
            {
                var dialog = new MessageDialog($"Do you really wish to remove this post?", "Are you sure");
                dialog.Commands.Add(new UICommand("Yes"));
                dialog.Commands.Add(new UICommand("No"));

                if ((await dialog.ShowAsync()).Label == "No")
                    return;

                DisplayUser.RemovePost(AllPosts[NewsFeed.SelectedIndex]);
                UpdateCollections();
            }
        }

        private void ModeChange_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            DisplayUser = ActiveUser;
            if (displayMode == DisplayMode.ActiveUserFeed)
                displayMode = DisplayMode.ActiveUserPosts;
            else displayMode = DisplayMode.ActiveUserFeed;


            NewsFeedLabel.Content = displayMode == DisplayMode.ActiveUserPosts ? "Your Posts" : "News Feed";
            UpdateCollections();
        }

        private void RepeatBD()
        {
            var prev = DateTime.UtcNow;

            var t = new Timer(delegate
                              {

                              }
                             ,null,0,10000);
        }

    }
}