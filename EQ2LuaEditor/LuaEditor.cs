using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using ScintillaNET;

namespace EQ2LuaEditor
{
    public partial class LuaEditor : DockContent
    {
        private string m_savedString;
        private bool m_saveNeeded;
        private List<string> EmuKeywords = new List<string>();
        private List<string> AutoCompleteWords = new List<string>();
        private List<EQ2EmuKeywords> keywords;

        public LuaEditor()
        {
            InitializeComponent();

            // Set variables to know if the file needs to be saved or not
            m_savedString = "";
            m_saveNeeded = true;

            // Load up scintilla
            scintilla1.ConfigurationManager.CustomLocation = "lua.xml";
            scintilla1.ConfigurationManager.Language = "lua";

            // Set highlighting colors
            scintilla1.Styles[1].ForeColor = Color.FromName(EQ2LuaEditor.Settings.CommentColor);
            scintilla1.Styles[1].BackColor = Color.FromName(EQ2LuaEditor.Settings.CommentBackColor);
            scintilla1.Styles[4].ForeColor = Color.FromName(EQ2LuaEditor.Settings.NumberColor);
            scintilla1.Styles[4].BackColor = Color.FromName(EQ2LuaEditor.Settings.NumberBackColor);
            scintilla1.Styles[5].ForeColor = Color.FromName(EQ2LuaEditor.Settings.Keyword0Color);
            scintilla1.Styles[5].BackColor = Color.FromName(EQ2LuaEditor.Settings.Keyword0BackColor);
            scintilla1.Styles[6].ForeColor = Color.FromName(EQ2LuaEditor.Settings.StringColor);
            scintilla1.Styles[6].BackColor = Color.FromName(EQ2LuaEditor.Settings.StringBackColor);
            scintilla1.Styles[11].ForeColor = Color.FromName(EQ2LuaEditor.Settings.TextColor);
            scintilla1.Styles[11].BackColor = Color.FromName(EQ2LuaEditor.Settings.TextBackColor);
            // 12 is for a unterminated string, set it to match string
            scintilla1.Styles[12].ForeColor = Color.FromName(EQ2LuaEditor.Settings.StringColor);
            scintilla1.Styles[12].BackColor = Color.FromName(EQ2LuaEditor.Settings.StringBackColor);
            scintilla1.Styles[13].ForeColor = Color.FromName(EQ2LuaEditor.Settings.Keyword1Color);
            scintilla1.Styles[13].BackColor = Color.FromName(EQ2LuaEditor.Settings.Keyword1BackColor);
            scintilla1.Styles[14].ForeColor = Color.FromName(EQ2LuaEditor.Settings.Keyword2Color);
            scintilla1.Styles[14].BackColor = Color.FromName(EQ2LuaEditor.Settings.Keyword2BackColor);
            scintilla1.Indentation.SmartIndentType = ScintillaNET.SmartIndent.None;
            scintilla1.Indentation.IndentWidth = 4;


            // Load the keywords from the new xml file
            keywords =
                (
                    from item in XDocument.Load(Application.StartupPath + "/EQ2EmuLuaFunctions.xml").Root.Elements("Function")
                    select new EQ2EmuKeywords
                    {
                        Name = item.FirstAttribute.Value,
                        ReturnType = (string)item.Element("ReturnType"),
                        Description = (string)item.Element("Description"),
                        Parameters = 
                        (
                            from p in item.Elements("Param")
                            select new Parameter
                            {
                                Type = (string)p.Attribute("Type"),
                                Optional = ((string)p.Attribute("Optional") == null || ((string)p.Attribute("Optional")).ToLower() == "false") ? false : true,
                                Name = (string)p
                            }).ToArray()
                    }).ToList();


            // Put the EMU keywords into a list for auto complete
            string Keys = "";
            foreach (EQ2EmuKeywords k in keywords)
            {
                EmuKeywords.Add(k.Name);
                Keys += k.Name + " ";
            }
            scintilla1.Lexing.Keywords[2] = Keys;

            // Set up auto complete list
            string autocomp = scintilla1.Lexing.Keywords[0] + scintilla1.Lexing.Keywords[1] + scintilla1.Lexing.Keywords[2];
            AutoCompleteWords = autocomp.Split(' ').ToList();
            AutoCompleteWords.Sort();
            scintilla1.AutoComplete.List = AutoCompleteWords;

            // Will fix auto complete to replace what was already typed with what you selected in autocomplete
            scintilla1.AutoComplete.IsCaseSensitive = false;
            // Hide auto complete when there are no more matches
            scintilla1.AutoComplete.AutoHide = true;
            // Set mouse hover delay to 500 milliseconds (0.5 seconds)
            scintilla1.NativeInterface.SetMouseDwellTime(500);
        }

        public void SetSaved()
        {
            m_savedString = scintilla1.Text;
            if (Text.Contains("*"))
                Text = Text.Substring(0, Text.Length - 1);
            m_saveNeeded = false;
        }

        public bool SaveNeeded
        {
            get { return m_saveNeeded; }
        }

        private void scintilla1_CharAdded(object sender, ScintillaNET.CharAddedEventArgs e)
        {
            char newline = (scintilla1.EndOfLine.Mode == EndOfLineMode.CR) ? '\r' : '\n';

            if (e.Ch != '\r' && e.Ch != ' ' && e.Ch != ')' && e.Ch != '(' && e.Ch !=',' && EQ2LuaEditor.Settings.ShowAutoComplete)
            {
                // Get the current position of the carrot
                int pos = scintilla1.NativeInterface.GetCurrentPos();
                // Get the start of the current word based on carrot position and subtract from current position to get the length
                int length = pos - scintilla1.NativeInterface.WordStartPosition(pos, true);

                if (length > 2)
                    scintilla1.AutoComplete.Show(length);
            }

            #region Auto formating
            if (EQ2LuaEditor.Settings.EnableAutoFormat)
            {
                Line curLine = scintilla1.Lines.Current;
                Line prevLine = scintilla1.Lines.Current.Previous;
                int tabWidth = scintilla1.Indentation.TabWidth;

                if (e.Ch == newline)
                {
                    // Match previous line indentation
                    curLine.Indentation = curLine.Previous.Indentation;
                    scintilla1.CurrentPos = curLine.IndentPosition;

                    if (prevLine.Text.Trim().StartsWith("function") && !prevLine.Text.Contains("end"))
                    {
                        curLine.Indentation += tabWidth;
                        scintilla1.CurrentPos = scintilla1.Lines.Current.IndentPosition;
                    }
                    else if (prevLine.Text.Trim().StartsWith("if") && prevLine.Text.Contains("then") && !prevLine.Text.Contains("end"))
                    {
                        curLine.Indentation += tabWidth;
                        scintilla1.CurrentPos = curLine.IndentPosition;
                    }
                    else if (prevLine.Text.Trim().StartsWith("else") && !prevLine.Text.Contains("end"))
                    {
                        curLine.Indentation += tabWidth;
                        scintilla1.CurrentPos = curLine.IndentPosition;
                    }
                    else if (prevLine.Text.Trim().StartsWith("for") && prevLine.Text.Contains("do") && !prevLine.Text.Contains("end"))
                    {
                        curLine.Indentation += tabWidth;
                        scintilla1.CurrentPos = curLine.IndentPosition;
                    }
                    else if (prevLine.Text.Trim().StartsWith("while") && prevLine.Text.Contains("do") && !prevLine.Text.Contains("end"))
                    {
                        curLine.Indentation += tabWidth;
                        scintilla1.CurrentPos = curLine.IndentPosition;
                    }
                    else if (prevLine.Text.Trim().StartsWith("repeat") && !prevLine.Text.Contains("until"))
                    {
                        curLine.Indentation += tabWidth;
                        scintilla1.CurrentPos = curLine.IndentPosition;
                    }

                }
                // end - remove 1 indent space
                if (e.Ch == 'd')
                {
                    if (curLine.Text.Trim().StartsWith("end") && curLine.Text.Trim().Length == 3)
                    {
                        int match = SafeBraceMatch(scintilla1.CurrentPos - 1);
                        if (match != -1)
                            curLine.Indentation = scintilla1.Lines.FromPosition(match).Indentation;

                        /*curLine.Indentation -= tabWidth;
                        scintilla1.CurrentPos = curLine.EndPosition;*/
                    }
                }
                // else - remove 1 indent space
                if (e.Ch == 'e')
                {
                    if (curLine.Text.Trim().StartsWith("else") && curLine.Text.Trim().Length == 4)
                    {
                        int match = SafeBraceMatch(scintilla1.CurrentPos - 1);
                        if (match != -1)
                            curLine.Indentation = scintilla1.Lines.FromPosition(match).Indentation;


                        /*curLine.Indentation -= tabWidth;
                        scintilla1.CurrentPos = curLine.EndPosition;*/
                    }
                }
                // until
                if (e.Ch == 'l')
                {
                    if (curLine.Text.Trim().StartsWith("until") && curLine.Text.Trim().Length == 5)
                    {
                        int match = SafeBraceMatch(scintilla1.CurrentPos - 1);
                        if (match != -1)
                            curLine.Indentation = scintilla1.Lines.FromPosition(match).Indentation;
                    }
                }
            }
            #endregion
            
        }

        private void scintilla1_TextChanged(object sender, EventArgs e)
        {
            if (m_savedString == null || scintilla1.Text == null)
                return;

            if (m_savedString.CompareTo(scintilla1.Text) != 0)
            {
                if (!m_saveNeeded)
                {
                    Text += "*";
                    m_saveNeeded = true;
                }
            }
            else
            {
                if (m_saveNeeded && m_savedString != "")
                {
                    Text = Text.Substring(0, Text.Length - 1);
                    m_saveNeeded = false;
                }
            }
        }

        // Taken from ScintillaNet source and modified to work on lua
        internal int SafeBraceMatch(int position)
        {
            string match = scintilla1.GetWordFromPosition(position);
            //string toMatch;
            string toMatch2 = null;
            List<string> toMatch = new List<string>();
            int length = scintilla1.TextLength;
            string ch;
            int sub = 0;
            Lexer lexer = scintilla1.Lexing.Lexer;
            scintilla1.Lexing.Colorize(0, -1);
            bool comment = scintilla1.PositionIsOnComment(position, lexer);
            position = scintilla1.NativeInterface.WordStartPosition(position, true);
            switch (match)
            {
                case "end":
                    toMatch.Add("if");
                    toMatch.Add("function");
                    toMatch.Add("for");
                    toMatch.Add("while");
                    //toMatch = "if";
                    //toMatch2 = "function";
                    goto up;
                case "else":
                    toMatch.Add("if");
                    //toMatch = "if";
                    goto up;
                case "until":
                    toMatch.Add("repeat");
                    goto up;
                /*case '[':
                    toMatch = ']';
                    goto down;
                case '}':
                    toMatch = '{';
                    goto up;
                case ')':
                    toMatch = '(';
                    goto up;
                case ']':
                    toMatch = '[';
                    goto up;*/
            }
            return -1;
        // search up
        up:
            while (position >= 0)
            {
                position--;
                ch = scintilla1.GetWordFromPosition(position);
                if (ch == match)
                {
                    if (comment == scintilla1.PositionIsOnComment(position, lexer))
                        sub++;
                    
                    position = scintilla1.NativeInterface.WordStartPosition(position, true);
                }
                else /*if (ch == toMatch && comment == scintilla1.PositionIsOnComment(position, lexer))*/
                {
                    foreach (string s in toMatch)
                    {
                        if (ch == s && comment == scintilla1.PositionIsOnComment(position, lexer) && scintilla1.Lines.FromPosition(position).Text.Trim().StartsWith(ch))
                        {
                            sub--;
                            if (sub < 0)
                                return position;

                            position = scintilla1.NativeInterface.WordStartPosition(position, true);
                            break;
                        }
                    }
                    /*sub--;
                    if (sub < 0)
                        return position;

                    position = scintilla1.NativeInterface.WordStartPosition(position, true);*/
                }
                /*else if (toMatch2 != null && ch == toMatch2 && comment == scintilla1.PositionIsOnComment(position, lexer))
                {
                    sub--;
                    if (sub < 0)
                        return position;

                    position = scintilla1.NativeInterface.WordStartPosition(position, true);
                }*/
            }
            return -1;
        // search down
        /*down:
            while (position < length)
            {
                position++;
                ch = scintilla1.GetWordFromPosition(position);
                if (ch == match)
                {
                    if (comment == scintilla1.PositionIsOnComment(position, lexer))
                        sub++;

                    position = scintilla1.NativeInterface.WordEndPosition(position, true);
                }
                else if (ch == toMatch && comment == scintilla1.PositionIsOnComment(position, lexer))
                {
                    sub--;
                    if (sub < 0)
                        return position;

                    position = scintilla1.NativeInterface.WordEndPosition(position, true);
                }
            }
            return -1;*/
        }

        private void scintilla1_DwellStart(object sender, ScintillaMouseEventArgs e)
        {
            if (scintilla1.CallTip.IsActive)
                return;

            string word = scintilla1.GetWordFromPosition(e.Position);
            string callTipText = "";
            if (EmuKeywords.Contains(word))
            {
                foreach (EQ2EmuKeywords k in keywords)
                {
                    if (k.Name == word)
                    {
                        // Set the return type and name
                        callTipText = k.ReturnType + " " + k.Name + "(";

                        // loop through the parameters this function has
                        bool i_suck_with_names = false;
                        foreach (Parameter p in k.Parameters)
                        {
                            // if not the first loop add a comma and space
                            if (i_suck_with_names)
                                callTipText += ", ";

                            // if the parameter is optional add the [
                            if (p.Optional)
                                callTipText += "[";

                            // Add the parameter type and name
                            callTipText += p.Type + " " + p.Name;

                            // if optional add the closing ]
                            if (p.Optional)
                                callTipText += "]";

                            // set the flag to true so any other parameters have a comma
                            i_suck_with_names = true;
                        }

                        // Add the closing ) and a line break followed by the function description
                        callTipText += ")\n" + k.Description;
                        break;
                    }
                }
            }

            if (callTipText != "")
                scintilla1.CallTip.Show(callTipText, e.Position);
        }

        private void scintilla1_DwellEnd(object sender, ScintillaMouseEventArgs e)
        {
            // Hide the call tip
            scintilla1.CallTip.Hide();
        }
    }

    public class EQ2EmuKeywords
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public string Description { get; set; }
        public Parameter[] Parameters { get; set; }
    }

    public class Parameter
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public bool Optional { get; set; }
    }
}