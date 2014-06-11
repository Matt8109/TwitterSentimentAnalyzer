using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using Oclumen.Core.Entities;
using Oclumen.Core.Helpers;

namespace Oclumen.Core.Twitter
{
    /// <summary>
    ///     Code originally from https://github.com/swhitley/TwitterStreamClient,
    ///     also include Tweet and Twitter Account Classes
    ///     Accessed April 14, 2013
    /// </summary>
    public class TwitterStreamClient
    {
        public TwitterStreamClient(Queue<string> unprocessedTweets, CrawlerStatus crawlerStatus, ref Object syncRoot)
        {
            UnprocessedTweets = unprocessedTweets;
            SyncRoot = syncRoot;
            CrawlerStatus = crawlerStatus;
        }

        private Queue<String> UnprocessedTweets { get; set; }
        private Object SyncRoot { get; set; }
        private CrawlerStatus CrawlerStatus { get; set; }

        public void ConsumeStream(String twitterUsername, String twitterPassword, String trackKeywords,
                                  String twitterStreamApiUrl)
        {
            HttpWebRequest webRequest = null;
            HttpWebResponse webResponse = null;
            StreamReader responseStream = null;

            string postparameters = "&track=" + trackKeywords;


            int wait = 250;
            string jsonText = "";

            try
            {
                while (CrawlerStatus.KeepRunning)
                {
                    try
                    {
                        //Connect
                        webRequest = (HttpWebRequest) WebRequest.Create(twitterStreamApiUrl);
                        webRequest.Credentials = new NetworkCredential(twitterUsername, twitterPassword);
                        webRequest.Timeout = -1;

                        Encoding encode = Encoding.GetEncoding("utf-8");
                        if (postparameters.Length > 0)
                        {
                            webRequest.Method = "POST";
                            webRequest.ContentType = "application/x-www-form-urlencoded";

                            byte[] _twitterTrack = encode.GetBytes(postparameters);

                            webRequest.ContentLength = _twitterTrack.Length;
                            Stream twitterPost = webRequest.GetRequestStream();
                            twitterPost.Write(_twitterTrack, 0, _twitterTrack.Length);
                            twitterPost.Close();
                        }

                        webResponse = (HttpWebResponse) webRequest.GetResponse();
                        responseStream = new StreamReader(webResponse.GetResponseStream(), encode);

                        //Read the stream.
                        while (CrawlerStatus.KeepRunning)
                        {
                            jsonText = responseStream.ReadLine();
                            //Post each message to the queue.

                            //Success
                            wait = 250;

                            //Write Status
                            lock (SyncRoot)
                            {
                                UnprocessedTweets.Enqueue(jsonText);
                            }
                        }
                        //Abort is needed or responseStream.Close() will hang.
                        webRequest.Abort();
                        responseStream.Close();
                        responseStream = null;
                        webResponse.Close();
                        webResponse = null;
                    }

                    catch (WebException ex)
                    {
                        Console.WriteLine(ex.Message);
                        //logger.append(ex.Message, Logger.LogLevel.ERROR);
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            //-- From Twitter Docs -- 
                            //When a HTTP error (> 200) is returned, back off exponentially. 
                            //Perhaps start with a 10 second wait, double on each subsequent failure, 
                            //and finally cap the wait at 240 seconds. 
                            //Exponential Backoff
                            if (wait < 10000)
                            {
                                wait = 10000;
                            }
                            else
                            {
                                if (wait < 240000)
                                {
                                    wait = wait*2;
                                }
                            }
                        }
                        else
                        {
                            //-- From Twitter Docs -- 
                            //When a network error (TCP/IP level) is encountered, back off linearly. 
                            //Perhaps start at 250 milliseconds and cap at 16 seconds.
                            //Linear Backoff
                            if (wait < 16000)
                            {
                                wait += 250;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        //logger.append(ex.Message, Logger.LogLevel.ERROR);
                    }
                    finally
                    {
                        if (webRequest != null)
                        {
                            webRequest.Abort();
                        }
                        if (responseStream != null)
                        {
                            responseStream.Close();
                            responseStream = null;
                        }

                        if (webResponse != null)
                        {
                            webResponse.Close();
                            webResponse = null;
                        }
                        Console.WriteLine("Waiting: " + wait);
                        Thread.Sleep(wait);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ////logger.append(ex.Message, Logger.LogLevel.ERROR);
                //Console.WriteLine("Waiting: " + wait);
                Thread.Sleep(wait);
            }
        }
    }
}