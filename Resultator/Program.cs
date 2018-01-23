using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSVMonipulator;
namespace Resultator
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"D:\ex.csv";
            do
            {
                Console.Write("Введите путь до файла: ");
                path = Console.ReadLine();
            } while (!System.IO.File.Exists(path));

            CSVReader reader = null;
            try
            {
                reader = new CSVFile(path).ExecuteReader();
            }
            catch(Exception ex)
            {
                Console.WriteLine("\aОшибка! " + ex.Message);
                Console.ReadKey();
                return;
            }

            while (reader.NextLine())
            {
                //1 вариант
                foreach (string item in reader.ReadToEnd())
                {
                    Console.Write(item + "\t");
                }

                //2 вариант
                //while (reader.Read())
                //{
                //    Console.Write(reader.CurrentValue + "\t");
                //}
                Console.WriteLine();
            }
            Console.ReadKey();
        }
    }
}
