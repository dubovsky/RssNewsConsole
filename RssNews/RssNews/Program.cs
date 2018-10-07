using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace RssNews
{

    class Program
    {
        //Программа
        #region Program
        static void Main(string[] args)
        {
            Model1 context = new Model1();
            int ReadCount = 0; //Количество прочитанных новостей интерфакс
            int SaveCount = 0; //Количество сохраненных новостей интерфакс
            int ReadCount1 = 0; //Количество прочитанных новостей хабрахабр
            int SaveCount1 = 0; //Количество сохраненных новостей хабрахабр
            int i; //для switch
            int rez1, rez2;

            //Получаем ссылки на источники из базы
            var item1 = context.RSS_sources.Find(1);
            string HabrUrl = item1.URL;
            var item2 = context.RSS_sources.Find(2);
            string InterfaxUrl = item2.URL;

            //Файл на ресурсе интерфакс записан с пустым символом в начале, удаляем символ из файла, потом читаем файл
            #region interfax 
            WebRequest request = WebRequest.Create(InterfaxUrl);
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            string xmlText, FilePath = "C://Windows//Temp//rss.xml";
            using (StreamReader reader = new StreamReader(stream))
            {
                xmlText = reader.ReadToEnd();
            }
            using (StreamWriter writer = new StreamWriter(FilePath))
            {
                writer.Write(xmlText.Remove(0, 1));
            }
            #endregion

            //Список для Хабрахабр
            List<News> habr = new List<News>();
            //Список для Интерфакс
            List<News> ifax = new List<News>();

            //Меню программы
            while (true)
            {
                Console.WriteLine("____________________________________________\n" +
                    "\t\tМеню программы\n____________________________________________\n" +
                    "1. Прочитать новости из всех RSS-источников\n" +
                    "2. Сохранить в БД только свежие новости\n" +
                    "3. Вывод информации по источнику\n" +
                    "4. Выйти из программы" +
                    "\n____________________________________________\n");
                try
                {
                    i = int.Parse(Console.ReadLine());
                    switch (i)
                    {
                        case 1:
                            Console.Clear();
                            ReadRssNews(HabrUrl, 1, ref habr, ref ReadCount);
                            ReadRssNews(FilePath, 2, ref ifax, ref ReadCount1);
                            Console.WriteLine("\nНовости из RSS источников успешно прочитаны...\n");
                            break;
                        case 2:
                            Console.Clear();
                            rez1 = SaveRssNews(context, habr, out SaveCount);
                            rez2 = SaveRssNews(context, ifax, out SaveCount1);
                            if (rez1 == -1 && rez2 == -1)
                            {
                                Console.WriteLine("\n\tНеобходимо сначала прочитать новости!\n");
                            }
                            else if (SaveCount == 0 && SaveCount1 == 0)
                            {
                                Console.WriteLine("\nСвежих новостей нет.\n");

                            }
                            else
                            {
                                Console.WriteLine("\nСвежие новости из RSS источников успешно сохранены\n\t\t в базе данных...\n");
                            }
                            break;
                        case 3:
                            Console.Clear();
                            Console.WriteLine("\nНовости из источника Хабрахабр\n");
                            Console.WriteLine("Прочитано: {0}\tСохранено: {1}\n" +
                                "____________________________________________\n", ReadCount, SaveCount);
                            Console.WriteLine("Новости из источника Интерфакс\n");
                            Console.WriteLine("Прочитано: {0}\tСохранено: {1}\n", ReadCount1, SaveCount1);
                            break;
                        case 4:
                            Console.WriteLine("Завершаем работу программы...\n");
                            return;
                        default:
                            Console.WriteLine("Вы выбрали неверный пункт меню!\nПопробуйте снова.\n");
                            break;
                    }
                }
                catch (FormatException)
                {
                    Console.Clear();

                }
            }
        }
        #endregion

        //Метод чтения Rss новостей из всех источников
        #region ReadRss
        private static void ReadRssNews(string url, int k, ref List<News> lst, ref int c)
        {

            lst.Clear(); //Очищаем список, чтобы не суммировались записи при повторнов вызове 1 пункта менюю
            SyndicationFeed feed = new SyndicationFeed();
            try
            {
                using (XmlReader reader = XmlReader.Create(url)) //Создаем экземпляр
                {
                    feed = SyndicationFeed.Load(reader); //загружаем rss feed
                }
            }
            catch (Exception ex)
            {

                if (ex is WebException)
                {
                    Console.WriteLine("\tОшибка подключения к веб ресурсу");
                }
                else if (ex is XmlException)
                {
                    Console.WriteLine("\tНе удалось прочитать файл источника");
                }
                else
                {
                    Console.WriteLine("Непредвиденная ошибка");
                    throw;
                }
            }

            foreach (SyndicationItem item in feed.Items)
            {
                string LinkUrl = null;
                string title = item.Title.Text;
                string description = item.Summary.Text;
                // Убираем не нужные теги из содержания
                description = Regex.Replace(description, @"<.+?>", String.Empty);
                description = Regex.Replace(description, @"Читать.дальше..", String.Empty);
                // Декодируем HTML сущности
                description = WebUtility.HtmlDecode(description);
                DateTime date = item.PublishDate.UtcDateTime;

                foreach (var link in item.Links)
                {
                    LinkUrl = link.Uri.ToString();
                }
                //Создаем экземпляр rss новости
                News i = new News
                {
                    SourceId = k,
                    Description = description,
                    PubDate = date,
                    URL = LinkUrl,
                    Title = title
                };
                lst.Add(i); //Добавляем в список
            }
            int count = lst.Count; //Количество прочитанных новостей
            c = count;  //выходной параметр
        }
        #endregion

        //Метод сохранения свежих новостей в базе
        #region SaveRss
        private static int SaveRssNews(Model1 context, List<News> l, out int result)
        {
            result = 0;
            if (l.Count == 0)
            {

                return -1;
            }
            else
            {
                foreach (News item in l) //Поиск в бд соответсвующих записей по первичным ключам заголовка и даты
                {
                    //DateTime dt=new DateTime(item.PubDate.)
                    News element = context.NewsItems.Find(item.PubDate, item.SourceId, item.Title);
                    if (element is null)
                    {
                        context.NewsItems.Add(item); //Если записей в бд не найдено, то добавляем новую запись в бд
                        result++;
                    }
                }
                context.SaveChanges();
                return 0;
            }
        }
        #endregion

    }

}
