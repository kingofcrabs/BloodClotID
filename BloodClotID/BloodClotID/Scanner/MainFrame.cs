using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using TwainLib;
using System.IO;
using System.Runtime.InteropServices;
using GdiPlusLib;
using System.Configuration;
using BloodClotID;
using BloodClotID.Properties;
using EngineDll;

namespace TwainGui
{
public class MainFrame : System.Windows.Forms.Form, IMessageFilter
	{

	    private System.Windows.Forms.MdiClient mdiClient1;
	    private System.Windows.Forms.MenuItem menuMainFile;
	    private System.Windows.Forms.MenuItem menuItemScan;
        private System.Windows.Forms.MenuItem menuItemSelSrc;
	    private System.Windows.Forms.MenuItem menuItemExit;
	    private System.Windows.Forms.MenuItem menuItemSepr;
        private System.Windows.Forms.MainMenu mainFrameMenu;
        private ToolStrip toolStrip1;
        private ToolStripButton btnScan;
        private MenuItem menuItemHelp;
        private MenuItem menuItemAbout;
        private MenuItem menuItemResult;
        private ToolStripButton btnExit;
        private ToolStripButton btnSetting;
        private PictureBox pic1;
        private BloodClotID.Scanner.ListViewEx lstViewResult;
        private ColumnHeader columnNum;
        private ColumnHeader columnPos;
        private Label lblResult;
        private ColumnHeader columnValue;
        private Label label1;
        private TextBox textBox1;
        private IContainer components;
	    public MainFrame()
		{
            Application.EnableVisualStyles();
		    InitializeComponent();
            //Random r = new Random();
            //for (int i = 0; i < 5; i++ )
            //{
            //    var lstItem = new System.Windows.Forms.ListViewItem(string.Format("item{0}",i+1));
            //    lstViewResult.Items.Add(lstItem);

            //}
            //foreach (ListViewItem i in lstViewResult.Items)
            //{
            //    int cnt = r.Next(100);
            //    i.SubItems.Add(cnt.ToString());
            //    ProgressBar pb = new ProgressBar();
            //    pb.Value = cnt;
            //    // Embed the ProgressBar in Column 2
            //    lstViewResult.AddEmbeddedControl(pb, 1, i.Index);
            //    Label lbl = new Label();
            //    lbl.Text = cnt.ToString();
            //    lstViewResult.AddEmbeddedControl(lbl, 2, i.Index);
            //}
		    tw = new Twain();
		    tw.Init( this.Handle );
         
		}

	    protected override void Dispose( bool disposing )
		    {
		    if( disposing )
			    {
			    tw.Finish();
			    if (components != null) 
				    {
				    components.Dispose();
				    }
			    }
		    base.Dispose( disposing );
		    }

		                                                                                                                                                                                                                                                                                                                                                                                                                                                            #region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.menuMainFile = new System.Windows.Forms.MenuItem();
            this.menuItemSelSrc = new System.Windows.Forms.MenuItem();
            this.menuItemScan = new System.Windows.Forms.MenuItem();
            this.menuItemResult = new System.Windows.Forms.MenuItem();
            this.menuItemSepr = new System.Windows.Forms.MenuItem();
            this.menuItemExit = new System.Windows.Forms.MenuItem();
            this.mainFrameMenu = new System.Windows.Forms.MainMenu(this.components);
            this.menuItemHelp = new System.Windows.Forms.MenuItem();
            this.menuItemAbout = new System.Windows.Forms.MenuItem();
            this.mdiClient1 = new System.Windows.Forms.MdiClient();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnScan = new System.Windows.Forms.ToolStripButton();
            this.btnSetting = new System.Windows.Forms.ToolStripButton();
            this.btnExit = new System.Windows.Forms.ToolStripButton();
            this.pic1 = new System.Windows.Forms.PictureBox();
            this.lblResult = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.lstViewResult = new BloodClotID.Scanner.ListViewEx();
            this.columnNum = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnPos = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pic1)).BeginInit();
            this.SuspendLayout();
            // 
            // menuMainFile
            // 
            this.menuMainFile.Index = 0;
            this.menuMainFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemSelSrc,
            this.menuItemScan,
            this.menuItemResult,
            this.menuItemSepr,
            this.menuItemExit});
            this.menuMainFile.MergeType = System.Windows.Forms.MenuMerge.MergeItems;
            this.menuMainFile.Text = "文件";
            // 
            // menuItemSelSrc
            // 
            this.menuItemSelSrc.Index = 0;
            this.menuItemSelSrc.MergeOrder = 11;
            this.menuItemSelSrc.Text = "选择设备";
            this.menuItemSelSrc.Click += new System.EventHandler(this.menuItemSelSrc_Click);
            // 
            // menuItemScan
            // 
            this.menuItemScan.Index = 1;
            this.menuItemScan.MergeOrder = 12;
            this.menuItemScan.Text = "扫描";
            this.menuItemScan.Click += new System.EventHandler(this.menuItemScan_Click);
            // 
            // menuItemResult
            // 
            this.menuItemResult.Index = 2;
            this.menuItemResult.Text = "";
            // 
            // menuItemSepr
            // 
            this.menuItemSepr.Index = 3;
            this.menuItemSepr.MergeOrder = 19;
            this.menuItemSepr.Text = "-";
            // 
            // menuItemExit
            // 
            this.menuItemExit.Index = 4;
            this.menuItemExit.MergeOrder = 21;
            this.menuItemExit.Text = "退出";
            this.menuItemExit.Click += new System.EventHandler(this.menuItemExit_Click);
            // 
            // mainFrameMenu
            // 
            this.mainFrameMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuMainFile,
            this.menuItemHelp});
            // 
            // menuItemHelp
            // 
            this.menuItemHelp.Index = 1;
            this.menuItemHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemAbout});
            this.menuItemHelp.Text = "帮助";
            // 
            // menuItemAbout
            // 
            this.menuItemAbout.Index = 0;
            this.menuItemAbout.Text = "关于";
            this.menuItemAbout.Click += new System.EventHandler(this.menuItemAbout_Click);
            // 
            // mdiClient1
            // 
            this.mdiClient1.BackColor = System.Drawing.Color.White;
            this.mdiClient1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mdiClient1.Location = new System.Drawing.Point(0, 39);
            this.mdiClient1.Name = "mdiClient1";
            this.mdiClient1.Size = new System.Drawing.Size(974, 388);
            this.mdiClient1.TabIndex = 0;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnScan,
            this.btnSetting,
            this.btnExit});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(974, 39);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnScan
            // 
            this.btnScan.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnScan.Image = global::BloodClotID.Properties.Resources.barcode;
            this.btnScan.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(36, 36);
            this.btnScan.Text = "Start Scan";
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // btnSetting
            // 
            this.btnSetting.Image = global::BloodClotID.Properties.Resources.setting;
            this.btnSetting.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSetting.Name = "btnSetting";
            this.btnSetting.Size = new System.Drawing.Size(36, 36);
            this.btnSetting.Click += new System.EventHandler(this.btnSetting_Click);
            // 
            // btnExit
            // 
            this.btnExit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnExit.Image = global::BloodClotID.Properties.Resources.exit;
            this.btnExit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(36, 36);
            this.btnExit.Text = "Exit";
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // pic1
            // 
            this.pic1.BackColor = System.Drawing.Color.White;
            this.pic1.Location = new System.Drawing.Point(0, 42);
            this.pic1.Name = "pic1";
            this.pic1.Size = new System.Drawing.Size(720, 480);
            this.pic1.TabIndex = 2;
            this.pic1.TabStop = false;
            // 
            // lblResult
            // 
            this.lblResult.AutoSize = true;
            this.lblResult.BackColor = System.Drawing.Color.White;
            this.lblResult.Location = new System.Drawing.Point(726, 44);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(65, 12);
            this.lblResult.TabIndex = 4;
            this.lblResult.Text = "分析结果：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(728, 290);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "信息：";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(727, 306);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(240, 214);
            this.textBox1.TabIndex = 6;
            // 
            // lstViewResult
            // 
            this.lstViewResult.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnNum,
            this.columnPos,
            this.columnValue});
            this.lstViewResult.HideSelection = false;
            this.lstViewResult.Location = new System.Drawing.Point(726, 63);
            this.lstViewResult.Name = "lstViewResult";
            this.lstViewResult.Size = new System.Drawing.Size(240, 220);
            this.lstViewResult.TabIndex = 3;
            this.lstViewResult.UseCompatibleStateImageBehavior = false;
            this.lstViewResult.View = System.Windows.Forms.View.Details;
            // 
            // columnNum
            // 
            this.columnNum.Text = "行号";
            this.columnNum.Width = 53;
            // 
            // columnPos
            // 
            this.columnPos.Text = "位置";
            this.columnPos.Width = 121;
            // 
            // columnValue
            // 
            this.columnValue.Text = "值";
            // 
            // MainFrame
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(974, 427);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.lstViewResult);
            this.Controls.Add(this.pic1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.mdiClient1);
            this.IsMdiContainer = true;
            this.Menu = this.mainFrameMenu;
            this.Name = "MainFrame";
            this.Text = "血凝分析";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pic1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                OnHelp();
                return true;    // indicate that you handled this keystroke
            }

            // Call the base class
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void OnHelp()
        {
            AboutForm helpForm = new AboutForm();
            helpForm.ShowDialog();
        }
        private void menuItemAbout_Click(object sender, EventArgs e)
        {
            OnHelp();
        }

	    private void menuItemExit_Click(object sender, System.EventArgs e)
		{
		    Close();
	    }
        private void Scan()
        {
            try
            {
                string latestImage = FolderHelper.GetOutputFolder() + Resources.latestImage;
                if (File.Exists(latestImage))
                    File.Delete(latestImage);

                if (!msgfilter)
                {
                    this.Enabled = false;
                    msgfilter = true;
                    Application.AddMessageFilter(this);
                }
                bool bok = tw.Acquire();
                if (!bok)
                    EndingScan();
            }
            catch(Exception ex)
            {
                EndingScan();
            }
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            Scan();
        }

	    private void menuItemScan_Click(object sender, System.EventArgs e)
		{
            Scan();   
		}

	    private void menuItemSelSrc_Click(object sender, System.EventArgs e)
		{
		    tw.Select();
		}


	    bool IMessageFilter.PreFilterMessage( ref Message m )
		{
		    TwainCommand cmd = tw.PassMessage( ref m );
		    if( cmd == TwainCommand.Not )
			    return false;

		    switch( cmd )
			{
			    case TwainCommand.CloseRequest:
				{
				    EndingScan();
				    tw.CloseSrc();
				    break;
				}
			    case TwainCommand.CloseOk:
				{
				    EndingScan();
				    tw.CloseSrc();
				    break;
				}
			    case TwainCommand.DeviceEvent:
				        break;
			    case TwainCommand.TransferReady:
				{
				    ArrayList pics = tw.TransferPictures();
				    EndingScan();
				    tw.CloseSrc();
				    picnumber++;
                    IntPtr dibhandp = (IntPtr)pics[0];
                    dibhand = dibhandp;
                    bmpptr = GlobalLock(dibhand);
                    pixptr = GetPixelInfo(bmpptr);
                    string sFile = FolderHelper.GetOutputFolder() + Resources.latestImage;
                    Gdip.SaveDIBAs(sFile,bmpptr, pixptr);
                    AnalysisImage(sFile);
                    GlobalFree(dibhand);
                    dibhand = IntPtr.Zero;
                    break;
				}
			}
		    return true;
		}

        private void AnalysisImage(string sFile)
        {
            IEngine engine = new IEngine();
            engine.Analysis(sFile, null);
        }

	    private void EndingScan()
		{
		    if( msgfilter )
			{
			    Application.RemoveMessageFilter( this );
			    msgfilter = false;
			    this.Enabled = true;
			    this.Activate();
			}
		}

	    private bool	msgfilter;
	    private Twain	tw;
	    private int		picnumber = 0;

        //[STAThread]
        //static void Main() 
        //{
        //    if( Twain.ScreenBitDepth < 15 )
        //    {
        //        MessageBox.Show( "Need high/true-color video mode!", "Screen Bit Depth", MessageBoxButtons.OK, MessageBoxIcon.Information );
        //        return;
        //    }
        //    Application.EnableVisualStyles();
        //    MainFrame mf = new MainFrame();
        //    Application.Run( mf );
        //}

        BITMAPINFOHEADER bmi;
        Rectangle bmprect;
        IntPtr dibhand;
        internal IntPtr bmpptr;
        internal IntPtr pixptr;

        [DllImport("gdi32.dll", ExactSpelling = true)]
        internal static extern int SetDIBitsToDevice(IntPtr hdc, int xdst, int ydst,
                                                int width, int height, int xsrc, int ysrc, int start, int lines,
                                                IntPtr bitsptr, IntPtr bmiptr, int color);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GlobalLock(IntPtr handle);
        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GlobalFree(IntPtr handle);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern void OutputDebugString(string outstr);

        protected IntPtr GetPixelInfo(IntPtr bmpptr)
        {
            bmi = new BITMAPINFOHEADER();
            Marshal.PtrToStructure(bmpptr, bmi);

            bmprect.X = bmprect.Y = 0;
            bmprect.Width = bmi.biWidth;
            bmprect.Height = bmi.biHeight;

            if (bmi.biSizeImage == 0)
                bmi.biSizeImage = ((((bmi.biWidth * bmi.biBitCount) + 31) & ~31) >> 3) * bmi.biHeight;

            int p = bmi.biClrUsed;
            if ((p == 0) && (bmi.biBitCount <= 8))
                p = 1 << bmi.biBitCount;
            p = (p * 4) + bmi.biSize + (int)bmpptr;
            return (IntPtr)p;
        }
      

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {

        }

   
        

      
	} // class MainFrame


[StructLayout(LayoutKind.Sequential, Pack = 2)]
internal class BITMAPINFOHEADER
{
    public int biSize;
    public int biWidth;
    public int biHeight;
    public short biPlanes;
    public short biBitCount;
    public int biCompression;
    public int biSizeImage;
    public int biXPelsPerMeter;
    public int biYPelsPerMeter;
    public int biClrUsed;
    public int biClrImportant;
}
} // namespace TwainGui
