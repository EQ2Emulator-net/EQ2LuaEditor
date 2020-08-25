using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.IO;
using System.Diagnostics; // to launch a browser
using System.Threading;

namespace EQ2LuaEditor
{
    public partial class EQ2LuaEditor : Form
    {
        private struct ErrorStats
        {
            public string File;
            public int Line;
        }

        SolutionExplorer m_solutionExplorer;
        static EditorSettings m_settings;
        static Errors m_errors;
        static Output m_output;
        List<ErrorStats> m_errorList = new List<ErrorStats>();

        private int m_index = 0;

        // Needed to change scroll position of the tree view
        private const int SB_HORZ = 0;
        private const int SB_VERT = 1;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetScrollPos(IntPtr hWnd, int nBar);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetScrollPos(IntPtr hWnd, int nBar, int pos, bool redraw);

        public EQ2LuaEditor(string[] args)
        {
            InitializeComponent();

            m_solutionExplorer = new SolutionExplorer();
            // Default the solution explorer to the right side
            m_solutionExplorer.ShowHint = DockState.DockRight;
            m_solutionExplorer.tvExplorer.NodeMouseDoubleClick += tvExplorer_NodeMouseDoubleClick;
            m_solutionExplorer.Show(dockPanel);

            m_output = new Output();
            m_output.ShowHint = DockState.DockBottomAutoHide;
            m_output.Show(dockPanel);

            m_errors = new Errors();
            m_errors.ShowHint = DockState.DockBottomAutoHide;
            m_errors.lvErrors.MouseDoubleClick += lvErrors_MouseDoubleClick;
            m_errors.Show(dockPanel);

            m_settings = new EditorSettings();
            
            if (File.Exists(Application.StartupPath + "/settings.xml"))
                m_settings.Load();

            LoadTV();

            // set the check marks on the menu items that can be toggled
            if (m_settings.ShowLineNumbers)
                mbLineNumbers.Image = Resource.CheckMark;
            if (m_settings.ShowAutoComplete)
                mbAutoComplete.Image = Resource.CheckMark;
            if (m_settings.EnableAutoFormat)
                mbAutoFormat.Image = Resource.CheckMark;

            // check if any lua files were passed as command lines
            if (args.Length > 0)
            {
                foreach (string file in args)
                {
                    if (File.Exists(file) && file.EndsWith(".lua"))
                    {
                        OpenFile(file);
                    }
                }
            }
        }

        public static EditorSettings Settings
        {
            get { return m_settings; }
        }

        private LuaEditor NewLuaEditor()
        {
            LuaEditor newLua = new LuaEditor();
            // Set the closing even
            newLua.FormClosing += LuaEditor_FormClosing;
            newLua.scintilla1.KeyDown += LuaEditor_KeyDown;
            newLua.Name = null;
            newLua.Text = "Untitled Lua Script";
            int count = 0;
            foreach (LuaEditor lua in dockPanel.Documents)
            {
                if (lua.Text.Contains(newLua.Text))
                    count++;
            }

            if (count > 0)
            {
                count++;
                newLua.Text += " " + count.ToString();
            }

            if (!m_settings.ShowLineNumbers)
                newLua.scintilla1.Margins.Margin0.Width = 0;

            newLua.Show(dockPanel);

            return newLua;
        }

        public void OpenFile(string path)
        {
            if (!path.Contains(".lua"))
                return;

            // Check to see if the file is already opened and if so bring that tab to the front
            foreach (LuaEditor opened in dockPanel.Documents)
            {
                if (opened.Name == path)
                {
                    opened.Activate();
                    return;
                }
            }

            // Open the file
            LuaEditor doc = NewLuaEditor();
            doc.Name = path;
            doc.Text = path.Substring(path.LastIndexOf("\\") + 1);

            StreamReader reader = new StreamReader(path);
            doc.scintilla1.Text = reader.ReadToEnd();
            reader.Close();
            doc.scintilla1.UndoRedo.EmptyUndoBuffer();
            doc.SetSaved();
            doc.Show(dockPanel);
        }

        private string Save(string path, string text)
        {
            if (path == null || path == "")
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Lua files|*.lua";
                if (save.ShowDialog() == DialogResult.OK)
                {
                    path = save.FileName;
                }
            }

            if (path != null && path != "")
            {
                StreamWriter writer = new StreamWriter(path);
                writer.Write(text);
                writer.Close();
            }

            return path;
        }

        #region Solution Explorer

        List<string> GetExpanded(TreeNodeCollection nodes)
        {
            List<string> ret = new List<string>();

            foreach (TreeNode n in nodes)
            {
                if (n.IsExpanded)
                    ret.Add(n.Name);

                if (n.Nodes.Count > 0)
                    ret.AddRange(GetExpanded(n.Nodes));
            }

            return ret;
        }

        void ExpandNodes(TreeNodeCollection nodes, List<String> expanded)
        {
            foreach (TreeNode n in nodes)
            {
                if (expanded.Contains(n.Name))
                    n.Expand();

                if (n.Nodes.Count > 0)
                    ExpandNodes(n.Nodes, expanded);
            }
        }

        public void LoadTV()
        {
            TreeView tvFiles = m_solutionExplorer.tvExplorer;

            // Save the scroll position
            int scroll = GetScrollPos(tvFiles.Handle, SB_VERT);

            // Get a list of expanded nodes
            List<string> expanded = new List<string>();
            if (tvFiles.Nodes.Count > 0)
                expanded = GetExpanded(tvFiles.Nodes);

            // Prevent the tree view from re drawing while we repopulate the tree view
            tvFiles.BeginUpdate();
            tvFiles.Nodes.Clear();
            if (m_settings.ScriptFolder != null && m_settings.ScriptFolder != "NULL" && m_settings.ScriptFolder != "" && Directory.Exists(m_settings.ScriptFolder))
            {
                foreach (string s in Directory.EnumerateDirectories(m_settings.ScriptFolder))
                {
                    TreeNode node = new TreeNode();
                    node.Name = s;
                    node.Text = s.Substring(s.LastIndexOf("\\") + 1);

                    foreach (TreeNode n in GetNodes(s, false))
                        node.Nodes.Add(n);

                    foreach (TreeNode n in GetNodes(s, true))
                        node.Nodes.Add(n);

                    tvFiles.Nodes.Add(node);
                }
            }

            // Expand all previously expanded modes
            if (expanded.Count > 0)
                ExpandNodes(tvFiles.Nodes, expanded);

            // Allow the redraw
            tvFiles.EndUpdate();

            // Set the scroll position
            SetScrollPos(tvFiles.Handle, SB_VERT, scroll, true);
        }

        private static IEnumerable<TreeNode> GetNodes(string path, bool files)
        {
            //IEnumerable<TreeNode> nodes;
            List<TreeNode> nodes = new List<TreeNode>();
            if (files)
            {
                foreach (string s in Directory.EnumerateFiles(path, "*.lua"))
                {
                    TreeNode node = new TreeNode();
                    node.Name = s;
                    node.Text = s.Substring(s.LastIndexOf("\\") + 1);
                    node.ImageIndex = 1;
                    node.SelectedImageIndex = node.ImageIndex;
                    nodes.Add(node);
                }
            }
            else
            {
                foreach (string s in Directory.EnumerateDirectories(path))
                {
                    TreeNode node = new TreeNode();
                    node.Name = s;
                    node.Text = s.Substring(s.LastIndexOf("\\") + 1);
                    node.ImageIndex = 0;
                    node.SelectedImageIndex = node.ImageIndex;
                    foreach (TreeNode n in GetNodes(s, false))
                        node.Nodes.Add(n);

                    foreach (TreeNode n in GetNodes(s, true))
                        node.Nodes.Add(n);

                    nodes.Add(node);
                }
            }

            return nodes.AsEnumerable<TreeNode>();
        }

        private void tvExplorer_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (m_solutionExplorer.tvExplorer.SelectedNode != null)
                OpenFile(m_solutionExplorer.tvExplorer.SelectedNode.Name);
        }

        #endregion

        #region LuaEditor Event Handlers

        private void LuaEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sender is LuaEditor)
            {
                LuaEditor lua = sender as LuaEditor;
                if (lua.SaveNeeded)
                {
                    DialogResult result = MessageBox.Show("Do you want to save changes to " + lua.Text + "?", "Save Changes?", MessageBoxButtons.YesNoCancel);
                    if (result == DialogResult.Yes)
                        Save(lua.Name, lua.scintilla1.Text);
                    if (result == DialogResult.Cancel)
                        e.Cancel = true;
                }
            }
                
        }

        private void LuaEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is ScintillaNET.Scintilla)
            {
                // F1 pressed in the text error
                if (e.KeyCode == Keys.F1)
                {
                    ScintillaNET.Scintilla scintilla = sender as ScintillaNET.Scintilla;
                    // Get the current carrot postion
                    int pos = scintilla.CurrentPos;
                    // Get the start of the word position based on carrot position
                    int start = scintilla.NativeInterface.WordStartPosition(pos, true);
                    // Get the end of the word position based on carrot position
                    int end = scintilla.NativeInterface.WordEndPosition(pos, true);
                    // Get the word
                    string function = scintilla.Text.Substring(start, end - start);
                    // Check to see if this is a EQ2Emu lua function
                    if (scintilla.Lexing.Keywords[2].Contains(function))
                    {
                        // If so open the browser to its wiki page
                        ProcessStartInfo sInfo = new ProcessStartInfo("http://www.eq2emulator.net/wiki/index.php/LUA:" + function);
                        Process.Start(sInfo);
                    }
                }
            }
        }

        #endregion

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Save settings in settings.xml
            m_settings.Save();
        }

        #region Menu and Tool strip events

        private void tsbNew_Click(object sender, EventArgs e)
        {
            LuaEditor lua = NewLuaEditor();
            lua.SetSaved();
        }

        private void tsbOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Lua files (*.lua)|*lua";
            if (open.ShowDialog() == DialogResult.OK)
                OpenFile(open.FileName);
        }

        private void solutionExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_solutionExplorer.Show(dockPanel);
        }

        private void openProjectFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog project = new FolderBrowserDialog();
            if (project.ShowDialog() == DialogResult.OK)
            {
                m_settings.ScriptFolder = project.SelectedPath;
                LoadTV();
            }
        }

        private void tsbSave_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument is LuaEditor)
            {
                string path = (dockPanel.ActiveDocument as LuaEditor).Name;
                string text = (dockPanel.ActiveDocument as LuaEditor).scintilla1.Text;
                string path2 = Save(path, text);
                if (path2 != null && path2 != "")
                {
                    if ((dockPanel.ActiveDocument as LuaEditor).Name == "" || (dockPanel.ActiveDocument as LuaEditor).Name == null)
                    {
                        (dockPanel.ActiveDocument as LuaEditor).Name = path2;
                        (dockPanel.ActiveDocument as LuaEditor).Text = path2.Substring(path2.LastIndexOf("\\") + 1);
                    }
                    (dockPanel.ActiveDocument as LuaEditor).SetSaved();
                    LoadTV();
                }
            }
        }

        private void tsbSaveAs_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument == null)
                return;

            string save = Save(null, (dockPanel.ActiveDocument as LuaEditor).scintilla1.Text);

            if (save != null && save != "")
            {
                (dockPanel.ActiveDocument as LuaEditor).Name = save;
                (dockPanel.ActiveDocument as LuaEditor).Text = save.Substring(save.LastIndexOf("\\") + 1);
                (dockPanel.ActiveDocument as LuaEditor).SetSaved();
                LoadTV();
            }
        }

        private void tsbUndo_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument != null) 
                (dockPanel.ActiveDocument as LuaEditor).scintilla1.UndoRedo.Undo();
        }

        private void tsbRedo_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument != null)
                (dockPanel.ActiveDocument as LuaEditor).scintilla1.UndoRedo.Redo();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tsbCut_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument != null)
                (dockPanel.ActiveDocument as LuaEditor).scintilla1.Clipboard.Cut();
        }

        private void tsbCopy_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument != null)
                (dockPanel.ActiveDocument as LuaEditor).scintilla1.Clipboard.Copy();
        }

        private void tsbPaste_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument != null)
                (dockPanel.ActiveDocument as LuaEditor).scintilla1.Clipboard.Paste();
        }

        private void tsbDelete_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument != null)
                (dockPanel.ActiveDocument as LuaEditor).scintilla1.Selection.Text = "";
        }

        private void tsbSelectAll_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument != null)
                (dockPanel.ActiveDocument as LuaEditor).scintilla1.Selection.SelectAll();
        }

        private void mbLineNumbers_Click(object sender, EventArgs e)
        {
            if (m_settings.ShowLineNumbers)
            {
                m_settings.ShowLineNumbers = false;
                mbLineNumbers.Image = null;
                foreach (LuaEditor lua in dockPanel.Documents)
                    lua.scintilla1.Margins.Margin0.Width = 0;
            }
            else
            {
                m_settings.ShowLineNumbers = true;
                mbLineNumbers.Image = Resource.CheckMark;
                foreach (LuaEditor lua in dockPanel.Documents)
                    lua.scintilla1.Margins.Margin0.Width = 35;
            }
        }

        private void mbSpawnScript_Click(object sender, EventArgs e)
        {
            LuaEditor lua = NewLuaEditor();
            lua.scintilla1.Text = Scripts.GetSpawnScript();
            lua.SetSaved();
        }

        private void mbZoneScript_Click(object sender, EventArgs e)
        {
            LuaEditor lua = NewLuaEditor();
            lua.scintilla1.Text = Scripts.GetZoneScript();
            lua.SetSaved();
        }

        private void mbSpellScript_Click(object sender, EventArgs e)
        {
            LuaEditor lua = NewLuaEditor();
            lua.scintilla1.Text = Scripts.GetSpellScript();
            lua.SetSaved();
        }

        private void mbItemScript_Click(object sender, EventArgs e)
        {
            LuaEditor lua = NewLuaEditor();
            lua.scintilla1.Text = Scripts.GetItemScript();
            lua.SetSaved();
        }

        private void mbQuestScript_Click(object sender, EventArgs e)
        {
            LuaEditor lua = NewLuaEditor();
            lua.scintilla1.Text = Scripts.GetQuestScript();
            lua.SetSaved();
        }

        private void mbAutoComplete_Click(object sender, EventArgs e)
        {
            if (m_settings.ShowAutoComplete)
            {
                m_settings.ShowAutoComplete = false;
                mbAutoComplete.Image = null;
            }
            else
            {
                m_settings.ShowAutoComplete = true;
                mbAutoComplete.Image = Resource.CheckMark;
            }
        }

        private void mbAutoFormat_Click(object sender, EventArgs e)
        {
            if (m_settings.EnableAutoFormat)
            {
                m_settings.EnableAutoFormat = false;
                mbAutoFormat.Image = null;
            }
            else
            {
                m_settings.EnableAutoFormat = true;
                mbAutoFormat.Image = Resource.CheckMark;
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorSettings frm = new ColorSettings();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                foreach (LuaEditor lua in dockPanel.Documents)
                {
                    lua.scintilla1.Styles[1].ForeColor = Color.FromName(Settings.CommentColor);
                    lua.scintilla1.Styles[4].ForeColor = Color.FromName(Settings.NumberColor);
                    lua.scintilla1.Styles[5].ForeColor = Color.FromName(Settings.Keyword0Color);
                    lua.scintilla1.Styles[6].ForeColor = Color.FromName(Settings.StringColor);
                    lua.scintilla1.Styles[11].ForeColor = Color.FromName(Settings.TextColor);
                    lua.scintilla1.Styles[13].ForeColor = Color.FromName(Settings.Keyword1Color);
                    lua.scintilla1.Styles[14].ForeColor = Color.FromName(Settings.Keyword2Color);

                    lua.scintilla1.Styles[1].BackColor = Color.FromName(Settings.CommentBackColor);
                    lua.scintilla1.Styles[4].BackColor = Color.FromName(Settings.NumberBackColor);
                    lua.scintilla1.Styles[5].BackColor = Color.FromName(Settings.Keyword0BackColor);
                    lua.scintilla1.Styles[6].BackColor = Color.FromName(Settings.StringBackColor);
                    lua.scintilla1.Styles[11].BackColor = Color.FromName(Settings.TextBackColor);
                    lua.scintilla1.Styles[13].BackColor = Color.FromName(Settings.Keyword1BackColor);
                    lua.scintilla1.Styles[14].BackColor = Color.FromName(Settings.Keyword2BackColor);
                }
            }
        }
        #endregion

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument != null)
                (dockPanel.ActiveDocument as LuaEditor).scintilla1.FindReplace.ShowFind();
        }

        private void findReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument != null)
                (dockPanel.ActiveDocument as LuaEditor).scintilla1.FindReplace.ShowReplace();
        }

        private void tsbCompile_Click(object sender, EventArgs e)
        {
            if (dockPanel.ActiveDocument == null)
                return;

            if ((dockPanel.ActiveDocument as LuaEditor).SaveNeeded)
                tsbSave_Click(null, null);

            ListView lv = m_errors.lvErrors;
            lv.Items.Clear();
            m_errorList.Clear();
            m_index = 0;

            StartBuild();
            CompileLua((dockPanel.ActiveDocument as LuaEditor).Name);
            FinishBuild();
        }

        private void CompileLua(string file)
        {
            string output = "Compiling " + file.Substring(file.LastIndexOf('\\') + 1) + " ...\n";
            AddOutput(output);

            Process compiler = new Process();
            compiler.StartInfo.FileName = "luac.exe";
            compiler.StartInfo.Arguments = "-p -- \"" + file + "\"";
            compiler.StartInfo.UseShellExecute = false;
            compiler.StartInfo.RedirectStandardError = true;
            compiler.StartInfo.RedirectStandardOutput = true;
            compiler.StartInfo.CreateNoWindow = true;
            compiler.Start();

            string output2 = compiler.StandardOutput.ReadToEnd();
            string error = compiler.StandardError.ReadToEnd();


            compiler.WaitForExit();
            compiler.Close();

            if (error.Contains(Settings.ScriptFolder))
                error = error.Remove(error.IndexOf(Settings.ScriptFolder), Settings.ScriptFolder.Length);

            AddOutput(output2);
            string[] errors = error.Split(':');
            if (errors.Count() > 1)
            {
                AddRow(m_errors.lvErrors, errors, file);
                AddOutput(error);
            }
        }

        public void AddRow(ListView lv, string[] errors, string file)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(delegate() { AddRow(lv, errors, file); }));
                return;
            }
            

            /*
            * luac.exe outputs the following
            * luac52.exe: template.lua:14: ')' expected (to close '(' at line 13) near 'end'
            */

            m_index++;

            ListViewItem item = new ListViewItem(m_index.ToString());
            item.SubItems.Add(errors[3]);
            if (errors[1].Contains('\\'))
                item.SubItems.Add(errors[1].Substring(errors[1].LastIndexOf('\\') + 1));
            else
                item.SubItems.Add(errors[1]);

            item.SubItems.Add(errors[2]);

            lv.Items.Add(item);

            ErrorStats stat;
            stat.File = file;
            stat.Line = int.Parse(errors[2]);
            m_errorList.Add(stat);
        }

        private void tsbCompileAll_Click(object sender, EventArgs e)
        {
            m_errors.lvErrors.Items.Clear();
            m_errorList.Clear();
            Thread thread_CompileAll = new Thread(new ThreadStart(CompileAll));
            thread_CompileAll.Start();
        }

        private void CompileAll()
        {
            m_index = 0;
            StartBuild();
            CompileNodes(m_solutionExplorer.tvExplorer.Nodes);
            FinishBuild();
        }

        private void CompileNodes(TreeNodeCollection Nodes)
        {
            foreach (TreeNode n in Nodes)
            {
                if (n.Name.EndsWith(".lua"))
                    CompileLua(n.Name);
                if (n.Nodes.Count > 0)
                    CompileNodes(n.Nodes);
            }
        }

        private void StartBuild()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(delegate() { StartBuild(); }));
                return;
            }

            m_output.rtbOutput.Clear();
            m_output.rtbOutput.Text = "------ Build Started ------\n";

			if (m_output.DockState == DockState.DockBottomAutoHide)
			{
				dockPanel.ActiveAutoHideContent = m_output;
				m_output.rtbOutput.Focus();
			}
            //m_output.BringToFront();
                //m_output.Focus();
        }

        private void FinishBuild()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(delegate() { FinishBuild(); }));
                return;
            }


            m_output.rtbOutput.AppendText("------ Build: " + m_errorList.Count.ToString() + " failed ------\n");
            ScrollToEnd();
        }

        private void AddOutput(string output)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(delegate() { AddOutput(output); }));
                return;
            }

            m_output.rtbOutput.AppendText(output);
            ScrollToEnd();
        }

        private static void ScrollToEnd()
        {
            m_output.rtbOutput.SelectionStart = m_output.rtbOutput.Text.Length;
            m_output.rtbOutput.ScrollToCaret();
        }

        public void lvErrors_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenFile(m_errorList[m_errors.lvErrors.SelectedIndices[0]].File);
            // need the - 1 so the caret is put on the currect line
            (dockPanel.ActiveDocument as LuaEditor).scintilla1.Caret.LineNumber = m_errorList[m_errors.lvErrors.SelectedIndices[0]].Line - 1;
        }

        private const int WM_COPYDATA = 0x4A;
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_COPYDATA:
                    COPYDATASTRUCT cds = (COPYDATASTRUCT)m.GetLParam(typeof(COPYDATASTRUCT));
                    byte[] b = new byte[cds.cbData];
                    IntPtr lpData = new IntPtr(cds.lpData);
                    System.Runtime.InteropServices.Marshal.Copy(lpData, b, 0, cds.cbData);
                    string file = Encoding.Default.GetString(b);
                    //MessageBox.Show(file);
                    // check if any lua files were passed as command lines
                    if (File.Exists(file) && file.EndsWith(".lua"))
                    {
                        OpenFile(file);
                    }
                    break;
            }
            base.WndProc(ref m);
        }
    }
}
