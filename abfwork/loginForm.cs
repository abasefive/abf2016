using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAL;
using System.Management;
using System.IO;

namespace abfwork
{
    public partial class loginForm : Form
    {
        public loginForm()
        {
            InitializeComponent();
            if (File.Exists("user.bin") == true)
            {
                StreamReader reader = new StreamReader("user.bin", Encoding.Default);
                int i = 0;
                //循环读取所有行
                while (!reader.EndOfStream)
                {
                    if (i == 0)
                        textBox1.Text = reader.ReadLine();
                    if (i == 1)
                        textBox2.Text = reader.ReadLine();
                    i++;
                }
                //关闭文件
                reader.Close();

            }
            else
            {
                FileStream myFs = new FileStream("user.bin", FileMode.Create);
                StreamWriter mySw = new StreamWriter(myFs);
                mySw.Close();
                myFs.Close();
            }
        }
       
        /// <summary>
        /// 登录到系统
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text;//用户名
            string psw = textBox2.Text;//密码
            if (username == "" | psw == "")
            {
                MessageBox.Show("用户名或密码不能为空！");
                return;
            }
            string sql = "select * from lz_admin where username='" + username + "' and psw='" + psw + "' and type=3";
            DataTable dt = GetConnection.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                double logindate = ConvertDateTimeInt(DateTime.Now);//登录时间
                string ip = GetIpv4();//注册时的IP
                int login_count = Int32.Parse(dt.Rows[0]["login_count"].ToString()) + 1;//登录次数+1
                string sqlist = "update lz_admin set logindate = '" + logindate + "' ,login_count='" + login_count + "',ip='" + ip + "' where username = '" + username + "'";
                GetConnection.NoSelect(sqlist);//更新信息

                FileStream aFile = new FileStream("user.bin", FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(textBox1.Text);
                sw.WriteLine(textBox2.Text);
                sw.Close();
                aFile.Close();

                this.Close();
                this.DialogResult = System.Windows.Forms.DialogResult.OK;

            }
            else
            {
                MessageBox.Show("登录失败！忘记密码，请联系QQ:1070387250");

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";

        }

        /// <summary>
        /// 将c# DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns>double</returns>
        public static double ConvertDateTimeInt(System.DateTime time)
        {
            double intResult = 0;
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            intResult = (time - startTime).TotalSeconds;
            return intResult;
        }

        /// <summary>
        /// 获取ip地址
        /// </summary>
        /// <returns></returns>
        private string GetIpv4()
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection nics = mc.GetInstances();
            foreach (ManagementObject nic in nics)
            {
                if (Convert.ToBoolean(nic["ipEnabled"]) == true)
                {
                    return (nic["IPAddress"] as String[])[0];
                }
            }
            return null;
        }

    }
}
