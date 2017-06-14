using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Net.NetworkInformation;
using System.IO;
using Microsoft.Win32;
using System.Xml;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace netPING
{
    partial class netPING : ServiceBase
    {
        System.Timers.Timer timer1;
        string localIP = "127.0.0.1";
        const string note = "★参数修改生效需重启动netPING服务★";
        string IP1 = "127.0.0.1";
        string IP2 = "127.0.0.1";
        string PHONE1 = "12345678901";
        string PHONE2 = "12345678901";
        string SMSID  = "请输入";
        string SMSPW = "请输入";
        int Interval = 60;
        Ping ping;
        string netPINGpath = "c://netPING.txt";
        const string debugpath = "c://debugPING.txt";
        bool once = true;
        bool once2 = true;

        public netPING()
        {
            InitializeComponent();
            timer1 = new System.Timers.Timer();
            ping = new Ping();
            netPINGpath = "c://netPING" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + ".txt";

            //在HKEY_LOCAL_MACHINE\SOFTWARE下名为netPING的注册表项。
            if (IsRegeditItemExist())
            {
                RegistryKey key = Registry.LocalMachine;
                RegistryKey software = key.OpenSubKey("software\\netPING");
                IP1 = software.GetValue("设置需要PING的IP地址1", "127.0.0.1").ToString();
                IP2 = software.GetValue("设置需要PING的IP地址2", "127.0.0.1").ToString();
                Interval = Convert.ToInt32(software.GetValue("设置PING命令发送周期(S)", "60").ToString());
                PHONE1 = software.GetValue("设置短信报警电话1", "12345678901").ToString();
                PHONE2 = software.GetValue("设置短信报警电话2", "12345678901").ToString();
                SMSID = software.GetValue("短信服务账号", "请输入").ToString();
                SMSPW = software.GetValue("短信服务密码", "请输入").ToString();
                key.Close();

            }
            else
            {
                RegistryKey key = Registry.LocalMachine;
                RegistryKey software = key.CreateSubKey("software\\netPING");
                software.SetValue("★参数修改生效需重启动netPING服务★", note);
                software.SetValue("设置需要PING的IP地址1", IP1);
                software.SetValue("设置需要PING的IP地址2", IP2);
                software.SetValue("设置PING命令发送周期(S)", Interval);
                software.SetValue("设置短信报警电话1", PHONE1);
                software.SetValue("设置短信报警电话2", PHONE2);
                software.SetValue("短信服务账号", SMSID);
                software.SetValue("短信服务密码", SMSPW);
                key.Close();
            }

            timer1.Interval = Interval * 1000;
            timer1.Elapsed += timer1_Elapsed;

            Thread nThread = new Thread(new ThreadStart(() => GetIP()));
            nThread.IsBackground = true;
            nThread.Start(); 
        }

        void LogOUT(string str,string path)
        {
            StreamWriter sw = new StreamWriter(path, true);
            sw.WriteLine(str);
            sw.Close();
        }
        private bool IsRegeditItemExist()
        {
            string[] subkeyNames;
            RegistryKey hkml = Registry.LocalMachine;
            RegistryKey software = hkml.OpenSubKey("SOFTWARE");
            //RegistryKey software = hkml.OpenSubKey("SOFTWARE", true); 
            subkeyNames = software.GetSubKeyNames();
            //取得该项下所有子项的名称的序列，并传递给预定的数组中 
            foreach (string keyName in subkeyNames)
            //遍历整个数组 
            {
                if (keyName == "netPING")
                //判断子项的名称 
                {
                    hkml.Close();
                    return true;
                }
            }
            hkml.Close();
            return false;
        }

        protected override void OnStart(string[] args)
        {
            this.timer1.Start();
        }

        protected override void OnStop()
        {
            this.timer1.Stop();
        }

       
        void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (ping.Send(IP1).Status != IPStatus.Success)
                {
                    string str = DateTime.Now.ToString() + "：PING " + IP1 + " 失败，本地IP：" + localIP;

                    LogOUT(str, netPINGpath);
                    if (once)
                    {
                        once = false;
                        //只执行一次的代码
                        sentSMS(str, PHONE1, PHONE2);
                    }
                }
                else
                {
                    if (!once)
                    {
                        once = true;
                        //只执行一次的代码
                        LogOUT(DateTime.Now.ToString() + "：PING " + IP1 + " 恢复正常，本地IP：" + localIP, netPINGpath);
                    }
                }
                
                if (ping.Send(IP2).Status != IPStatus.Success)
                {
                    string str = DateTime.Now.ToString() + "：PING " + IP2 + " 失败，本地IP：" + localIP;
                    LogOUT(str, netPINGpath);
                 
                    if (once2)
                    {
                        once2 = false;
                        //只执行一次的代码
                        sentSMS(str, PHONE1, PHONE2);
                    }
                }
                else
                {
                    if (!once2)
                    {
                        once2 = true;
                        //只执行一次的代码
                        LogOUT(DateTime.Now.ToString() + "：PING " + IP2 + " 恢复正常，本地IP：" + localIP, netPINGpath);
                    }
                }
            }
            catch
            {
                LogOUT(DateTime.Now.ToString() + "：本地网络断开，本地IP：" + localIP, netPINGpath);
            }
        }

        void GetIP()
        {
            string tempip = "";
            WebRequest request = WebRequest.Create("http://ip.qq.com/");
            request.Timeout = 10000;
            WebResponse response = request.GetResponse();
            Stream resStream = response.GetResponseStream();
            StreamReader sr = new StreamReader(resStream, System.Text.Encoding.Default);
            string htmlinfo = sr.ReadToEnd();
            //匹配IP的正则表达式
            Regex r = new Regex("((25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]\\d|\\d)\\.){3}(25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]\\d|[1-9])", RegexOptions.None);
            Match mc = r.Match(htmlinfo);
            //获取匹配到的IP
            tempip = mc.Groups[0].Value;

            resStream.Close();
            sr.Close();
            localIP= tempip;
        }

        void sentSMS(string content, string phone1, string phone2)
        {
            Thread nThread = new Thread(new ThreadStart(() => sentSMS2(content, PHONE1, PHONE2)));
            nThread.IsBackground = true;
            nThread.Start();
        }
        void sentSMS2(string content, string phone1, string phone2)
        {
            if ((DateTime.Now.Hour > 6) & (DateTime.Now.Hour<20))
            {
                Encoding gb2312 = Encoding.GetEncoding("GB2312");
                content = System.Web.HttpUtility.UrlEncode(System.Web.HttpUtility.UrlEncode(content, gb2312), gb2312);
                string data = "func=sendsms&username=" + SMSID + "&password=" + MD5.HashString(SMSPW) + "&smstype=0&timerflag=0&mobiles=" + phone1 + phone2 + "&message=【百川】网络故障：" + content;
               
                string outStr;
                int ret = NetHelperHttp.httpPost("http://sms.c8686.com/Api/BayouSmsApiEx.aspx", data, EncodeFormat.EF_GB2312, out outStr);
                if (ret == 200)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(outStr);
                    LogOUT(DateTime.Now.ToString() + "：短信发送成功：" + phone1 + "，" + phone2, debugpath);
                }
                else
                {
                    LogOUT(DateTime.Now.ToString() + "：短信发送失败：" + phone1 + "，" + phone2, debugpath);
                }
            }
            else
            {
                LogOUT(DateTime.Now.ToString() + "：晚间故障记录", debugpath);
            }
           
        }
    }
}
