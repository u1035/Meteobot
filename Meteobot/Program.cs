using System;
using S22.Xmpp.Client;
using System.Xml;

namespace Meteobot
{
    class Program
    {

        const string XMPP_HOSTNAME = "xmppserver.com";     //Bot's XMPP server
        const string XMPP_USERNAME = "meteobot";     //Bot's XMPP username
        const string XMPP_PASSWORD = "bot's_password";     //Bot's XMPP password
        const string WEATHER_URL = "https://xml.meteoservice.ru/export/gismeteo/point/116.xml";     //Link for weather info for my city

        static XmppClient client;
        static void Main(string[] args)
        {
            client = new XmppClient(XMPP_HOSTNAME, XMPP_USERNAME, XMPP_PASSWORD);

            using (client)
            {
                client.Message += OnNewMessage;
                client.Connect();

                Console.WriteLine("Type \"quit\" to exit");

                while (true)
                {
                    string s = Console.ReadLine();

                    if (s == "quit") return;
                }
            }
        }

        static void OnNewMessage(object sender, S22.Xmpp.Im.MessageEventArgs e)
        {
            Console.WriteLine(DateTime.Now.ToString() + ": Message from <" + e.Jid + ">: " + e.Message.Body);
            string answer = "";

            switch (e.Message.Body)
            {

                default:
                    answer = ParseWeatherXML(WEATHER_URL);
                    break;
            }
            client.SendMessage(e.Jid, answer);
        }

        private static string ParseWeatherXML(string URL)
        {
            string answer = "";

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(URL);
                XmlNode town = doc.DocumentElement.SelectSingleNode("/MMWEATHER/REPORT/TOWN");
                foreach (XmlNode forecast in town.ChildNodes)
                {
                    string date = forecast.Attributes["day"].InnerText + ".";
                    date += forecast.Attributes["month"].InnerText + ".";
                    date += forecast.Attributes["year"].InnerText + " ";
                    date += forecast.Attributes["hour"].InnerText + ":00 ";
                    date += GetWeekDay(forecast.Attributes["weekday"].InnerText);
                    answer += date + "\r\n"; ;

                    foreach (XmlNode detail in forecast.ChildNodes)
                    {
                        if (detail.Name == "PHENOMENA")
                        {
                            answer += GetPhenomena(detail.Attributes["cloudiness"].InnerText, detail.Attributes["precipitation"].InnerText, detail.Attributes["rpower"].InnerText, detail.Attributes["spower"].InnerText) + "\r\n";
                        }
                        if (detail.Name == "TEMPERATURE")
                        {
                            string temp = "Температура воздуха: " + detail.Attributes["min"].InnerText + " - " + detail.Attributes["max"].InnerText + " C°";
                            answer += temp + "\r\n";
                        }
                        if (detail.Name == "WIND")
                        {
                            string temp = "Ветер: " + detail.Attributes["min"].InnerText + " - " + detail.Attributes["max"].InnerText + " м/с";
                            answer += temp + "\r\n";
                        }
                        if (detail.Name == "RELWET")
                        {
                            string temp = "Влажность: " + detail.Attributes["min"].InnerText + " - " + detail.Attributes["max"].InnerText + " %";
                            answer += temp + "\r\n";
                        }
                    }

                    answer += "\r\n";
                }
                answer += "\r\nПо данным meteoservice.ru";
            }
            catch (Exception ex)
            {
                return "Что-то пошло не так...\r\nДанные об ошибке:\r\n" + ex.Message;
            }
            return answer;
        }

        private static string GetPhenomena(string cloudiness, string precipitation, string rpower, string spower)
        {
            string result = "Атмосферные явления: ";

            if (cloudiness == "-1") result += " туман";
            if (cloudiness == "0") result += " ясно";
            if (cloudiness == "1") result += " малооблачно";
            if (cloudiness == "2") result += " облачно";
            if (cloudiness == "3") result += " пасмурно";


            if ((precipitation == "3") && ((rpower == "0"))) result += ", возможны смешанные осадки";
            if ((precipitation == "3") && ((rpower == "1"))) result += ", смешанные осадки";
            if ((precipitation == "4") && ((rpower == "0"))) result += ", возможен дождь";
            if ((precipitation == "4") && ((rpower == "1"))) result += ", дождь";
            if ((precipitation == "5") && ((rpower == "0"))) result += ", возможен ливень";
            if ((precipitation == "5") && ((rpower == "1"))) result += ", ливень";
            if ((precipitation == "6") && ((rpower == "0"))) result += ", возможен снег";
            if ((precipitation == "6") && ((rpower == "1"))) result += ", снег";
            if ((precipitation == "7") && ((rpower == "0"))) result += ", возможен снег";
            if ((precipitation == "7") && ((rpower == "1"))) result += ", снег";
            if ((precipitation == "8") && ((spower == "0"))) result += ", возможна гроза";
            if ((precipitation == "8") && ((spower == "1"))) result += ", гроза";
            if (precipitation == "9") result += ", нет данных";
            if ((precipitation == "10") && ((rpower == "0"))) result += ", без осадков, возможна гроза";
            if ((precipitation == "10") && ((rpower == "1"))) result += ", без осадков, гроза";

            if ((precipitation != "8") && (spower == "1")) result += ", гроза";

            return result;
        }

        private static string GetWeekDay(string day)
        {
            switch (day)
            {
                case "2":
                    return " (Понедельник)";
                case "3":
                    return " (Вторник)";
                case "4":
                    return " (Среда)";
                case "5":
                    return " (Четверг)";
                case "6":
                    return " (Пятница)";
                case "7":
                    return " (Суббота)";
                case "1":
                    return " (Воскресенье)";
            }
            return "";
        }
    }
}
