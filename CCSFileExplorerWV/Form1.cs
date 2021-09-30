using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Be.Windows.Forms;
using System.Collections;
namespace CCSFileExplorerWV
{
    public partial class Form1 : Form
    {
        public CCSFile ccsfile;
        public CCSFile.FileVersionEnum SelectedFileFormat;
        public string lastfolder;
        public List<Block> currPalettes;
        public Block currTexture;
        public Block0800 currModel;
        public float viewRotation = 0;
        public string ccsFileName = "";
        public string folder, infilename;

        public Form1()
        {
            InitializeComponent();
            this.SelectedFileFormat = CCSFile.FileVersionEnum.HACK_GU;
            if (tabControl1.TabPages.Contains(tabPage2))
                tabControl1.TabPages.Remove(tabPage2);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
        }

        private void unpackBINToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.ccs|*.ccs|*.bin|*.bin";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fbd.SelectedPath = lastfolder;
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    lastfolder = fbd.SelectedPath + "\\";
                    string file = d.FileName;
                    this.Enabled = false;
                    BINHelper.UnpackToFolder(d.FileName, lastfolder, pb1, status);
                    this.Enabled = true;
                }
            }
        }

        private void repackBINToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fbd.SelectedPath = lastfolder;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                lastfolder = fbd.SelectedPath + "\\";
                List<string> files = new List<string>(Directory.GetFiles(lastfolder, "*.ccs", SearchOption.TopDirectoryOnly));
                files.Sort();
                SaveFileDialog d = new SaveFileDialog();
                d.Filter = "*.ccs|*.ccs|*.bin|*.bin";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    bool include = MessageBox.Show("Salvar no formato Naruto?", "Como salvar", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes;
                    this.Enabled = false;
                    BINHelper.RepackFromFolder(d.FileName, lastfolder, pb1, status);
                    this.Enabled = true;
                }
            }
        }

        private void openCCSFileToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.tmp|*.tmp|*.ccs|*.ccs";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ccsfile = new CCSFile(File.ReadAllBytes(d.FileName), SelectedFileFormat);
                ccsFileName = d.SafeFileName.Remove(d.SafeFileName.Length - 4,4);
                AddRecent(d.FileName);
                RefreshStuff();
            }
        }

        private void saveCCSFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ccsfile == null)
                return;
            if (!ccsfile.isvalid)
            {
                MessageBox.Show("Você está tentando salvar esse arquivo como inválido.");
                return;
            }
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.tmp|*.tmp";
            d.FileName = ccsfile.header.Name + ".tmp";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ccsfile.Save(d.FileName);
                RefreshStuff();
                MessageBox.Show("Feito.");
            }
        }

        private void exportAsBitmapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode sel = treeView1.SelectedNode;
            if (ccsfile == null || !ccsfile.isvalid || sel == null || pic1 == null)
                return;
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.png|*.png|*.bmp|*.bmp|*.jpg|*.jpg";
            d.FileName = Path.GetFileNameWithoutExtension(sel.Text);
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                switch (Path.GetExtension(d.FileName).ToLower())
                {
                    case ".png":
					    pic1.Image.Save(d.FileName, System.Drawing.Imaging.ImageFormat.Png);
						break;
					case ".jpg":
                        pic1.Image.Save(d.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                    case ".bmp":
                        pic1.Image.Save(d.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                    default:
                        MessageBox.Show("Unknown Format Extension!");
                        return;
                }
                MessageBox.Show("Feito.");
            }
        }

        private void exportRawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode sel = treeView1.SelectedNode;
            if (ccsfile == null || !ccsfile.isvalid || sel == null)
                return;
            if (sel.Level == 3)
            {
                TreeNode obj = sel.Parent;
                TreeNode file = obj.Parent;
                FileEntry entryf = ccsfile.files[file.Index];
                ObjectEntry entryo = entryf.objects[obj.Index];
                SaveFileDialog d = new SaveFileDialog();
                d.Filter = "*.bin|*.bin";
                d.FileName = entryo.name + ".bin";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllBytes(d.FileName, entryo.blocks[sel.Index].Data);
                }
            }
        }

        private void RefreshStuff()
        {
            rtb1.Text = ccsfile.Info();
            treeView1.Nodes.Clear();
            if (!ccsfile.isvalid)
                return;
            TreeNode t = new TreeNode(ccsfile.header.Name);
            foreach (FileEntry entry in ccsfile.files)
                t.Nodes.Add(entry.ToNode());
            t.Expand();
            treeView1.Nodes.Add(t);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            
            if (tabControl1.TabPages.Contains(tabPage2))
                tabControl1.TabPages.Remove(tabPage2);
            if (tabControl1.TabPages.Contains(tabPage3))
                tabControl1.TabPages.Remove(tabPage3);
            hb1.ByteProvider = new DynamicByteProvider(new byte[0]);
            timer1.Enabled = false;
            pic1.Image = null;
            pic2.Image = null;
            if (ccsfile == null || !ccsfile.isvalid)
                return;
            TreeNode sel = e.Node;
            if (sel.Level == 1)
                arquivoSelecionadoToolStripMenuItem.Enabled = true;
            else
                arquivoSelecionadoToolStripMenuItem.Enabled = false;
            if (sel.Level == 3)
            {
                TreeNode obj = sel.Parent;
                TreeNode file = obj.Parent;
                FileEntry entryf = ccsfile.files[file.Index];
                ObjectEntry entryo = entryf.objects[obj.Index];
                hb1.ByteProvider = new DynamicByteProvider(entryo.blocks[sel.Index].FullBlockData);
                if (entryo.blocks[sel.Index].BlockID == 0xCCCC0800)
                {
                    currModel = (Block0800)entryo.blocks[sel.Index];
                    currModel.ProcessData();
                    if (!SceneHelper.init)
                        SceneHelper.InitializeDevice(pic2);
                    comboBox2.Items.Clear();
                    for (int i = 0; i < currModel.models.Count; i++)
                        comboBox2.Items.Add("Model " + (i + 1));
                    if (comboBox2.Items.Count > 0)
                        comboBox2.SelectedIndex = 0;
                    timer1.Enabled = true;
                    tabControl1.TabPages.Add(tabPage3);
                    tabControl1.SelectedTab = tabPage3;
                }
            }
            if (sel.Level == 1)
            {
                string ext = Path.GetExtension(sel.Text).ToLower();
                switch (ext)
                {
                    case ".bmp":
                        comboBox1.Items.Clear();
                        currPalettes = new List<Block>();
                        currTexture = null;
                        foreach (ObjectEntry obj in ccsfile.files[sel.Index].objects)
                            foreach (Block b in obj.blocks)
                            {
                                if (b.BlockID == 0xCCCC0400)
                                {
                                    comboBox1.Items.Add(obj.name);
                                    currPalettes.Add(b);
                                }
                                if (b.BlockID == 0xCCCC0300)
                                    currTexture = b;
                            }
                        if (comboBox1.Items.Count > 0)
                        {
                            tabControl1.TabPages.Add(tabPage2);
                            comboBox1.SelectedIndex = 0;
                            tabControl1.SelectedTab = tabPage2;
                        }
                        break;
                }
            }
            treeView1.Focus();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = comboBox1.SelectedIndex;
            if (n == -1 || currTexture == null || currPalettes == null || currPalettes.Count == 0)
                return;
            pic1.Image = CCSFile.CreateImage(currPalettes[n].Data, currTexture.Data);
        }

        private void importRawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode sel = treeView1.SelectedNode;
            if (ccsfile == null || !ccsfile.isvalid || sel == null)
                return;
            if (sel.Level == 3)
            {
                TreeNode obj = sel.Parent;
                TreeNode file = obj.Parent;
                FileEntry entryf = ccsfile.files[file.Index];
                ObjectEntry entryo = entryf.objects[obj.Index];
                OpenFileDialog d = new OpenFileDialog();
                d.Filter = "*.bin|*.bin";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    entryo.blocks[sel.Index].Data = File.ReadAllBytes(d.FileName);
                    ccsfile.Rebuild();
                    ccsfile.Reload();
                    RefreshStuff();
                }
            }
        }

        private void openInImageImporterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ccsfile == null || !ccsfile.isvalid || treeView1.SelectedNode == null)
                return;
            TreeNode sel = treeView1.SelectedNode;
            if (sel.Level == 1)
            {
                string ext = Path.GetExtension(sel.Text).ToLower();
                if (ext != ".bmp")
                {
                    MessageBox.Show("Not a bmp file!");
                    return;
                }
                ImageImporter f = new ImageImporter();
                f.index = sel.Index;
                f.ccsfile = ccsfile;
                f.ShowDialog();
                if (f.exitok)
                {
                    ccsfile = f.ccsfile;
                    RefreshStuff();
                }
            }
        }

        private void exportToObjToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ccsfile == null || !ccsfile.isvalid || treeView1.SelectedNode == null)
                return;
            TreeNode sel = treeView1.SelectedNode;
            if (sel.Level == 3)
            {
                TreeNode obj = sel.Parent;
                TreeNode file = obj.Parent;
                FileEntry entryf = ccsfile.files[file.Index];
                ObjectEntry entryo = entryf.objects[obj.Index];
                hb1.ByteProvider = new DynamicByteProvider(entryo.blocks[sel.Index].FullBlockData);
                if (entryo.blocks[sel.Index].BlockID == 0xCCCC0800)
                {
                    Block0800 mdl = (Block0800)entryo.blocks[sel.Index];
                    mdl.ProcessData();
                    SaveFileDialog d = new SaveFileDialog();
                    d.Filter = "*.obj|*.obj";
                    d.FileName = obj.Text + ".obj";
                    if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string input = Microsoft.VisualBasic.Interaction.InputBox("Which Model to export? (1 - " + (mdl.models.Count) + "). Input 0 to export all.", "Export Model", (comboBox2.SelectedIndex + 1).ToString());
                        if (input != "")
                        {
                            mdl.SaveModel(Convert.ToInt32(input) - 1, d.FileName);
                            MessageBox.Show("Feito.");
                        }
                    }
                }
            }
            else if (sel.Level == 1)
            {
                TreeNode obj = sel;
                TreeNode file = obj;
                FileEntry entryf = ccsfile.files[file.Index];
                ObjectEntry entryo = entryf.objects[obj.Index];
                hb1.ByteProvider = new DynamicByteProvider(entryo.blocks[sel.Index].FullBlockData);
                FolderBrowserDialog d = new FolderBrowserDialog();
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    int k = 0;
                    foreach (var block in entryo.blocks)
                    {
                        if (block.BlockID == 0xCCCC0800)
                        {
                            Block0800 mdl = (Block0800)block;
                            mdl.ProcessData();
                            string input = Microsoft.VisualBasic.Interaction.InputBox("Which Model to export? (1 - " + (mdl.models.Count) + "). Input 0 to export all.", "Export Model", (comboBox2.SelectedIndex + 1).ToString());
                            if (input != "")
                            {
                                mdl.SaveModel(Convert.ToInt32(input) - 1, d.SelectedPath + @"\" + ccsfile.files[k].name);
                                MessageBox.Show("Feito.");
                            }

                        }
                        k++;
                    }
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            SceneHelper.Render();
            if (SceneHelper.doRotate)
            {
                viewRotation += 1f;
                SceneHelper.SetRotation360(viewRotation);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = comboBox2.SelectedIndex;
            if (n == -1 || currModel == null || !SceneHelper.init)
                return;
            currModel.CopyToScene(n);
        }

        private void pic2_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                float p = e.Y / (float)pic2.Height - 0.5f;
                SceneHelper.SetHeight(p);
                SceneHelper.doRotate = false;
                autoRotationOnToolStripMenuItem.Checked = false;
                p = e.X / (float)pic2.Width;
                SceneHelper.SetRotation360(p * 360f);
            }
            if (e.Button == MouseButtons.Right)
            {
                float p = e.Y / (float)pic2.Height + 0.001f;
                p *= 3;
                SceneHelper.SetZoomFactor(p);
            }
        }

        private void pic2_Resize(object sender, EventArgs e)
        {
            try { SceneHelper.Resize(); } catch (Exception) { }
        }

        private void autoRotationOnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SceneHelper.doRotate = autoRotationOnToolStripMenuItem.Checked;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckRecent();
        }

        public void CheckRecent()
        {
            recentToolStripMenuItem.Enabled = false;
            if (recentToolStripMenuItem.HasDropDownItems)
                recentToolStripMenuItem.DropDownItems.Clear();
            if (File.Exists("recent.txt"))
            {
                string[] lines = File.ReadAllLines("recent.txt");
                foreach (string line in lines)
                    if (line.Trim() != "")
                    {
                        ToolStripMenuItem item = new ToolStripMenuItem(line.Trim());
                        item.Click += recentClick;
                        recentToolStripMenuItem.DropDownItems.Add(item);
                        recentToolStripMenuItem.Enabled = true;
                    }
            }
        }

        public void recentClick(object sender, EventArgs e)
        {
            if ((sender as ToolStripMenuItem).Text.EndsWith(".CCS")||((sender as ToolStripMenuItem).Text.EndsWith(".ccs")))
            {
                ccsfile = new CCSFile(FileHelper.unzipArray(File.ReadAllBytes((sender as ToolStripMenuItem).Text)), SelectedFileFormat);
            }
            else
            {
                ccsfile = new CCSFile(File.ReadAllBytes(((ToolStripMenuItem)sender).Text), this.SelectedFileFormat);

            }
            RefreshStuff();
        }

        public void AddRecent(string path)
        {
            if (!File.Exists("recent.txt"))
                File.WriteAllLines("recent.txt", new string[0]);
            string[] lines = File.ReadAllLines("recent.txt");
            List<string> result = new List<string>();
            result.Add(path);
            int index = 0;
            while (result.Count < 10 && index < lines.Length && lines[index] != path)
                result.Add(lines[index++]);
            File.WriteAllLines("recent.txt", result.ToArray());
            CheckRecent();
        }

        private void wireframeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SceneHelper.wireframe = wireframeToolStripMenuItem.Checked;
        }

        private void hackGUToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hackGUToolStripMenuItem.Checked = true;
            this.SelectedFileFormat = CCSFile.FileVersionEnum.HACK_GU;
            if (ccsfile != null)
                ccsfile.FileVersion = SelectedFileFormat;
        }
        public void ImportAllBinary()
        {
            FolderBrowserDialog fd = new FolderBrowserDialog();
            fd.Description = "Escolha onde estão os arquivos para Importar no Principal\n\nA pasta escolhida deve ser a raiz do arquivo principal com mais pastas, ex: title_s -> title_s/m, title_s/#e:";
            if (fd.ShowDialog() == DialogResult.OK)
            {
                string raiz = fd.SelectedPath;
                foreach(var file in ccsfile.files)
                {
                    string filepath = raiz + @"\" + file.name;
                    if (Directory.Exists(filepath))
                    {
                        foreach (var obj in file.objects)
                        {
                            string objpath = filepath + @"\" + obj.name;
                            foreach (var block in obj.blocks)
                            {
                                string blockst = objpath + @"\" + block.ID.ToString() +@"\"+block.BlockID.ToString("X2") + ".bin";
                                if (File.Exists(blockst))
                                {
                                    byte[] newdata = File.ReadAllBytes(blockst);
                                    block.Data = new byte[newdata.Length];
                                    block.Size = (uint)(newdata.Length /4) +1;
                                    block.Data = newdata;
                                }
                            }
                        }
                        RefreshStuff();
                    }
                }
                MessageBox.Show("Concluído!!\nNão se esqueça de salvar o arquivo modificado!");
            }
        }
        public void ExtractAllBinary()
        {
            FolderBrowserDialog fd = new FolderBrowserDialog();
            fd.Description = "Escolha onde salvar os blocos do arquivo selecionado:\nEu criarei uma pasta com o nome do arquivo para você, não se preocupe!!";
            if(fd.ShowDialog()==DialogResult.OK)
            {
                string ccsname = ccsfile.header.Name;
                if (!Directory.Exists(fd.SelectedPath+@"\"+ ccsname))
                    Directory.CreateDirectory(fd.SelectedPath + @"\" + ccsname);
                int index = treeView1.SelectedNode.Index;
                string name = ccsfile.files[index].name;
                string path = fd.SelectedPath + @"\" + ccsname + @"\" + name;
                if(!Directory.Exists(path))
                  Directory.CreateDirectory(path);
                //---------------------------------//
                FileEntry entry = ccsfile.files[index];
                foreach(var obj in entry.objects)
                {
                    string objpath = path + @"\" + obj.name;
                    if (!Directory.Exists(objpath))
                        Directory.CreateDirectory(objpath);
                    foreach(var block in obj.blocks)
                    {
                        string blockidpath = objpath + @"\" + block.ID.ToString();
                        if (!Directory.Exists(blockidpath))
                            Directory.CreateDirectory(blockidpath);
                        string blockpath = blockidpath + @"\" + block.BlockID.ToString("X2") + ".bin";
                        File.WriteAllBytes(blockpath, block.Data);
                    }
                }
                MessageBox.Show("Concluído!");
            }
        }
        public void ExtractAllFileBinary()
        {
            FolderBrowserDialog fd = new FolderBrowserDialog();
            fd.Description = "Escolha onde salvar os blocos do arquivo selecionado:\nEu criarei uma pasta com o nome do arquivo para você, não se preocupe!!";
            if (fd.ShowDialog() == DialogResult.OK)
            {
                string ccsname = ccsfile.header.Name;
                if (!Directory.Exists(fd.SelectedPath + @"\" + ccsname))
                    Directory.CreateDirectory(fd.SelectedPath + @"\" + ccsname);
                foreach (var file in ccsfile.files)
                {
                    string name = file.name;
                    string path = fd.SelectedPath + @"\" + ccsname+@"\"+name;
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    //---------------------------------//
                    foreach (var obj in file.objects)
                    {
                        string objpath = path + @"\" + obj.name;
                        if (!Directory.Exists(objpath))
                            Directory.CreateDirectory(objpath);
                        foreach (var block in obj.blocks)
                        {
                            string blockidpath = objpath + @"\" + block.ID.ToString();
                            if (!Directory.Exists(blockidpath))
                                Directory.CreateDirectory(blockidpath);
                            string blockpath = blockidpath + @"\" + block.BlockID.ToString("X2") + ".bin";
                            File.WriteAllBytes(blockpath, block.Data);
                        }
                    }
                }
                MessageBox.Show("Concluído!");
            }
        }
        //2017-05-09 #1UP
        //Added in functionality to extract all models at once.
        private void extractAllModelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //FileName Path structure
            //GameName/Models/CCSFileName/ModelName/
            string gameName = "";

            if (hackGUToolStripMenuItem.Checked) { gameName = "Naruto"; }

            //Loops through the Main parent node (there should really only be one of these).
            foreach (TreeNode mainNode in treeView1.Nodes)
            {
                //This loops through all the child nodes of the main node. ex. s\r\anim\
                foreach(TreeNode fileNode in mainNode.Nodes)
                {
                    //We want to skip over any fileNode that contains anim since those are animations (may come back to this later for a different extraction method)
                    //The texture files are in the files that end in .max however the anim files also have this extention so we must check for that.
                    if(!fileNode.Text.Contains("anim"))
                    {
                        if(fileNode.Text.Contains(".max"))
                        {
                            //We want to go through all the nodes and check for files that contain MDL since those are our model files.
                            //We need to make sure that the modelFileName also has a child node because there are MDLs without children. These are apart of a list for
                            //a model. These can be skipped over.
                            foreach (TreeNode modelFileNode in fileNode.Nodes)
                            {
                                if(modelFileNode.Text.Contains("MDL") && modelFileNode.Nodes.Count != 0)
                                {
                                    //This is where we are going to start exporting our files.
                                    //We now have the ModelFileName and now we just have to get the child  Node
                                    //We also need to update the dropdown so it grabs all the models as a list. This is currently in a massive chunk of code that needs
                                    //to be broken up.
                                    FileEntry entryf = ccsfile.files[fileNode.Index];
                                    ObjectEntry entryo = entryf.objects[modelFileNode.Index];
                                    hb1.ByteProvider = new DynamicByteProvider(entryo.blocks[modelFileNode.FirstNode.Index].FullBlockData);
                                    if (entryo.blocks[modelFileNode.FirstNode.Index].BlockID == 0xCCCC0800)
                                    {
                                        //Create our directories
                                        Directory.CreateDirectory("./" + gameName + "/Models/" + ccsFileName + "/" + modelFileNode.Text);
                                        currModel = (Block0800)entryo.blocks[modelFileNode.FirstNode.Index];
                                        currModel.ProcessData();
                                    
                                        for (int modelIndex = 1; modelIndex <= currModel.models.Count; modelIndex++)
                                        {
                                            string filePath = "./" + gameName + "/Models/" + ccsFileName + "/" + modelFileNode.Text + "/" + modelFileNode.Text + "-" + modelIndex + ".obj";
                                            currModel.SaveModel(Convert.ToInt32(modelIndex) - 1, filePath);
                                        }
                                    }

                                }
                            }
                        }
                    }
                }

            }
        }
        //2017-05-09 #1UP
        //Added in functionality to extract all textures at once.
        private void extractAllTexturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //FileName Path structure
            //GameName/Textures/CCSFileName/ModelName/
            string gameName = "";
            string[] splitTextureName;
            string textureName = "";
            PictureBox pct_Temp = new PictureBox();
            if (hackGUToolStripMenuItem.Checked) { gameName = "Naruto"; }
            //Loops through the Main parent node (there should really only be one of these).
            foreach (TreeNode mainNode in treeView1.Nodes)
            {
                //This loops through all the child nodes of the main node. ex. s\r\anim\
                foreach (TreeNode fileNode in mainNode.Nodes)
                {
                   
                    if (fileNode.Text.Contains("tex"))
                    {
                        currPalettes = new List<Block>();
                        currTexture = null;
                        foreach (ObjectEntry obj in ccsfile.files[fileNode.Index].objects)
                            foreach (Block b in obj.blocks)
                            {
                                if (b.BlockID == 0xCCCC0400)
                                {
                                    currPalettes.Add(b);
                                }
                                if (b.BlockID == 0xCCCC0300)
                                    currTexture = b;
                            }
                        
                        if (currPalettes.Count > 0)
                        {
                            for (int textureIndex = 0; textureIndex < currPalettes.Count; textureIndex++)
                            {
                                //We need to get th name of the texture from the node (the node has \'s in it that we need to split on
                                splitTextureName = fileNode.Text.ToString().Split('\\');

                                //The texture name is the last one in the array
                                textureName = splitTextureName[splitTextureName.Length - 1];

                                //We now want to remove the extention
                                textureName = textureName.Remove(textureName.Length - 4, 3);

                                Directory.CreateDirectory("./" + gameName + "/Textures/" + ccsFileName + "/" + textureName);
                                string filePath = "./" + gameName + "/Textures/" + ccsFileName + "/" + textureName + "/" +  textureName + "-"+ textureIndex + ".png";
                                pct_Temp.Image = CCSFile.CreateImage(currPalettes[textureIndex].Data, currTexture.Data);
                           
                                pct_Temp.Image.Save(@filePath, System.Drawing.Imaging.ImageFormat.Png);
                        
                            }
                        }
                    
                    }

                }

            }
        }


        private void sairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void fecharToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ccsfile.Reload();
            hb1.Update();
            treeView1.Nodes.Clear();
            rtb1.Clear();
        }

        private void salvarECompactarEmCCSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ccsfile == null)
                return;
            if (!ccsfile.isvalid)
            {
                MessageBox.Show("Você está tentando salvar esse arquivo como inválido.");
                return;
            }
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.tmp|*.tmp";
            d.FileName = ccsfile.header.Name + ".tmp";
            if (d.ShowDialog() == DialogResult.OK)
            {
                FileInfo f = new FileInfo(d.FileName);
                lastfolder = f.Directory.ToString() + @"\";
                ccsfile.Save(d.FileName);
                FileInfo k = new FileInfo(d.FileName);
                BINHelper.Repack(k.FullName + "2", lastfolder, k.Name);
                BINHelper.MoveWithReplace(k.FullName + "2", k.FullName.Remove(k.FullName.Length - 4) + ".ccs");
                RefreshStuff();
                MessageBox.Show("Feito.");
                
            }
        }

        private void scrib(object sender, EventArgs e)
        {

        }

        private void arquivoSelecionadoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int c = treeView1.SelectedNode.Index;
            SaveFileDialog sv = new SaveFileDialog();
            sv.FileName = Path.GetFileName(ccsfile.files[c].name);
            List<byte> tosv = new List<byte>();
            if (sv.ShowDialog() == DialogResult.OK)        
            {
                foreach (var obje in ccsfile.files[c].objects)
                    foreach (var blocs in obje.blocks)
                    {
                        tosv.AddRange(blocs.Data);
                        
                    }
                File.WriteAllBytes(sv.FileName, tosv.ToArray());
                MessageBox.Show("Feito!", "Sucesso", MessageBoxButtons.OK);
            }
        }

        private void extrairTodosOsBlocosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExtractAllBinary();
        }

        private void extrairArquivosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExtractAllFileBinary();
        }

        private void importarArquivosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportAllBinary();
        }

        private void importarTodosOsBlocosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportAllBinary();
        }

        private void pesquisarToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void abrirCCSPACKEDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.ccs|*.ccs";
            if (d.ShowDialog() == DialogResult.OK)
            {
                ccsfile = new CCSFile(FileHelper.unzipArray(File.ReadAllBytes(d.FileName)), SelectedFileFormat);
                ccsFileName = d.SafeFileName.Remove(d.SafeFileName.Length - 4, 4);
                AddRecent(d.FileName);
                RefreshStuff();
            }
        }
    }
}
