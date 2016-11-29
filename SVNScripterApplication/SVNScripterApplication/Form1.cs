using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;


namespace SVNScripterApplication
{
    public partial class Scripter : Form
    {
        public Scripter()
        {
            InitializeComponent();
            button2.Enabled = false;
            checkBox1.Enabled = false;
            checkBox1.Checked = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {   
            //TextBox validation
            if (textBox1.Text == "")
            {
                MessageBox.Show("Enter a Directory Path!", "Important Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                //Store the diretory path
                string path = textBox1.Text;

                //Set selection mode to multi select and clear it
                listBox1.SelectionMode = SelectionMode.MultiExtended;
                listBox1.Items.Clear();

                //If the directory exists populate the content with .sql extension files
                if (CheckPathExists(path))
                {
                    string[] files = Directory.GetFiles(path,"*.sql");
                    textBox2.Text = path;
                    int fileExists = 0;

                    foreach (string file in files)
                    {
                        listBox1.Items.Add(Path.GetFileName(file));
                        fileExists++;
                    }

                    //If the fileExists is not equal to zero there are found .sql files
                    if (fileExists != 0)
                    {
                        button2.Enabled = true;
                        checkBox1.Enabled = true;
                    }
                    else
                    {
                        button2.Enabled = false;
                        checkBox1.Enabled = false;
                        checkBox1.Checked = false;
                    }
                }        
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string fpath = textBox2.Text;
                var count = 0;
                bool success = true; 
 
                //Must be declare out of scope to avoid error.
                string currentEndLine = null;
                string prvFileLastLine = null;

                //Declare the fileName
                string newFileName = "UpgradeScript.sql";
                string filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + newFileName;

                int count1 = 1;
                string fileNameOnly = Path.GetFileNameWithoutExtension(filePath);
                string extension = Path.GetExtension(filePath);
                string path = Path.GetDirectoryName(filePath);
                string newFullPath = filePath;

                //If the filename exists create a new name
                while (File.Exists(newFileName))
                {
                    string tempFileName = string.Format("{0}({1})", fileNameOnly, count1++);
                    newFullPath = Path.Combine(path, tempFileName + extension);
                    string fileTitle = Path.GetFileName(newFullPath);
                    newFileName = fileTitle;
                }

                foreach (var listItem in listBox1.SelectedItems)
                {
                    //For each file copy its content to a new file 
                    string Dpath = fpath + "\\" + listItem.ToString();

                    //If this is not the first file store the previous last line
                    if (count != 0)
                    {
                        prvFileLastLine = currentEndLine;
                    }
                    currentEndLine = lastLine(Dpath);
                    
                    //If the endLine is null, then the file is blank
                    if (currentEndLine != null)
                    {
                        CreateScriptFile(newFileName, Dpath, count, currentEndLine, prvFileLastLine);
                        success = true;
                        count++;
                    }
                    else
                    {
                        string fileName = Path.GetFileName(Dpath);
                        MessageBox.Show("The file ( " + fileName + " ) is blank!", "Important Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        listBox1.SelectedIndex = -1;
                        success = false;
                        break;
                    }
                }

                //If the file has been created. All good!
                if (success)
                {
                    //Success
                    Process.Start(new ProcessStartInfo(newFileName, " /select, " + path));
                }
            }
            else
            {
                MessageBox.Show("Select a file(s) from the listbox", "Important Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFD.Title = "Select a Script";
                OpenFD.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                OpenFD.FileName = "";
                OpenFD.Filter = "SQL|*.sql";
                OpenFD.Multiselect = true;

                if (OpenFD.ShowDialog() == DialogResult.OK)
                {
                    listBox1.Items.Clear();
                    string[] chosenFile = OpenFD.FileNames;
                    foreach (var itemList in chosenFile)
                    {
                        string files = Path.GetFileName(itemList);
                        listBox1.Items.Add(files);
                    }
                    listBox1.SelectionMode = SelectionMode.MultiExtended;

                    textBox1.Text = GetPath(OpenFD.FileName);
                } 
           }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count != 0)
            {
                textBox1.Clear();
                listBox1.Items.Clear();
                button2.Enabled = false;
                checkBox1.Enabled = false;
                checkBox1.Checked = false;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.Items.Count != 0)
            {
                listBox1.SelectionMode = SelectionMode.MultiExtended;
                button2.Enabled = true;
                checkBox1.Enabled = true;
            }
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop,false) == true)
            {
                e.Effect = DragDropEffects.All;
            }  
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
       {
            //If the listbox already has values - clear it.
            if (listBox1.Items.Count != 0)
            {
                listBox1.Items.Clear();
            }
            //Gather the selected files and pass them to the listbox.
            string[] dropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            Array.Sort(dropFiles);
            foreach (var dropFile in dropFiles)
            {
                if (Path.GetExtension(dropFile) == ".sql")
                {
                    listBox1.Items.Add(Path.GetFileName(dropFile));
                    textBox1.Text = GetPath(dropFile.ToString());
                    checkBox1.Enabled = true;
                }       
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    listBox1.SetSelected(i, true);
                }
            }
            else
            {
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    listBox1.SetSelected(i,false);
                }
            }
        }


        //-----------------------------------------------------------------//
        //-----------------------------------------------------------------//
        //Methods
        public bool CheckPathExists(string path)
        {
            //Check the directory exists
            bool directoryFound = true;
            do
            {
                if (!directoryFound)
                {
                    MessageBox.Show("This directory does not exist.\n" + path, "Important Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    button2.Enabled = false;
                    checkBox1.Enabled = false;
                    checkBox1.Checked = false;
                    return false;
                }
                directoryFound = Directory.Exists(path);

            } while (!directoryFound);

            return true;
        }

        private string GetPath(string path)
        {
            string dirPath = Path.GetDirectoryName(path);
            textBox2.Text = dirPath;
            return dirPath;
        }

        public static void CreateScriptFile(string fileName, string contentFile, int countPosition, string endLine, string lastEndLine)
        {
            //Create the UpdateScript if it does not exist already
            if (!File.Exists(fileName))
            {
                File.Create(fileName).Dispose();
            }

            //Read each line of the file to contentObj
            string contentObj = "";
            bool isFirstLine = true;

            System.IO.StreamReader objReader;
            objReader = new System.IO.StreamReader(contentFile);
            do
            {
                //Store the current line of the file
                string CurrentLine = objReader.ReadLine();

                //If the first line and it is not Go, add it.
                if (isFirstLine && countPosition == 0)
                {
                    //Check if the first line of the text file is blank, if so, next line is still the first
                    if (CurrentLine != "")
                    {
                        if (!(CurrentLine.ToUpper() == "GO"))
                        {
                            contentObj = "GO" + "\r\n";
                        }
                        isFirstLine = false;
                        contentObj = contentObj + CurrentLine + "\r\n";
                    }
                    else
                    {
                        contentObj = contentObj + CurrentLine + "\r\n";
                    }
                }
                else if (countPosition != 0)
                {
                    //Multiple File(s) have been selected.

                    //Check if the first line of the text file is blank, if so, next line is still the first
                    if (CurrentLine != "")
                    {
                        //Check if the last line of the previous file is not equal GO
                        if (!(lastEndLine.ToUpper() == "GO"))
                        {
                            //Check if the first line of the current file DOES NOT equals GO
                            if (isFirstLine && (!(CurrentLine.ToUpper() == "GO")))
                            {
                                contentObj = "GO" + "\r\n";
                                isFirstLine = false;
                            }
                            contentObj = contentObj + CurrentLine + "\r\n";
                        }
                        //If the last line of the previous file is GO, ignore the first GO on the curent file.
                        else if (lastEndLine.ToUpper() == "GO")
                        {
                            //Check if the first line of the current file DOES NOT equals GO
                            if (isFirstLine && (!(CurrentLine.ToUpper() == "GO")))
                            {
                                if (lastEndLine.ToUpper() == "GO")
                                {
                                    contentObj = contentObj + CurrentLine + "\r\n";
                                    isFirstLine = false;
                                }
                                else
                                {
                                    contentObj = "GO" + "\r\n";
                                    isFirstLine = false;
                                }                               
                            }
                            else if (CurrentLine.ToUpper() == "GO")
                            {
                                contentObj = "" + "\r\n";
                                isFirstLine = false;
                            }
                            else
                            {
                                contentObj = contentObj + CurrentLine + "\r\n";
                            }                           
                            lastEndLine = "";
                            isFirstLine = false;
                        }
                        else
                        {
                            isFirstLine = false;
                            lastEndLine = "";
                            contentObj = contentObj + CurrentLine + "\r\n";
                        }
                    }
                    else
                    {
                        contentObj = contentObj + CurrentLine + "\r\n";
                    }
                }
                else
                {
                    contentObj = contentObj + CurrentLine + "\r\n";
                }

            } while (objReader.Peek() != -1);

            objReader.Close();

            //write the content of the object to the new file.
            System.IO.StreamWriter objWriter;
            objWriter = new System.IO.StreamWriter(fileName, true);

            objWriter.Write(contentObj);
            objWriter.Write("----------------------------------------------------------------\r\n");
            objWriter.WriteLine();
            objWriter.Close();
        }

        public static string lastLine(string path)
        {
            var lines = File.ReadLines(path);
            if (new FileInfo(path).Length != 0)
            {
                string line = lines.Last();
                //If the last line is blank, seach backward for the last Go
                if (line == "")
                {
                    List<string> textLines = File.ReadLines(path).Reverse().Take(4).ToList();
                    for (int i = 0; i < textLines.Count; i++)
                    {
                        if (textLines[i].ToUpper() == "GO")
                        {
                            line = textLines[i];
                            break;
                        }
                    }
                }
                return line;
            }
            return null;
        }
    }
}
