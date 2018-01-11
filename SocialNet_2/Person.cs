using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace SocialNet
{
    public enum eGender
    {
        Male,
        Female
    }

    public enum eRelationshipStatus
    {
        Single, 
        Taken, 
        Married, 
        NotMarried
    }

    public class Person
    {
        public string First { get; protected set; }
        public string Last { get; protected set; }
        public eGender PersonGender { get; protected set; }
        public DateTime DateOfBirth { get; protected set; }

        public eRelationshipStatus RelationshipStatus { get; protected set; }
        public string School { get; protected set; }
        public string University { get; protected set; }

        public Person() { }
        public Person(eGender PersonGender, string First, string Last, DateTime DateOfBirth,
            eRelationshipStatus RelationshipStatus, string School = null, string University = null)
        {
            this.PersonGender = PersonGender;
            this.First = First;
            this.Last = Last;
            this.DateOfBirth = DateOfBirth;
            this.RelationshipStatus = RelationshipStatus;
            this.School = School;
            this.University = University;
        }

        static List<string> MaleFirstNames = new List<string>(0);
        static List<string> MaleLastNames = new List<string>(0);
        static List<string> FemaleFirstNames = new List<string>(0);
        static List<string> FemaleLastNames = new List<string>(0);

        public static async Task<IList<string>> ReadFile(string path) => await FileIO.ReadLinesAsync(await StorageFile.GetFileFromApplicationUriAsync(new Uri($@"ms-appx:///{path}")));

        public async Task<Person> FillBlanks(eGender PersonGender)
        {
            switch (PersonGender)
            {
                case eGender.Male:
                {
                    if (MaleFirstNames.Count == 0)
                        MaleFirstNames = (await ReadFile(@"=malefirstnames.txt")).ToList();

                    if (MaleLastNames.Count == 0)
                        MaleLastNames = (await ReadFile(@"=malelastnames.txt")).ToList();

                    First = MaleFirstNames[Generate.Int(MaleFirstNames.Count)];
                    Last = MaleLastNames[Generate.Int(MaleLastNames.Count)];
                    break;
                }

                case eGender.Female:
                {
                    if (FemaleFirstNames.Count == 0)
                        FemaleFirstNames = (await ReadFile(@"=femalefirstnames.txt")).ToList();
                    
                    if (FemaleLastNames.Count == 0)
                        FemaleLastNames = (await ReadFile(@"=femalelastnames.txt")).ToList();


                    First = FemaleFirstNames[Generate.Int(FemaleFirstNames.Count)];
                    Last = FemaleLastNames[Generate.Int(FemaleLastNames.Count)];
                    break;
                }
            }

            this.PersonGender = PersonGender;
            DateOfBirth = Generate.NewDateTime(new DateTime(1970, 1, 1), DateTime.UtcNow);
            RelationshipStatus = (eRelationshipStatus)Generate.Int(4);

            switch (Generate.Int(3))
            {
                case 0:
                    School = "Школа";
                    break;

                case 1:
                    School = "Лицей";
                    break;

                case 2:
                    School = "Гимназия";
                    break;
            }
            School += " №" + Generate.Int(38,147);

            switch (Generate.Int(6))
            {
                case 0:
                    University = "НИУ ВШЭ";
                    break;

                case 1:
                    University = "СПБГУ";
                    break;

                case 2:
                    University = "МГУ";
                    break;

                case 3:
                    University = "ИТМО";
                    break;

                case 4:
                    University = "МГИМО";
                    break;
                    
                case 5:
                    University = "МФТИ";
                    break;
            }

            return this;
        }

        public static async Task<Person> MakeNew() => await MakeNew((eGender)Generate.Int(2));
        public static async Task<Person> MakeNew(eGender PersonGender)
        {
            var p = new Person();
            p = await p.FillBlanks(PersonGender);
            return p;
        }

        public override string ToString()
        {
            return First + " " + Last;
        }
    }
}