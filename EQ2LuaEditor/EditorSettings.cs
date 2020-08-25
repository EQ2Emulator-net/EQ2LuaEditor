using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows.Forms;

namespace EQ2LuaEditor
{
    public class EditorSettings
    {
        private string m_scriptfolder;
        private bool m_showLineNumbers;
        private bool m_showAutoComplete;
        private bool m_enableAutoFormat;
        private string m_authorName;
        private string m_commentColor;
        private string m_numberColor;
        private string m_stringColor;
        private string m_textColor;
        private string m_keywords0Color;
        private string m_keywords1Color;
        private string m_keywords2Color;

        private string m_commentBackColor;
        private string m_numberBackColor;
        private string m_stringBackColor;
        private string m_textBackColor;
        private string m_keywords0BackColor;
        private string m_keywords1BackColor;
        private string m_keywords2BackColor;

        public EditorSettings()
        {
            m_enableAutoFormat = true;
            m_showAutoComplete = true;
            m_showLineNumbers = true;
            m_scriptfolder = "NULL";
            m_authorName = "NULL";

            // Colors
            m_commentColor = "Green";
            m_stringColor = "Magenta";
            m_textColor = "Black";
            m_numberColor = "Red";
            m_keywords0Color = "Blue";
            m_keywords1Color = "Teal";
            m_keywords2Color = "Teal";

            m_commentBackColor = "Transparent";
            m_stringBackColor = "Transparent";
            m_textBackColor = "Transparent";
            m_numberBackColor = "Transparent";
            m_keywords0BackColor = "Transparent";
            m_keywords1BackColor = "Transparent";
            m_keywords2BackColor = "Transparent";
        }

        public bool EnableAutoFormat
        {
            get { return m_enableAutoFormat; }
            set { m_enableAutoFormat = value; }
        }

        public bool ShowAutoComplete
        {
            get { return m_showAutoComplete; }
            set { m_showAutoComplete = value; }
        }

        public bool ShowLineNumbers
        {
            get { return m_showLineNumbers; }
            set { m_showLineNumbers = value; }
        }

        public string ScriptFolder
        {
            get { return m_scriptfolder; }
            set { m_scriptfolder = value; }
        }

        public string AuthorName
        {
            get { return m_authorName; }
            set { m_authorName = value; }
        }

        public string CommentColor
        {
            get { return m_commentColor; }
            set { m_commentColor = value; }
        }

        public string StringColor
        {
            get { return m_stringColor; }
            set { m_stringColor = value; }
        }

        public string TextColor
        {
            get { return m_textColor; }
            set { m_textColor = value; }
        }

        public string NumberColor
        {
            get { return m_numberColor; }
            set { m_numberColor = value; }
        }

        public string Keyword0Color
        {
            get { return m_keywords0Color; }
            set { m_keywords0Color = value; }
        }

        public string Keyword1Color
        {
            get { return m_keywords1Color; }
            set { m_keywords1Color = value; }
        }

        public string Keyword2Color
        {
            get { return m_keywords2Color; }
            set { m_keywords2Color = value; }
        }

        public string CommentBackColor
        {
            get { return m_commentBackColor; }
            set { m_commentBackColor = value; }
        }

        public string StringBackColor
        {
            get { return m_stringBackColor; }
            set { m_stringBackColor = value; }
        }

        public string TextBackColor
        {
            get { return m_textBackColor; }
            set { m_textBackColor = value; }
        }

        public string NumberBackColor
        {
            get { return m_numberBackColor; }
            set { m_numberBackColor = value; }
        }

        public string Keyword0BackColor
        {
            get { return m_keywords0BackColor; }
            set { m_keywords0BackColor = value; }
        }

        public string Keyword1BackColor
        {
            get { return m_keywords1BackColor; }
            set { m_keywords1BackColor = value; }
        }

        public string Keyword2BackColor
        {
            get { return m_keywords2BackColor; }
            set { m_keywords2BackColor = value; }
        }

        public void Save()
        {
            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
                                            new XElement("EQ2EditorSettings",
                                                new XElement("EnableAutoFormat", m_enableAutoFormat),
                                                new XElement("ShowAutoComplete", m_showAutoComplete),
                                                new XElement("ShowLineNumbers", m_showLineNumbers),
                                                new XElement("ScriptFolder", m_scriptfolder),
                                                new XElement("AuthorName", m_authorName),
                                                new XElement("CommentColor", m_commentColor),
                                                new XElement("StringColor", m_stringColor),
                                                new XElement("TextColor", m_textColor),
                                                new XElement("NumberColor", m_numberColor),
                                                new XElement("Keyword0Color", m_keywords0Color),
                                                new XElement("Keyword1Color", m_keywords1Color),
                                                new XElement("Keyword2Color", m_keywords2Color),
                                                new XElement("CommentBackColor", m_commentBackColor),
                                                new XElement("StringBackColor", m_stringBackColor),
                                                new XElement("TextBackColor", m_textBackColor),
                                                new XElement("NumberBackColor", m_numberBackColor),
                                                new XElement("Keyword0BackColor", m_keywords0BackColor),
                                                new XElement("Keyword1BackColor", m_keywords1BackColor),
                                                new XElement("Keyword2BackColor", m_keywords2BackColor)
                                                )
                                            );
            doc.Save(Application.StartupPath + "/settings.xml");
        }

        public void Load()
        {
            XDocument doc = XDocument.Load(Application.StartupPath + "/settings.xml");

            m_enableAutoFormat =
                (from item in doc.Descendants("EnableAutoFormat")
                 select (bool)item).FirstOrDefault();

            m_showAutoComplete =
                (from item in doc.Descendants("ShowAutoComplete")
                 select (bool)item).FirstOrDefault();

            m_showLineNumbers =
                (from item in doc.Descendants("ShowLineNumbers")
                 select (bool)item).FirstOrDefault();

            m_scriptfolder =
                (from item in doc.Descendants("ScriptFolder")
                 select (string)item).FirstOrDefault();

            m_authorName =
                (from item in doc.Descendants("AuthorName")
                 select (string)item).FirstOrDefault();

            // Colors
            m_commentColor =
                (from item in doc.Descendants("CommentColor")
                 select (string)item).FirstOrDefault();

            m_stringColor =
                (from item in doc.Descendants("StringColor")
                 select (string)item).FirstOrDefault();

            m_textColor =
                (from item in doc.Descendants("TextColor")
                 select (string)item).FirstOrDefault();

            m_numberColor =
                (from item in doc.Descendants("NumberColor")
                 select (string)item).FirstOrDefault();

            m_keywords0Color =
                (from item in doc.Descendants("Keyword0Color")
                 select (string)item).FirstOrDefault();

            m_keywords1Color =
                (from item in doc.Descendants("Keyword1Color")
                 select (string)item).FirstOrDefault();

            m_keywords2Color =
                (from item in doc.Descendants("Keyword2Color")
                 select (string)item).FirstOrDefault();

            m_commentBackColor =
                (from item in doc.Descendants("CommentBackColor")
                 select (string)item).FirstOrDefault();

            m_stringBackColor =
                (from item in doc.Descendants("StringBackColor")
                 select (string)item).FirstOrDefault();

            m_textBackColor =
                (from item in doc.Descendants("TextBackColor")
                 select (string)item).FirstOrDefault();

            m_numberBackColor =
                (from item in doc.Descendants("NumberBackColor")
                 select (string)item).FirstOrDefault();

            m_keywords0BackColor =
                (from item in doc.Descendants("Keyword0BackColor")
                 select (string)item).FirstOrDefault();

            m_keywords1BackColor =
                (from item in doc.Descendants("Keyword1BackColor")
                 select (string)item).FirstOrDefault();

            m_keywords2BackColor =
                (from item in doc.Descendants("Keyword2BackColor")
                 select (string)item).FirstOrDefault();
        }
    }
}
