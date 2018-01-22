
using System;
using Windows.UI.Xaml;

namespace SocialNet
{
    public class BirthdayCrawler
    {
        private static DispatcherTimer timer = new DispatcherTimer()
        {
            Interval = new TimeSpan(0, 0, 10)
        };

        private DateTime PreviousCheck;
        private readonly User user;


        public DateTime NotificationTime => (new DateTime(user.DateOfBirth.Ticks, DateTimeKind.Utc)).AddYears(DateTime.Today.Year - user.DateOfBirth.Year).Subtract(new TimeSpan(1,0,0,0));

        public BirthdayCrawler(User user)
        {
            PreviousCheck = DateTime.UtcNow;
            this.user = user;
            
            timer.Tick += Notify;
            timer.Start();
        }

        private void Notify(object sender, object o)
        {
            if (NotificationTime > PreviousCheck && DateTime.UtcNow > NotificationTime)
            {
                var post = new NewsItem(user, $"{user} has a birthday tomorrow");
                user.AddPost(post);

                user.News.UserPosts.Remove(post);
                user.News.Feed.Remove(post);
            }
            PreviousCheck = DateTime.UtcNow;
        }
    }
}