using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;
using GameCore;
using DataBuildSystem;

namespace BigfileFileReorder
{
    public partial class MainForm : Form
    {
		public MainForm(CommandLine cmdLine)
        {
            InitializeComponent();
			if (!Config.init(cmdLine["name"], cmdLine["srcpath"], cmdLine["dstpath"], cmdLine["toolpath"], cmdLine["publishpath"], cmdLine["platform"]))
			{
				Console.WriteLine("Usage: -name [NAME] -srcpath [SRCPATH] -dstpath [DSTPATH] -toolpath [TOOLPATH] -publishpath [PUBLISHPATH] -platform [PLATFORM]");
				Console.WriteLine("Press a key");
			}
        }

        private void OnView(object sender, EventArgs e)
        {
            LayoutForm layout = new LayoutForm();
            layout.Visible = true;
        }

		private void OnBrowseBigFile(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Big file(*.gda)|*.gda";
			dialog.FilterIndex = 0;
			dialog.RestoreDirectory = true;

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				BigfileFilenameTB.Text = dialog.FileName;
			}

		}

		private void OnBrowseTocFile(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Toc file(*.gdt)|*.gdt";
			dialog.FilterIndex = 0;
			dialog.RestoreDirectory = true;

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				MFTFilenameTB.Text = dialog.FileName;
			}
		}

		private void OnBrowseOrderFile(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Order file(*.rec)|*.rec";
			dialog.FilterIndex = 0;
			dialog.RestoreDirectory = true;

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				ReorderFilenameTB.Text = dialog.FileName;
			}
		}

		private void updateState()
		{
			if ( BigfileFilenameTB.Text.Length > 0 && MFTFilenameTB.Text.Length > 0 && ReorderFilenameTB.Text.Length > 0 )
			{
				SaveButton.Enabled = true;
			}
			else
			{
				SaveButton.Enabled = false;
			}
		}

		private void OnBigFileChanged(object sender, EventArgs e)
		{
			updateState();
		}

		private void OnTocFileChanged(object sender, EventArgs e)
		{
			updateState();
		}

		private void OnOrderFileChanged(object sender, EventArgs e)
		{
			updateState();
		}

		private void OnSaveButtonClicked(object sender, EventArgs e)
		{
			SaveButton.Enabled = false;

			// Construct an assembly with the config object
            List<Filename> assemblyReferences = new List<Filename>();
			assemblyReferences.Add(new Filename("System.dll"));
            List<Filename> filenames = new List<Filename>();
			if (PlatformPCRadio.Checked)
			{
				filenames.Add(Config.SrcPath + new Filename(String.Format("Config.BigfileBuilder.{0}.cs", "PC")));
			}
			else
			{
				filenames.Add(Config.SrcPath + new Filename(String.Format("Config.BigfileBuilder.{0}.cs", "PS2")));
			}

            Assembly configAssembly = AssemblyUtil.BuildAssemblyDirectly(Config.SrcPath + new Filename(Config.Name + "config.dll"), filenames, assemblyReferences);

			IBigfileConfig bigFileConfig = new BigfileDefaultConfig();
			if (configAssembly != null)
				bigFileConfig = AssemblyUtil.Create1<IBigfileConfig>(configAssembly);
            if (bigFileConfig!=null)
                BigfileConfig.Init(bigFileConfig);

			BigfileFileOrder order = new BigfileFileOrder();
			order.Reorder(BigfileFilenameTB.Text, MFTFilenameTB.Text, ReorderFilenameTB.Text, PlatformPCRadio.Checked ? "PC" : "PS2");
			SaveButton.Enabled = true;
		}
    }
}
