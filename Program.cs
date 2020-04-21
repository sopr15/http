using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Http
{
    class Program
    {
        static public void getRequest(Uri uri)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(uri.Host, uri.Port);
            Console.WriteLine("Connection set up");

            string request = "GET " + uri.AbsolutePath + " HTTP/1.1\r\n" +
                           "Host:" + uri.Host +
                           "\r\n" +
                           "\r\n";
            socket.Send(Encoding.ASCII.GetBytes(request));
            Console.WriteLine("Server Response :");

            bool loop = true;
            string headerString = "";
            string wholeBody = "";
            int ContentLenght = 0;
            int readSoFar = 0;
            while (loop)
            {
                byte[] buffer = new byte[2000];
                socket.Receive(buffer, 0, 2000, 0);
                headerString += Encoding.ASCII.GetString(buffer);
                if (headerString.Contains("\r\n\r\n"))
                {
                    readSoFar += headerString.Length - (headerString.IndexOf("\r\n\r\n") + 4);
                    string temp = headerString.Substring(headerString.IndexOf("\r\n\r\n") + 4, readSoFar);
                    headerString = headerString.Substring(0, headerString.IndexOf("\r\n\r\n") + 4);
                    Console.WriteLine(temp);
                    wholeBody += temp;
                    ContentLenght += temp.Length;
                    string[] lines = headerString.Split('\r');
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].Contains("Content-Length"))
                        {
                            ContentLenght = Int32.Parse(Regex.Match(lines[i], @"\d+").Value);
                        }
                    }
                    byte[] bodyBuff = new byte[2000];
                    while (readSoFar < ContentLenght)
                    {
                        Console.WriteLine(readSoFar);
                        int rec = socket.Receive(bodyBuff, 0, 2000, 0);
                        readSoFar += rec;
                        temp = Encoding.ASCII.GetString(bodyBuff);
                        temp = temp.Substring(0, rec);
                        wholeBody += temp;
                        Console.Write(temp);
                    }
                    loop = false;
                }
            }
            System.IO.File.WriteAllText(@"get_request.txt", headerString + "\n\n" + wholeBody);
            socket.Close();
        }

        static public void headRequest(Uri uri)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(uri.Host, uri.Port);
            Console.WriteLine("Connection set up");

            string request = "HEAD / HTTP/1.1\r\n" +
                            "Host:" + uri.Host +
                            "\r\n" +
                            "\r\n";
            socket.Send(Encoding.ASCII.GetBytes(request));
            Console.WriteLine("Server Response :");

            bool loop = true;
            string headerString = "";
            while (loop)
            {
                byte[] buffer = new byte[2000];
                socket.Receive(buffer, 0, 2000, 0);
                headerString += Encoding.ASCII.GetString(buffer);
                if (headerString.Contains("\r\n\r\n"))
                {
                    Console.WriteLine(headerString);
                    loop = false;
                }
            }

            System.IO.File.WriteAllText(@"head_request.txt", headerString);
            socket.Close();
        }

        static public void postRequest(Uri uri)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(uri.Host, uri.Port);
            Console.WriteLine("Connection set up");

            //ContentType = "application/x-www-form-urlencoded\r\n";    Ši eilutė tekstą spausdina kaip parametrus.
            String ContentType = "";
            ContentType = "text/plain\r\n";
            string postData = "Body posted.";

            string request = "POST " + uri.AbsolutePath + " HTTP/1.1\r\n" +
                            "Host: " + uri.Host + "\r\n" +
                            "Content-length: " + postData.Length + "\r\n" +
                            "Connection: Keep - Alive\r\n" +
                            "Accept: \r\n" +
                            "Content-Type:" + ContentType + "\r\n" + "\r\n";

            byte[] dataSent = Encoding.UTF8.GetBytes(request +postData);
            byte[] dataReceived = new byte[10000];

            socket.Send(dataSent, dataSent.Length, 0);
            int bytes = 0;
            
            Console.WriteLine("Server Response :");
            bool loop = true;
            string headerString = "";
            while (loop)
            {
                bytes = socket.Receive(dataReceived, dataReceived.Length, 0);
                headerString = headerString + Encoding.ASCII.GetString(dataReceived, 0, bytes);
                
                if (bytes > 0)
                {
                    Console.WriteLine(headerString);
                    loop = false;
                }
            }
            System.IO.File.WriteAllText(@"post_request.txt", headerString);
            socket.Close();
        }
            static void Main(string[] args)
            {
                Uri uri = new Uri("http://ptsv2.com/t/wrnkm-1587408206/post"); //http://info.cern.ch  
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Console.WriteLine("Setting up connection to {0} port: {1}", uri.Host, uri.Port);
                Console.WriteLine(uri.AbsolutePath);
                headRequest(uri);
                postRequest(uri);
                getRequest(uri);
                Console.ReadKey();
            }
        }
    }
