using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.App_Start
{
    public class Const
    {
        public static string server = "robots.1gb.ru";
        public static string login = "u485698";
        public static string password = "12b9e0a15klz";
        public static string from = "help@skrindisk.com";
        public static float mount = 1;
        public static string AdminLogin= "skrindisk@admin.panel";
        public static string PostMvd = "info@mvd.gov.ru";
        public static string PostAdmin = "skrindiskhelp@yandex.ru";
        public static string MessageAdmin = "В системе произведена проверка дисков с уникальным номером:";
        public static string MessageUser = "Оплата прошла успешно. Ваш уникальный код:";
        public static string MessageAdminACD = "В системе создан новый уникальный код";
        public static string MessageMVDAT = "Информируем о краже дисков с уникальным номером:";
        public static string MessageMVDAС = "Зафиксирована проверка краденных дисков с уникальным номером:";
        public static string Tema = "Проверка дисков";
        public static string TemaTheft = "Кража дисков";
        public static bool SendMVD = false;
        public static int Price1 = 2;
        public static int PriceVip = 3;
    }
}