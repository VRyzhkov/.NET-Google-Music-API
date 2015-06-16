﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using System.Net.Cache;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Web;
using System.Collections;
using System.Threading.Tasks;

namespace GoogleMusic.Net
{
    internal class Http
    {
        #region Members
        private DecompressionMethods _DecompressionMethod = DecompressionMethods.GZip;
        private int _Timeout = 10000;
        private string _UserAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.124 Safari/537.36";
        private string _LastUrl = "http://www.google.com/";
        private bool _UseProxy = false;
        private WebProxy _Proxy;
        private bool _UsePipelining = false;
        private Encoding _Encoding = Encoding.UTF8;
        private int _BufferSize = 8192;
        private HttpRequestCacheLevel _CachingPolicy = HttpRequestCacheLevel.Revalidate;


        private CookieContainer _Cookies = new CookieContainer();
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value that indicates whether to use a proxy.
        /// </summary>
        public bool UseProxy
        {
            get { return _UseProxy; }
            set { _UseProxy = value; }
        }

        /// <summary>
        /// Gets or sets the proxy to use if UseProxy is true.
        /// </summary>
        public WebProxy Proxy
        {
            get { return _Proxy; }
            set { _Proxy = value; }
        }

        /// <summary>
        /// Returns a value that represents the last requested URL. This property is read-only.
        /// </summary>
        public string LastUrl
        {
            get { return _LastUrl; }
        }

        /// <summary>
        /// Gets or sets the useragent being imitated.
        /// </summary>
        public string UserAgent
        {
            get { return _UserAgent; }
            set { _UserAgent = value; }
        }

        /// <summary>
        /// Gets or sets a value in milliseconds that indicates the time after initiating a request to wait before timing out.
        /// </summary>
        public int Timeout
        {
            get { return _Timeout; }
            set { _Timeout = Math.Abs(value); }
        }

        /// <summary>
        /// Gets or sets the decompression method to use. GZip is default.
        /// </summary>
        public DecompressionMethods DecompressionMethod
        {
            get { return _DecompressionMethod; }
            set { _DecompressionMethod = value; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether to pipeline to each request.
        /// </summary>
        public bool UsePipelining
        {
            get { return _UsePipelining; }
            set { _UsePipelining = value; }
        }

        /// <summary>
        /// Gets or sets the cookies to use.
        /// </summary>
        public CookieContainer Cookies
        {
            get { return _Cookies; }
            set { _Cookies = value; }
        }

        /// <summary>
        /// Gets or sets the encoding to use.
        /// </summary>
        public Encoding Encoding
        {
            get { return _Encoding; }
            set { _Encoding = value; }
        }

        /// <summary>
        /// Gets or sets the buffer size to use when reading streams.
        /// </summary>
        public int BufferSize
        {
            get { return _BufferSize; }
            set { _BufferSize = value; }
        }

        /// <summary>
        /// Gets or sets the caching policy.
        /// </summary>
        public HttpRequestCacheLevel CachingPolicy
        {
            get { return _CachingPolicy; }
            set { _CachingPolicy = value; }
        }

        #endregion

        #region Cookies

        /// <summary>
        /// Deletes all cookies and logins.
        /// </summary>
        public void ClearCookies()
        {
            _Cookies = new CookieContainer();
        }


        /// <summary>
        /// Deletes all cookies associated with a specified URL.
        /// </summary>
        /// <param name="URL">Required. The specified URL's cookies to clear.</param>
        public void ClearCookies(string URL)
        {
            try
            {
                CookieCollection CC = _Cookies.GetCookies(new Uri(URL));
                foreach (Cookie Cookie in CC)
                {
                    Cookie.Expired = true;
                }
            }
            catch (Exception)
            {
            }
        }


        /// <summary>
        /// Retrieves all cookies stored in the wrapper and returns them in a CookieCollection.
        /// </summary>
        /// <returns>A CookieCollection instance containing all the cookies in the wrapper.</returns>
        public CookieCollection GetAllCookies()
        {
            CookieCollection lstCookies = new CookieCollection();
            Hashtable table = (Hashtable)_Cookies.GetType().InvokeMember("m_domainTable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, _Cookies, new object[] { });
            foreach (object pathList in table.Values)
            {
                SortedList lstCookieCol = (SortedList)pathList.GetType().InvokeMember("m_list", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, pathList, new object[] { });
                foreach (CookieCollection colCookies in lstCookieCol.Values)
                    foreach (Cookie c in colCookies)
                    {
                        lstCookies.Add(c);
                    }
            }
            return lstCookies;
        }


        /// <summary>
        /// Returns the HTTP cookie headers that contain the HTTP cookies that represent the System.Net.Cookie instances that are associated with the specified URL.
        /// </summary>
        /// <param name="URL">Required. The URL of the cookies desired.</param>
        /// <returns></returns>
        public string GetCookieString(string URL)
        {
            try
            {
                return _Cookies.GetCookieHeader(new Uri(URL));
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }


        /// <summary>
        /// Returns System.Net.CookieCollection that contains the System.Net.Cookie instances that associate with the specified URL
        /// </summary>
        /// <param name="URL">Required. The URL of the cookies desired.</param>
        /// <returns></returns>
        public CookieCollection GetCookieCollection(string URL)
        {
            try
            {
                return _Cookies.GetCookies(new Uri(URL));
            }
            catch (Exception)
            {
                return null;
            }
        }


        /// <summary>
        /// Adds and associates a specified cookie with a URL in the wrapper's CookieCollection.
        /// </summary>
        /// <param name="URL">Required. The URL to associate the cookie with.</param>
        /// <param name="Cookie">Required. The cookie to add to the CookieCollection.</param>
        public void AddCookie(string URL, Cookie Cookie)
        {
            try
            {
                _Cookies.Add(new Uri(URL), Cookie);
            }
            catch (Exception)
            {
            }
        }


        /// <summary>
        /// Adds and associates an array of cookies with a URL in the wrapper's CookieCollection.
        /// </summary>
        /// <param name="URL">Required. The URL to associate all cookies with.</param>
        /// <param name="Cookie">Required. The array of cookies to add to the CookieCollection.</param>
        public void AddCookieArray(string URL, Cookie[] Cookie)
        {
            try
            {
                foreach (Cookie c in Cookie)
                {
                    _Cookies.Add(new Uri(URL), c);
                }
            }
            catch (Exception)
            {
            }
        }


        /// <summary>
        /// Sets and associates a specified CookieCollection with a URL in the wrapper's CookieCollection.
        /// </summary>
        /// <param name="URL">Required. The URL to associate the CookieCollection with.</param>
        /// <param name="CCollection">Required. The CookieCollection to add to the wrapper's CookieCollection.</param>
        public void AddCookieCollection(string URL, CookieCollection CCollection)
        {
            try
            {
                _Cookies.Add(new Uri(URL), CCollection);
            }
            catch (Exception)
            {
            }
        }


        /// <summary>
        /// Adds and associates System.Net.Cookie instances from an HTTP cookie header to the wrapper's CookieCollection with a specific URL.
        /// </summary>
        /// <param name="URL">Required. The specific URL to associate the cookie instances with.</param>
        /// <param name="CookieString">Required. The string of cookies or cookie header to add.</param>
        public void AddCookieString(string URL, string CookieString)
        {
            string[] strCookies = CookieString.Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string strCookie in strCookies)
            {
                _Cookies.SetCookies(new Uri(URL), strCookie);
            }
        }


        /// <summary>
        /// Clones the wrapper's cookies of a specific domain and associates them with a new domain.
        /// </summary>
        /// <param name="OldDomain">Required. The old domain to clone cookies from.</param>
        /// <param name="NewDomain">Required. The new domain to clone cookies to.</param>
        /// <returns></returns>
        public bool CloneCookies(string OldDomain, string NewDomain)
        {
            try
            {
                return CloneCookies(new Uri(OldDomain), new Uri(NewDomain));
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// Clones the wrapper's cookies of a specific domain and associates them with a new domain.
        /// </summary>
        /// <param name="OldDomain">Required. The old domain to clone cookies from.</param>
        /// <param name="NewDomain">Required. The new domain to clone cookies to.</param>
        /// <returns></returns>
        public bool CloneCookies(Uri OldDomain, Uri NewDomain)
        {
            try
            {
                string CookNewStr = string.Empty;
                foreach (System.Net.Cookie Cook in _Cookies.GetCookies(OldDomain))
                {
                    _Cookies.SetCookies(NewDomain, Cook.Name + "=" + Cook.Value + ((Cook.Expires != null) ? "; expires=" + Cook.Expires.ToString() : "") + (!(Cook.Path == string.Empty) ? "; path=" + Cook.Path : "" + "; domain=") + NewDomain.Host + (Cook.HttpOnly ? "; HttpOnly" : ""));
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Pre-Request

        /// <summary>
        /// Creates and prepares a new HttpWebRequest object to use.
        /// </summary>
        /// <param name="url">Required. The destination URL to visit.</param>
        /// <param name="method">Optional. The HTTP method of the request. The default value is GET.</param>
        /// <param name="referer">Optional. The Referer HTTP header to send with the request. The default is nothing.</param>
        /// <param name="accept">Optional. The Accept HTTP header to send with the request. The default is */*.</param>
        /// <returns></returns>
        public HttpWebRequest CreateRequest(string url, string method = "GET", string referer = "", string accept = "*/*")
        {
            HttpWebRequest request;

            request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = method;
            request.Proxy = (_UseProxy ? _Proxy : null);
            request.AutomaticDecompression = _DecompressionMethod;

            request.CachePolicy = new HttpRequestCachePolicy(_CachingPolicy);
            request.Accept = accept;
            request.Referer = referer;
            request.CookieContainer = _Cookies;
            request.UserAgent = _UserAgent;
            request.Pipelined = (_UsePipelining && method != "POST" && method != "PUT");

            return request;
        }

        #endregion

        #region Request

        /// <summary>
        /// Performs an HttpWebRequest with request data.
        /// </summary>
        /// <param name="request">Required. The HttpWebRequest to represent the base request.</param>
        /// <param name="contentType">Required. The Content-Type HTTP header to send with the request.</param>
        /// <param name="data">Required. A byte array representing the request data to send with the request.</param>
        /// <returns>Returns a HttpWebResponse representing the response from the server.</returns>
        public HttpWebResponse Request(HttpWebRequest request, string contentType, byte[] data)
        {
            request.ContentLength = (data != null) ? data.Length : 0;

            if (!String.IsNullOrEmpty(contentType))
                request.ContentType = contentType;

            // Write to upload stream
            using (Stream uploadStream = request.GetRequestStream())
            {
                uploadStream.Write(data, 0, data.Length);
            }

            return (HttpWebResponse)request.GetResponse();
        }

        /// <summary>
        /// Asynchronously performs an HttpWebRequest with request data.
        /// </summary>
        /// <param name="request">Required. The HttpWebRequest to represent the base request.</param>
        /// <param name="contentType">Required. The Content-Type HTTP header to send with the request.</param>
        /// <param name="data">Required. A byte array representing the request data to send with the request.</param>
        /// <param name="progressHandler">Optional. The event handler to invoke when progress has changed.</param>
        /// <param name="completeHandler">Optional. The event handler to invoke when the request has finished.</param>
        /// <returns>Returns a HttpWebResponse representing the response from the server.</returns>
        public async Task<HttpWebResponse> RequestAsync(HttpWebRequest request, string contentType, byte[] data, TaskProgressEventHandler progressHandler = null, TaskCompleteEventHandler completeHandler = null)
        {
            request.ContentLength = (data != null) ? data.Length : 0;

            if (!String.IsNullOrEmpty(contentType))
                request.ContentType = contentType;

            // Write to upload stream
            using (Stream uploadStream = await request.GetRequestStreamAsync())
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    // Make buffer either the buffer size or smaller if the data is small
                    byte[] buffer = new byte[Math.Min(_BufferSize, data.Length)];
                    long bytesRead = 0;
                    int chunkRead;

                    while ((chunkRead = await ms.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        // Start writing to stream asynchronously
                        Task writeTask = uploadStream.WriteAsync(buffer, 0, chunkRead);

                        // Calculate bytes
                        bytesRead += chunkRead;

                        // ProgressHandler: Begin asynchronous invoke
                        IAsyncResult progressHandlerResult = null;
                        if (progressHandler != null)
                            progressHandlerResult = progressHandler.BeginInvoke(Math.Min(1.0, bytesRead / ms.Length), null, null);

                        // Wait for chunk upload to finish
                        await writeTask;

                        // End asynchronous ProgressHandler
                        if (progressHandler != null && progressHandlerResult != null)
                            progressHandler.EndInvoke(progressHandlerResult);
                    }
                    // No progress update version:
                    //await ms.CopyToAsync(uploadStream, _BufferSize);
                }
            }

            HttpWebResponse result;

            {
                // HttpWebResponse: Start task
                Task<WebResponse> resultTask = request.GetResponseAsync();

                // CompleteHandler: Begin asynchronous invoke
                IAsyncResult completeHandlerResult = null;
                if (completeHandler != null)
                    completeHandler.BeginInvoke(null, null);

                // HttpWebResponse: Await result
                result = (HttpWebResponse)(await resultTask);

                // CompleteHandler: End asynchronous invoke
                if (completeHandler != null && completeHandlerResult != null)
                    completeHandler.EndInvoke(completeHandlerResult);
            }

            return result;
        }

        /// <summary>
        /// Calculates an optimal buffer size given a content length
        /// </summary>
        /// <param name="contentLength">Required. The size, in bytes, of the content being buffered.</param>
        /// <returns>Returns a 32-bit signed integer representing a suggested buffer size.</returns>
        internal int CalculateOptimalBufferSize(long contentLength)
        {
            if (contentLength < 0L)
                return 65536; // Default 60 KB
            else if (contentLength > 33554432L)
                return 1048576; // 1 MB when content is > 32 MB
            else if (contentLength > 8388608L)
                return 524288; // 0.5 MB when content is > 32 MB
            else if (contentLength > 1048576L)
                return 131072;
            else if (contentLength > 131072L)
                return 32768;
            else
                return 8192;
        }

        #endregion

        #region Post-Request

        /// <summary>
        /// Reads the content of an HTTP response to a byte array.
        /// </summary>
        /// <param name="response">Required. The HttpWebResponse object representing the response to read.</param>
        /// <param name="disposeResponse">Optional. A boolean value determining whether to dispose of the response when finished. The default is true.</param>
        /// <returns>Returns a byte array representing the content of the response.</returns>
        public byte[] ResponseToArray(HttpWebResponse response, bool disposeResponse = true)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    responseStream.CopyTo(memory, _BufferSize);
                    if (disposeResponse)
                        response.Close();

                    return memory.ToArray();
                }
            }
        }

        /// <summary>
        /// Reads the content of an HTTP response to a byte array.
        /// </summary>
        /// <param name="response">Required. The HttpWebResponse object representing the response to read.</param>
        /// <param name="disposeResponse">Optional. A boolean value determining whether to dispose of the response when finished. The default is true.</param>
        /// <param name="progressHandler">Optional. The event handler to invoke when progress has changed.</param>
        /// <param name="completeHandler">Optional. The event handler to invoke when the response has been read.</param>
        /// <returns>Returns a byte array representing the content of the response.</returns>
        public async Task<byte[]> ResponseToArrayAsync(HttpWebResponse response, bool disposeResponse = true, TaskProgressEventHandler progressHandler = null, TaskCompleteEventHandler completeHandler = null)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    byte[] buffer = new byte[_BufferSize];
                    long bytesRead = 0;
                    int chunkRead;

                    while ((chunkRead = await responseStream.ReadAsync(buffer, 0, _BufferSize)) > 0)
                    {
                        // Start writing to memory asynchronously
                        Task readTask = memory.WriteAsync(buffer, 0, _BufferSize);

                        // Calculate bytes
                        bytesRead += chunkRead;

                        // ProgressHandler: Begin asynchronous invoke
                        IAsyncResult progressHandlerResult = null;
                        if (progressHandler != null && response.ContentLength > 0)
                            progressHandlerResult = progressHandler.BeginInvoke(Math.Min(1.0, bytesRead / response.ContentLength), null, null);

                        // Wait for chunk upload to finish
                        await readTask;

                        // End asynchronous ProgressHandler
                        if (progressHandler != null && progressHandlerResult != null)
                            progressHandler.EndInvoke(progressHandlerResult);
                    }
                }

                if (disposeResponse)
                    response.Close();

                // Invoke 
                if (completeHandler != null)
                    completeHandler.Invoke();

                return memory.ToArray();
            }
        }

        #endregion

    }


    internal class FormBuilder : IDisposable
    {
        #region Members
        private string boundary = "----------" + DateTime.Now.Ticks.ToString("x");
        private List<Stream> streams = new List<Stream>();

        private long _Length = new long();
        #endregion

        #region Properties

        public static FormBuilder Empty
        {
            get
            {
                return new FormBuilder();
            }
        }

        public String ContentType
        {
            get { return "multipart/form-data; boundary=" + boundary; }
        }

        public long Length
        {
            get { return _Length; }
            set { _Length = value; }
        }

        #endregion

        #region Add

        public void AddField(string key, string value)
        {
            if (streams.Count < 1 || !(streams[streams.Count - 1] is MemoryStream))
                streams.Add(new MemoryStream());

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("\r\n--{0}\r\n", boundary);
            sb.AppendFormat("Content-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}", key, value);

            byte[] sbData = Encoding.UTF8.GetBytes(sb.ToString());

            streams[streams.Count - 1].Write(sbData, 0, sbData.Length);
        }

        public void AddFields(Dictionary<string, string> fields)
        {
            foreach (KeyValuePair<string, string> key in fields)
                this.AddField(key.Key, key.Value);
        }

        public void AddFile(string name, string fileName, FileStream file)
        {
            if (streams.Count < 1 || !(streams[streams.Count - 1] is MemoryStream))
                streams.Add(new MemoryStream());

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("\r\n--{0}\r\n", boundary);
            sb.AppendFormat("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n", name, fileName);

            sb.AppendFormat("Content-Type: {0}\r\n\r\n", "application/octet-stream");

            byte[] sbData = Encoding.UTF8.GetBytes(sb.ToString());
            streams[streams.Count - 1].Write(sbData, 0, sbData.Length);

            streams.Add(file);
        }

        #endregion

        #region WriteTo

        public void WriteTo(Stream destination, int bufferSize)
        {
            foreach (Stream s in streams)
            {
                if (s is MemoryStream)
                {
                    MemoryStream ms = (MemoryStream)s;
                    ms.WriteTo(destination);
                }
                else
                {
                    s.CopyTo(destination, bufferSize);
                }
            }
        }

        public void WriteToAsync(Stream destination, int bufferSize,
            CancellationToken cancellationToken = default(CancellationToken), TaskProgressEventHandler progressHandler = null, TaskCompleteEventHandler completeHandler = null)
        {

        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            foreach (Stream s in streams)
                s.Dispose();
        }

        #endregion

    }
}


