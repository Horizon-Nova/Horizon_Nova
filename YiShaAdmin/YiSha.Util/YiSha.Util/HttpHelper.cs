using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using YiSha.Util.Extension;

namespace YiSha.Util
{
    /// <summary>
    /// Http连接操作帮助類 
    /// </summary>
    public class HttpHelper
    {
        #region 是否是网址
        public static bool IsUrl(string url)
        {
            url = url.ParseToString().ToLower();
            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 模擬GET
        /// <summary>
        /// GET請求
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="postDataStr">The post data string.</param>
        /// <returns>System.String.</returns>
        public static string HttpGet(string url, int timeout = 10 * 1000)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            request.Timeout = timeout;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }
        #endregion

        #region 模擬POST
        /// <summary>
        /// POST請求
        /// </summary>
        /// <param name="posturl">The posturl.</param>
        /// <param name="postData">The post data.</param>
        /// <returns>System.String.</returns>
        public static string HttpPost(string posturl, string postData, string contentType = "application/x-www-form-urlencoded", int timeout = 10 * 1000)
        {
            Stream outstream = null;
            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            Encoding encoding = Encoding.GetEncoding("utf-8");
            byte[] data = encoding.GetBytes(postData);
            // 准備請求...
            try
            {
                // 設置參數
                request = WebRequest.Create(posturl) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "POST";
                request.ContentType = contentType;
                request.Timeout = timeout;
                request.ContentLength = data.Length;
                outstream = request.GetRequestStream();
                outstream.Write(data, 0, data.Length);
                outstream.Close();
                //發送請求並獲取相應回應資料
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才開始向目標网頁發送Post請求
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, encoding);
                //返回結果网頁（html）代碼
                string content = sr.ReadToEnd();
                string err = string.Empty;
                return content;
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return string.Empty;
            }
        }

        /// <summary>
        /// 模擬httpPost提交表單
        /// </summary>
        /// <param name="url">POS請求的网址</param>
        /// <param name="data">表單里的參數和值</param>
        /// <param name="encoder">頁面編碼</param>
        /// <returns></returns>
        public static string CreateAutoSubmitForm(string url, Dictionary<string, string> data, Encoding encoder)
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendFormat("<meta http-equiv=\"Content-Type\" content=\"text/html; charset={0}\" />", encoder.BodyName);
            html.AppendLine("</head>");
            html.AppendLine("<body onload=\"OnLoadSubmit();\">");
            html.AppendFormat("<form id=\"pay_form\" action=\"{0}\" method=\"post\">", url);
            foreach (KeyValuePair<string, string> kvp in data)
            {
                html.AppendFormat("<input type=\"hidden\" name=\"{0}\" id=\"{0}\" value=\"{1}\" />", kvp.Key, kvp.Value);
            }
            html.AppendLine("</form>");
            html.AppendLine("<script type=\"text/javascript\">");
            html.AppendLine("<!--");
            html.AppendLine("function OnLoadSubmit()");
            html.AppendLine("{");
            html.AppendLine("document.getElementById(\"pay_form\").submit();");
            html.AppendLine("}");
            html.AppendLine("//-->");
            html.AppendLine("</script>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            return html.ToString();
        }
        #endregion

        #region 預定義方法或者變更
        //默認的編碼
        private Encoding encoding = Encoding.Default;
        //HttpWebRequest對象用來發起請求
        private HttpWebRequest request = null;
        //獲取影响流的資料對象
        private HttpWebResponse response = null;
        /// <summary>
        /// 根据相傳入的資料，得到相應頁面資料
        /// </summary>
        /// <param name="strPostdata">傳入的資料Post方式,get方式傳NUll或者空字符串都可以</param>
        /// <returns>string類型的响應資料</returns>
        private HttpResult GetHttpRequestData(HttpItem httpItem)
        {
            //返回參數
            HttpResult result = new HttpResult();
            try
            {
                #region 得到請求的response
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    result.Header = response.Headers;
                    if (response.Cookies != null)
                    {
                        result.CookieCollection = response.Cookies;
                    }
                    if (response.Headers["set-cookie"] != null)
                    {
                        result.Cookie = response.Headers["set-cookie"];
                    }

                    MemoryStream _stream = new MemoryStream();
                    //GZIIP處理
                    if (response.ContentEncoding != null && response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //開始讀取流並設置編碼方式
                        //new GZipStream(response.GetResponseStream(), CompressionMode.Decompress).CopyTo(_stream, 10240);
                        //.net4.0以下寫法
                        _stream = GetMemoryStream(new GZipStream(response.GetResponseStream(), CompressionMode.Decompress));
                    }
                    else
                    {
                        //開始讀取流並設置編碼方式
                        //response.GetResponseStream().CopyTo(_stream, 10240);
                        //.net4.0以下寫法
                        _stream = GetMemoryStream(response.GetResponseStream());
                    }
                    //獲取Byte
                    byte[] RawResponse = _stream.ToArray();
                    //是否返回Byte類型資料
                    if (httpItem.ResultType == ResultType.Byte)
                    {
                        result.ResultByte = RawResponse;
                    }
                    //從這里開始我們要无視編碼了
                    if (encoding == null)
                    {
                        string temp = Encoding.Default.GetString(RawResponse, 0, RawResponse.Length);
                        //<meta(.*?)charset([\s]?)=[^>](.*?)>
                        Match meta = Regex.Match(temp, "<meta([^<]*)charset=([^<]*)[\"']", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string charter = (meta.Groups.Count > 2) ? meta.Groups[2].Value : string.Empty;
                        charter = charter.Replace("\"", string.Empty).Replace("'", string.Empty).Replace(";", string.Empty);
                        if (charter.Length > 0)
                        {
                            charter = charter.ToLower().Replace("iso-8859-1", "gbk");
                            encoding = Encoding.GetEncoding(charter);
                        }
                        else
                        {
                            if (response.CharacterSet !=null && response.CharacterSet.ToLower().Trim() == "iso-8859-1")
                            {
                                encoding = Encoding.GetEncoding("gbk");
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(response.CharacterSet.Trim()))
                                {
                                    encoding = Encoding.UTF8;
                                }
                                else
                                {
                                    encoding = Encoding.GetEncoding(response.CharacterSet);
                                }
                            }
                        }
                    }
                    //得到返回的HTML
                    result.Html = encoding.GetString(RawResponse);
                    //最後释放流
                    _stream.Close();
                }
                #endregion
            }
            catch (WebException ex)
            {
                //這里是在發生異常時返回的錯誤信息
                result.Html = "String Error";
                response = (HttpWebResponse)ex.Response;
            }
            if (httpItem.IsToLower)
            {
                result.Html = result.Html.ToLower();
            }
            return result;
        }

        /// <summary>
        /// 4.0以下.net版本取資料使用
        /// </summary>
        /// <param name="streamResponse">流</param>
        private static MemoryStream GetMemoryStream(Stream streamResponse)
        {
            MemoryStream _stream = new MemoryStream();
            int Length = 256;
            Byte[] buffer = new Byte[Length];
            int bytesRead = streamResponse.Read(buffer, 0, Length);
            // write the required bytes  
            while (bytesRead > 0)
            {
                _stream.Write(buffer, 0, bytesRead);
                bytesRead = streamResponse.Read(buffer, 0, Length);
            }
            return _stream;
        }

        /// <summary>
        /// 為請求准備參數
        /// </summary>
        ///<param name="httpItem">參數列表</param>
        /// <param name="_Encoding">讀取資料時的編碼方式</param>
        private void SetRequest(HttpItem httpItem)
        {
            // 驗證證书
            SetCer(httpItem);
            // 設置代理
            SetProxy(httpItem);
            //請求方式Get或者Post
            request.Method = httpItem.Method;
            request.Timeout = httpItem.Timeout;
            request.ReadWriteTimeout = httpItem.ReadWriteTimeout;
            //Accept
            request.Accept = httpItem.Accept;
            //ContentType返回類型
            request.ContentType = httpItem.ContentType;
            //UserAgent客户端的訪問類型，包括瀏覽器版本和操作系统信息
            request.UserAgent = httpItem.UserAgent;
            // 編碼
            SetEncoding(httpItem);
            //設置Cookie
            SetCookie(httpItem);
            //來源地址
            request.Referer = httpItem.Referer;
            //是否執行跳转功能
            request.AllowAutoRedirect = httpItem.Allowautoredirect;
            //設置Post資料
            SetPostData(httpItem);
            //設置最大连接
            if (httpItem.Connectionlimit > 0)
            {
                request.ServicePoint.ConnectionLimit = httpItem.Connectionlimit;
            }
        }
        /// <summary>
        /// 設置證书
        /// </summary>
        /// <param name="httpItem"></param>
        private void SetCer(HttpItem httpItem)
        {
            if (!string.IsNullOrEmpty(httpItem.CerPath))
            {
                //這一句一定要寫在創建连接的前面。使用回調的方法進行證书驗證。
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                //初始化對像，並設置請求的URL地址
                request = (HttpWebRequest)WebRequest.Create(GetUrl(httpItem.URL));
                //創建證书文件
                X509Certificate objx509 = new X509Certificate(httpItem.CerPath);
                //添加到請求里
                request.ClientCertificates.Add(objx509);
            }
            else
            {
                //初始化對像，並設置請求的URL地址
                request = (HttpWebRequest)WebRequest.Create(GetUrl(httpItem.URL));
            }
        }
        /// <summary>
        /// 設置編碼
        /// </summary>
        /// <param name="httpItem">Http參數</param>
        private void SetEncoding(HttpItem httpItem)
        {
            if (string.IsNullOrEmpty(httpItem.Encoding) || httpItem.Encoding.ToLower().Trim() == "null")
            {
                //讀取資料時的編碼方式
                encoding = null;
            }
            else
            {
                //讀取資料時的編碼方式
                encoding = System.Text.Encoding.GetEncoding(httpItem.Encoding);
            }
        }
        /// <summary>
        /// 設置Cookie
        /// </summary>
        /// <param name="httpItem">Http參數</param>
        private void SetCookie(HttpItem httpItem)
        {
            if (!string.IsNullOrEmpty(httpItem.Cookie))
            {
                //Cookie
                request.Headers[HttpRequestHeader.Cookie] = httpItem.Cookie;
            }
            //設置Cookie
            if (httpItem.CookieCollection != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(httpItem.CookieCollection);
            }
        }
        /// <summary>
        /// 設置Post資料
        /// </summary>
        /// <param name="httpItem">Http參數</param>
        private void SetPostData(HttpItem httpItem)
        {
            //驗證在得到結果時是否有傳入資料
            if (request.Method.Trim().ToLower().Contains("post"))
            {
                //寫入Byte類型
                if (httpItem.PostDataType == PostDataType.Byte)
                {
                    //驗證在得到結果時是否有傳入資料
                    if (httpItem.PostdataByte != null && httpItem.PostdataByte.Length > 0)
                    {
                        request.ContentLength = httpItem.PostdataByte.Length;
                        request.GetRequestStream().Write(httpItem.PostdataByte, 0, httpItem.PostdataByte.Length);
                    }
                }//寫入文件
                else if (httpItem.PostDataType == PostDataType.FilePath)
                {
                    StreamReader r = new StreamReader(httpItem.Postdata, encoding);
                    byte[] buffer = Encoding.Default.GetBytes(r.ReadToEnd());
                    r.Close();
                    request.ContentLength = buffer.Length;
                    request.GetRequestStream().Write(buffer, 0, buffer.Length);
                }
                else
                {
                    //驗證在得到結果時是否有傳入資料
                    if (!string.IsNullOrEmpty(httpItem.Postdata))
                    {
                        byte[] buffer = Encoding.Default.GetBytes(httpItem.Postdata);
                        request.ContentLength = buffer.Length;
                        request.GetRequestStream().Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }
        /// <summary>
        /// 設置代理
        /// </summary>
        /// <param name="httpItem">參數對象</param>
        private void SetProxy(HttpItem httpItem)
        {
            if (string.IsNullOrEmpty(httpItem.ProxyUserName) && string.IsNullOrEmpty(httpItem.ProxyPwd) && string.IsNullOrEmpty(httpItem.ProxyIp))
            {
                //不需要設置
            }
            else
            {
                //設置代理服務器
                WebProxy myProxy = new WebProxy(httpItem.ProxyIp, false);
                //建议连接
                myProxy.Credentials = new NetworkCredential(httpItem.ProxyUserName, httpItem.ProxyPwd);
                //給當前請求對象
                request.Proxy = myProxy;
                //設置安全凭證
                request.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
        }
        /// <summary>
        /// 回調驗證證书問題
        /// </summary>
        /// <param name="sender">流對象</param>
        /// <param name="certificate">證书</param>
        /// <param name="chain">X509Chain</param>
        /// <param name="errors">SslPolicyErrors</param>
        /// <returns>bool</returns>
        public bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            // 總是接受    
            return true;
        }
        #endregion

        #region 普通類型
        /// <summary>    
        /// 傳入一個正確或不正確的URl，返回正確的URL
        /// </summary>    
        /// <param name="URL">url</param>   
        /// <returns>
        /// </returns>
        public static string GetUrl(string URL)
        {
            if (!(URL.Contains("http://") || URL.Contains("https://")))
            {
                URL = "http://" + URL;
            }
            return URL;
        }
        ///<summary>
        ///采用https协议訪問网络,根据傳入的URl地址，得到响應的資料字符串。
        ///</summary>
        ///<param name="httpItem">參數列表</param>
        ///<returns>String類型的資料</returns>
        public HttpResult GetHtml(HttpItem httpItem)
        {
            //准備參數
            SetRequest(httpItem);
            //調用專門讀取資料的類
            return GetHttpRequestData(httpItem);
        }
        #endregion
    }

    /// <summary>
    /// Http請求参考類 
    /// </summary>
    public class HttpItem
    {
        string _URL;
        /// <summary>
        /// 請求URL必須填寫
        /// </summary>
        public string URL
        {
            get { return _URL; }
            set { _URL = value; }
        }
        string _Method = "GET";
        /// <summary>
        /// 請求方式默認為GET方式
        /// </summary>
        public string Method
        {
            get { return _Method; }
            set { _Method = value; }
        }
        int _Timeout = 100000;
        /// <summary>
        /// 默認請求超時時間
        /// </summary>
        public int Timeout
        {
            get { return _Timeout; }
            set { _Timeout = value; }
        }
        int _ReadWriteTimeout = 30000;
        /// <summary>
        /// 默認寫入Post資料超時間
        /// </summary>
        public int ReadWriteTimeout
        {
            get { return _ReadWriteTimeout; }
            set { _ReadWriteTimeout = value; }
        }
        string _Accept = "text/html, application/xhtml+xml, */*";
        /// <summary>
        /// 請求標頭值 默認為text/html, application/xhtml+xml, */*
        /// </summary>
        public string Accept
        {
            get { return _Accept; }
            set { _Accept = value; }
        }
        string _ContentType = "text/html";
        /// <summary>
        /// 請求返回類型默認 text/html
        /// </summary>
        public string ContentType
        {
            get { return _ContentType; }
            set { _ContentType = value; }
        }
        string _UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
        /// <summary>
        /// 客户端訪問信息默認Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)
        /// </summary>
        public string UserAgent
        {
            get { return _UserAgent; }
            set { _UserAgent = value; }
        }
        string _Encoding = string.Empty;
        /// <summary>
        /// 返回資料編碼默認為NUll,可以自動識別
        /// </summary>
        public string Encoding
        {
            get { return _Encoding; }
            set { _Encoding = value; }
        }
        private PostDataType _PostDataType = PostDataType.String;
        /// <summary>
        /// Post的資料類型
        /// </summary>
        public PostDataType PostDataType
        {
            get { return _PostDataType; }
            set { _PostDataType = value; }
        }
        string _Postdata;
        /// <summary>
        /// Post請求時要發送的字符串Post資料
        /// </summary>
        public string Postdata
        {
            get { return _Postdata; }
            set { _Postdata = value; }
        }
        private byte[] _PostdataByte = null;
        /// <summary>
        /// Post請求時要發送的Byte類型的Post資料
        /// </summary>
        public byte[] PostdataByte
        {
            get { return _PostdataByte; }
            set { _PostdataByte = value; }
        }
        CookieCollection cookiecollection = null;
        /// <summary>
        /// Cookie對象集合
        /// </summary>
        public CookieCollection CookieCollection
        {
            get { return cookiecollection; }
            set { cookiecollection = value; }
        }
        string _Cookie = string.Empty;
        /// <summary>
        /// 請求時的Cookie
        /// </summary>
        public string Cookie
        {
            get { return _Cookie; }
            set { _Cookie = value; }
        }
        string _Referer = string.Empty;
        /// <summary>
        /// 來源地址，上次訪問地址
        /// </summary>
        public string Referer
        {
            get { return _Referer; }
            set { _Referer = value; }
        }
        string _CerPath = string.Empty;
        /// <summary>
        /// 證书绝對路徑
        /// </summary>
        public string CerPath
        {
            get { return _CerPath; }
            set { _CerPath = value; }
        }
        private Boolean isToLower = true;
        /// <summary>
        /// 是否設置為全文小寫
        /// </summary>
        public Boolean IsToLower
        {
            get { return isToLower; }
            set { isToLower = value; }
        }
        private Boolean allowautoredirect = true;
        /// <summary>
        /// 支持跳转頁面，查询結果將是跳转後的頁面
        /// </summary>
        public Boolean Allowautoredirect
        {
            get { return allowautoredirect; }
            set { allowautoredirect = value; }
        }
        private int connectionlimit = 1024;
        /// <summary>
        /// 最大连接數
        /// </summary>
        public int Connectionlimit
        {
            get { return connectionlimit; }
            set { connectionlimit = value; }
        }
        private string proxyusername = string.Empty;
        /// <summary>
        /// 代理Proxy 服務器使用者名
        /// </summary>
        public string ProxyUserName
        {
            get { return proxyusername; }
            set { proxyusername = value; }
        }
        private string proxypwd = string.Empty;
        /// <summary>
        /// 代理 服務器密碼
        /// </summary>
        public string ProxyPwd
        {
            get { return proxypwd; }
            set { proxypwd = value; }
        }
        private string proxyip = string.Empty;
        /// <summary>
        /// 代理 服務IP
        /// </summary>
        public string ProxyIp
        {
            get { return proxyip; }
            set { proxyip = value; }
        }
        private ResultType resulttype = ResultType.String;
        /// <summary>
        /// 設置返回類型String和Byte
        /// </summary>
        public ResultType ResultType
        {
            get { return resulttype; }
            set { resulttype = value; }
        }
    }

    /// <summary>
    /// Http返回參數類
    /// </summary>
    public class HttpResult
    {
        string _Cookie = string.Empty;
        /// <summary>
        /// Http請求返回的Cookie
        /// </summary>
        public string Cookie
        {
            get { return _Cookie; }
            set { _Cookie = value; }
        }
        CookieCollection cookiecollection = null;
        /// <summary>
        /// Cookie對象集合
        /// </summary>
        public CookieCollection CookieCollection
        {
            get { return cookiecollection; }
            set { cookiecollection = value; }
        }
        private string html = string.Empty;
        /// <summary>
        /// 返回的String類型資料 只有ResultType.String時才返回資料，其它情况為空
        /// </summary>
        public string Html
        {
            get { return html; }
            set { html = value; }
        }
        private byte[] resultbyte = null;
        /// <summary>
        /// 返回的Byte數組 只有ResultType.Byte時才返回資料，其它情况為空
        /// </summary>
        public byte[] ResultByte
        {
            get { return resultbyte; }
            set { resultbyte = value; }
        }
        private WebHeaderCollection header = new WebHeaderCollection();
        //header對象
        public WebHeaderCollection Header
        {
            get { return header; }
            set { header = value; }
        }
    }

    /// <summary>
    /// 返回類型
    /// </summary>
    public enum ResultType
    {
        String, //表示只返回字符串
        Byte //表示返回字符串和字節流
    }

    /// <summary>
    /// Post的資料格式默認為string
    /// </summary>
    public enum PostDataType
    {
        String, //字符串
        Byte, //字符串和字節流
        FilePath //表示傳入的是文件
    }
}
