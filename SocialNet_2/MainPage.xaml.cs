using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Pickers.Provider;
using Windows.System;
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
    public enum DisplayMode
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
                AllUsers.Add(await User.MakeNew());
            
            AllUsers[1].Subscribe(AllUsers[5]);
            AllUsers[0].Subscribe(AllUsers[1]);
            AllUsers[1].Subscribe(AllUsers[2]);
            AllUsers[2].Subscribe(AllUsers[3]);
            AllUsers[4].Subscribe(AllUsers[5]);
            AllUsers[0].Subscribe(AllUsers[3]);
            AllUsers[2].Subscribe(AllUsers[4]);
            AllUsers[0].Subscribe(AllUsers[5]);

            displayMode = DisplayMode.ActiveUserInfo;
            ActiveUser = AllUsers[0];
            DisplayUser = ActiveUser;

            ChooseUser.SelectedIndex = 0;
            
            //TODO: add a testing thingy??


            UpdateCollections();
        }

        public ObservableCollection<User> AllUsers = new ObservableCollection<User>();

        public User ActiveUser;
        public User DisplayUser;
        public ObservableCollection<User> ActiveUserSubscriptions;

        public ObservableCollection<NewsItem> AllPosts;
        public DisplayMode displayMode;

        public NewsItem NewNewsItem = new NewsItem();

        public void ReadInfo()
        {
            UserInfo_First_Edit.Visibility = Visibility.Collapsed;
            UserInfo_First.Visibility = Visibility.Visible;
            UserInfo_First.Text = DisplayUser.First;

            UserInfo_Last_Edit.Visibility = Visibility.Collapsed;
            UserInfo_Last.Visibility = Visibility.Visible;
            UserInfo_Last.Text = DisplayUser.Last;

            UserInfo_RelationshipStatus.IsEnabled = ActiveUser == DisplayUser;
            UserInfo_RelationshipStatus.SelectedIndex = (int)DisplayUser.RelationshipStatus;

            UserInfo_DateOfBirth.IsEnabled = ActiveUser == DisplayUser;
            UserInfo_DateOfBirth.Date = DisplayUser.DateOfBirth;

            UserInfo_School_Edit.Visibility = Visibility.Collapsed;
            UserInfo_School.Visibility = Visibility.Visible;
            UserInfo_School.Text = DisplayUser.School;

            UserInfo_University_Edit.Visibility = Visibility.Collapsed;
            UserInfo_University.Visibility = Visibility.Visible;
            UserInfo_University.Text = DisplayUser.University;

            UserInfo_Gender.IsEnabled = ActiveUser == DisplayUser;
            UserInfo_Gender.SelectedIndex = (int)DisplayUser.PersonGender;
        }

        public void UpdateFriends()
        {
            ActiveUserSubscriptions = new ObservableCollection<User>(ActiveUser.Subscriptions);
            FriendsList.ItemsSource = ActiveUserSubscriptions;
            FriendsList.UpdateLayout();
        }

        public void UpdateCollections()
        {
            switch (displayMode)
            {
                case DisplayMode.ActiveUserFeed:
                {
                    NewsView.Visibility = Visibility.Visible;
                    UserInfo.Visibility = Visibility.Collapsed;

                    NewsFeedLabel.Content = "News Feed";
                    AllPosts = new ObservableCollection<NewsItem>(ActiveUser.News.Feed);
                }
                break;

                case DisplayMode.ActiveUserPosts:
                {
                    NewsView.Visibility = Visibility.Visible;
                    UserInfo.Visibility = Visibility.Collapsed;

                    NewsFeedLabel.Content = "Your Posts";
                    AllPosts = new ObservableCollection<NewsItem>(ActiveUser.News.UserPosts);
                }
                break;

                case DisplayMode.ActiveUserInfo:
                {
                    ReadInfo();
                    NewsView.Visibility = Visibility.Collapsed;
                    UserInfo.Visibility = Visibility.Visible;

                    NewsFeedLabel.Content = "Your Info";

                }
                break;

                case DisplayMode.SubscriptionPosts:
                {
                    NewsView.Visibility = Visibility.Visible;
                    UserInfo.Visibility = Visibility.Collapsed;

                    NewsFeedLabel.Content = DisplayUser.FullName + "'s Posts";
                    AllPosts = new ObservableCollection<NewsItem>(DisplayUser.News.UserPosts);
                }
                break;

                case DisplayMode.UserInfo:
                {
                    ReadInfo();
                    NewsView.Visibility = Visibility.Collapsed;
                    UserInfo.Visibility = Visibility.Visible;

                    NewsFeedLabel.Content = DisplayUser.FullName + "'s Info";
                }
                    break;
            }

            UserName.Text = ActiveUser.FullName;

            NewsFeed.ItemsSource = AllPosts;
            NewsFeed.UpdateLayout();
        }
        
        private void AddFriendButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayUser = ActiveUser;
            //TODO: new friends picker

            User r;

            do
            {
                r = AllUsers[Generate.Int(AllUsers.Count)];
            } while (ActiveUserSubscriptions.Contains(r) || ActiveUser == r);

            ActiveUser.Subscribe(r);

            UpdateCollections();
            UpdateFriends();
        }
        private async void NewUser_Click(object sender, RoutedEventArgs e)
        {
            AllUsers.Add(new User(await Person.MakeNew()));

            ChooseUser.SelectedIndex = AllUsers.Count;
        }
        private void ChooseUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActiveUser = e.AddedItems[0] as User;
            UserName.Text = ActiveUser.ToString();

            displayMode = DisplayMode.ActiveUserInfo;
            DisplayUser = ActiveUser;

            UpdateCollections();
            UpdateFriends();
            ReadInfo();
        }

        private void FriendItemName_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            switch (displayMode)
            {
                case DisplayMode.ActiveUserFeed:
                case DisplayMode.ActiveUserPosts:
                    displayMode = DisplayMode.UserInfo;
                    break;
                case DisplayMode.ActiveUserInfo:
                    displayMode = DisplayMode.UserInfo;
                    break;
            }
            DisplayUser = ActiveUserSubscriptions[FriendsList.SelectedIndex];
            
            UpdateCollections();
            ReadInfo();
        }
        private async void FriendItemName__Tapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            DisplayUser = ActiveUser;
            var dialog = new MessageDialog($"Do you really wish to remove {ActiveUserSubscriptions[FriendsList.SelectedIndex]} from your friends?", "Are you sure");
            dialog.Commands.Add(new UICommand("Yes"));
            dialog.Commands.Add(new UICommand("No"));

            if ((await dialog.ShowAsync()).Label == "No")
                return;
            DisplayUser.UnSubscribe(ActiveUserSubscriptions[FriendsList.SelectedIndex]);

            UpdateCollections();
            UpdateFriends();
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
            switch (displayMode)
            {
                case DisplayMode.ActiveUserFeed:
                    displayMode = DisplayMode.ActiveUserPosts;
                    break;

                case DisplayMode.ActiveUserPosts:
                    displayMode = DisplayMode.ActiveUserInfo;
                    UserInfo_DateOfBirth.IsEnabled = true;
                    UserInfo_RelationshipStatus.IsEnabled = true;
                    break;

                case DisplayMode.ActiveUserInfo:
                    displayMode = DisplayMode.ActiveUserFeed;
                    break;

                case DisplayMode.SubscriptionPosts:
                    displayMode = DisplayMode.UserInfo;
                    UserInfo_DateOfBirth.IsEnabled = false;
                    UserInfo_RelationshipStatus.IsEnabled = false;
                break;

                case DisplayMode.UserInfo:
                    displayMode = DisplayMode.SubscriptionPosts;
                break;
            }
            
            UpdateCollections();
        }

        private void ChangeField(TextBlock f1, TextBox f2)
        {
            if(DisplayUser != ActiveUser)
                return;

            f1.Visibility = Visibility.Collapsed;
            f2.Text = f1.Text;
            f2.Visibility = Visibility.Visible;

            f2.Focus(FocusState.Pointer);
            f2.SelectionStart = f2.Text.Length;
            f2.SelectionLength = 0;
        }
        private void ChangeField(TextBlock f1, TextBox f2, KeyRoutedEventArgs e, bool override1 = false)
        {
            if (override1 || e?.Key == VirtualKey.Escape || e?.Key == VirtualKey.Enter)
            {
                f1.Text = f2.Text.Replace('\n', ' ').Replace('\t', ' ');

                f1.Visibility = Visibility.Visible;
                f2.Visibility = Visibility.Collapsed;

                f2.Text = f1.Text;
            }
        }

        private void UserInfo_First_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ChangeField(UserInfo_First, UserInfo_First_Edit);
        }
        private void UserInfo_Last_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ChangeField(UserInfo_Last, UserInfo_Last_Edit);
        }
        private void UserInfo_School_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ChangeField(UserInfo_School, UserInfo_School_Edit);
        }
        private void UserInfo_University_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ChangeField(UserInfo_University, UserInfo_University_Edit);
        }

        private void UserInfo_First_Edit_OnKeyUp(object sender, RoutedEventArgs e)
        {
            ChangeField(UserInfo_First, UserInfo_First_Edit, e as KeyRoutedEventArgs, !(e is KeyRoutedEventArgs));
            DisplayUser.First = UserInfo_First.Text;
        }
        private void UserInfo_Last_Edit_OnKeyUp(object sender, RoutedEventArgs e)
        {
            ChangeField(UserInfo_Last, UserInfo_Last_Edit, e as KeyRoutedEventArgs, !(e is KeyRoutedEventArgs));
            DisplayUser.Last = UserInfo_Last.Text;
        }
        private void UserInfo_School_Edit_OnKeyUp(object sender, RoutedEventArgs e)
        {
            ChangeField(UserInfo_School, UserInfo_School_Edit, e as KeyRoutedEventArgs, !(e is KeyRoutedEventArgs));
            DisplayUser.School = UserInfo_School.Text;
        }
        private void UserInfo_University_Edit_OnKeyUp(object sender, RoutedEventArgs e)
        {
            ChangeField(UserInfo_University, UserInfo_University_Edit, e as KeyRoutedEventArgs, !(e is KeyRoutedEventArgs));
            DisplayUser.University = UserInfo_University.Text;
        }

        private void UserInfo_RelationshipStatus_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ActiveUser == DisplayUser)
                DisplayUser.RelationshipStatus = (eRelationshipStatus) UserInfo_RelationshipStatus.SelectedIndex;
        }
        private void UserInfo_Gender_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ActiveUser == DisplayUser)
                DisplayUser.PersonGender = (eGender)UserInfo_Gender.SelectedIndex;
        }
        private void UserInfo_DateOfBirth_OnDateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            if (ActiveUser == DisplayUser)
                DisplayUser.DateOfBirth = UserInfo_DateOfBirth.Date.UtcDateTime;
        }

        private void NewsFeedLabel_OnDoubleTapped(object sender, RoutedEventArgs e)
        {
            DisplayUser = ActiveUser;
            displayMode = DisplayMode.ActiveUserInfo;

            UpdateCollections();
            ReadInfo();
        }

    }
}