using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.App_Start
{
    public class Kod
    {
        static Dictionary<int, char> dictionary;
        static public  bool close=false;
        static public int count1 = 0;
        static public int count2 = 0;
        static public int count3 = 0;
        static public int count4 = 0;
        
       

        public static string GetKode() {
            DateTime dt = new DateTime();
            dt = DateTime.Now;
            int year = 18;
            int.TryParse(dt.Year.ToString().Substring(2,2), out year);
            string kod = getSimvol(dt.Second) + getSimvol(dt.Minute) + getSimvol(dt.Hour) + getSimvol(dt.Day) + getSimvol(dt.Month) + getSimvol(year) + "" + getSimvol(count1) + getSimvol(count2) + getSimvol(count3) + getSimvol(count4);
            reCount();
            return kod;
        }

        public static void reCount() {
            if (count1 == 59) {
                count1 = 0;
                count2++;
            }
            if (count2 == 59)
            {
                count2 = 0;
                count3++;
            }
            if (count3 == 59)
            {
                count3 = 0;
                count4++;
            }
            if (count4 == 59)
            {
                count4 = 0;
               
            }
        }

        public static char getSimvol(int c) { 
            
            char x;

            if (dictionary.TryGetValue(c, out x)) {
                return x;
            }
            else{
                return '?';
            }

        }

        public static void createDictionary()
        {
            dictionary = new Dictionary<int, char>();
            dictionary.Add(0, 'Q');
            dictionary.Add(1, 'W');
            dictionary.Add(2, 'E');
            dictionary.Add(3, 'R');
            dictionary.Add(4, 'T');
            dictionary.Add(5, 'Y');
            dictionary.Add(6, 'U');
            dictionary.Add(7, 'I');
            dictionary.Add(8, 'O');
            dictionary.Add(9, 'P');
            dictionary.Add(10, 'A');
            dictionary.Add(11, 'S');
            dictionary.Add(12, 'D');
            dictionary.Add(13, 'F');
            dictionary.Add(14, 'G');
            dictionary.Add(15, 'H');
            dictionary.Add(16, 'J');
            dictionary.Add(17, 'K');
            dictionary.Add(18, 'L');
            dictionary.Add(19, 'Z');
            dictionary.Add(20, 'X');
            dictionary.Add(21, 'C');
            dictionary.Add(22, 'V');
            dictionary.Add(23, 'B');
            dictionary.Add(24, 'N');
            dictionary.Add(25, 'M');
            dictionary.Add(26, '1');
            dictionary.Add(27, '2');
            dictionary.Add(28, '3');
            dictionary.Add(29, '4');
            dictionary.Add(30, '5');
            dictionary.Add(31, '6');
            dictionary.Add(32, '7');
            dictionary.Add(33, '8');
            dictionary.Add(34, '9');
            dictionary.Add(35, '0');
            dictionary.Add(36, 'Г');
            dictionary.Add(37, 'Ш');
            dictionary.Add(38, 'Ф');
            dictionary.Add(39, 'П');
            dictionary.Add(40, 'Л');
            dictionary.Add(41, 'Д');
            dictionary.Add(42, 'Ж');
            dictionary.Add(43, 'Э');
            dictionary.Add(44, 'Я');
            dictionary.Add(45, 'Ч');
            dictionary.Add(46, 'И');
            dictionary.Add(47, 'Ь');
            dictionary.Add(48, 'Б');
            dictionary.Add(49, 'Ю');
            dictionary.Add(50, '^');
            dictionary.Add(51, '=');
            dictionary.Add(52, '+');
            dictionary.Add(53, '-');
            dictionary.Add(54, '%');
            dictionary.Add(55, '#');
            dictionary.Add(56, '!');
            dictionary.Add(57, '*');
            dictionary.Add(58, '<');
            dictionary.Add(59, '>');
           
        }
    }
}