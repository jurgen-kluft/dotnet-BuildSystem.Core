using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using AdvancedDataGridView;
using System.Collections.Generic;
using GameCore;
using DataBuildSystem;

namespace BigfileViewer
{
    public partial class MainGUI : Form
    {
        private BigfileToc mBigfileToc = new BigfileToc();
        private const int FILEID_COLUMN = 0;
        private const int FILESIZE_COLUMN = 1;
        private const int OFFSET_COLUMN = 3;
        private const int FILENAME_COLUMN = 4;

        // -name MJ -platform PC -territory Europe -config Config.%PLATFORM%.cs -srcpath "I:\HgDev\.NET_BuildSystem\Data.Test" -dstpath "%SRCPATH%\Bin.%PLATFORM%" -deppath "%SRCPATH%\Dep.%PLATFORM%" -toolpath "%SRCPATH%\Tools" -pubpath "%SRCPATH%\Publish.%PLATFORM%"
        public MainGUI(CommandLine cmdLine)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            if (!BuildSystemCompilerConfig.init(cmdLine["name"], cmdLine["config"], cmdLine.HasParameter("bigfile"), cmdLine["platform"], cmdLine["target"], cmdLine["territory"], cmdLine["srcpath"], cmdLine["folder"], cmdLine["dstpath"], cmdLine["deppath"], cmdLine["toolpath"], cmdLine["pubpath"]))
            {
                Console.WriteLine("Usage: -name [NAME]");
                Console.WriteLine("       -config [FILENAME]");
                Console.WriteLine("       -platform [PLATFORM]");
                Console.WriteLine("       -target [PLATFORM]");
                Console.WriteLine("       -territory [Europe/USA/Asia/Japan]");
                Console.WriteLine("       -srcpath [SRCPATH]");
                Console.WriteLine("       -file* [FILENAME]");
                Console.WriteLine("       -folder* [PATH]");
                Console.WriteLine("       -dstpath [DSTPATH]");
                Console.WriteLine("       -deppath [DSTDEPPATH]");
                Console.WriteLine("       -toolpath [TOOLPATH]");
                Console.WriteLine("       -pubpath [PUBLISHPATH]");
                Console.WriteLine();
                Console.WriteLine("Optional:");
                Console.WriteLine("          -bigfile             Do build the Bigfile, if not provided only the BigfileToc and FilenameDb will be build.");
                Console.WriteLine();

                InitializeComponent();

                this.Text += "BigfileViewer v" + version + ", file [ command line error! ]";
            }
            else
            {
                Console.WriteLine("------ BigfileViewer: v{0} (Platform: {1}) ------", version, BuildSystemCompilerConfig.PlatformName);

                // Referenced assemblies, we always include ourselves
                List<Filename> referencedAssemblies = new List<Filename>();
                cmdLine.CollectIndexedParams(0, true, "asm", delegate(string param) { referencedAssemblies.Add(new Filename(param)); });
                referencedAssemblies.Insert(0, new Filename(Assembly.GetExecutingAssembly().Location));

                // Construct an assembly with the config object
                Assembly configDataAssembly = null;
                if (!BuildSystemCompilerConfig.ConfigFilename.IsEmpty)
                {
                    List<Filename> configSrcFiles = new List<Filename>();
                    configSrcFiles.Add(new Filename(GameCore.Environment.expandVariables(BuildSystemCompilerConfig.ConfigFilename)));
                    Filename configAsmFilename = new Filename(BuildSystemCompilerConfig.Name + ".BigfileViewer.Config.dll");
                    Filename configAssemblyFilename = configAsmFilename;
                    configDataAssembly = AssemblyCompiler.Compile(configAssemblyFilename, configSrcFiles.ToArray(), new Filename[0], BuildSystemCompilerConfig.SrcPath, BuildSystemCompilerConfig.SubPath, BuildSystemCompilerConfig.DstPath, BuildSystemCompilerConfig.DepPath, referencedAssemblies.ToArray());
                }

                // BuildSystem.DataCompiler configuration
                IBuildSystemCompilerConfig configForCompiler = AssemblyUtil.Create1<IBuildSystemCompilerConfig>(configDataAssembly);
                if (configForCompiler != null)
                    BuildSystemCompilerConfig.Init(configForCompiler);

                // Bigfile configuration
                IBigfileConfig configForBigfileBuilder = AssemblyUtil.Create1<IBigfileConfig>(configDataAssembly);
                if (configForBigfileBuilder != null)
                    BigfileConfig.Init(configForBigfileBuilder);

                EEndian bigFileEndian = BigfileConfig.LittleEndian ? EEndian.LITTLE : EEndian.BIG;
                Filename bigfileFilename = new Filename(BuildSystemCompilerConfig.Name + BigfileConfig.BigFileExtension);
                mBigfileToc.load(BuildSystemCompilerConfig.PubPath + bigfileFilename, bigFileEndian);

                InitializeComponent();

                Filename fullFilename = new Filename(BuildSystemCompilerConfig.PubPath + bigfileFilename);
                this.Text += "BigfileViewer v" + version + ", file [" + fullFilename + "]";

                populateTreeView();
            }
        }

        private void populateTreeView()
        {
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeGridView1)).BeginInit();

            treeGridView1.Nodes.Clear();

            for (int i = 0; i < mBigfileToc.Count; i++)
            {
                BigfileFile file = mBigfileToc.infoOf(i);

                int instancesNum = file.offsets.Length;

                string filenameStr = file.filename.ToString();
                Hash128 hash = Hash128.FromString(filenameStr.ToLower());

                TreeGridNode srcFileNode = treeGridView1.Nodes.Add(hash.ToString(), file.size.ToString(), instancesNum.ToString(), null, filenameStr, "False");
                DataGridViewComboBoxCell cell = (DataGridViewComboBoxCell)treeGridView1.GetNodeForRow(treeGridView1.RowCount - 1).Cells[3];
                if (null != cell)
                {
                    if (instancesNum > 0)
                    {
                        foreach (StreamOffset offset in file.offsets)
                        {
                            cell.Items.Add(offset.value32.ToString());
                        }
                    }
                    else
                    {
                        cell.Items.Add("-1");
                    }

                    cell.Value = cell.Items[0];
                }
            }

            ((System.ComponentModel.ISupportInitialize)(this.treeGridView1)).EndInit();
            this.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void findNext_Click(object sender, EventArgs e)
        {
            if (searchText.Text.Length < 1)
            {
                return;
            }

            int column = FILENAME_COLUMN;
            if (fileIDMode.Checked)
            {
                column = FILEID_COLUMN;
            }
            else if (fileOffsetMode.Checked)
            {
                column = OFFSET_COLUMN;
            }
            else if (fileSizeMode.Checked)
            {
                column = FILESIZE_COLUMN;
            }
            else
            {
                column = FILENAME_COLUMN;
            }

            bool found = false;
            int startRow = (treeGridView1.CurrentCell.RowIndex + 1) < treeGridView1.RowCount ? (treeGridView1.CurrentCell.RowIndex + 1) : treeGridView1.CurrentCell.RowIndex;
            for (int i = startRow; i < treeGridView1.RowCount; i++)
            {
                AdvancedDataGridView.TreeGridNode node = treeGridView1.Nodes[i];
                if (OFFSET_COLUMN == column)
                {
                    string findText = searchText.Text.ToLower();
                    DataGridViewComboBoxCell cell = (DataGridViewComboBoxCell)node.Cells[column];
                    for (int j = 0; j < cell.Items.Count; j++)
                    {
                        string itemText = cell.Items[j].ToString().ToLower();
                        if (itemText == findText)
                        {
                            treeGridView1.CurrentCell = cell;
                            found = true;
                            cell.Value = itemText;
                            break;
                        }
                    }
                    if (found)
                    {
                        break;
                    }
                }
                else
                {
                    DataGridViewTextBoxCell cell = (DataGridViewTextBoxCell)node.Cells[column];
                    string cellText = (string)cell.Value;
                    cellText = cellText.ToLower();
                    string findText = searchText.Text.ToLower();
                    if (checkMatchWhole.Checked)
                    {
                        if (cellText == findText)
                        {
                            treeGridView1.CurrentCell = cell;
                            found = true;
                            break;
                        }
                    }
                    else
                    {
                        if (cellText.Contains(findText))
                        {
                            treeGridView1.CurrentCell = cell;
                            found = true;
                            break;
                        }
                    }
                }
            }

            if (!found)
            {
                for (int i = 0; i < startRow; i++)
                {
                    AdvancedDataGridView.TreeGridNode node = treeGridView1.Nodes[i];
                    if (OFFSET_COLUMN == column)
                    {
                        string findText = searchText.Text.ToLower();
                        DataGridViewComboBoxCell cell = (DataGridViewComboBoxCell)node.Cells[column];
                        for (int j = 0; j < cell.Items.Count; j++)
                        {
                            string itemText = cell.Items[j].ToString().ToLower();
                            if (itemText == findText)
                            {
                                treeGridView1.CurrentCell = cell;
                                found = true;
                                cell.Value = itemText;
                                break;
                            }
                        }
                        if (found)
                        {
                            break;
                        }
                    }
                    else
                    {
                        DataGridViewTextBoxCell cell = (DataGridViewTextBoxCell)node.Cells[column];
                        string cellText = (string)cell.Value;
                        cellText = cellText.ToLower();
                        string findText = searchText.Text.ToLower();
                        if (checkMatchWhole.Checked)
                        {
                            if (cellText == findText)
                            {
                                treeGridView1.CurrentCell = cell;
                                found = true;
                                break;
                            }
                        }
                        else
                        {
                            if (cellText.Contains(findText))
                            {
                                treeGridView1.CurrentCell = cell;
                                found = true;
                                break;
                            }
                        }
                    }
                }
            }

            if (!found)
            {
                MessageBox.Show("\"" + searchText.Text.ToLower() + "\" is not found!", "BigfileViewer");
            }
        }

        private void findPrevious_Click(object sender, EventArgs e)
        {
            if (searchText.Text.Length < 1)
            {
                return;
            }

            int column = 3;
            if (fileIDMode.Checked)
            {
                column = 0;
            }
            else if (fileOffsetMode.Checked)
            {
                column = 1;
            }
            else if (fileSizeMode.Checked)
            {
                column = 2;
            }
            else
            {
                column = 3;
            }
            bool found = false;
            int startRow = (treeGridView1.CurrentCell.RowIndex - 1) >= 0 ? (treeGridView1.CurrentCell.RowIndex - 1) : treeGridView1.CurrentCell.RowIndex;
            for (int i = startRow; i >= 0; i--)
            {
                AdvancedDataGridView.TreeGridNode node = treeGridView1.Nodes[i];
                DataGridViewTextBoxCell cell = (DataGridViewTextBoxCell)node.Cells[column];
                string cellText = (string)cell.Value;
                cellText = cellText.ToLower();
                string findText = searchText.Text.ToLower();
                if (checkMatchWhole.Checked)
                {
                    if (cellText == findText)
                    {
                        treeGridView1.CurrentCell = cell;
                        found = true;
                        break;
                    }
                }
                else
                {
                    if (cellText.Contains(findText))
                    {
                        treeGridView1.CurrentCell = cell;
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
                for (int i = treeGridView1.RowCount - 1; i > startRow; i--)
                {
                    AdvancedDataGridView.TreeGridNode node = treeGridView1.Nodes[i];
                    DataGridViewTextBoxCell cell = (DataGridViewTextBoxCell)node.Cells[column];
                    string cellText = (string)cell.Value;
                    cellText = cellText.ToLower();
                    string findText = searchText.Text.ToLower();
                    if (checkMatchWhole.Checked)
                    {
                        if (cellText == findText)
                        {
                            treeGridView1.CurrentCell = cell;
                            found = true;
                            break;
                        }
                    }
                    else
                    {
                        if (cellText.Contains(findText))
                        {
                            treeGridView1.CurrentCell = cell;
                            found = true;
                            break;
                        }
                    }
                }
            }

            if (!found)
            {
                MessageBox.Show("\"" + searchText.Text.ToLower() + "\" is not found!", "GameArchiver");
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}