﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
//using DotNet.Utilities;
using System.Text.RegularExpressions;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Diagnostics;
using UUWiseCSWrapper;
using mshtml;
using SUP;
using System.Timers;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using CsharpHttpHelper;
using ConnectionDial;
using Microsoft.Win32;


namespace abfwork
{
    public partial class mainForm : Form
    {

        //datatable用于保存读入的数据
        DataTable dt_num = new DataTable();
        DataTable dt_cdk = new DataTable();
        int dtnumflag = 0,dtcdkflag=0;//是否第一次添加数据

        private Ras ras;//宽带拨号

        int pinglv = 0;//定时兑换频率
        int pinglv_t = 0;//定时兑换频率

        int num_ds = 0;//定时兑换设定数量
        int num_ds_t = 0;

        int index;
        int wenjian_ok = 0;//文件是否保存完毕
        string duankou_url = "";
        int yzm_weishu = 4;
        string prizeID = "00";//价值类别
        string freedrink_cnt = "0";//免喝数量
        int is_yzm_TextChanged = 0;//是否触发
        int is_huanzh_ok = 0;//换账号是否成功
        int pwd_weishu = 0;//密码位数
        int retry_num = 0;//重新尝试次数
        string yanchi = "0";
        int duihuan_count = 0;//兑换次数
        int getyzm_num = 0;
        bool is_yzm_ok = false;
        int flag_f02_ok = 0;
        int flag_f02_err = 0;
        string token="";//爱码令牌
        string logdir = "log";
        string txje="" ;//提现金额
        int isbtok = 0;//提交执行情况
        int lgnsuccesscount = 0;//成功统计
        int lgnerrcount = 0;//失败统计
        HttpHelper http = new HttpHelper();
        HttpItem item;
        HttpResult resultitem;
        bool isloginok=false;//登录密码检测是否完成
        bool isstop=false;//程序暂停
        int tixianGetinfoisok = 0;//提现标记
        bool []loginres={false,false};//成功 失败 计数
        string strCheckKey = "65BD22F2-4576-4FFE-B7FB-A0B1807BCC37".ToUpper();
        string useName = "";//帐号
        string password = "";//密码
        string authenticode = "";//CDK
        string captcha = "";//验证码
        string f02_smsyzm = "";//爱码平台验证码
        string cookie = "";//cookie数据
        int numIndexOfDatagridview1Row = 0;//帐号数据中选中行的编号
        int numIndexOfDatagridview2Row = 0;//CDK数据中选中行的编号
        static int SoftID = 97505;
        static string SoftKey = "13f445c8864c41cdaa304c022fe9c34b";
        bool isDamaLogin = false;//是否使用uu打码
        int codeType = 1005;//验证码长度
        int m_codeID;//上报错误使用
        bool iscaptchaerr=false;//验证码是否错误

        [DllImport("AntiVC.dll")]
        public static extern int LoadCdsFromFile(string FilePath, string Password);

        [DllImport("AntiVC.dll")]
        public static extern int LoadCdsFromBuffer(byte[] FileBuffer, int FileBufLen, string Password);

        [DllImport("AntiVC.dll")]
        public static extern bool GetVcodeFromFile(int CdsFileIndex, string FilePath, StringBuilder Vcode);

        [DllImport("AntiVC.dll")]
        public static extern bool GetVcodeFromBuffer(int CdsFileIndex, byte[] FileBuffer, int ImgBufLen, StringBuilder Vcode);

        [DllImport("AntiVC.dll")]
        public static extern bool GetVcodeFromURL(int CdsFileIndex, string ImgURL, StringBuilder Vcode);

        /// <summary>
        /// 生成随机数
        /// </summary>
        private static char[] constantNum = {'0','1','2','3','4','5','6','7','8','9'};
        private static char[] constant = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'};
        private static char[] constant_daxie = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        public static string GenerateRandomNumber(int Length, int j)
        {
            System.Text.StringBuilder newRandom = new System.Text.StringBuilder(36);
            Random rd = new Random(DateTime.Now.Millisecond + j);
            for (int i = 0; i < Length; i++)
            {
                newRandom.Append(constant[rd.Next(36)]);
            }
            return newRandom.ToString();
        }
        /// <summary>
        /// 启动加载
        /// </summary>
        public mainForm()
        {
            InitializeComponent();
            
            //this.ras = new Ras(new Ras.ConnectionNotify(RasConnectNotify), 1000.0);
            this.ras = new Ras();

            if (File.Exists("dama.bin") == true)
            {
                StreamReader reader = new StreamReader("dama.bin", Encoding.Default);
                int i = 0;
                //循环读取所有行
                while (!reader.EndOfStream)
                {
                    if (i == 0)
                        textBox_uuname.Text = reader.ReadLine();
                    if (i == 1)
                        textBox_uupwd.Text = reader.ReadLine();
                    i++;
                }
                //关闭文件
                reader.Close();

            }
            else
            {
                FileStream myFs = new FileStream("dama.bin", FileMode.Create);
                StreamWriter mySw = new StreamWriter(myFs);
                mySw.Close();
                myFs.Close();
            }

            //if (File.Exists("cookie.txt") == true)
            //{//读取cookie
            //    StreamReader reader = new StreamReader("cookie.txt", Encoding.Default);
            //    cookie = reader.ReadLine();
            //    reader.Close();

            //}
            //else
            //{
            //    FileStream myFs = new FileStream("cookie.txt", FileMode.Create);
            //    StreamWriter mySw = new StreamWriter(myFs);
            //    mySw.Close();
            //    myFs.Close();
            //}          
            Directory.CreateDirectory(logdir);//创建日志目录
            
            
            //textBox_yzm.Focus();
            this.comboBox1.SelectedIndex = 1;
            this.comboBox_url.SelectedIndex = 0;
            duankou_url = "http://2016utc.pepsi.cn/";//默认值
            timer2.Start();//显示时间
            //webBrowser1.ScriptErrorsSuppressed = true;
            //webBrowser1.BeginInvoke(new EventHandler(delegate
            //{//后台线程开启浏览器
            //    webBrowser1.Navigate(duankou_url);
            //}));
        }

        /// <summary>
        /// 自动加载宽带连接信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainForm_Load_1(object sender, EventArgs e)
        {

            string[] strs;
            string str;
            ////strs = GetAllAdslNameByKey();//获取所有的宽带连接名称
            //this.ras.GetEntries(out strs, out str);
            //foreach (string s in strs)
            //{
            //    comboBox_kd.Items.Add(s);
            //}
            //comboBox_kd.SelectedIndex = 0;

            //showAttr();

            checkBox_yzm_zds.Checked = true;

            index = LoadCdsFromFile("百事.cds", "19870216");
            if (checkBox_yzm_zds.Checked && index == 1)
            {
                richTextBox1.AppendText(string.Format("\r\n{0}: {1}", DateTime.Now.ToString("HH:mm:ss"), "成功载入验证码自动识别模块！"));
            }
            else if(index == -1)
            {
                richTextBox1.AppendText(string.Format("\r\n{0}: {1}", DateTime.Now.ToString("HH:mm:ss"), "载入验证码自动识别模块失败！"));
            }

            bintodatagridview1(dataGridView1, "data1.data");
            bintodatagridview2(dataGridView2, "data2.data");
            
            System.Media.SoundPlayer sp = new System.Media.SoundPlayer();
            sp.SoundLocation = "sound/go.wav";
            sp.Play();

            
        }

        /// <summary>
        /// 显示宽带连接信息
        /// </summary>
        private void showAttr()
        {
            int index = comboBox_kd.SelectedIndex;
            if (index >= 0)
            {
                string str;
                string str2;
                string str3;
                string str4;
                bool flag;
                this.ras.GetEntryParams((string)comboBox_kd.SelectedItem, out str, out str2, out str3, out flag, out str4);
                
                this.textBox1.Text = str2;
                this.textBox2.Text = str3;
            }
        }

        public void RasConnectNotify(string strNotify, int Connect)
        {
            richTextBox1.BeginInvoke(new EventHandler(delegate
            {
                richTextBox1.AppendText(strNotify);
            }));
        }

        /// <summary>
        /// 让DataGridView显示行号   见：www.cnblogs.com/JuneZhang/archive/2011/11/21/2257630.html
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {

            System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(e.RowBounds.Location.X,
        e.RowBounds.Location.Y,
        dataGridView1.RowHeadersWidth - 4,
        e.RowBounds.Height);

            TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(),
                dataGridView1.RowHeadersDefaultCellStyle.Font,
                rectangle,
                dataGridView1.RowHeadersDefaultCellStyle.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        }

        /// <summary>
        /// 让DataGridView显示行号   见：www.cnblogs.com/JuneZhang/archive/2011/11/21/2257630.html
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView2_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(e.RowBounds.Location.X,
        e.RowBounds.Location.Y,
        dataGridView2.RowHeadersWidth - 4,
        e.RowBounds.Height);

            TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(),
                dataGridView2.RowHeadersDefaultCellStyle.Font,
                rectangle,
                dataGridView2.RowHeadersDefaultCellStyle.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Right);

        }

        /// <summary>
        /// 拖拽导入帐号数据1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        /// <summary>
        /// 拖拽导入帐号数据2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_DragDrop(object sender, DragEventArgs e)
        {
            int cunt = 0;
            try
            {
                string strFile = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();//文件路径
                if (dtnumflag == 0)
                {
                    //给datatable添加三个列
                    dt_num.Columns.Add("选择", typeof(bool));
                    dt_num.Columns.Add("帐号", typeof(String));
                    dt_num.Columns.Add("密码", typeof(String));
                    dt_num.Columns.Add("状态", typeof(String));
                    dt_num.Columns.Add("cookie", typeof(String));
                    dt_num.PrimaryKey = new DataColumn[] { dt_num.Columns["帐号"] };//设置datatable主键
                    dtnumflag++;//标记
                }
                //读入文件
                StreamReader reader = new StreamReader(strFile, Encoding.Default);
                //循环读取所有行
                while (!reader.EndOfStream)
                {
                    //将每行数据，用-分割成3段
                    string[] data = reader.ReadLine().Replace("----", "-").Split('-');
                    //新建一行，并将读出的数据分段，分别存入3个对应的列中
                    DataRow dr = dt_num.NewRow();
                    dr[0] = false;
                    dr[1] = data[0];
                    dr[2] = data[1];
                    dr[3] = "";
                    //dr[4] = "ASP.NET_SessionId=" + GenerateRandomNumber(16,1);
                    if (dt_num.Rows.Find(dr[1]) == null)
                    {//判断是否重复
                        //将这行数据加入到datatable中
                        dt_num.Rows.Add(dr);
                        cunt++;
                    }
                    else
                        richTextBox1.AppendText(string.Format("\r\n{0}: 去除重复帐号:{1}", DateTime.Now.ToString("HH:mm:ss"), dr[1]));
                }
                richTextBox1.AppendText(string.Format("\r\n{0}: 导入不重复帐号:{1}个", DateTime.Now.ToString("HH:mm:ss"), cunt));
                //关闭文件
                
                reader.Close();
                //将datatable绑定到datagridview上显示结果
                dataGridView1.DataSource = dt_num;
                label_usenamecount.Text = dataGridView1.RowCount.ToString();//显示帐号总数

                datagridviewtobin(dataGridView1, "data1.data");
            }
            catch (Exception e1)
            {
                MessageBox.Show("文件内容格式错误：请用“帐号----密码”的格式导入数据！", "错误："+e1.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            

        }

        /// <summary>
        /// 拖拽导入cdk数据1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView2_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        /// <summary>
        /// 拖拽导入cdk数据2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView2_DragDrop(object sender, DragEventArgs e)
        {
            int cunt = 0;
            try
            {
                string strFile = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();//文件路径
                if (dtcdkflag == 0)
                {
                    //给datatable添加1个列
                    dt_cdk.Columns.Add("CDK", typeof(String));
                    dt_cdk.Columns.Add("兑换结果", typeof(String));
                    dt_cdk.Columns.Add("时间", typeof(String));
                    dt_cdk.PrimaryKey = new DataColumn[] { dt_cdk.Columns["CDK"] };//设置datatable主键
                    dtcdkflag++;
                }

                //读入文件
                StreamReader reader = new StreamReader(strFile, Encoding.Default);

                //循环读取所有行
                while (!reader.EndOfStream)
                {
                    //将每行数据，用-分割成3段
                    //string[] data = reader.ReadLine().Replace("----", "-").Split('-');
                    //新建一行，并将读出的数据存入对应的列中
                    DataRow dr = dt_cdk.NewRow();
                    dr[0] = reader.ReadLine();
                    if (dt_cdk.Rows.Find(dr[0]) == null)
                    {
                        //将这行数据加入到datatable中
                        dt_cdk.Rows.Add(dr);
                        cunt++;
                    }
                    else
                        richTextBox1.AppendText(string.Format("\r\n{0}: 去除重复cdk:{1}", DateTime.Now.ToString("HH:mm:ss"), dr[0]));
                }
                richTextBox1.AppendText(string.Format("\r\n{0}: 导入不重复cdk:{1}个", DateTime.Now.ToString("HH:mm:ss"), cunt));
                
                //关闭文件
                reader.Close();
                //将datatable绑定到datagridview上显示结果
                dataGridView2.DataSource = dt_cdk;
                label_cdkcount.Text = dataGridView2.RowCount.ToString();//显示cdk总数

                datagridviewtobin(dataGridView2, "data2.data");
            }
            catch (Exception e1)
            {
                MessageBox.Show("文件内容错误：请导入正确的CDK数据！", "错误：" + e1.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        
        /// <summary>
        /// 剪切板数据操作
        /// </summary>
        /// <param name="DBGrid"></param>
        public static void DataGirdViewCellPaste(DataGridView DBGrid)
        {
            try
            {
                // 获取剪切板的内容，并按行分割 
                string pasteText = "";
                pasteText = Clipboard.GetText();

                if (string.IsNullOrEmpty(pasteText))
                    return;
                if (pasteText == "pasteText")
                {
                    return;
                }
                int tnum = 0;
                int nnum = 0;
                //获得当前剪贴板内容的行、列数
                for (int i = 0; i < pasteText.Length; i++)
                {
                    if (pasteText.Substring(i, 1) == "\t")
                    {
                        tnum++;
                    }
                    if (pasteText.Substring(i, 1) == "\n")
                    {
                        nnum++;
                    }
                }
                Object[,] data;
                //粘贴板上的数据来自于EXCEL时，每行末都有\n，在DATAGRIDVIEW内复制时，最后一行末没有\n
                if (pasteText.Substring(pasteText.Length - 1, 1) == "\n")
                {
                    nnum = nnum - 1;
                }
                tnum = tnum / (nnum + 1);
                data = new object[nnum + 1, tnum + 1];//定义一个二维数组

                String rowstr;
                rowstr = "";
                //MessageBox.Show(pasteText.IndexOf("B").ToString());
                //对数组赋值
                for (int i = 0; i < (nnum + 1); i++)
                {
                    for (int colIndex = 0; colIndex < (tnum + 1); colIndex++)
                    {
                        //一行中的最后一列
                        if (colIndex == tnum && pasteText.IndexOf("\r") != -1)
                        {
                            rowstr = pasteText.Substring(0, pasteText.IndexOf("\r"));
                        }
                        //最后一行的最后一列
                        if (colIndex == tnum && pasteText.IndexOf("\r") == -1)
                        {
                            rowstr = pasteText.Substring(0);
                        }
                        //其他行列
                        if (colIndex != tnum)
                        {
                            rowstr = pasteText.Substring(0, pasteText.IndexOf("\t"));
                            pasteText = pasteText.Substring(pasteText.IndexOf("\t") + 1);
                        }
                        data[i, colIndex] = rowstr;
                    }
                    //截取下一行数据
                    pasteText = pasteText.Substring(pasteText.IndexOf("\n") + 1);
                }
                //获取当前选中单元格所在的列序号
                int curntindex = DBGrid.CurrentRow.Cells.IndexOf(DBGrid.CurrentCell);
                //获取获取当前选中单元格所在的行序号
                int rowindex = DBGrid.CurrentRow.Index;
                //MessageBox.Show(curntindex.ToString ());
                for (int j = 0; j < (nnum + 1); j++)
                {
                    for (int colIndex = 0; colIndex < (tnum + 1); colIndex++)
                    {
                        if (!DBGrid.Columns[colIndex + curntindex].Visible)
                        {
                            continue;
                        }
                        if (!DBGrid.Rows[j + rowindex].Cells[colIndex + curntindex].ReadOnly)
                        {
                            DBGrid.Rows[j + rowindex].Cells[colIndex + curntindex].Value = data[j, colIndex];
                        }
                    }
                }
                Clipboard.Clear();
            }
            catch
            {
                Clipboard.Clear();
                //MessageBox.Show("粘贴区域大小不一致");
                return;
            }
        }

        /// <summary>
        /// DataGridView数据保存到bin文件
        /// </summary>
        /// <param name="datagridview"></param>
        /// <param name="bin"></param>
        private void datagridviewtobin(DataGridView datagridview, string bin)
        {

            FileStream fileStream = new FileStream(bin, FileMode.OpenOrCreate);
            StreamWriter streamWriter = new StreamWriter(fileStream, System.Text.Encoding.Unicode);

            StringBuilder strBuilder = new StringBuilder();

            try
            {
                for (int i = 0; i < datagridview.Rows.Count; i++)
                {
                    strBuilder = new StringBuilder();
                    for (int j = 0; j < datagridview.Columns.Count; j++)
                    {
                        strBuilder.Append(datagridview.Rows[i].Cells[j].Value.ToString() + "----");
                    }
                    //strBuilder.Remove(strBuilder.Length - 1, 1);
                    streamWriter.WriteLine(strBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                string strErrorMessage = ex.Message;
            }
            finally
            {
                streamWriter.Close();
                fileStream.Close();
                wenjian_ok = 1;//文件保存完成
            }


        }

        /// <summary>
        /// 加载数据到DataGridView1
        /// </summary>
        /// <param name="datagridview"></param>
        /// <param name="bin"></param>
        private void bintodatagridview1(DataGridView datagridview, string bin)
        {
            int cunt = 0;
            try
            {
                string strFile = bin;//文件路径
                if (dtnumflag == 0)
                {
                    //给datatable添加三个列
                    dt_num.Columns.Add("选择", typeof(bool));
                    dt_num.Columns.Add("帐号", typeof(String));
                    dt_num.Columns.Add("密码", typeof(String));
                    dt_num.Columns.Add("状态", typeof(String));
                    dt_num.Columns.Add("cookie", typeof(String));
                    dt_num.PrimaryKey = new DataColumn[] { dt_num.Columns["帐号"] };//设置datatable主键
                    dtnumflag++;//标记
                }
                FileStream fileStream = new FileStream(bin, FileMode.OpenOrCreate);
                //读入文件
                StreamReader reader = new StreamReader(fileStream, Encoding.Unicode);
                //循环读取所有行
                while (!reader.EndOfStream)
                {
                    //将每行数据，用-分割成3段
                    string[] data = reader.ReadLine().Replace("----", "-").Split('-');
                    //新建一行，并将读出的数据分段，分别存入4个对应的列中
                    DataRow dr = dt_num.NewRow();
                    if (data[0] == "True")
                    {
                        dr[0] = true;
                    }
                    else
                    {
                        dr[0] = false; ;
                    }
                    dr[1] = data[1];
                    dr[2] = data[2];
                    dr[3] = data[3];
                    dr[4] = data[4];
                    dt_num.Rows.Add(dr);
                    cunt++;
                }
                //关闭文件
                reader.Close();
                fileStream.Close();
                //将datatable绑定到datagridview上显示结果
                datagridview.DataSource = dt_num;
                label_usenamecount.Text = datagridview.RowCount.ToString();//显示帐号总数
            }
            catch (Exception e1)
            {
                MessageBox.Show("文件内容格式错误：请用“帐号----密码”的格式导入数据！", "错误：" + e1.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 加载数据到DataGridView2
        /// </summary>
        /// <param name="datagridview"></param>
        /// <param name="bin"></param>
        private void bintodatagridview2(DataGridView datagridview, string bin)
        {
            int cunt = 0;
            try
            {
                //string strFile = bin;//文件路径
                if (dtcdkflag == 0)
                {
                    //给datatable添加1个列
                    dt_cdk.Columns.Add("CDK", typeof(String));
                    dt_cdk.Columns.Add("兑换结果", typeof(String));
                    dt_cdk.Columns.Add("时间", typeof(String));
                    dt_cdk.PrimaryKey = new DataColumn[] { dt_cdk.Columns["CDK"] };//设置datatable主键
                    dtcdkflag++;
                }
                FileStream fileStream = new FileStream(bin, FileMode.OpenOrCreate);
                //读入文件
                StreamReader reader = new StreamReader(fileStream, Encoding.Unicode);

                //循环读取所有行
                while (!reader.EndOfStream)
                {
                    //将每行数据，用-分割成3段
                    string[] data2 = reader.ReadLine().Replace("----", "-").Split('-');

                    //新建一行，并将读出的数据分段，分别存入3个对应的列中
                    DataRow dr2 = dt_cdk.NewRow();
                    dr2[0] = data2[0];
                    dr2[1] = data2[1];
                    dr2[2] = data2[2];
                    
                    //将这行数据加入到datatable中
                    dt_cdk.Rows.Add(dr2);
                    cunt++;

                }
                //关闭文件
                reader.Close();
                fileStream.Close();
                //将datatable绑定到datagridview上显示结果
                datagridview.DataSource = dt_cdk;
                label_cdkcount.Text = datagridview.RowCount.ToString();//显示帐号总数
            }
            catch (Exception e1)
            {
                MessageBox.Show("文件内容格式错误：请导入正确的CDK数据！", "错误：" + e1.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// 开始操作-定时器开启
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            
            yanchi = textBox_yc.Text;
            if (dataGridView1.RowCount == 0)
            {
                MessageBox.Show("请导入帐号！", "提示");
                return;
            }
            if (comboBox1.SelectedIndex.ToString() == "0")
            {//登录查询信息
                richTextBox1.AppendText(string.Format("\r\n{0}: 开始登录...", DateTime.Now.ToString("HH:mm:ss")));
                timer1.Start();
                LoginOn_result();
            }
            else if (comboBox1.SelectedIndex.ToString() == "1")
            {//兑换cdk
                if (dataGridView2.RowCount == 0)
                {
                    MessageBox.Show("请导入cdk", "提示");
                    return;
                }
                richTextBox1.AppendText(string.Format("\r\n{0}: 开始兑换cdk...", DateTime.Now.ToString("HH:mm:ss")));
                timer1.Interval = Int32.Parse(textBox_yc.Text);
                timer1.Start();
                //textBox_yzm.Focus();
                duihuan();
            }
            else if (comboBox1.SelectedIndex.ToString() == "2")
            {//注册账号
                richTextBox1.AppendText(string.Format("\r\n{0}: 开始注册账号...", DateTime.Now.ToString("HH:mm:ss")));
                timer1.Start();
                getVorityImage();
                //textBox_yzm.Focus();
            }
            else if (comboBox1.SelectedIndex.ToString() == "3")
            {//修改密码
                richTextBox1.AppendText(string.Format("\r\n{0}: 开始修改密码...", DateTime.Now.ToString("HH:mm:ss")));
                pwd_weishu = Int32.Parse(textBox_pwd_weishu.Text);
                timer1.Start();
                PwdChange();
            }
            else if (comboBox1.SelectedIndex.ToString() == "4")
            {//渴望币兑换
                if (prizeID == "00")
                {
                    //导入的帐号注册完成
                    MessageBox.Show("请选择合适的兑换子选项目！", "提示");
                    return;
                }
                richTextBox1.AppendText(string.Format("\r\n{0}: 开始渴望币兑换...", DateTime.Now.ToString("HH:mm:ss")));
                
                timer1.Start();
                CashLotteryExchange();
            }
            else if (comboBox1.SelectedIndex.ToString() == "5")
            {//渴望币抽奖
                if (prizeID == "00")
                {
                    //导入的帐号注册完成
                    MessageBox.Show("请选择合适的兑换子选项目！", "提示");
                    return;
                }
                richTextBox1.AppendText(string.Format("\r\n{0}: 开始渴望币抽奖...", DateTime.Now.ToString("HH:mm:ss")));
                timer1.Start();
                CashLotteryExchange();
            }
            else if (comboBox1.SelectedIndex.ToString() == "6")
            {//秒杀

                richTextBox1.AppendText(string.Format("\r\n{0}: 开始秒杀...", DateTime.Now.ToString("HH:mm:ss")));
                timer1.Interval = 50;//提交秒杀频率
                //timer1.Start();
                SecKillAward();
            }
            button1.Enabled = false;

        }

        /// <summary>
        /// 暂停定时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "暂停")
            {
                timer1.Stop();
                richTextBox1.AppendText(string.Format("\r\n{0}: 程序暂停！", DateTime.Now.ToString("HH:mm:ss")));
                button2.Text = "继续";
            }
            else if (button2.Text == "继续")
            {
                timer1.Start();
                richTextBox1.AppendText(string.Format("\r\n{0}: 程序继续...", DateTime.Now.ToString("HH:mm:ss")));
                button2.Text = "暂停";
            }
        }

        /// <summary>
        /// 通过加载验证码 获取cookie******************
        /// </summary>
        /// <param name="str_cookie"></param>
        private string get_cookie()
        {
            HttpHelper http = new HttpHelper();
            HttpItem itemGet = new HttpItem()
            {
                URL = duankou_url + "SecurityCode/Code",//URL     必需项    
                Method = "get",
                //Cookie = cookie,//字符串Cookie     可选项
                //用户代理设置为手机浏览器
                UserAgent = "Mozilla/5.0 (iPhone; U; CPU iPhone OS 3_0 like Mac OS X; en-us) AppleWebKit/528.18 (KHTML, like Gecko) Version/4.0 Mobile/7A341 Safari/528.16",
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写 
                Referer = duankou_url + "Home/Index",
                //Postdata = "&oldvalue=" + captcha,//Post数据     可选项GET时不需要写
                Timeout = 100000,//连接超时时间     可选项默认为100000    
                ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                //ContentType = "application/x-www-form-urlencoded",//返回类型    可选项有默认值 
            };
            resultitem = http.GetHtml(itemGet);
            cookie = resultitem.Cookie.ToString().Split(';')[0];
            return cookie;
        }

        /// <summary>
        /// 检测密码登录
        /// </summary>
        private string LoginOn(string useName, string password,string cookies)
        {
            //cookie = "ASP.NET_SessionId=" + GenerateRandomNumber(24, 1);
            item = new HttpItem()
            {//登录验证
                URL = duankou_url + "Account/LoginOn",//URL     必需项    
                Method = "POST",
                Cookie = cookies,//字符串Cookie     可选项
                //用户代理设置为手机浏览器
                UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.110 Safari/537.36",
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写 
                Referer = duankou_url + "Account/Login",
                Postdata = "phone=" + useName + "&password=" + password ,//Post数据     可选项GET时不需要写
                Timeout = 100000,//连接超时时间     可选项默认为100000    
                ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8",//返回类型    可选项有默认值   
            };

            resultitem = http.GetHtml(item);
            return resultitem.Html;

        }

        /// <summary>
        /// 获取渴望币金额
        /// </summary>
        private string GetOneDesire_Money()
        {//获取渴望币金额
            item = new HttpItem()
            {//登录验证
                URL = duankou_url + "Account/GetOneDesire_Money",//URL     必需项    
                Method = "GET",//这里与浏览器不一样的*************
                Cookie = cookie,//字符串Cookie     可选项
                //用户代理设置为手机浏览器
                UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.110 Safari/537.36",
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写 
                Referer = duankou_url + "Home/Index",
                //Postdata = "phone=" + useName + "&password=" + password,//Post数据     可选项GET时不需要写
                Timeout = 100000,//连接超时时间     可选项默认为100000    
                ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                //ContentType = "application/x-www-form-urlencoded",//返回类型    可选项有默认值 
            };

            resultitem = http.GetHtml(item);
            return  resultitem.Html.Split('|')[1];
        }

        /// <summary>
        /// 个人中心
        /// </summary>
        private string WinPrizeLog()
        {
            item = new HttpItem()
            {//登录验证
                URL = duankou_url + "WinPrize/WinPrizeLog",//URL     必需项    
                Method = "POST",//这里与浏览器不一样的*************
                Cookie = cookie,//字符串Cookie     可选项
                //用户代理设置为手机浏览器
                UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.110 Safari/537.36",
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写 
                Referer = duankou_url + "WinPrize/WinPrizeIndex",
                Postdata = "pageindex=1" ,//Post数据     可选项GET时不需要写
                Timeout = 100000,//连接超时时间     可选项默认为100000    
                ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8",//返回类型    可选项有默认值 
            };

            resultitem = http.GetHtml(item);
            return resultitem.Html;
        }

        /// <summary>
        /// 登录查询信息
        /// </summary>
        private void LoginOn_result()
        {
            useName = dataGridView1.SelectedRows[0].Cells["ColumnPhoneNum"].Value.ToString();
            password = dataGridView1.SelectedRows[0].Cells["ColumnPwd"].Value.ToString();
            cookie = dataGridView1.SelectedRows[0].Cells["Column_cookie"].Value.ToString();
            if (cookie == "")
            {
                cookie = get_cookie();
            }
            string result = LoginOn(useName, password,cookie);
            if (result == "true")
            {//列出详细的账号信息
                richTextBox1.AppendText(string.Format("\r\n{0}: {1} - 登录成功！", DateTime.Now.ToString("HH:mm:ss"), useName));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "渴望币:" + GetOneDesire_Money() + "-中奖纪录:" + WinPrizeLog();//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "查询账号信息_成功.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + password + "----" + dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value + "----" + DateTime.Now.ToString("HH:mm:ss"));
                sw.Close();
                aFile.Close();
                lgnsuccesscount++ ;
                label_lgnsuccess.Text = lgnsuccesscount.ToString();
                isbtok = 1;
            }
            else if (result == "false")
            {//登录失败
                richTextBox1.AppendText(string.Format("\r\n{0}: {1} - 登录失败！", DateTime.Now.ToString("HH:mm:ss"), useName));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "登录失败";//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "查询账号信息_登录失败.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + password + "----" + DateTime.Now.ToString("HH:mm:ss"));
                sw.Close();
                aFile.Close();
                lgnerrcount++;
                label_lgnerr.Text = lgnerrcount.ToString();
                isbtok = 2;
            }
        }

        /// <summary>
        /// 兑换cdk
        /// </summary>
        private void duihuan()
        {
            useName = dataGridView1.SelectedRows[0].Cells["ColumnPhoneNum"].Value.ToString();
            password = dataGridView1.SelectedRows[0].Cells["ColumnPwd"].Value.ToString();
            cookie = dataGridView1.SelectedRows[0].Cells["Column_cookie"].Value.ToString();
            if (cookie == "")
            {
                cookie = get_cookie();
            }
            if (LoginOn(useName, password,cookie) == "true")
            {//登录成功
                
                richTextBox1.AppendText(string.Format("\r\n{0}: {1} - 登录成功！", DateTime.Now.ToString("HH:mm:ss"), useName));
                //dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "渴望币:" + GetOneDesire_Money();//更新结果
                duihuan_count = 0;//登录时清零
                if (lgnsuccesscount == 0)
                {//首次加载图片验证码
                    Thread t = new Thread(new ThreadStart(delegate
                    {
                        getVorityImage();//加载验证码
                    }));
                    t.IsBackground = true;
                    t.Start();
                }
                lgnsuccesscount++;
                label_lgnsuccess.Text = lgnsuccesscount.ToString();
                isloginok = true;
                if (is_huanzh_ok == 1)
                {
                    is_huanzh_ok = 0;//复位

                    //duihuan_bt();
                }

            }
            else
            {//登录失败
                
                richTextBox1.AppendText(string.Format("\r\n{0}: {1} - 登录失败,尝试继续登录...", DateTime.Now.ToString("HH:mm:ss"), useName));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "登录失败";//更新结果
                isloginok = false;
                duihuan();//登录失败,继续登录
            }
            
        }

        private void duihuan_bt()
        {

            // -----------------兑换开始------------------
            authenticode = dataGridView2.SelectedRows[0].Cells["ColumnCDK"].Value.ToString();
            //HttpHelper http = new HttpHelper();
            item = new HttpItem()
            {//兑换cdk
                URL = duankou_url + "Lottery/Prize",//URL     必需项    
                Method = "POST",
                Cookie = cookie,//字符串Cookie     可选项
                //用户代理设置为手机浏览器
                UserAgent = "Mozilla/5.0 (iPhone; U; CPU iPhone OS 3_0 like Mac OS X; en-us) AppleWebKit/528.18 (KHTML, like Gecko) Version/4.0 Mobile/7A341 Safari/528.16",
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写 
                Referer = duankou_url + "Lottery/Index",
                Postdata = "ticket=" + authenticode + "&code=" + captcha,//Post数据     可选项GET时不需要写
                //Timeout = 100000,//连接超时时间     可选项默认为100000    
                //ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8",//返回类型    可选项有默认值   
            };
            resultitem = http.GetHtml(item);
            string result = resultitem.Html;

            if (result == "请输入正确的验证码")
            {//验证码错误

                richTextBox1.AppendText(string.Format("\r\n{0}: {1}-{2}", DateTime.Now.ToString("HH:mm:ss"), useName, "验证码错误"));
                getVorityImage();
                isbtok = 88;
            }
            else if (result == "0")
            {//渴望币100个

                richTextBox1.AppendText(string.Format("\r\n{0}: {1}-{2}", DateTime.Now.ToString("HH:mm:ss"), useName, "渴望币100个"));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value += "渴望币+100 ";//更新结果
                dataGridView2.SelectedRows[0].Cells["ColumnResult"].Value = "渴望币100个";//更新结果
                dataGridView2.SelectedRows[0].Cells["ColumnTime"].Value = DateTime.Now.ToString("HH:mm:ss");//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "兑换cdk_渴望币100个.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + password + "----" + authenticode + "----" + "渴望币100个" + "----" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss"));
                sw.Close();
                aFile.Close();
                duihuan_count++;
                isbtok = 7;
            }
            else if (result == "1")
            {//双人大礼包

                richTextBox1.AppendText(string.Format("\r\n{0}: {1}-{2}", DateTime.Now.ToString("HH:mm:ss"), useName, result));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value += "渴望币+100 双人大礼包+1 ";//更新结果
                dataGridView2.SelectedRows[0].Cells["ColumnResult"].Value = "双人大礼包";//更新结果
                dataGridView2.SelectedRows[0].DefaultCellStyle.BackColor = Color.Red;
                dataGridView2.SelectedRows[0].Cells["ColumnTime"].Value = DateTime.Now.ToString("HH:mm:ss");//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "兑换cdk_双人大礼包.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + password + "----" + authenticode + "----" + "双人大礼包" + "----" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss"));
                sw.Close();
                aFile.Close();
                lgnsuccesscount++;
                label_lgnsuccess.Text = lgnsuccesscount.ToString();
                duihuan_count++;
                isbtok = 1;
                System.Media.SoundPlayer sp = new System.Media.SoundPlayer();
                sp.SoundLocation = "sound/1.wav";
                sp.Play();

            }
            else if (result == "2")
            {//单人迪斯尼

                richTextBox1.AppendText(string.Format("\r\n{0}: {1}-{2}", DateTime.Now.ToString("HH:mm:ss"), useName, result));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value += "渴望币+100 单人迪斯尼+1 ";//更新结果
                dataGridView2.SelectedRows[0].Cells["ColumnResult"].Value = "单人迪斯尼";//更新结果
                dataGridView2.SelectedRows[0].DefaultCellStyle.BackColor = Color.Orange;
                dataGridView2.SelectedRows[0].Cells["ColumnTime"].Value = DateTime.Now.ToString("HH:mm:ss");//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "兑换cdk_单人迪斯尼.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + password + "----" + authenticode + "----" + "单人迪斯尼" + "----" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss"));
                sw.Close();
                aFile.Close();
                lgnsuccesscount++;
                label_lgnsuccess.Text = lgnsuccesscount.ToString();
                duihuan_count++;
                isbtok = 2;
                System.Media.SoundPlayer sp = new System.Media.SoundPlayer();
                sp.SoundLocation = "sound/1.wav";
                sp.Play();
            }
            else if (result == "3")
            {//3元话费

                richTextBox1.AppendText(string.Format("\r\n{0}: {1}-{2}", DateTime.Now.ToString("HH:mm:ss"), useName, result));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value += "渴望币+100 话费+3 ";//更新结果
                dataGridView2.SelectedRows[0].Cells["ColumnResult"].Value = "3元话费";//更新结果
                dataGridView2.SelectedRows[0].DefaultCellStyle.BackColor = Color.SkyBlue;
                dataGridView2.SelectedRows[0].Cells["ColumnTime"].Value = DateTime.Now.ToString("HH:mm:ss");//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "兑换cdk_3元话费.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + password + "----" + authenticode + "----" + "3元话费" + "----" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss"));
                sw.Close();
                aFile.Close();
                lgnsuccesscount++;
                label_lgnsuccess.Text = lgnsuccesscount.ToString();
                duihuan_count++;
                isbtok = 3;
                System.Media.SoundPlayer sp = new System.Media.SoundPlayer();
                sp.SoundLocation = "sound/2.wav";
                sp.Play();
            }
            else if (result == "4")
            {//1元话费

                richTextBox1.AppendText(string.Format("\r\n{0}: {1}-{2}", DateTime.Now.ToString("HH:mm:ss"), useName, result));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value += "渴望币+100 话费+1 ";//更新结果
                dataGridView2.SelectedRows[0].Cells["ColumnResult"].Value = "1元话费";//更新结果
                dataGridView2.SelectedRows[0].DefaultCellStyle.BackColor = Color.SpringGreen;
                dataGridView2.SelectedRows[0].Cells["ColumnTime"].Value = DateTime.Now.ToString("HH:mm:ss");//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "兑换cdk_1元话费.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + password + "----" + authenticode + "----" + "1元话费" + "----" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss"));
                sw.Close();
                aFile.Close();
                lgnsuccesscount++;
                label_lgnsuccess.Text = lgnsuccesscount.ToString();
                duihuan_count++;
                isbtok = 4;
                System.Media.SoundPlayer sp = new System.Media.SoundPlayer();
                sp.SoundLocation = "sound/2.wav";
                sp.Play();
            }
            else if (result == "串码输入有误哦，请再仔细核对下重新输入，如有疑问，请拨打客服热线：4000647746。" || result=="串码输入有误。咨询热线：4000647746")
            {//串码输入有误

                richTextBox1.AppendText(string.Format("\r\n{0}: {1}-{2}", DateTime.Now.ToString("HH:mm:ss"), useName, result));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value += "cdk错误 ";//更新结果
                dataGridView2.SelectedRows[0].Cells["ColumnResult"].Value = "cdk错误";//更新结果
                dataGridView2.SelectedRows[0].Cells["ColumnTime"].Value = DateTime.Now.ToString("HH:mm:ss");//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "兑换cdk_cdk错误.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + password + "----" + authenticode + "----" + "cdk错误" + "----" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss"));
                sw.Close();
                aFile.Close();
                isbtok = 5;
            }
            else if (result == "此串码已经兑换过了，快去购买促销装再来兑换吧！如有疑问，请拨打客服热线：4000647746" )
            {//串码已使用

                richTextBox1.AppendText(string.Format("\r\n{0}: {1}-{2}", DateTime.Now.ToString("HH:mm:ss"), useName, result));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value += "cdk已使用 ";//更新结果
                dataGridView2.SelectedRows[0].Cells["ColumnResult"].Value = "cdk已使用";//更新结果
                dataGridView2.SelectedRows[0].Cells["ColumnTime"].Value = DateTime.Now.ToString("HH:mm:ss");//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "兑换cdk_cdk已使用.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + password + "----" + authenticode + "----" + "ccdk已使用" + "----" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss"));
                sw.Close();
                aFile.Close();
                isbtok = 6;
            }
            else if(result=="您可能需要休息一会儿，30分钟后再来试吧！如有疑问，请拨打客服热线：4000647746。")
            {//其他错误

                richTextBox1.AppendText(string.Format("\r\n{0}: {1}-{2}", DateTime.Now.ToString("HH:mm:ss"), useName, result));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value += "账号需要休息 ";//更新结果
                dataGridView2.SelectedRows[0].Cells["ColumnResult"].Value = "";//更新结果
                dataGridView2.SelectedRows[0].Cells["ColumnTime"].Value = DateTime.Now.ToString("HH:mm:ss");//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "兑换cdk_账号需要休息.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + password + "----" + authenticode + "----" + result + "----" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss"));
                sw.Close();
                aFile.Close();
                isbtok = 888;
            }
            else if (result == "已经达到每日兑换5次的上限咯，明天再来吧！ 如有疑问，请拨打客服热线：4000647746。")
            {//兑换5次的上限

                richTextBox1.AppendText(string.Format("\r\n{0}: {1}-{2}", DateTime.Now.ToString("HH:mm:ss"), useName, result));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value += "*达到每日兑换上限*";//更新结果
                dataGridView2.SelectedRows[0].Cells["ColumnResult"].Value = "*达到每日兑换上限*";//更新结果
                dataGridView2.SelectedRows[0].Cells["ColumnTime"].Value = DateTime.Now.ToString("HH:mm:ss");//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "兑换cdk_5次上限.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + password + "----" + authenticode + "----" + result + "----" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss"));
                sw.Close();
                aFile.Close();
                isbtok = 888;
            }
            else 
            {
                richTextBox1.AppendText(string.Format("\r\n{0}: {1}-{2}", DateTime.Now.ToString("HH:mm:ss"), useName, result + "\r\n返回错误，尝试继续兑换..."));
                isbtok = 999;
            }
            
        }


        /// <summary>
        /// 注册账号
        /// </summary>
        private void RegisterUserID()
        {
            if (numIndexOfDatagridview1Row >= dataGridView1.RowCount)
            {
                //导入的帐号注册完成
                MessageBox.Show("帐号全部查找完成！", "提示");
                return;
            }

            useName = dataGridView1.SelectedRows[0].Cells["ColumnPhoneNum"].Value.ToString();
            //password = dataGridView1.SelectedRows[0].Cells["ColumnPwd"].Value.ToString();
            //useEmail = dataGridView1.SelectedRows[0].Cells["ColumnEmail"].Value.ToString();

            //开始注册
            item = new HttpItem()
            {//登录验证
                URL = duankou_url + "Account/RegisterUserID",//URL     必需项    
                Method = "POST",
                Cookie = cookie,//字符串Cookie     可选项
                //用户代理设置为手机浏览器
                UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.110 Safari/537.36",
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写 
                Referer = duankou_url + "Account/Register",
                Postdata = "phone=" + useName + "&valicode=" + captcha,//Post数据     可选项GET时不需要写
                Timeout = 100000,//连接超时时间     可选项默认为100000    
                ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8",//返回类型    可选项有默认值   
            };

            resultitem = http.GetHtml(item);
            string result = resultitem.Html;

            if (result == "1")
            {//注册成功
                string pwd = useName.Substring(5);
                richTextBox1.AppendText(string.Format("\r\n{0}: {1} - 注册成功！", DateTime.Now.ToString("HH:mm:ss"), useName));

                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "注册成功";//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "注册账号_成功.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + pwd + "----" + DateTime.Now.ToString("HH:mm:ss"));
                sw.Close();
                aFile.Close();
                lgnsuccesscount++;
                label_lgnsuccess.Text = lgnsuccesscount.ToString();
                isbtok = 1;
            }
            else if (result == "2")
            {//手机号已注册
                richTextBox1.AppendText(string.Format("\r\n{0}: {1} - 手机号已注册！", DateTime.Now.ToString("HH:mm:ss"), useName));

                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "手机号已注册";//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "注册账号_失败_手机号已注册.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + "手机号已注册----" + DateTime.Now.ToString("HH:mm:ss"));
                sw.Close();
                aFile.Close();
                lgnerrcount++;
                label_lgnerr.Text = lgnerrcount.ToString();
                isbtok = 2;
            }
            else
            {//验证码错误
                isbtok = 0;
            }
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        private void PwdChange()
        {
            useName = dataGridView1.SelectedRows[0].Cells["ColumnPhoneNum"].Value.ToString();
            password = dataGridView1.SelectedRows[0].Cells["ColumnPwd"].Value.ToString();
            string newPwd = "";
            if (checkBox_sjpwd.Checked)
            {
                newPwd = GenerateRandomNumber(pwd_weishu, 1);
            }
            else
            {
                newPwd = textBox_newpwd.Text;
            }
            cookie = dataGridView1.SelectedRows[0].Cells["Column_cookie"].Value.ToString();
            if (cookie == "")
            {
                cookie = get_cookie();
            }
            string result = LoginOn(useName, password,cookie);
            if (result == "true")
            {

                item = new HttpItem()
                {//兑换cdk
                    URL = duankou_url + "WinPrize/PwdChange",//URL     必需项    
                    Method = "POST",
                    Cookie = cookie,//字符串Cookie     可选项
                    //用户代理设置为手机浏览器
                    UserAgent = "Mozilla/5.0 (iPhone; U; CPU iPhone OS 3_0 like Mac OS X; en-us) AppleWebKit/528.18 (KHTML, like Gecko) Version/4.0 Mobile/7A341 Safari/528.16",
                    IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写 
                    Referer = duankou_url + "WinPrize/PwdChangeIndex",
                    Postdata = "oldPwd=" + password + "&newPwd=" + newPwd,//Post数据     可选项GET时不需要写
                    Timeout = 100000,//连接超时时间     可选项默认为100000    
                    ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",//返回类型    可选项有默认值   
                };
                resultitem = http.GetHtml(item);

                if (resultitem.Html == "0")
                {//修改密码成功

                    richTextBox1.AppendText(string.Format("\r\n{0}: {1} - 修改密码成功！", DateTime.Now.ToString("HH:mm:ss"), useName));
                    dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "修改密码成功:" + newPwd;//更新结果
                    FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "修改密码_成功.txt", FileMode.Append, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(aFile);
                    sw.WriteLine(useName + "----" + newPwd + "----" + DateTime.Now.ToString("HH:mm:ss"));
                    sw.Close();
                    aFile.Close();
                    lgnsuccesscount++;
                    label_lgnsuccess.Text = lgnsuccesscount.ToString();
                    isbtok = 1;
                }
                else
                { //修改密码失败
                    richTextBox1.AppendText(string.Format("\r\n{0}: {1} - 修改密码失败！", DateTime.Now.ToString("HH:mm:ss"), useName));
                    dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "修改密码失败:" + newPwd;//更新结果
                    FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "修改密码_失败.txt", FileMode.Append, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(aFile);
                    sw.WriteLine(useName + "----" + password + "----修改密码失败----" + DateTime.Now.ToString("HH:mm:ss"));
                    sw.Close();
                    aFile.Close();
                    lgnerrcount++;
                    label_lgnerr.Text = lgnerrcount.ToString();
                    isbtok = 2;
                }

                

            }
            else if (result == "false")
            {//登录失败
                richTextBox1.AppendText(string.Format("\r\n{0}: {1} - 登录失败！", DateTime.Now.ToString("HH:mm:ss"), useName));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "登录失败";//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "修改密码_登录失败.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + password + "----" + DateTime.Now.ToString("HH:mm:ss"));
                sw.Close();
                aFile.Close();
                lgnerrcount++;
                label_lgnerr.Text = lgnerrcount.ToString();
                isbtok = 3;
            }
        }

        /// <summary>
        /// 渴望币兑换-登录
        /// </summary>
        private void CashLotteryExchange()
        {
            richTextBox1.AppendText(string.Format("\r\n{0}: {1} - 登录成功", DateTime.Now.ToString("HH:mm:ss"), useName));
            useName = dataGridView1.SelectedRows[0].Cells["ColumnPhoneNum"].Value.ToString();
            password = dataGridView1.SelectedRows[0].Cells["ColumnPwd"].Value.ToString();
            cookie = dataGridView1.SelectedRows[0].Cells["Column_cookie"].Value.ToString();
            if (cookie == "")
            {
                cookie = get_cookie();
            }
            string result = LoginOn(useName, password,cookie);
            if (result == "true")
            {//登录成功
                isloginok = true;
                CashLotteryExchange_bt(prizeID);
            }
            else if (result == "false")
            {//登录失败
                richTextBox1.AppendText(string.Format("\r\n{0}: {1} - 登录失败,尝试继续登录...", DateTime.Now.ToString("HH:mm:ss"), useName));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "登录失败";//更新结果
                isloginok = false;
                CashLotteryExchange();//登录失败,继续登录
            }


            
        }

        /// <summary>
        /// 渴望币兑换-提交
        /// </summary>
        /// <param name="prizeID">子选项</param>
        private void CashLotteryExchange_bt(string prizeID)
        {
            item = new HttpItem()
            {//兑换cdk
                URL = duankou_url + "Home/CashLotteryExchange",//URL     必需项    
                Method = "POST",
                Cookie = cookie,//字符串Cookie     可选项
                //用户代理设置为手机浏览器
                UserAgent = "Mozilla/5.0 (iPhone; U; CPU iPhone OS 3_0 like Mac OS X; en-us) AppleWebKit/528.18 (KHTML, like Gecko) Version/4.0 Mobile/7A341 Safari/528.16",
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写 
                Referer = duankou_url + "Home/Index",
                Postdata = "prizeID=" + prizeID,//Post数据     可选项GET时不需要写
                Timeout = 100000,//连接超时时间     可选项默认为100000    
                ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8",//返回类型    可选项有默认值   
            };
            resultitem = http.GetHtml(item);
            string result = resultitem.Html;
            if (result == "")
            {
                richTextBox1.AppendText(string.Format("\r\n{0}: 请重新登录", DateTime.Now.ToString("HH:mm:ss")));
                isbtok = 88;
            }
            else if (result == "-1")
            {
                richTextBox1.AppendText(string.Format("\r\n{0}: 渴望币不足", DateTime.Now.ToString("HH:mm:ss")));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "渴望币不足";//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "渴望币兑换_渴望币不足.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + password + "----" + DateTime.Now.ToString("HH:mm:ss"));
                sw.Close();
                aFile.Close();
                lgnsuccesscount++;
                label_lgnsuccess.Text = lgnsuccesscount.ToString();
                isbtok = -1;
            }
            else if (result == "-2")
            {
                richTextBox1.AppendText(string.Format("\r\n{0}: 你参加活动的总次数已经超过50次，不能继续参加", DateTime.Now.ToString("HH:mm:ss")));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "你参加活动的总次数已经超过50次，不能继续参加";//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "渴望币兑换_超过50次.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + password + "----" + DateTime.Now.ToString("HH:mm:ss"));
                sw.Close();
                aFile.Close();
                lgnsuccesscount++;
                label_lgnsuccess.Text = lgnsuccesscount.ToString();
                isbtok = -2;
            }
            else if (result == "-3")
            {
                richTextBox1.AppendText(string.Format("\r\n{0}: 今日您已参与过5次活动，把机会留给其他小伙伴吧", DateTime.Now.ToString("HH:mm:ss")));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "今日您已参与过5次活动，把机会留给其他小伙伴吧";//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "渴望币兑换_已参与过5次活动.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + password + "----" + DateTime.Now.ToString("HH:mm:ss"));
                sw.Close();
                aFile.Close();
                lgnsuccesscount++;
                label_lgnsuccess.Text = lgnsuccesscount.ToString();
                isbtok = -3;
            }
            else if (result == "-4")
            {
                timer1.Stop();//兑换停止
                richTextBox1.AppendText(string.Format("\r\n{0}: 今日已全部兑换完，请明日再来", DateTime.Now.ToString("HH:mm:ss")));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "今日已全部兑换完，请明日再来";//更新结果

                isbtok = -4;
            }
            else if (result == "-5")
            {
                richTextBox1.AppendText(string.Format("\r\n{0}: 该手机号1个月话费中奖上限超过20次", DateTime.Now.ToString("HH:mm:ss")));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "该手机号1个月话费中奖上限超过20次";//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "渴望币兑换_该手机号1个月话费中奖上限超过20次.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + password + "----" + DateTime.Now.ToString("HH:mm:ss"));
                sw.Close();
                aFile.Close();
                lgnsuccesscount++;
                label_lgnsuccess.Text = lgnsuccesscount.ToString();
                isbtok = -5;
            }
            else if (result == "-6")
            {
                richTextBox1.AppendText(string.Format("\r\n{0}: 今日已兑换过，请明日再来", DateTime.Now.ToString("HH:mm:ss")));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "今日已兑换过，请明日再来";//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "渴望币兑换_今日已兑换过.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + password + "----" + DateTime.Now.ToString("HH:mm:ss"));
                sw.Close();
                aFile.Close();
                lgnsuccesscount++;
                label_lgnsuccess.Text = lgnsuccesscount.ToString();
                isbtok = -6;
            }
            else if (result == "-7")
            {
                richTextBox1.AppendText(string.Format("\r\n{0}: 网络异常", DateTime.Now.ToString("HH:mm:ss")));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "网络异常";//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "渴望币兑换_网络异常.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + password + "----" + DateTime.Now.ToString("HH:mm:ss"));
                sw.Close();
                aFile.Close();
                lgnsuccesscount++;
                label_lgnsuccess.Text = lgnsuccesscount.ToString();
                isbtok = -7;
            }
            else
            {
                string type_t = "";
                if(prizeID == "10")
                {
                    type_t = "迪士尼门票";
                }
                else if(prizeID=="11")
                {
                    type_t="1元话费";
                }
                richTextBox1.AppendText(string.Format("\r\n{0}: 兑换成功 返回：{1}", DateTime.Now.ToString("HH:mm:ss"),result));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "兑换成功:"+result;//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "渴望币兑换_兑换成功.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + password + "----" + type_t + "返回：" + result + "----" + DateTime.Now.ToString("HH:mm:ss"));
                sw.Close();
                aFile.Close();
                lgnsuccesscount++;
                label_lgnsuccess.Text = lgnsuccesscount.ToString();
                isbtok = 8;
            }

        }

        /// <summary>
        /// 秒杀
        /// </summary>
        private void SecKillAward()
        {
            useName = dataGridView1.SelectedRows[0].Cells["ColumnPhoneNum"].Value.ToString();
            password = dataGridView1.SelectedRows[0].Cells["ColumnPwd"].Value.ToString();
            cookie = dataGridView1.SelectedRows[0].Cells["Column_cookie"].Value.ToString();
            if (cookie == "")
            {
                cookie = get_cookie();
            }
            string result = LoginOn(useName, password,cookie);
            if (result == "true")
            {//列出详细的账号信息
                richTextBox1.AppendText(string.Format("\r\n{0}: {1} - 登录成功！", DateTime.Now.ToString("HH:mm:ss"), useName));
                HttpHelper http = new HttpHelper();
                HttpItem itemGet = new HttpItem()
                {
                    URL = duankou_url + "SecurityCode/SecKillCode",//URL     必需项    
                    Method = "get",
                    Cookie = cookie,//字符串Cookie     可选项
                    //用户代理设置为手机浏览器
                    UserAgent = "Mozilla/5.0 (iPhone; U; CPU iPhone OS 3_0 like Mac OS X; en-us) AppleWebKit/528.18 (KHTML, like Gecko) Version/4.0 Mobile/7A341 Safari/528.16",
                    IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写 
                    Referer = duankou_url + "Home/Index",
                    //Postdata = "&oldvalue=" + captcha,//Post数据     可选项GET时不需要写
                    Timeout = 100000,//连接超时时间     可选项默认为100000    
                    ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                    ContentType = "application/x-www-form-urlencoded",//返回类型    可选项有默认值 
                };
                pictureBox1.Image = http.GetImage(itemGet);//加载秒杀验证码

                textBox_yzm.Text="";
                is_yzm_TextChanged = 0;
            }
        }

        /// <summary>
        /// 秒杀提交
        /// </summary>
        private void SecKillAward_bt()
        { //秒杀提交
            item = new HttpItem()
            {//兑换cdk
                URL = duankou_url + "Activity/SecKillAward",//URL     必需项    
                Method = "POST",
                Cookie = cookie,//字符串Cookie     可选项
                //用户代理设置为手机浏览器
                UserAgent = "Mozilla/5.0 (iPhone; U; CPU iPhone OS 3_0 like Mac OS X; en-us) AppleWebKit/528.18 (KHTML, like Gecko) Version/4.0 Mobile/7A341 Safari/528.16",
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写 
                Referer = duankou_url + "Home/Index",
                Postdata = "code=" + captcha,//Post数据     可选项GET时不需要写
                Timeout = 100000,//连接超时时间     可选项默认为100000    
                ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8",//返回类型    可选项有默认值   
            };
            resultitem = http.GetHtml(item);
            string result = resultitem.Html;
            if (result == "0")
            {
                richTextBox1.AppendText(string.Format("\r\n{0}: {1} - 秒杀成功！返回：{2}", DateTime.Now.ToString("HH:mm:ss"), useName, result)); 
            }
            else if (result == "1")
            {
                timer1.Stop();
                richTextBox1.AppendText(string.Format("\r\n{0}: {1} - 秒杀失败，返回：{2}", DateTime.Now.ToString("HH:mm:ss"), useName, result)); 
            }
            else if (result == "6")
            {
                //timer1.Stop();
                richTextBox1.AppendText(string.Format("\r\n{0}: {1} - 秒杀还没有开始，返回：{2}", DateTime.Now.ToString("HH:mm:ss"), useName, result));
            }
            else if (result == "4")
            {
                //timer1.Stop();
                richTextBox1.AppendText(string.Format("\r\n{0}: {1} - 你已经参加过该活动了，返回：{2}", DateTime.Now.ToString("HH:mm:ss"), useName, result));
            }
            else if (result == "5")
            {
                timer1.Stop();
                richTextBox1.AppendText(string.Format("\r\n{0}: {1} - 渴望币不足，返回：{2}", DateTime.Now.ToString("HH:mm:ss"), useName, result));
            }
            else if (result == "11")
            {
                timer1.Stop();
                richTextBox1.AppendText(string.Format("\r\n{0}: {1} - 验证码错误，返回：{2}", DateTime.Now.ToString("HH:mm:ss"), useName, result));
            }
        }

        /// <summary>
        /// 心跳-维持在线状态
        /// </summary>
        private void CheckSession()
        {
            item = new HttpItem()
            {//兑换cdk
                URL = duankou_url + "Lottery/CheckSession",//URL     必需项    
                Method = "POST",
                Cookie = cookie,//字符串Cookie     可选项
                //用户代理设置为手机浏览器
                UserAgent = "Mozilla/5.0 (iPhone; U; CPU iPhone OS 3_0 like Mac OS X; en-us) AppleWebKit/528.18 (KHTML, like Gecko) Version/4.0 Mobile/7A341 Safari/528.16",
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写 
                Referer = duankou_url + "Lottery/Index",
                Postdata = "ticket=" + authenticode + "&code=" + captcha,//Post数据     可选项GET时不需要写
                Timeout = 100000,//连接超时时间     可选项默认为100000    
                ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                //ContentType = "application/x-www-form-urlencoded; charset=UTF-8",//返回类型    可选项有默认值   
            };
            resultitem = http.GetHtml(item);
            string result = resultitem.Html;
        }

        
        //取当前webBrowser登录后的Cookie值   
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        static extern bool InternetGetCookieEx(string pchURL, string pchCookieName, StringBuilder pchCookieData, ref int pcchCookieData, int dwFlags, object lpReserved);

        /// <summary>
        /// 取出Cookie，当登录后才能取 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static string GetCookieString(string url)
        {
            // Determine the size of the cookie      
            int datasize = 256;
            StringBuilder cookieData = new StringBuilder(datasize);
            if (!InternetGetCookieEx(url, null, cookieData, ref datasize, 0x00002000, null))
            {
                if (datasize < 0)
                    return null;
                // Allocate stringbuilder large enough to hold the cookie    
                cookieData = new StringBuilder(datasize);
                if (!InternetGetCookieEx(url, null, cookieData, ref datasize, 0x00002000, null))
                    return null;
            }
            return cookieData.ToString();
        }

        public enum ShowCommands : int
        {

            SW_HIDE = 0,

            SW_SHOWNORMAL = 1,

            SW_NORMAL = 1,

            SW_SHOWMINIMIZED = 2,

            SW_SHOWMAXIMIZED = 3,

            SW_MAXIMIZE = 3,

            SW_SHOWNOACTIVATE = 4,

            SW_SHOW = 5,

            SW_MINIMIZE = 6,

            SW_SHOWMINNOACTIVE = 7,

            SW_SHOWNA = 8,

            SW_RESTORE = 9,

            SW_SHOWDEFAULT = 10,

            SW_FORCEMINIMIZE = 11,

            SW_MAX = 11

        }
        [DllImport("shell32.dll")]
        //调用IE API进行清除浏览器数据
        /*
        其中ClearMyTracksByProcess 可进行选择设置 ：

        //Temporary Internet Files  （Internet临时文件）
        //RunDll32.exe InetCpl.cpl,ClearMyTracksByProcess 8
         * 
        //Cookies
        //RunDll32.exe InetCpl.cpl,ClearMyTracksByProcess 2
         * 
        //History (历史记录)
        //RunDll32.exe InetCpl.cpl,ClearMyTracksByProcess 1
         * 
        //Form. Data （表单数据）
        //RunDll32.exe InetCpl.cpl,ClearMyTracksByProcess 16
         * 
        //Passwords (密码）
        //RunDll32.exe InetCpl.cpl,ClearMyTracksByProcess 32
         * 
        //Delete All  （全部删除）
        //RunDll32.exe InetCpl.cpl,ClearMyTracksByProcess 255
         */
        static extern IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, ShowCommands nShowCmd);
        

        /// <summary>
        /// 获取cookie
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void webBrowser1_DocumentCompleted_1(object sender, WebBrowserDocumentCompletedEventArgs e)
        //{//获取cookie
        //    if (webBrowser1.Url.ToString() == duankou_url)
        //    {
        //        cookie = GetCookieString(duankou_url);//获得cookie 

        //        webBrowser1.Document.Cookie.Remove(0, (webBrowser1.Document.Cookie.Count() - 1));
        //        richTextBox1.BeginInvoke(new EventHandler(delegate
        //        {
        //            richTextBox1.AppendText(string.Format("\r\n{0}: 获得新cookie", DateTime.Now.ToString("HH:mm:ss")));
        //        }));
        //        //dataGridView1.BeginInvoke(new ErrorEventHandler(delegate
        //        //{
        //            dataGridView1.SelectedRows[0].Cells["Column_cookie"].Value = cookie;//写入新cookie
        //        //}));

                
        //    }
        //}

        
        /// <summary>   
        /// 获取WebBrowser指定的图片   
        /// </summary>   
        /// <param name="webBrowser">需要获取图片的WebBrowser</param>   
        /// <param name="imgID">指定的图片的id(优先查找指定id)</param>   
        /// <param name="imgSrc">指定的图片的src，支持模糊查询</param>   
        /// <param name="imgAlt">指定的图片的src， 支持模糊查询</param>   
        /// <returns></returns> 
        public static Image GetRegCodePic(WebBrowser webBrowser, String imgID, String imgSrc, String imgAlt)
        {

            HTMLDocument doc = (HTMLDocument)webBrowser.Document.DomDocument;
            HTMLBody body = (HTMLBody)doc.body;
            IHTMLControlRange rang = (IHTMLControlRange)body.createControlRange();
            IHTMLControlElement img;

            // 如果没有图片的ID,通过Src或Alt中的关键字来取   
            if (imgID.Length == 0)
            {
                Int32 ImgNum = GetPicIndex(webBrowser, ref imgSrc, ref imgAlt);

                if (ImgNum == -1)
                    return null;

                img = (IHTMLControlElement)webBrowser.Document.Images[ImgNum].DomElement;
            }
            else
                img = (IHTMLControlElement)webBrowser.Document.All[imgID].DomElement;

            rang.add(img);
            rang.execCommand("Copy", false, null);
            Image regImg = Clipboard.GetImage();
            Clipboard.Clear();
            return regImg;
        }
        /// <summary>   
        /// 获取WebBrowser指定图片的索引   
        /// </summary>   
        /// <param name="webBrowser">指定的WebBrowser</param>   
        /// <param name="imgSrc">指定的图片src，支持模糊查询</param>   
        /// <param name="imgAlt">指定的图片alt，支持模糊查询</param>   
        /// <returns></returns>   
        public static Int32 GetPicIndex(WebBrowser webBrowser, ref String imgSrc, ref String imgAlt)
        {
            IHTMLImgElement img;

            // 获取所有的Image元素   
            for (Int32 i = 0; i < webBrowser.Document.Images.Count; i++)
            {
                img = (IHTMLImgElement)webBrowser.Document.Images[i].DomElement;

                if (imgAlt.Length == 0)
                {
                    if (img.src.IndexOf(imgSrc) >= 0)
                        return i;
                }
                else
                {
                    if (imgSrc.Length == 0)
                    {
                        // 当imgSrc为空时，只匹配imgAlt   
                        if (img.alt.IndexOf(imgAlt) >= 0)
                            return i;
                    }
                    else
                    {
                        // 当imgSrc不为空时，匹配imgAlt和imgSrc任意一个   
                        if (img.alt.IndexOf(imgAlt) >= 0 || img.src.IndexOf(imgSrc) >= 0)
                            return i;
                    }
                }
            }
            return -1;
        }
        /// <summary>
        /// 远程打验证码
        /// 远程打码 可选
        /// </summary>
        /// <param name="strUrl">请求验证码的url</param>
        private void getVorityImage()
        {
            HttpHelper http = new HttpHelper();
            HttpItem itemGet = new HttpItem()
            {
                URL = duankou_url + "SecurityCode/Code",//URL     必需项    
                Method = "get",
                Cookie = cookie,//字符串Cookie     可选项
                //用户代理设置为手机浏览器
                UserAgent = "Mozilla/5.0 (iPhone; U; CPU iPhone OS 3_0 like Mac OS X; en-us) AppleWebKit/528.18 (KHTML, like Gecko) Version/4.0 Mobile/7A341 Safari/528.16",
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写 
                Referer = duankou_url + "Home/Index",
                //Postdata = "&oldvalue=" + captcha,//Post数据     可选项GET时不需要写
                Timeout = 100000,//连接超时时间     可选项默认为100000    
                ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                ContentType = "application/x-www-form-urlencoded",//返回类型    可选项有默认值 
            };
            pictureBox1.Image = http.GetImage(itemGet);

            //***********是否自动识别验证码*****************
            if(checkBox_yzm_zds.Checked)
            {
                Thread t = new Thread(new ThreadStart(delegate
                {//后台线程自动识别
                    StringBuilder Result = new StringBuilder('\0', 256);

                    MemoryStream ms = new MemoryStream();
                    pictureBox1.Image.Save(ms, ImageFormat.Gif);
                    byte[] buffer = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(buffer, 0, buffer.Length);
                    ms.Flush();
                    bool is_sb_ok = GetVcodeFromBuffer(index, buffer, buffer.Length, Result);
                    ms.Close();
                    ms.Dispose();
                    //pictureBox1.Image.Save("yzm.gif", ImageFormat.Gif);
                    //bool is_sb_ok = GetVcodeFromFile(index, "yzm.gif", Result);
                    textBox_yzm.BeginInvoke(new EventHandler(delegate
                    {
                        if (is_sb_ok)
                        {
                            if (Result.ToString().Length == 4)
                            {
                            //    richTextBox1.BeginInvoke(new EventHandler(delegate
                            //    {
                            //        richTextBox1.AppendText(string.Format("\r\n{0}: 识别结果-{1}！", DateTime.Now.ToString("HH:mm:ss"), Result.ToString()));
                            //    }));
                                textBox_yzm.Text = Result.ToString();
                            }
                            else
                            {
                                getVorityImage();//换验证码图片
                            }
                        }
                        else
                        {
                            //richTextBox1.BeginInvoke(new EventHandler(delegate
                            //{
                            //    richTextBox1.AppendText(string.Format("\r\n{0}: 识别失败！", DateTime.Now.ToString("HH:mm:ss")));
                            //}));
                            getVorityImage();//换验证码图片
                        }
                    }));

                }));
                t.IsBackground = true;
                t.Start();
            }

            //***********判断UU打码平台可用性，即对接打码*****************
            else if (isDamaLogin)
            {
                //string captchaURL = "http://www.ksfteasp.com/CheckCode.aspx";
                //下面是软件id对应的dll校验key。在开发者后台-我的软件里面可以查的到。
                //string strCheckKey = "65BD22F2-4576-4FFE-B7FB-A0B1807BCC37".ToUpper();

                //开启后台打码线程
                Thread t = new Thread(new ThreadStart(delegate
                {
                    richTextBox1.BeginInvoke(new EventHandler(delegate
                    {
                        richTextBox1.AppendText(string.Format("\r\n{0}: 等待uu远程验证码...", DateTime.Now.ToString("HH:mm:ss")));
                    }));

                    //Thread.Sleep(200);//延迟
                    string resultCode="";
                    StringBuilder captchaResult = new StringBuilder();
                    MemoryStream ms = new MemoryStream();
                    pictureBox1.Image.Save(ms, ImageFormat.Jpeg);
                    byte[] buffer = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(buffer, 0, buffer.Length);
                    ms.Flush();
                    //新版本dll需要预先分配50个字节的空间，否则dll会崩溃！！！！
                    StringBuilder res = new StringBuilder(50);
                    int codeId = Wrapper.uu_recognizeByCodeTypeAndBytes(buffer, buffer.Length, codeType, res);
                    resultCode = CheckResult(res.ToString(), Convert.ToInt32(SoftID.ToString().Trim()), codeId, strCheckKey);
                    m_codeID = codeId;
                    ms.Close();
                    ms.Dispose();
                    captcha = resultCode;//复制给验证码
                    is_yzm_ok = true;
                    richTextBox1.BeginInvoke(new EventHandler(delegate
                    {
                        richTextBox1.AppendText(string.Format("\r\n{0}: UU打码 - {1}", DateTime.Now.ToString("HH:mm:ss"), resultCode));
                    }));
                    textBox_yzm.BeginInvoke(new EventHandler(delegate
                    {
                        textBox_yzm.Text = resultCode;//触发操作
                    }));
                    
                }));
                t.IsBackground = true;
                t.Start();
            }
            else
            {
                richTextBox1.BeginInvoke(new EventHandler(delegate
                {
                    richTextBox1.AppendText(string.Format("\r\n{0}: {1}", DateTime.Now.ToString("HH:mm:ss"), "请输入验证码..."));
                }));
                

            }

        }
        /// <summary>
        /// 单条数据正则获取
        /// </summary>
        /// <param name="code">源码</param>
        /// <param name="ruler">正则表达式</param>
        /// <param name="id">返回结果下标</param>
        /// <returns>string</returns>
        private static string GetRegex(string code, string ruler, int id)
        {
            string matchValue = "";
            Regex r = new Regex(ruler, RegexOptions.Singleline);
            Match m = r.Match(code);
            if (m.Success)
            {
                matchValue = m.Groups[id].ToString();
            }
            return matchValue;
        }
        /// <summary>
        /// 字节流转图片
        /// </summary>
        /// <param name="Bytes">源字节</param>
        /// <returns></returns>
        private Image byteArrayToImage(byte[] Bytes)
        {
            using (MemoryStream ms = new MemoryStream(Bytes))
            {
                Image outputImg = Image.FromStream(ms);
                return outputImg;
            }
        }
        /// <summary>
        /// 登录uu
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_uu_Click(object sender, EventArgs e)
        {
            if ((textBox_uuname.Text == "" || textBox_uupwd.Text == "") && checkBox_uu.Checked)
            {
                MessageBox.Show("请输入用户名和密码！", "错误");
                checkBox_uu.Checked = false;
            }
            else if (checkBox_uu.Checked)
            {
                //dll校验
                string DLLPath = System.Environment.CurrentDirectory + "\\UUWiseHelper.dll";
                string strSoftID = SoftID.ToString().Trim();
                int softId = int.Parse(strSoftID);
                string softKey = SoftKey.ToString().Trim();
                Guid guid = Guid.NewGuid();
                string strGuid = guid.ToString().Replace("-", "").Substring(0, 32).ToUpper();
                //string DLLPath = "E:\\work\\UUWiseHelper 新版http协议\\输出目录\\UUWiseHelper.dll";
                string strDllMd5 = GetFileMD5(DLLPath);
                CRC32 objCrc32 = new CRC32();
                string strDllCrc = String.Format("{0:X}", objCrc32.FileCRC(DLLPath));
                //CRC不足8位，则前面补0，补足8位
                int crcLen = strDllCrc.Length;
                if (crcLen < 8)
                {
                    int miss = 8 - crcLen;
                    for (int i = 0; i < miss; ++i)
                    {
                        strDllCrc = "0" + strDllCrc;
                    }
                }
                //下面是软件id对应的dll校验key。在开发者后台-我的软件里面可以查的到。
                string strCheckKey = "65BD22F2-4576-4FFE-B7FB-A0B1807BCC37".ToUpper();
                string yuanshiInfo = strSoftID + strCheckKey + strGuid + strDllMd5.ToUpper() + strDllCrc.ToUpper();
                string localInfo = MD5Encoding(yuanshiInfo);
                StringBuilder checkResult = new StringBuilder();
                Wrapper.uu_CheckApiSign(softId, softKey, strGuid, strDllMd5, strDllCrc, checkResult);
                string strCheckResult = checkResult.ToString();
                if (localInfo.Equals(strCheckResult))
                {             
                    Wrapper.uu_login(textBox_uuname.Text, textBox_uupwd.Text);//先登录，然后才能获得剩余题分。
                    label7.Text = Wrapper.uu_getScore(textBox_uuname.Text, textBox_uupwd.Text).ToString();
                    isDamaLogin = true;//uu登录成功并使用
                    textBox_yzm.Visible = false;
                    pictureBox1.Visible = false;
                    label3.Visible = false;
                    richTextBox1.AppendText(string.Format("\r\n{0}: 成功登录uu打码平台 - 题分:{1}", DateTime.Now.ToString("HH:mm:ss"), label7.Text));
                    //保存配置文件
                    FileStream aFile = new FileStream("dama.bin", FileMode.Create, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(aFile);
                    sw.WriteLine(textBox_uuname.Text);
                    sw.WriteLine(textBox_uupwd.Text);
                    sw.Close();
                    aFile.Close();
                    
                    //不显示验证码相关信息
                    //pictureBox1.Visible = false;
                    //label3.Visible = false;
                    //textBox_yzm.Visible = false;
                }
                else
                {
                    richTextBox1.AppendText(string.Format("\r\n{0}: {1}", DateTime.Now.ToString("HH:mm:ss"), "校验失败！请使用官方提供UUWiseHelper.dll"));
                    checkBox_uu.Checked = false;
                }
            }
            else
            {
                isDamaLogin = false;//uu登录成功并使用                
            }
        }
        private bool CheckDll(string softid, string softkey, string dllPath)
        {
            String[] URLs = { "http://v.uuwise.com/service/verify.aspx",
                             "http://v.uuwise.net/service/verify.aspx",
                             "http://v.uudama.com/service/verify.aspx",
                             "http://v.uudati.cn/service/verify.aspx",
                             "http://v.taskok.com/service/verify.aspx"};
            bool isPass = false;
            foreach (String url in URLs)
            {
                try
                {
                    HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                    request.Method = "POST";
                    //设置http请求的超时时间为10s
                    request.Timeout = 10 * 1000;
                    request.UserAgent = "VersionClient";
                    request.ContentType = "application/x-www-form-urlencoded";
                    //构造post数据流
                    //String strPostData = "SID=2097&dllkey=";
                    String strPostData = "SID=" + softid + "&dllkey=";
                    String strDllKey = GetFileMD5(dllPath);
                    strPostData += strDllKey + "&key=" + MD5Encoding(softid + strDllKey.ToUpper());

                    byte[] data = Encoding.ASCII.GetBytes(strPostData);
                    request.ContentLength = data.Length;
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }

                    //获取返回结果
                    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                    {
                        Encoding responseEncoding = Encoding.GetEncoding(response.CharacterSet);
                        using (Stream stream = response.GetResponseStream())
                        {
                            using (StreamReader objReader = new StreamReader(stream, responseEncoding))
                            {
                                String strRes = objReader.ReadToEnd();
                                if (strRes.Equals(MD5Encoding(softid + softkey.ToUpper())))
                                {
                                    isPass = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            if (isPass)
                richTextBox1.AppendText(string.Format("\r\n{0}: {1}", DateTime.Now.ToString("HH:mm:ss"), "UUWiseHelper.dll校验成功！登录打码..."));
            else
                richTextBox1.AppendText(string.Format("\r\n{0}: {1}", DateTime.Now.ToString("HH:mm:ss"), "UUWiseHelper.dll校验失败！"));
            return isPass;
        }
        /// <summary>
        /// MD5 加密字符串
        /// </summary>
        /// <param name="rawPass">源字符串</param>
        /// <returns>加密后字符串</returns>
        public static string MD5Encoding(string rawPass)
        {
            // 创建MD5类的默认实例：MD5CryptoServiceProvider
            MD5 md5 = MD5.Create();
            byte[] bs = Encoding.UTF8.GetBytes(rawPass);
            byte[] hs = md5.ComputeHash(bs);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hs)
            {
                // 以十六进制格式格式化
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
        #region 根据路径获取文件MD5
        /// <summary>
        /// 获取文件MD5校验值
        /// </summary>
        /// <param name="filePath">校验文件路径</param>
        /// <returns>MD5校验字符串</returns>
        private string GetFileMD5(string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] md5byte = md5.ComputeHash(fs);
            int i, j;
            StringBuilder sb = new StringBuilder(16);
            foreach (byte b in md5byte)
            {
                i = Convert.ToInt32(b);
                j = i >> 4;
                sb.Append(Convert.ToString(j, 16));
                j = ((i << 4) & 0x00ff) >> 4;
                sb.Append(Convert.ToString(j, 16));
            }
            return sb.ToString();
        }
        #endregion
        public string CheckResult(string result, int softId, int codeId, string checkKey)
        {
            //对验证码结果进行校验，防止dll被替换
            if (string.IsNullOrEmpty(result))
                return result;
            else
            {
                string[] modelReult = result.Split('_');
                //解析出服务器返回的校验结果
                string strServerKey = modelReult[0];
                string strCodeResult = modelReult[1];
                //本地计算校验结果
                string localInfo = softId.ToString() + checkKey + codeId.ToString() + strCodeResult.ToUpper();
                string strLocalKey = MD5Encoding(localInfo).ToUpper();
                //相等则校验通过
                if (strServerKey.Equals(strLocalKey))
                    return strCodeResult;
                return "结果校验不正确";
            }
        }        

        private void timer1_Tick(object sender, EventArgs e)
        {//检测
            if (dataGridView1.SelectedRows[0].Cells["Column_cookie"].Value.ToString() == "")
            {
                timer1.Stop();
                get_cookie();
            }
            if (comboBox1.SelectedIndex.ToString() == "0" && isbtok != 0)
            {//登录查询
                 isbtok = 0;//重置
                if (++numIndexOfDatagridview1Row == dataGridView1.RowCount)
                {
                    timer1.Stop();
                    MessageBox.Show("全部帐号完成！", "提示");
                    return;
                }
                else
                {

                    this.dataGridView1.Rows[numIndexOfDatagridview1Row].Selected = true;//换下一个帐号
                    if (numIndexOfDatagridview1Row > 10)
                        dataGridView1.FirstDisplayedScrollingRowIndex = numIndexOfDatagridview1Row - 10;//滑块位置更新

                }
                LoginOn_result();
            }

            else if (comboBox1.SelectedIndex.ToString() == "1")
            {//兑换cdk
                if (duihuan_count.ToString() == textBox_duihuancishu.Text.ToString() || isbtok == 888)
                { //换账号&cdk
                    num_ds_t++;
                    if (isbtok != 888 && isloginok == true && (isbtok >= 1 && isbtok <= 7))
                    {
                        isbtok = 0;//复位
                        if (++numIndexOfDatagridview2Row == dataGridView2.RowCount)
                        {
                            timer1.Stop();
                            MessageBox.Show("全部cdk完成！", "提示");
                            return;
                        }
                        else
                        {

                            this.dataGridView2.Rows[numIndexOfDatagridview2Row].Selected = true;//换下一个cdk
                            if (numIndexOfDatagridview2Row > 10)
                                dataGridView2.FirstDisplayedScrollingRowIndex = numIndexOfDatagridview2Row - 10;//滑块位置更新

                        }

                    }
                    isloginok = false;//复位
                    //isbtok = 0;//复位
                    if (++numIndexOfDatagridview1Row == dataGridView1.RowCount)
                    {
                        timer1.Stop();
                        MessageBox.Show("全部帐号完成！", "提示");
                        return;
                    }
                    else
                    {

                        this.dataGridView1.Rows[numIndexOfDatagridview1Row].Selected = true;//换下一个帐号
                        if (numIndexOfDatagridview1Row > 10)
                            dataGridView1.FirstDisplayedScrollingRowIndex = numIndexOfDatagridview1Row - 10;//滑块位置更新

                    }
                    is_huanzh_ok = 1;
                    duihuan();
                    getVorityImage();
                }
                else if (isloginok == true && (isbtok >= 1 && isbtok <= 7))
                {//只用换cdk
                    isbtok = 0;//复位
                    if (++numIndexOfDatagridview2Row == dataGridView2.RowCount)
                    {
                        timer1.Stop();
                        MessageBox.Show("全部cdk完成！", "提示");
                        return;
                    }
                    else
                    {

                        this.dataGridView2.Rows[numIndexOfDatagridview2Row].Selected = true;//换下一个cdk
                        if (numIndexOfDatagridview2Row > 10)
                            dataGridView2.FirstDisplayedScrollingRowIndex = numIndexOfDatagridview2Row - 10;//滑块位置更新

                    }
                    getVorityImage();
                }
                else if (isloginok == true && isbtok == 999)
                {//服务器错误 ，休息一下 继续
                    isbtok = 0;//复位
                    getVorityImage();
                }

            }


            else if (comboBox1.SelectedIndex.ToString() == "2" && isbtok != 0)
            {//注册账号 
                isbtok = 0;//重置状态
                if (++numIndexOfDatagridview1Row == dataGridView1.RowCount)
                {
                    timer1.Stop();
                    MessageBox.Show("全部完成！", "提示");
                    return;
                }
                else
                {

                    this.dataGridView1.Rows[numIndexOfDatagridview1Row].Selected = true;//换下一个帐号
                    if (numIndexOfDatagridview1Row > 10)
                        dataGridView1.FirstDisplayedScrollingRowIndex = numIndexOfDatagridview1Row - 10;//滑块位置更新

                }
                RegisterUserID();
            }
            else if (comboBox1.SelectedIndex.ToString() == "3" && isbtok != 0)
            {//修改密码 
                isbtok = 0;//重置状态
                if (++numIndexOfDatagridview1Row == dataGridView1.RowCount)
                {
                    timer1.Stop();
                    MessageBox.Show("全部完成！", "提示");
                    return;
                }
                else
                {

                    this.dataGridView1.Rows[numIndexOfDatagridview1Row].Selected = true;//换下一个帐号
                    if (numIndexOfDatagridview1Row > 10)
                        dataGridView1.FirstDisplayedScrollingRowIndex = numIndexOfDatagridview1Row - 10;//滑块位置更新

                }
                PwdChange();
            }
            else if (comboBox1.SelectedIndex.ToString() == "4" && isbtok != 0)
            {//渴望币兑换 
                isbtok = 0;//重置状态
                if (++numIndexOfDatagridview1Row == dataGridView1.RowCount)
                {
                    timer1.Stop();
                    MessageBox.Show("全部完成！", "提示");
                    return;
                }
                else
                {

                    this.dataGridView1.Rows[numIndexOfDatagridview1Row].Selected = true;//换下一个帐号
                    if (numIndexOfDatagridview1Row > 10)
                        dataGridView1.FirstDisplayedScrollingRowIndex = numIndexOfDatagridview1Row - 10;//滑块位置更新

                }
                CashLotteryExchange();
            }
            else if (comboBox1.SelectedIndex.ToString() == "5" && isbtok != 0)
            {//渴望币抽奖 
                isbtok = 0;//重置状态
                if (++numIndexOfDatagridview1Row == dataGridView1.RowCount)
                {
                    timer1.Stop();
                    MessageBox.Show("全部完成！", "提示");
                    return;
                }
                else
                {

                    this.dataGridView1.Rows[numIndexOfDatagridview1Row].Selected = true;//换下一个帐号
                    if (numIndexOfDatagridview1Row > 10)
                        dataGridView1.FirstDisplayedScrollingRowIndex = numIndexOfDatagridview1Row - 10;//滑块位置更新

                }
                //CashLotteryExchange();
            }

            else if (comboBox1.SelectedIndex.ToString() == "6")
            {//秒杀
                SecKillAward_bt();
            }
            else if (comboBox1.SelectedIndex.ToString() == "7"  && wenjian_ok == 1)
            {//获取cookie
                wenjian_ok = 0;//复位
                if (++numIndexOfDatagridview1Row == dataGridView1.RowCount)
                {
                    timer1.Stop();
                    MessageBox.Show("全部帐号完成！", "提示");
                    return;
                }
                else
                {

                    this.dataGridView1.Rows[numIndexOfDatagridview1Row].Selected = true;//换下一个帐号
                    if (numIndexOfDatagridview1Row > 10)
                        dataGridView1.FirstDisplayedScrollingRowIndex = numIndexOfDatagridview1Row - 10;//滑块位置更新

                }
                get_cookie();//检查cookie是否已存在
            }
        }

        private void textBox_yzm_TextChanged(object sender, EventArgs e)
        {
            if (textBox_yzm.Text.Length == yzm_weishu  && checkBox_uu.Checked==false)
            {
                captcha = textBox_yzm.Text;
                if( comboBox1.SelectedIndex.ToString() == "1")
                {
                    duihuan_bt();
                }
                else if (comboBox1.SelectedIndex.ToString() == "2")
                {
                    RegisterUserID();
                
                }
                else if (comboBox1.SelectedIndex.ToString() == "6")
                {
                    timer1.Start();

                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            getVorityImage();//加载验证码图片，打码
        }
        
        /// <summary>
        /// 操作延迟设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_yc_TextChanged(object sender, EventArgs e)
        {
            timer1.Interval = Int32.Parse(textBox_yc.Text);
        }

        /// <summary>
        /// 渴望币兑换抽奖选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex.ToString() == "0" && comboBox1.SelectedIndex.ToString() == "4")
            {//兑换-1元话费
                prizeID = "11";
            }
            else if (comboBox2.SelectedIndex.ToString() == "1" && comboBox1.SelectedIndex.ToString() == "4")
            {//兑换-门票
                prizeID = "10";
                comboBox2.ForeColor = Color.Red;
            }
            else if (comboBox2.SelectedIndex.ToString() == "2" && comboBox1.SelectedIndex.ToString() == "5")
            {//抽奖-旅游卡
                prizeID = "5";
            }
            else if (comboBox2.SelectedIndex.ToString() == "3" && comboBox1.SelectedIndex.ToString() == "5")
            {//抽奖-电容笔
                prizeID = "4";
            }
            else if (comboBox2.SelectedIndex.ToString() == "4" && comboBox1.SelectedIndex.ToString() == "5")
            {//抽奖-时尚耳机
                prizeID = "9";
            }
            else if (comboBox2.SelectedIndex.ToString() == "5" && comboBox1.SelectedIndex.ToString() == "5")
            {//抽奖-运动摄像机
                prizeID = "8";
            }
        }

        /// <summary>
        /// 通过注册表获取宽带连接名称 win7已测试
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllAdslNameByKey()
        {
            RegistryKey userKey = Registry.CurrentUser;
            RegistryKey key = userKey.OpenSubKey(@"RemoteAccess\Profile");
            return key.GetSubKeyNames();//获取当前创建的adsl宽带列表
        }

        private void comboBox_kd_SelectedIndexChanged(object sender, EventArgs e)
        {
            showAttr();//自动更新宽带连接信息
        }

        /// <summary>
        /// 宽带断线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_kdbh_Click(object sender, EventArgs e)
        {
            string str;
            ras.HangUp(out str);

        }

        /// <summary>
        /// 宽带重拨
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_kdcb_Click(object sender, EventArgs e)
        {
            string str;
            ras.DialUp((string)comboBox1.SelectedItem, out str);
        }

        /// <summary>
        /// 协议端口选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_url_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox_url.SelectedIndex.ToString()=="0")
            {
                duankou_url = "http://2016utc.pepsi.cn/";
                comboBox_url.ForeColor = Color.Blue;
                yzm_weishu = 4;
            }
            else if (comboBox_url.SelectedIndex.ToString() == "1")
            {
                duankou_url = "http://utc.pepsi.cn/";
                comboBox_url.ForeColor = Color.YellowGreen;
                if (comboBox1.SelectedIndex.ToString() != "6")
                {
                    yzm_weishu = 5;
                }
                else
                {//秒杀验证码4位
                    yzm_weishu = 4;
                }
            }
           
        }

        /// <summary>
        /// 主操作变更事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox1.SelectedIndex.ToString()=="0")
            {//查询信息使用电脑web端口
                comboBox_url.SelectedIndex = 1;
            }
            else if(comboBox1.SelectedIndex.ToString() == "1")
            {//兑换使用手机wap端口
                comboBox_url.SelectedIndex = 0;
            }
            else if (comboBox1.SelectedIndex.ToString() == "4")
            {
                comboBox2.SelectedIndex = 1;
                comboBox_url.SelectedIndex = 1;
            }
            else if (comboBox1.SelectedIndex.ToString() == "6")
            {
                comboBox2.SelectedIndex = 6;
                comboBox_url.SelectedIndex = 1;
                checkBox_yzm_zds.Checked = false;
            }
        }

        /// <summary>
        /// 定时器显示时间，定时兑换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer2_Tick(object sender, EventArgs e)
        {
            label_time.Text = "客户端时间：" + DateTime.Now.ToString("HH:mm:ss");
            if(checkBox_dingshi.Checked)
            {//启用定时兑换
                if (DateTime.Now.ToString("mm") == (pinglv_t.ToString()) && textBox_yc.Text.ToString() != "1000")
                {//到时间 开始兑换       
                    num_ds_t = 0;
                    pinglv_t += pinglv;
                    if (pinglv_t >= 59)
                    {//60进制时间
                        pinglv_t = 0;
                    }
                    textBox_yc.Text = "1000";
                }
                if(num_ds_t >= num_ds)
                {
                    num_ds_t = 0;
                    textBox_yc.Text = "999999";
                }
            }
        }

        /// <summary>
        /// 整点定时开启兑换测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex.ToString() == "0")
            {
                pinglv = 10;
            }
            else if (comboBox3.SelectedIndex.ToString() == "1")
            {
                pinglv = 15;
            }
            else if (comboBox3.SelectedIndex.ToString() == "2")
            {
                pinglv = 30;
            }
            else if (comboBox3.SelectedIndex.ToString() == "3")
            {
                pinglv = 60;
            }
        }

        /// <summary>
        /// 提示程序状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_dingshi_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox_dingshi.Checked )
            {
                comboBox3.SelectedIndex = 1;//默认15分钟
                label_dszt.Visible = true;
                label_time.ForeColor = Color.Red;
                num_ds = Int32.Parse(textBox3.Text);
            }
        }

        /// <summary>
        /// 定时兑换数量改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            num_ds = Int32.Parse(textBox3.Text);
        }

        private void checkBox_yzm_zds_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox_yzm_zds.Checked)
            {
                pictureBox1.Visible = false;
                label3.Visible = false;
                textBox_yzm.Visible = false;
            }
            else if (!checkBox_yzm_zds.Checked)
            {
                pictureBox1.Visible = true;
                label3.Visible = true;
                textBox_yzm.Visible = true;
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            datagridviewtobin(dataGridView1, "data1.data");
        }

        private void dataGridView2_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            datagridviewtobin(dataGridView2, "data2.data");
        }

        /// <summary>
        /// 右键菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.RowIndex >= 0)
                {
                    //若行已是选中状态就不再进行设置
                    if (dataGridView1.Rows[e.RowIndex].Selected == false)
                    {
                        dataGridView1.ClearSelection();
                        dataGridView1.Rows[e.RowIndex].Selected = true;
                    }
                    //只选中一行时设置活动单元格
                    if (dataGridView1.SelectedRows.Count == 1)
                    {
                        dataGridView1.CurrentCell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    }
                    //弹出操作菜单
                    contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);
                }
            }
        }

        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //弹出操作菜单
                contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);

            }

        }

        private void dataGridView2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //弹出操作菜单
                contextMenuStrip2.Show(MousePosition.X, MousePosition.Y);

            }
        }

        private void 从该行开始ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            numIndexOfDatagridview1Row = dataGridView1.CurrentRow.Index;
        }

        private void 从该行开始ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            numIndexOfDatagridview2Row = dataGridView2.CurrentRow.Index;
        }
       

    }



}
