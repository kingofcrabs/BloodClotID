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
using BarcodesDecoder;
using BarcodesDecoder.Properties;
using System.Configuration;

namespace TwainGui
{
public class MainFrame : System.Windows.Forms.Form, IMessageFilter
	{
        PicForm newpic = null;
        ResultForm resForm = null;
	    private System.Windows.Forms.MdiClient mdiClient1;
	    private System.Windows.Forms.MenuItem menuMainFile;
	    private System.Windows.Forms.MenuItem menuItemScan;
	    private System.Windows.Forms.MenuItem menuItemSelSrc;
	    private System.Windows.Forms.MenuItem menuMainWindow;
	    private System.Windows.Forms.MenuItem menuItemExit;
	    private System.Windows.Forms.MenuItem menuItemSepr;
        private System.Windows.Forms.MainMenu mainFrameMenu;
        private ToolStrip toolStrip1;
        private ToolStripButton btnScan;
        private ToolStripButton btnSetting;
        private MenuItem menuItemHelp;
        private MenuItem menuItemAbout;
        private MenuItem menuItemResult;
        private ToolStripButton btnExit;
        private IContainer components;
	    public MainFrame()
		{
		    InitializeComponent();
		    tw = new Twain();
		    tw.Init( this.Handle );
         
            Helper.LoadSettings(ref GlobalVars.Instance.settings);
		}

        void hijackTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainFrame));
            this.menuMainFile = new System.Windows.Forms.MenuItem();
            this.menuItemSelSrc = new System.Windows.Forms.MenuItem();
            this.menuItemScan = new System.Windows.Forms.MenuItem();
            this.menuItemResult = new System.Windows.Forms.MenuItem();
            this.menuItemSepr = new System.Windows.Forms.MenuItem();
            this.menuItemExit = new System.Windows.Forms.MenuItem();
            this.mainFrameMenu = new System.Windows.Forms.MainMenu(this.components);
            this.menuMainWindow = new System.Windows.Forms.MenuItem();
            this.menuItemHelp = new System.Windows.Forms.MenuItem();
            this.menuItemAbout = new System.Windows.Forms.MenuItem();
            this.mdiClient1 = new System.Windows.Forms.MdiClient();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnScan = new System.Windows.Forms.ToolStripButton();
            this.btnSetting = new System.Windows.Forms.ToolStripButton();
            this.btnExit = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
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
            this.menuItemResult.Text = "结果设置";
            this.menuItemResult.Click += new System.EventHandler(this.menuItemResult_Click);
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
            this.menuMainWindow,
            this.menuItemHelp});
            // 
            // menuMainWindow
            // 
            this.menuMainWindow.Index = 1;
            this.menuMainWindow.MdiList = true;
            this.menuMainWindow.Text = "窗口";
            // 
            // menuItemHelp
            // 
            this.menuItemHelp.Index = 2;
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
            this.mdiClient1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mdiClient1.Location = new System.Drawing.Point(0, 39);
            this.mdiClient1.Name = "mdiClient1";
            this.mdiClient1.Size = new System.Drawing.Size(974, 550);
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
            this.btnScan.Image = ((System.Drawing.Image)(resources.GetObject("btnScan.Image")));
            this.btnScan.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(36, 36);
            this.btnScan.Text = "Start Scan";
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // btnSetting
            // 
            this.btnSetting.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSetting.Image = ((System.Drawing.Image)(resources.GetObject("btnSetting.Image")));
            this.btnSetting.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSetting.Name = "btnSetting";
            this.btnSetting.Size = new System.Drawing.Size(36, 36);
            this.btnSetting.Text = "Set Result File";
            this.btnSetting.Click += new System.EventHandler(this.btnSetting_Click);
            // 
            // btnExit
            // 
            this.btnExit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnExit.Image = ((System.Drawing.Image)(resources.GetObject("btnExit.Image")));
            this.btnExit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(36, 36);
            this.btnExit.Text = "Exit";
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // MainFrame
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(974, 589);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.mdiClient1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.Menu = this.mainFrameMenu;
            this.Name = "MainFrame";
            this.Text = "批量解码";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
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
            HelpForm helpForm = new HelpForm();
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
                if (newpic != null)
                    newpic.Close();

                if (resForm != null)
                    resForm.Close();
                bool bTest = bool.Parse(ConfigurationManager.AppSettings["Test"]);
                if (bTest)
                {
                    resForm = new ResultForm();
                    resForm.MdiParent = this;
                    resForm.Show();
                    return;
                }
                string latestImage = Helper.GetOutputFolder() + Resources.latestImage;
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
				    //for( int i = 0; i < pics.Count; i++ )
					{
                        IntPtr dibhandp = (IntPtr)pics[0];
                        //newpic = new PicForm( img );
                        //newpic.MdiParent = this;
                        //int picnum = i + 1;
                        //newpic.Text = "ScanPass" + picnumber.ToString() + "_Pic" + picnum.ToString();
                        //newpic.Show();
                        dibhand = dibhandp;
                        bmpptr = GlobalLock(dibhand);
                        pixptr = GetPixelInfo(bmpptr);
                        Gdip.SaveDIBAs(
                            Helper.GetOutputFolder() + Resources.latestImage 
                            ,bmpptr, pixptr);
                        GlobalFree(dibhand);
                        dibhand = IntPtr.Zero;
                        resForm = new ResultForm();
                        resForm.MdiParent = this;
                        resForm.Show();
				        //break;
					}
                    break;
				}
			}

		    return true;
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

	    [STAThread]
	    static void Main() 
	    {
		    if( Twain.ScreenBitDepth < 15 )
			{
			    MessageBox.Show( "Need high/true-color video mode!", "Screen Bit Depth", MessageBoxButtons.OK, MessageBoxIcon.Information );
			    return;
			}
            Application.EnableVisualStyles();
		    MainFrame mf = new MainFrame();
		    Application.Run( mf );
	    }

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
        private void OnSetting()
        {
            SettingForm settingForm = new SettingForm();
            settingForm.ShowDialog();
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            OnSetting();
        }

        private void menuItemResult_Click(object sender, EventArgs e)
        {
            OnSetting();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        

      
	} // class MainFrame

} // namespace TwainGui
