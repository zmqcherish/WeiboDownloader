﻿using AngleSharp.Parser.Html;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace WeiboDownloader
{
    class Program
    {
        static bool isDebug = false;
        static string errorMsg = "";
        static string folderPath = @"D:\WeiboFolder\";
        static string urlFilePath = "url.txt";
        static int perNum = 3;
        static List<FileInfo> fileInfoList = new List<FileInfo>();
        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 0 || File.Exists(urlFilePath))
                {
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);
                    var urls = args;
                    if (urls.Length == 0)
                        urls = File.ReadAllLines(urlFilePath);

                    foreach (var url in urls)
                    {
                        Console.WriteLine($"-----> 正在解析微博：{url}");
                        Handler(url);
                    }
                    Console.WriteLine("\n全部解析完成，开始下载。。。");
                    StratDownload();
                    Console.WriteLine("\n全部下载完成，按回车退出程序。");
                }
                else
                {
                    Console.WriteLine($"微博链接文件：{urlFilePath}不存在，请关闭软件后重试。");
                }
            }
            catch (Exception ex)
            {
                errorMsg = isDebug ? ex.ToString() : "未知错误";
                Console.WriteLine(errorMsg);
            }
            Console.ReadKey();
        }

        private static void StratDownload()
        {
            int len = fileInfoList.Count;
            int threadCount = len / perNum + 1;
            var threads = new List<Thread>();
            for (int i = 0; i < threadCount; i++)
            {
                var t = new Thread(DownloadFiles);
                threads.Add(t);
                t.Start(i * perNum);
            }
            foreach (var t in threads)
            {
                t.Join();
            }
        }

        /// <summary>
        /// 处理微博链接
        /// </summary>
        /// <param name="url"></param>
        private static void Handler(string url)
        {
            //url = "http://weibo.com/1880240653/F1iIvq6rb?type=comment";
            string wid = "";
            try
            {
                int left = url.LastIndexOf('/') + 1;
                int right = url.IndexOf('?');
                wid = right == -1 ? url.Substring(left) : url.Substring(left, right - left);
            }
            catch
            {
                Console.WriteLine("微博链接格式错误");
                return;
            }

            string newUrl = "http://m.weibo.cn/status/" + wid;
            try
            {
                string html = GetWebHtml(newUrl);

                //提取图片或视频数据
                var parser = new HtmlParser();
                var document = parser.Parse(html);
                html = document.QuerySelectorAll("script")[1].TextContent;
                html = html.Substring(html.IndexOf("render_data") + 15);
                html = html.Remove(html.Length - 12);

                var status = JObject.Parse(html)["status"];
                var repost = status["retweeted_status"];
                status = repost ?? status;
                var pics = status["pics"];
                if (pics == null)//并不是所有视频都能正常下载
                {
                    string videoUrl = status["page_info"]["media_info"]["stream_url"].ToString();
                    fileInfoList.Add(new FileInfo("视频", wid + ".mp4", videoUrl));
                    //DownloadFile("视频", videoUrl, wid + ".mp4");
                }
                else
                {
                    foreach (var p in pics)
                    {
                        fileInfoList.Add(new FileInfo("图片", p["pid"] + ".jpg", p["large"]["url"].ToString()));
                        //DownloadFile("图片", p["large"]["url"].ToString(), p["pid"] + ".jpg");
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = isDebug ? ex.ToString() : "处理过程出现错误";
                Console.WriteLine(errorMsg);
            }
        }

        private static void DownloadFiles(object startIndex)
        {
            
            int index = (int)startIndex;
            for (int i = index; i < index + perNum; i++)
            {
                if (i>=fileInfoList.Count)
                {
                    break;
                }
                var file = fileInfoList[i];
                var webclient = new WebClient();
                //webclient.DownloadProgressChanged += Webclient_DownloadProgressChanged;
                DownloadFile(webclient, file.Type, file.Url, file.Name);
            }
        }

        private static void DownloadFile(WebClient webclient, string type, string url,string name)
        {
            Console.WriteLine($"正在下载{type}：{name}... ");
            string filepath = folderPath + name;
            if (File.Exists(filepath))
            {
                Console.WriteLine($"{type}：{name}已经存在");
                return;
            }
            try
            {
                webclient.DownloadFile(url, filepath);
                Console.WriteLine($"{type} {name} 下载完成。");
            }
            catch (Exception ex)
            {
                errorMsg = isDebug ? ex.ToString() : "未知错误，下载失败";
                Console.WriteLine(errorMsg);
            }
        }

        private static string GetWebHtml(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "Get";
            request.UserAgent = "User-Agent:Mozilla/5.0 (Windows NT 5.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.202 Safari/535.1";

            var encoding = Encoding.UTF8;//根据网站的编码自定义  
            var response = (HttpWebResponse)request.GetResponse();
            var responseStream = response.GetResponseStream();
            var streamReader = new StreamReader(responseStream, encoding);
            string res = streamReader.ReadToEnd();

            streamReader.Close();
            responseStream.Close();
            return res;
        }
    }

    class FileInfo
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }

        public FileInfo(string type, string name, string url)
        {
            this.Name = name;
            this.Type = type;
            this.Url = url;
        }
    }
}
