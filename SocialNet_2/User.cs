using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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


        }

        public void AddNewPost(object sender, NewsItem item)
        {
            News.Feed.Add(item);
        }
        public void RemovePost(object sender, NewsItem item)
        {
            News.Feed.Remove(item);
        }

        public void Subscribe(User Target)
        {
            if(Target == this || Subscriptions.Contains(Target))
                return;

            Subscriptions.Add(Target);
            Target.Subscriptions.Add(this);

            AddPost(new NewsItem(this, $"{this} and {Target} are friends now"));
            Target.AddPost(new NewsItem(Target, $"{Target} and {this} are friends now"));

            News.AddNewItem += Target.AddNewPost;
            Target.News.AddNewItem += AddNewPost;

            News.RemoveNewsItem += Target.RemovePost;
            Target.News.RemoveNewsItem += RemovePost;

        }
        public void UnSubscribe(User Target)
        {
            if(!Subscriptions.Contains(Target))
                return;

            Subscriptions.Remove(Target);
            Target.Subscriptions.Remove(this);

            News.AddNewItem -= Target.AddNewPost;
            Target.News.AddNewItem -= AddNewPost;

            AddPost(new NewsItem(this, $"{this} and {Target} are no longer friends"));
            Target.AddPost(new NewsItem(Target, $"{Target} and {this} are no longer friends"));

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