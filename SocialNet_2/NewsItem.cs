using System;

using Windows.Graphics.Imaging;
using Windows.UI.Text;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media.Imaging;

namespace SocialNet
{
    public class NewsItem: IComparable<NewsItem>
    {
        public User publisher { get; set; }
        public DateTime publishTime { get; set; } = DateTime.UtcNow;
        public string Content { get; set; }
        public BitmapImage Image { get; set; }

        public NewsItem() { }
        public NewsItem(User publisher)
        {
            this.publisher = publisher;
        }
        public NewsItem(User publisher, string Content, BitmapImage Image = null):
            this(publisher, Image)
        {
            this.Content = Content;
        }
        public NewsItem(User publisher, BitmapImage Image) : this(publisher)
        {
            this.Image = Image;
        }

        public int CompareTo(NewsItem o) => -publishTime.CompareTo(o.publishTime);
    }
}