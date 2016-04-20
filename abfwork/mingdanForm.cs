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
using DotNet.Utilities;
using System.Text.RegularExpressions;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Diagnostics;


namespace abfwork
{
    public partial class phoneForm : Form
    {
        public phoneForm()
        {
            InitializeComponent();
            this.comboBox1.SelectedIndex = 0;
        }


        /// <summary>
        /// 开始操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            regAccounts();

        }

        /// <summary>
        /// 注册康师傅冰红茶帐号
        /// </summary>
        private void regAccounts()
        {
            int i = 0;
            //开始
            richTextBox1.AppendText(string.Format("\r\n{0}: 开始...", DateTime.Now.ToString("HH:mm:ss")));

            for (int num = 1; ; num++)
            {
                HttpHelper http = new HttpHelper();
                HttpItem itemPhone = new HttpItem()
                {
                    URL = "http://www.ksfteasp.com/PC/sharemillion.aspx",//URL     必需项    
                    Method = "POST",
                    //Cookie = cookie,//字符串Cookie     可选项
                    //用户代理设置为手机浏览器
                    UserAgent = "Mozilla/5.0 (iPhone; U; CPU iPhone OS 3_0 like Mac OS X; en-us) AppleWebKit/528.18 (KHTML, like Gecko) Version/4.0 Mobile/7A341 Safari/528.16",
                    IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写   
                    Referer = "http://www.ksfteasp.com/PC/sharemillion.aspx",//来源URL     可选项   
                    Postdata = "__EVENTTARGET=one&__EVENTARGUMENT=" + num + "&__LASTFOCUS=&__VIEWSTATE=a4DscDI5oTuzdj5I2HvYo%2Fi1DnPD9gBIBBhM9b8JEnJT828P8V48UpecE%2Byd0QseoeoXaBtlZfuUOhmPPWJEktiEeDApU5tqIWWIMKpSbXqFdbgLDHkIhhbLvNnZjZzuVpWHESbwV%2BpGgqP5uHXgqixs4hhbceRxf4ubJvOAk%2F2ahKyMXK1QUiGu3AIKPu3QQGuKlaKI5rE2XzpT0jqf6zrf%2FNPl5u2yylnNYElTwAdipqQw5NBThsoJcQWm5OtpoZLr8%2Fn0nDG%2FwiRhdrguHVe3j1H3RndoWWduXOYwfl0jYaCVLRxr31XS13Oh6IWu6kz1sdjyuTIbQZMTT%2FYTH2aON010pFyoG5MWxMMT%2Fx9nTIHkzvEJA7ng2QB2MPcvoSy7mGsZ10%2F02N8a5HJ%2FKe2Ec%2FXJay3IkAWCtYMpwNDu5pYHR0AS40ctuDXtfteBliPSigM2p5rHXP3iCKB3ogLk%2BUTrC1iK%2FgrZp2mWIAK2yP4RbF%2F4EwXmOfF%2BrcYW1WIsApRsO6CalzJ%2BUix11MDG%2BbkMlcbuDvNQMfq%2B9hVr%2F91sQLD9eCJE5DK0UQi6UygiC3AU5sLEA1eRjLJpnDa%2Ba%2BFMilxhvg2sdLwGG7DYEFqRrR%2BpzBohG59BL5U36Ii15cuOCoOxouOBZcWqTo5hiPpHCoHVRBMkqYkG9MDhNtwrTruhy98bFt6u%2BIR6OTz1HabFPQPnBir9UycXAVC1Tnvu9eb80sYpXpFSsYT8p3OgjBp3dHRa63A49063ldFfuV86nWHlunIbxuM5WCrxtEMsWRb9myxvZNq5np6EGh%2BFF9wzl1jOkVbMJ8O8lxm3CW9vU5Ai2EViBWhq2ZqXWstDxXA0Wc3iUz7whx63AdU0%2B%2BM3DDT6CL%2BhS5dEiL%2FxmKZ8WkgoaEQMMauRnNhzBy%2FVUV12oXebpSHBn7Q%3D&__VIEWSTATEGENERATOR=5EC3C27A&__EVENTVALIDATION=ZboC340cMLxNPdl0fl4sje6gzUnCpsH9wAdBPU1Wt%2Ff5UJDEtk00MCcnfxrdU9EdHxKhIBng2dHFWu5E0oYyl%2Fq0twaXPc0YGWK9Cp0Snjg7R3U2xwg%2BTBKV998c%2FhnEXJu%2BtgFk4HTgOW%2B4a3BPbKBTa2E2AbmJzVkNtz527cPXwktYIDyDq2yTZyZ8Fns6%2Bw8zigOREuT8tJs7Vh3HIDQC%2B9tO%2FxU9Aefl%2F4ugKLtom56%2BEN4%2BTBYT%2FudMqpwSRNZttO7E4KaqbgP61c3Nwz1Mw%2BKZjPQwGUsZbUvAJRoNkPddjqP6K3mU7ouHj5ty315BDM3mamKwrr%2BYZdAkAwzY2JrZ09W0r2a2Peio%2F5ark77c3e%2BGD3pRRAHN1lpqynVRq47LCUYyZkUtCPWGaZzXGZ33IaDFCTxH8hJSFahQwDHUPZtOjhY1gzdJ488T15JdyRJgctKf8R9AiO6ztFG078lZ2%2BUD%2BGsisgha5iK3TeRupFMJab6z2J%2FdvdlYbvZuWD42R%2FnW%2FK4MWzmWV%2B6LLFtIhBx0ydzm1C4inMA%3D&ddWeek=1",//Post数据     可选项GET时不需要写   
                    Timeout = 100000,//连接超时时间     可选项默认为100000    
                    ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000   
                    ContentType = "application/x-www-form-urlencoded",//返回类型    可选项有默认值   
                };
                HttpResult resultPhone = http.GetHtml(itemPhone);
                string pattern = @"\d{8}";//正则表达式
                FileStream aFile = new FileStream(comboBox1.SelectedItem.ToString() + "手机号段.txt", FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(aFile);
                richTextBox1.AppendText(string.Format("\r\n{0}: 开始获取第{1}页数据", DateTime.Now.ToString("HH:mm:ss"),num));
                label6.Text="第"+ num +"页";
                foreach(Match mt in  Regex.Matches(resultPhone.Html,pattern,RegexOptions.Multiline))
                {
                    string strPhone = mt.Value;
                    richTextBox1.AppendText(string.Format("\r\n{0}: {1}", DateTime.Now.ToString("HH:mm:ss"), strPhone));
                    sw.WriteLine(strPhone);
                    i++;
                }
                sw.Close();
                aFile.Close();

                if (num * 21 > i)
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            string path = System.Environment.CurrentDirectory;
            System.Diagnostics.Process.Start("explorer.exe", path);
        }

      


    }
}
