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


namespace abfwork
{
    public partial class mainForm : Form
    {

        //datatable用于保存读入的数据
        DataTable dt_num = new DataTable();
        DataTable dt_cdk = new DataTable();
        int dtnumflag = 0,dtcdkflag=0;//是否第一次添加数据

        string prizeID = "00";//价值类别
        string freedrink_cnt = "0";//免喝数量
        int is_yzm_TextChanged = 0;//是否触发
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

        /// <summary>
        /// 生成随机数
        /// </summary>
        private static char[] constantNum = {'0','1','2','3','4','5','6','7','8','9'};
        private static char[] constant = {'0','1','2','3','4','5','6','7','8','9','a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z', 'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};
        public static string GenerateRandomNumber(int Length, int j)
        {
            System.Text.StringBuilder newRandom = new System.Text.StringBuilder(62);
            Random rd = new Random(DateTime.Now.Millisecond + j);
            for (int i = 0; i < Length; i++)
            {
                newRandom.Append(constant[rd.Next(62)]);
            }
            return newRandom.ToString();
        }
        /// <summary>
        /// 启动加载
        /// </summary>
        public mainForm()
        {
            InitializeComponent();
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
            
            if (File.Exists("cookie.txt") == true)
            {//读取cookie
                StreamReader reader = new StreamReader("cookie.txt", Encoding.Default);
                cookie=reader.ReadLine();                
                reader.Close();

            }
            else
            {
                FileStream myFs = new FileStream("cookie.txt", FileMode.Create);
                StreamWriter mySw = new StreamWriter(myFs);
                mySw.Close();
                myFs.Close();
            }          
            Directory.CreateDirectory(logdir);//创建日志目录
            textBox_yzm.Focus();
            this.comboBox1.SelectedIndex = 1;
            
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
                    dt_num.Columns.Add("帐号", typeof(String));
                    dt_num.Columns.Add("密码", typeof(String));
                    dt_num.Columns.Add("状态", typeof(String));
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
                    dr[0] = data[0];
                    dr[1] = data[1];
                    dr[2] = "";
                    if (dt_num.Rows.Find(dr[0]) == null)
                    {//判断是否重复
                        //将这行数据加入到datatable中
                        dt_num.Rows.Add(dr);
                        cunt++;
                    }
                    else
                        richTextBox1.AppendText(string.Format("\r\n{0}: 去除重复帐号:{1}", DateTime.Now.ToString("HH:mm:ss"), dr[0]));
                }
                richTextBox1.AppendText(string.Format("\r\n{0}: 导入不重复帐号:{1}个", DateTime.Now.ToString("HH:mm:ss"), cunt));
                //关闭文件
                label_usenamecount.Text = cunt.ToString();//显示帐号总数
                reader.Close();
                //将datatable绑定到datagridview上显示结果
                dataGridView1.DataSource = dt_num;
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
                label_cdkcount.Text = cunt.ToString();//显示cdk总数
                //关闭文件
                reader.Close();
                //将datatable绑定到datagridview上显示结果
                dataGridView2.DataSource = dt_cdk;
            }
            catch (Exception e1)
            {
                MessageBox.Show("文件内容错误：请导入正确的CDK数据！", "错误：" + e1.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                timer1.Interval = 1000;
                timer1.Start();
                textBox_yzm.Focus();
                duihuan();
            }
            else if (comboBox1.SelectedIndex.ToString() == "2")
            {//注册账号
                richTextBox1.AppendText(string.Format("\r\n{0}: 开始注册账号...", DateTime.Now.ToString("HH:mm:ss")));
                timer1.Start();
                getVorityImage();
                textBox_yzm.Focus();
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
                pwd_weishu = Int32.Parse(textBox_pwd_weishu.Text);
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
                pwd_weishu = Int32.Parse(textBox_pwd_weishu.Text);
                timer1.Start();
                //PwdChange();
            }
        }
        /// <summary>
        /// 暂停定时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            richTextBox1.AppendText(string.Format("\r\n{0}: 定时器停止,程序暂停！", DateTime.Now.ToString("HH:mm:ss")));
        }

        /// <summary>
        /// 检测密码登录
        /// </summary>
        private string LoginOn(string useName, string password)
        {
            item = new HttpItem()
            {//登录验证
                URL = "http://utc.pepsi.cn/Account/LoginOn",//URL     必需项    
                Method = "POST",
                Cookie = cookie,//字符串Cookie     可选项
                //用户代理设置为手机浏览器
                UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.110 Safari/537.36",
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写 
                Referer = "http://utc.pepsi.cn/Account/Login",
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
                URL = "http://utc.pepsi.cn/Account/GetOneDesire_Money",//URL     必需项    
                Method = "GET",//这里与浏览器不一样的*************
                Cookie = cookie,//字符串Cookie     可选项
                //用户代理设置为手机浏览器
                UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.110 Safari/537.36",
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写 
                Referer = "http://utc.pepsi.cn/Home/Index",
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
                URL = "http://utc.pepsi.cn/WinPrize/WinPrizeLog",//URL     必需项    
                Method = "POST",//这里与浏览器不一样的*************
                Cookie = cookie,//字符串Cookie     可选项
                //用户代理设置为手机浏览器
                UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.110 Safari/537.36",
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写 
                Referer = "http://utc.pepsi.cn/WinPrize/WinPrizeIndex",
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
            string result = LoginOn(useName, password);
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
            if (LoginOn(useName, password) == "true")
            {//登录成功
                richTextBox1.AppendText(string.Format("\r\n{0}: {1} - 登录成功！", DateTime.Now.ToString("HH:mm:ss"), useName));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "渴望币:" + GetOneDesire_Money();//更新结果
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

            }
            else
            {//登录失败
                richTextBox1.AppendText(string.Format("\r\n{0}: {1} - 登录失败！", DateTime.Now.ToString("HH:mm:ss"), useName));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "登录失败";//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "兑换cdk_登录失败.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + password + "----" + DateTime.Now.ToString("HH:mm:ss"));
                sw.Close();
                aFile.Close();
                lgnerrcount++;
                label_lgnerr.Text = lgnerrcount.ToString();
                isloginok = false;
            }
            
        }

        private void duihuan_bt()
        {
            // -----------------兑换开始------------------
            authenticode = dataGridView2.SelectedRows[0].Cells["ColumnCDK"].Value.ToString();

            item = new HttpItem()
            {//兑换cdk
                URL = "http://utc.pepsi.cn/Lottery/Prize",//URL     必需项    
                Method = "POST",
                Cookie = cookie,//字符串Cookie     可选项
                //用户代理设置为手机浏览器
                UserAgent = "Mozilla/5.0 (iPhone; U; CPU iPhone OS 3_0 like Mac OS X; en-us) AppleWebKit/528.18 (KHTML, like Gecko) Version/4.0 Mobile/7A341 Safari/528.16",
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写 
                Referer = "http://utc.pepsi.cn/Home/Index",
                Postdata = "ticket=" + authenticode + "&code=" + captcha,//Post数据     可选项GET时不需要写
                Timeout = 100000,//连接超时时间     可选项默认为100000    
                ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8",//返回类型    可选项有默认值   
            };
            resultitem = http.GetHtml(item);
            string result = resultitem.Html;

            if (result == "请输入正确的验证码")
            {//验证码错误

                richTextBox1.AppendText(string.Format("\r\n{0}: {1}-{2}", DateTime.Now.ToString("HH:mm:ss"), useName, result));

                isbtok = 88;
            }
            else if (result == "0")
            {//渴望币100个

                richTextBox1.AppendText(string.Format("\r\n{0}: {1}-{2}", DateTime.Now.ToString("HH:mm:ss"), useName, "渴望币100个"));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "兑换成功";//更新结果
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
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "兑换成功";//更新结果
                dataGridView2.SelectedRows[0].Cells["ColumnResult"].Value = "双人大礼包";//更新结果
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
            }
            else if (result == "2")
            {//单人迪斯尼

                richTextBox1.AppendText(string.Format("\r\n{0}: {1}-{2}", DateTime.Now.ToString("HH:mm:ss"), useName, result));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "兑换成功";//更新结果
                dataGridView2.SelectedRows[0].Cells["ColumnResult"].Value = "单人迪斯尼";//更新结果
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
            }
            else if (result == "3")
            {//3元话费

                richTextBox1.AppendText(string.Format("\r\n{0}: {1}-{2}", DateTime.Now.ToString("HH:mm:ss"), useName, result));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "兑换成功";//更新结果
                dataGridView2.SelectedRows[0].Cells["ColumnResult"].Value = "3元话费";//更新结果
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
            }
            else if (result == "4")
            {//1元话费

                richTextBox1.AppendText(string.Format("\r\n{0}: {1}-{2}", DateTime.Now.ToString("HH:mm:ss"), useName, result));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "兑换成功";//更新结果
                dataGridView2.SelectedRows[0].Cells["ColumnResult"].Value = "1元话费";//更新结果
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
            }
            else if (result == "串码输入有误哦，请再仔细核对下重新输入，如有疑问，请拨打客服热线：4000647746。" || result=="串码输入有误。咨询热线：4000647746")
            {//串码输入有误

                richTextBox1.AppendText(string.Format("\r\n{0}: {1}-{2}", DateTime.Now.ToString("HH:mm:ss"), useName, result));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "cdk错误";//更新结果
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
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "cdk已使用";//更新结果
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
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "账号需要休息";//更新结果
                dataGridView2.SelectedRows[0].Cells["ColumnResult"].Value = "";//更新结果
                dataGridView2.SelectedRows[0].Cells["ColumnTime"].Value = DateTime.Now.ToString("HH:mm:ss");//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "兑换cdk_账号需要休息.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + password + "----" + authenticode + "----" + result + "----" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss"));
                sw.Close();
                aFile.Close();
                isbtok = 888;
            }
            else if(result=="已经达到每日兑换5次的上限咯，明天再来吧！ 如有疑问，请拨打客服热线：4000647746。")
            {//兑换5次的上限

                richTextBox1.AppendText(string.Format("\r\n{0}: {1}-{2}", DateTime.Now.ToString("HH:mm:ss"), useName, result));
                dataGridView1.SelectedRows[0].Cells["ColumnStatus"].Value = "达到每日兑换上限";//更新结果
                dataGridView2.SelectedRows[0].Cells["ColumnResult"].Value = "";//更新结果
                dataGridView2.SelectedRows[0].Cells["ColumnTime"].Value = DateTime.Now.ToString("HH:mm:ss");//更新结果
                FileStream aFile = new FileStream(logdir + "\\" + DateTime.Now.ToString("yyyyMMdd") + "兑换cdk_5次上限.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(useName + "----" + password + "----" + authenticode + "----" + result + "----" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss"));
                sw.Close();
                aFile.Close();
                isbtok = 888;
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
                URL = "http://utc.pepsi.cn/Account/RegisterUserID",//URL     必需项    
                Method = "POST",
                Cookie = cookie,//字符串Cookie     可选项
                //用户代理设置为手机浏览器
                UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.110 Safari/537.36",
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写 
                Referer = "http://utc.pepsi.cn/Account/Register",
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
            string result = LoginOn(useName, password);
            if (result == "true")
            {

                item = new HttpItem()
                {//兑换cdk
                    URL = "http://utc.pepsi.cn/WinPrize/PwdChange",//URL     必需项    
                    Method = "POST",
                    Cookie = cookie,//字符串Cookie     可选项
                    //用户代理设置为手机浏览器
                    UserAgent = "Mozilla/5.0 (iPhone; U; CPU iPhone OS 3_0 like Mac OS X; en-us) AppleWebKit/528.18 (KHTML, like Gecko) Version/4.0 Mobile/7A341 Safari/528.16",
                    IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写 
                    Referer = "http://utc.pepsi.cn/WinPrize/PwdChangeIndex",
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
        /// 渴望币兑换
        /// </summary>
        private void CashLotteryExchange()
        {
            useName = dataGridView1.SelectedRows[0].Cells["ColumnPhoneNum"].Value.ToString();
            password = dataGridView1.SelectedRows[0].Cells["ColumnPwd"].Value.ToString();
            string result2 = LoginOn(useName, password);
            if (result2 == "true")
            {//

                item = new HttpItem()
                {//兑换cdk
                    URL = "http://utc.pepsi.cn/Home/CashLotteryExchange",//URL     必需项    
                    Method = "POST",
                    Cookie = cookie,//字符串Cookie     可选项
                    //用户代理设置为手机浏览器
                    UserAgent = "Mozilla/5.0 (iPhone; U; CPU iPhone OS 3_0 like Mac OS X; en-us) AppleWebKit/528.18 (KHTML, like Gecko) Version/4.0 Mobile/7A341 Safari/528.16",
                    IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写 
                    Referer = "http://utc.pepsi.cn/Home/Index",
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
            }
            else if (result2 == "false")
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
            }


            
        }

        /*
        //取当前webBrowser登录后的Cookie值   
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        static extern bool InternetGetCookieEx(string pchURL, string pchCookieName, StringBuilder pchCookieData, ref int pcchCookieData, int dwFlags, object lpReserved);
        //取出Cookie，当登录后才能取    
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
        */
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
                URL = "http://utc.pepsi.cn/SecurityCode/Code",//URL     必需项    
                Method = "get",
                Cookie = cookie,//字符串Cookie     可选项
                //用户代理设置为手机浏览器
                UserAgent = "Mozilla/5.0 (iPhone; U; CPU iPhone OS 3_0 like Mac OS X; en-us) AppleWebKit/528.18 (KHTML, like Gecko) Version/4.0 Mobile/7A341 Safari/528.16",
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写 
                Referer = "http://www.ksfteasp.com/WAP/Register.html",
                //Postdata = "&oldvalue=" + captcha,//Post数据     可选项GET时不需要写
                Timeout = 100000,//连接超时时间     可选项默认为100000    
                ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                ContentType = "application/x-www-form-urlencoded",//返回类型    可选项有默认值 
            };
            pictureBox1.Image = http.GetImage(itemGet);
            //***********判断UU打码平台可用性，即对接打码*****************
            if (isDamaLogin)
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
                richTextBox1.AppendText(string.Format("\r\n{0}: {1}", DateTime.Now.ToString("HH:mm:ss"), "请输入验证码..."));

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
                    if (isbtok != 888)
                    {
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
                    isloginok=false;//复位
                    isbtok = 0;//复位
                    if (++numIndexOfDatagridview1Row == dataGridView1.RowCount )
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
                    
                    duihuan();
                    duihuan_bt();

                }
                else if (isloginok == true && isbtok >=1 && isbtok <=7)
                {//换下一个cdk
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
                    duihuan_bt();
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
            
        }
        private void textBox_yzm_TextChanged(object sender, EventArgs e)
        {
            if (textBox_yzm.Text.Length == 5  && checkBox_uu.Checked==false && is_yzm_TextChanged==0)
            {
                is_yzm_TextChanged = 1;
                captcha = textBox_yzm.Text;
                is_yzm_TextChanged = 1;
                if( comboBox1.SelectedIndex.ToString() == "1")
                {
                    duihuan_bt();
                }
                else if (comboBox1.SelectedIndex.ToString() == "2")
                {
                    RegisterUserID();
                
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
                prizeID = "1";
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


    }
}
