using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace CSVMonipulator
{
    /// <summary>
    /// Настройки монипуляций с файлом
    /// </summary>
    public class CSVSettings
    {
        //Путь до файла с настройками
        private const string path = "Settings.xml";
        //Сепаратор по умолчанию
        private const char defaultSeparator = ';';

        private XmlDocument doc = null;
        private XmlNode el = null;
        private void setValues()
        {
            XmlNode _pagePointer = doc.SelectSingleNode(string.Format("//module[@name='{0}']", "settings"));
            el = _pagePointer.SelectSingleNode(string.Format("resource[@id='{0}']", "separator"));
        }
        //Восстановление настроек
        private void SetDefaultSettings()
        {
            System.IO.FileStream stream = new System.IO.FileStream(path, System.IO.FileMode.Create);
            string data = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"
                          + "<resources>\n"
                          + "  <module name=\"settings\">\n"
                          + "    <resource id=\"separator\">;</resource>\n"
                          + "  </module>\n"
                          + "</resources>";
            byte[] byteArray = new byte[data.Length];
            int i = 0;
            foreach (char item in data)
                byteArray[i++] = (byte)item;
            stream.Write(byteArray, 0, byteArray.Length);
            stream.Close();
        }

        public CSVSettings()
        {
            Init();
        }
        //Подгрузить данные из файла настроек
        public void Init()
        {
            doc = new XmlDocument();
            try
            {
                doc.Load(path);
                setValues();
            }
            catch {
                SetDefaultSettings();
                doc.Load(path);
                setValues();
            }
        }
        //Разделитель
        public char Separator
        {
            get
            {
                try
                {
                    return el.InnerXml[0];
                }
                catch
                {
                    return defaultSeparator;
                }
            }
            set
            {
                el.InnerXml = "" + value;
                doc.Save(path);
            }
        }
    }

    //Считывальщик CSV файла
    public class CSVReader
    {
        //Данные CSV файла
        private string data = "";
        private CSVSettings settings = null;

        public CSVReader(string data, CSVSettings settings)
        {
            this.data = data;
            if (this.data[this.data.Length - 1] == '\n')
                this.data = this.data.Substring(0, this.data.Length - 1);
            this.settings = settings;
        }

        private int beginLine = 0;
        private string currentLine = null;
        private bool nextLineHasFalse = false;
        //Переводит на новую строку и возвращает false, если строк больше нет
        public bool NextLine()
        {
            nextRowHasFalse = false;
            beginRow = 0;
            if (nextLineHasFalse) return false;
            currentLine = data.Substring(beginLine, data.Length - beginLine);
            int beginIndex = currentLine.IndexOf('\n');
            if (beginIndex > -1)
            {
                currentLine = currentLine.Substring(0, beginIndex);
                beginLine += beginIndex + 1;
            }
            else nextLineHasFalse = true;
            return true;
        }

        private int beginRow = 0;
        private string currentRow = null;
        private bool nextRowHasFalse = false;
        //Считывает значение строки по сепаратору из настроек, если строка кончилась, возвращает false
        public bool Read()
        {
            if (nextRowHasFalse) return false;
            currentRow = currentLine.Substring(beginRow, currentLine.Length - beginRow);
            int beginIndex = currentRow.IndexOf(this.settings.Separator);
            if (beginIndex > -1)
            {
                currentRow = currentRow.Substring(0, beginIndex);
                beginRow += beginIndex + 1;
            }
            else nextRowHasFalse = true;
            return true;
        }
        //Считывает все элементы строки по сепаратору из настроек, и передает их в перечисление
        public IEnumerable<string> ReadToEnd()
        {
            beginRow = 0;
            nextRowHasFalse = false;
            while (this.Read())
            {
                yield return this.CurrentValue;
            }
            beginRow = 0;
            nextRowHasFalse = false;
        }
        //Возвращает значение текущей позиции
        public string CurrentValue { get { return currentRow.Trim(); } }

        //Заставляет считывальщик перейти в исходное положение
        public void Restart()
        {
            nextLineHasFalse = false;
            beginLine = 0;
        }
    }

    public class CSVFile
    {
        private string path = "";
        public CSVFile(string path)
        {
            this.path = path;
        }

        public CSVReader ExecuteReader()
        {
            string data = "";
            if (!System.IO.File.Exists(path))
            {
                throw new Exception("Файл \"" + path + "\" не найден.");
            }

            using (System.IO.StreamReader reader = new System.IO.StreamReader(path))
            {
                data = reader.ReadToEnd();
                reader.Close();
            }
            return new CSVReader(data, new CSVSettings());
        }
    }
}
