using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace GodFather
{
    public partial class Form1 : Form
    {
        private FileSystemWatcher _mWatcher;
        private int _chngNumber;
        private int _allNumber;
        public DirectoryInfo Info;
        public int TheTempFileCount;

        public Form1()
        {
            InitializeComponent();
            TheTempFileCount = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == @"Autorename On")
            {
                button2.Text = @"Autorename Off";
                StartWatching();
                pictureBox1.Visible = true;
            }
            else
            {
                button2.Text = @"Autorename On";
                StopWatching();
                pictureBox1.Visible = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
            Info = new DirectoryInfo(textBox1.Text + "\\");
            EnableButtons();
        }

        private void EnableButtons()
        {
            button5.Enabled = textBox1.Text.Length > 0;
            button4.Enabled = textBox1.Text.Length > 0;
            button3.Enabled = textBox1.Text.Length > 0;
            button2.Enabled = textBox1.Text.Length > 0;
            checkBox1.Enabled = textBox1.Text.Length > 0;
        }

        private void StartWatching()
        {
            _chngNumber = 0;
            _allNumber = 0;
            SetLabel(@"Auto-Renaming active!");
            _mWatcher = new FileSystemWatcher
            {
                Path = textBox1.Text + "\\",
                Filter = "*.*",
                NotifyFilter = NotifyFilters.DirectoryName |
                               NotifyFilters.FileName  |
                               NotifyFilters.LastWrite
            };

            _mWatcher.Changed += new FileSystemEventHandler(OnChanged);
            _mWatcher.Created += new FileSystemEventHandler(OnChanged);
            _mWatcher.Deleted += new FileSystemEventHandler(OnDeleted);
            _mWatcher.Renamed += new RenamedEventHandler(OnRenamed);
            _mWatcher.EnableRaisingEvents = true;
        }
        private void StopWatching()
        {
            TheTempFileCount = 0;
            try
            {
                _mWatcher.EnableRaisingEvents = false;
                _mWatcher.Dispose();
                _mWatcher = null;
            }
            catch
            {
                _mWatcher?.Dispose();
                _mWatcher = null;
            }
            finally
            {
                SetLabel($@"{_chngNumber} of {_allNumber} renamed");
            }
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
                RenameFiles();
        }

        private int GetLastNumber()
        {
            var retVal = 0;

            var theFiles = checkBox1.Checked
                ? Info.GetFiles().OrderBy(p => p.Name.Length).ThenBy(p=> p.Name.Trim().ToLower()).ToArray()
                : Info.GetFiles().OrderBy(p => p.LastWriteTime).ToArray();
            foreach (var file in theFiles)
            {
                int.TryParse(file.Name.Substring(0, 3), out var theInt);
                retVal = retVal < theInt ? theInt : retVal;
            }
            _allNumber = theFiles.Length;
            return ++retVal;
        }

        private void SetLabel(String msg)
        {
            label1.Invoke((MethodInvoker)(() => label1.Text = msg));
        }

        void OnRenamed(object sender, RenamedEventArgs e)
        {
            //not Implemented
        }

        void OnDeleted(object sender, FileSystemEventArgs e)
        {
            //not Implemented
        }

        int GetFileCount()
        {
            var theFiles = checkBox1.Checked
                ? Info.GetFiles().OrderBy(p => p.Name.Trim().ToLower()).ToArray()
                : Info.GetFiles().OrderBy(p => p.LastWriteTime).ToArray();
            return theFiles.Length;
        }

        void RenameFiles()
         {
            while (TheTempFileCount!=GetFileCount())
            {
                TheTempFileCount = GetFileCount();
                Thread.Sleep(1000);
            }

            var theFiles = checkBox1.Checked
                ? Info.GetFiles().OrderBy(p => p.Name.Length).ThenBy(p => p.Name.Trim().ToLower()).ToArray()
                : Info.GetFiles().OrderBy(p => p.LastWriteTime).ToArray();
            foreach (var ew in theFiles)
            {
                int.TryParse(ew.Name.Split(' ')[0], out var theN);
                if (theN == 0)
                {
                    try
                    {
                        var genNum = new StringBuilder();
                        genNum.Insert(0, 1);
                        var str = new StringBuilder(ew.Name);
                        var theNum = GetLastNumber().ToString().PadLeft(3, '0') + " ";
                        str.Insert(0, theNum);
                        var newName = textBox1.Text + "\\" + str;
                        File.Move(ew.FullName, newName);
                        _chngNumber++;
                        SetLabel($@"File {ew.Name} renamed");
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
             //TheTempFileCount = 0;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
          ClearNames();
        }

        void ClearNames()
        {
            var theFiles = Info.GetFiles().OrderBy(p => p.LastWriteTime).ToArray();
            foreach (var file in theFiles)
            {
                int.TryParse(file.Name.Trim().Substring(0, file.Name.Trim().IndexOf(' ') + 1), out var theInt);
                if (theInt > 0)
                {
                    var newName = textBox1.Text + "\\" + file.Name.Trim().Substring(file.Name.Trim().IndexOf(' ') + 1);
                    File.Move(file.FullName, newName);
                }
            }
        }
        
        private void Button4_Click_1(object sender, EventArgs e)
        {
            RenameFiles();
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            try
            {
                var theFiles = Info.GetFiles().OrderBy(p => p.LastWriteTime).ToArray();
                var folderName = theFiles.First().Name.Trim().Substring(theFiles.First().Name.Trim().IndexOf(' ') + 1).Split('.')[0];
                var promptValue = Prompt.ShowDialog("Modify the Folder Name bellow", "New Folder Name", folderName);
                Directory.CreateDirectory(textBox1.Text + "\\" + promptValue.Trim());
                foreach (var file in theFiles)
                {
                    var newName = ((textBox1.Text + "\\" + promptValue + "\\" + file.Name).Trim()
                                  .Split('(')[0]).Trim();
                    File.Move(file.FullName, (newName.Replace(file.Extension, "")).TrimEnd(' ') + file.Extension);
                }
                SetLabel($@"New Folder {promptValue} created, with {theFiles.Length} files.");
            }
            catch
            {
                SetLabel(@"No files found!");
            }
        }
    }

    public static class Prompt
    {
        public static string ShowDialog(string text, string caption, string defaultText="")
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
            TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
            textBox.Text = defaultText;
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}
