using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ShookaLogReader
{
    public enum LogSender
    {
        VidyoDLL, VidyoPortal, ShookaClient, GUI, ShookaWebService
    }
    public enum LogType
    {
        Info, Warning, Error, CriticalError
    }
    public enum LoggedFor
    {
        Programmer, Maintainer
    }

    public partial class LogForm : Form
    {
        public LogForm()
        {
            InitializeComponent();

            comboBox1.Items.Add(LoggedFor.Programmer.ToString());
            comboBox1.Items.Add(LoggedFor.Maintainer.ToString());
            comboBox1.SelectedIndex = 2;

            Text += " - نسخه " + Application.ProductVersion;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (File.Exists(openFileDialog1.FileName))
                OpenXML();
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                OpenXML();
            }
        }

        private void OpenXML()
        {
            string line = "";
            string directoryPath = Path.GetDirectoryName(openFileDialog1.FileName);
            //string LogFile = directoryPath + "ShookaLog_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".logxml";
            //string logfileName = "ShookaLog_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".logxml";
            string logfileName = "ShookaLog.logxml";
            string LogFile = directoryPath + logfileName;

            using (StreamWriter sw = new StreamWriter(LogFile, true, Encoding.UTF8))
            {
                //writing log file root
                using (XmlTextWriter w = new XmlTextWriter(sw))
                {
                    sw.WriteLine("<LogFile>");
                    using (StreamReader reader = new StreamReader(openFileDialog1.FileName, Encoding.UTF8))
                    {
                        while (!reader.EndOfStream)
                        {
                            //line = reader.ReadLine();
                            //line = EncryptionManager.Decrypt(reader.ReadLine());
                            string test = reader.ReadLine();
                            line = StringCipher.Decrypt(test);
                            string result = "";
                            string y;
                            if (line.Contains("messageString"))
                            {
                                try
                                {
                                    int pFrom = line.IndexOf("messageString:") + "messageString:".Length;
                                    int pTo = line.LastIndexOf("</message>");

                                    result = line.Substring(pFrom, pTo - pFrom);
                                    line = line.Replace(result, "");

                                    Debug.WriteLine(line);
                                }
                                catch (Exception ex)
                                { }
                            }
                            //if (line.Contains("&"))
                            while (line.Contains("&"))
                            {

                                int lineposition = line.IndexOf("&");
                                string strreplace = "amp;";
                                line = line.Substring(0, lineposition - 1) + strreplace + line.Substring(lineposition + 1);
                            }
                            if (line.Contains("<>"))
                            {
                                int pFrom = line.IndexOf("<message>") + "<message>".Length;
                                int pTo = line.LastIndexOf("</message>");

                                result = line.Substring(pFrom, pTo - pFrom);
                                foreach (char c in result)
                                {
                                    if (c == '<' || c == '>')
                                        line = line.Replace(c.ToString(), string.Empty);
                                }
                                Debug.WriteLine(line);
                            }
                            //var startTag = "<message>";
                            //int startIndex = line.IndexOf(startTag) + startTag.Length;
                            //int endIndex = line.IndexOf("</message>", startIndex);
                            //while(line.Substring(startIndex, endIndex - startIndex).Contains("<"))
                            //{
                            //   line = line.Remove('<');
                            //}
                            //line = Regex.Replace(line, "<message>([A-Za-z]{3})</message>", "&lt;span&gt;$1&lt;/span&gt;");
                            //  line = Regex.Replace(line, ":.*?<", string.Empty);
                            sw.WriteLine(line);
                            //writer.WriteLine(line);
                            // GatewayServer
                            //Writing Version and Encoding at first line of log file
                            //w.WriteStartDocument();
                            //start of application log. if </LogFile> will not write at end of log file,
                            //it means unsuccessful termination of current execution
                        }
                    }
                }
                //using (StreamWriter writer = new StreamWriter(openFileDialog1.FileName, true))
                //{

                //}
            }
            if (line != "</LogFile>")
            {
                using (StreamWriter writer = new StreamWriter(LogFile, true))
                    writer.WriteLine("</LogFile>");
                MessageBox.Show("این لاگ به صورت کامل پایان نیافته است.\nبه صورت خودکار لاگ اصلاح خواهد شد.");
            }

            //OpenFileDialog od = new OpenFileDialog();
            //od.FileName = LogFile;
            //Stream input = null;
            //if ((input = od.OpenFile()) != null)
            //{
            //    using (input)
            string input = LogFile;
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.CheckCharacters = false;
                //XmlReader xmlFile = XmlReader.Create(input, settings);
                XmlReader xmlFile = XmlReader.Create(input, settings);
                DataSet dataSet = new DataSet();
                dataSet.ReadXml(xmlFile);
                xmlFile.Close();

                for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                {
                    string s = dataSet.Tables[0].Rows[i]["loggedFor"].ToString();
                    if (!(comboBox1.SelectedIndex == 0 || comboBox1.Text == s))
                    {
                        dataSet.Tables[0].Rows[i].Delete();
                        i--;
                    }
                }

                dataGridView1.DataSource = dataSet.Tables[0];
                //}

                dataGridView1.Columns[0].Width = 100;
                dataGridView1.Columns[1].Width = 100;
                dataGridView1.Columns[2].Width = 50;
                dataGridView1.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridView1.Columns[4].Width = 100;

                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    DataGridViewRow row = dataGridView1.Rows[i];

                    string loggedFor = row.Cells["loggedFor"].Value.ToString();
                    string type = row.Cells["type"].Value.ToString();

                    string message = row.Cells["message"].Value.ToString();
                    int n = message.Split('\n').Length;
                    if (n > 1)
                        row.Height *= n;


                    if (loggedFor == LoggedFor.Maintainer.ToString())
                    {
                        row.DefaultCellStyle.Font = new Font(Font, FontStyle.Bold);
                        row.DefaultCellStyle.BackColor = Color.Khaki;
                    }

                    switch (type.ToString())
                    {
                        case "Warning":
                            row.DefaultCellStyle.BackColor = Color.Yellow;
                            break;
                        case "Error":
                            row.DefaultCellStyle.BackColor = Color.Red;
                            row.DefaultCellStyle.ForeColor = Color.White;
                            break;
                        case "CriticalError":
                            row.DefaultCellStyle.BackColor = Color.DarkRed;
                            row.DefaultCellStyle.ForeColor = Color.White;
                            break;
                    }
                }
            }
        }

        private void dataGridView1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            openFileDialog1.FileName = files[0];
            OpenXML();
        }

        private void dataGridView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void LogForm_Load(object sender, EventArgs e)
        {
            string[] files = Environment.GetCommandLineArgs();
            if (files.Length > 1)
            {
                openFileDialog1.FileName = files[1];
                OpenXML();
            }
        }
    }
}
