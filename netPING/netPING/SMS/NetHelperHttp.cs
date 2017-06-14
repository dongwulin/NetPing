using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace netPING
{
    public enum EncodeFormat : int
    {
        EF_GB2312,
        EF_UTF8,
        EF_UTF7,
        EF_UTF32,
        EF_UNICODE
    }
    public static class NetHelperHttp
    {
        public static int httpPost(string url, string data, EncodeFormat ef, out string result)
        {

            try
            {
                byte[] bData = getBytesByEncodeFormat(data, ef);
                HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(url);
                WebReq.Method = "POST";
                WebReq.ContentType = "application/x-www-form-urlencoded";
                WebReq.ContentLength = bData.Length;
                Stream PostData = WebReq.GetRequestStream();
                PostData.Write(bData, 0, bData.Length);
                PostData.Close();
                HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
                Stream Answer = WebResp.GetResponseStream();
                result = getResult(Answer, ef);
                return (int)WebResp.StatusCode;
            }
            catch (System.Exception ex)
            {
                result = ex.Message;
                return 0;
            }
        }
        public static string getResult(Stream ret, EncodeFormat ef)
        {
            StreamReader _Answer = new StreamReader(ret, getActrualEncoding(ef));
            string retStr = _Answer.ReadToEnd();
            return retStr;
        }
        public static Encoding getActrualEncoding(EncodeFormat ef)
        {
            switch (ef)
            {
                case EncodeFormat.EF_GB2312:
                    {
                        return Encoding.GetEncoding("gb2312");
                    }
                case EncodeFormat.EF_UNICODE:
                    {
                        return Encoding.Unicode;
                    }
                case EncodeFormat.EF_UTF32:
                    {
                        return Encoding.UTF32;
                    }
                case EncodeFormat.EF_UTF7:
                    {
                        return Encoding.UTF7;
                    }
                case EncodeFormat.EF_UTF8:
                    {
                        return Encoding.UTF8;
                    }
                default:
                    return Encoding.Default;
            }
        }
        public static byte[] getBytesByEncodeFormat(string source, EncodeFormat ef)
        {
            Encoding encoding = getActrualEncoding(ef);
            return encoding.GetBytes(source);
        }
        public static int httpGet(string url, string data, EncodeFormat ef, out string result)
        {
            try
            {
                url = url + "?" + data;
                HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(url);
                WebReq.Method = "GET";
                WebReq.ContentType = "application/x-www-form-urlencoded";
                HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
                Stream Answer = WebResp.GetResponseStream();
                result = getResult(Answer, ef);
                return (int)WebResp.StatusCode;
            }
            catch (System.Exception ex)
            {
                result = ex.Message;
                return 0;
            }
        }
    }
}
