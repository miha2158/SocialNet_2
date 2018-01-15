using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

namespace SocialNet
{
    public class User: Person
    {
        public string FullName => ToString();

        public List<User> Subscriptions = new List<User>(0);
        
        public NewsFeed News = new NewsFeed();

        public DateTime BirthdayNoYear => (new DateTime(DateOfBirth.Ticks, DateTimeKind.Utc)).AddYears(DateTime.Today.Year - DateOfBirth.Year);

        private int WaitTime = 8;

        public User()
        {
            Activate();
        }

        public User(BitmapImage Image, eGender PersonGender, string First, string Last, DateTime DateOfBitrh,
            eRelationshipStatus RelationshipStatus, string School = null, string University = null):
            this(PersonGender, First, Last, DateOfBitrh, RelationshipStatus, School, University)
        {
            Activate();
            News.UserPosts.Add(new NewsItem(this, Image));
        }
        public User(eGender PersonGender, string First, string Last, DateTime DateOfBitrh,
            eRelationshipStatus RelationshipStatus, string School = null, string University = null):
            base(PersonGender, First, Last, DateOfBitrh, RelationshipStatus, School, University)
        {
            Activate();
        }

        public User(Person Base): this(Base.PersonGender, Base.First, Base.Last,
            Base.DateOfBirth, Base.RelationshipStatus, Base.School, Base.University)
        {
            Activate();
        }
        public User(BitmapImage Image, Person Base): this(Image, Base.PersonGender, Base.First, Base.Last,
            Base.DateOfBirth, Base.RelationshipStatus, Base.School, Base.University)
        {
            Activate();
        }

        public void Activate()
        {
            News.AddNewItem += AddNewPost;
            News.RemoveNewsItem += RemovePost;
            RepeatBD();
        }

        public async void Subscribe(User Target)
        {
            if(Target == this || Subscriptions.Contains(Target))
                return;

            Subscriptions.Add(Target);
            Target.Subscriptions.Add(this);

            AddPost(new NewsItem(this, $"{this} and {Target} are friends now"));

            Target.AddPost(new NewsItem(Target, $"{this} and {Target} are friends now"));

            News.AddNewItem += Target.AddNewPost;
            News.RemoveNewsItem += Target.RemovePost;

            Target.News.AddNewItem += AddNewPost;
            Target.News.RemoveNewsItem += RemovePost;

            await Task.Delay(WaitTime);
        }
        public async void UnSubscribe(User Target)
        {
            if(!Subscriptions.Contains(Target))
                return;

            Subscriptions.Remove(Target);
            Target.Subscriptions.Remove(this);

            News.AddNewItem -= Target.AddNewPost;
            Target.News.AddNewItem -= AddNewPost;

            AddPost(new NewsItem(this, $"{this} and {Target} are no longer friends"));
            Target.AddPost(new NewsItem(Target, $"{this} and {Target} are no longer friends"));

            await Task.Delay(WaitTime);
        }

        public void AddNewPost(object sender, NewsItem item)
        {
            News.Feed.Add(item);
        }
        public void RemovePost(object sender, NewsItem item)
        {
            News.Feed.Remove(item);
        }

        public void AddPost(NewsItem Post)
        {
            News.UserPosts.Add(Post);
            News.InvokeAdd(this, Post);
        }
        public void RemovePost(NewsItem Post)
        {
            News.UserPosts.Remove(Post);
            News.InvokeRemove(this, Post);
        }

        DateTime prev = DateTime.UtcNow;

        private async void RepeatBD()
        {
            var t = new Timer(
                async delegate
                {
                    if (prev < BirthdayNoYear && false)
                    {
                        AddPost(new NewsItem(this, $"{this}'s Birthday is Tomorrow"));
                        await Task.Delay(WaitTime);
                        News.UserPosts.Remove(News.UserPosts.Last());
                    }
                },
                null,
                0,
                10000);
            prev = DateTime.UtcNow;
        }

        public new static async Task<User> MakeNew() => await MakeNew((eGender)Generate.Int(2));
        public new static async Task<User> MakeNew(eGender PersonGender)
        {
            var p = new User();
            p = await p.FillBlanks(PersonGender) as User;
            return p;
        }

        public static void Serialize(User user)
        {
            var s = new XmlSerializer(typeof (User));
            using (var fs = new FileStream($"{user.First}{user.Last}.persondata", FileMode.Create))
                s.Serialize(fs, user);
        }
        public static User Deserialize(string filename)
        {
            var s = new XmlSerializer(typeof (User));
            User NewUser;
            using (var fs = new FileStream(filename, FileMode.Create))
                NewUser = (User)s.Deserialize(fs);
            return NewUser;
        }
    }
}