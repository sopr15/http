using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Http
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri uri = new Uri("http://example.com"); //http://info.cern.ch

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine("Setting up connection to {0} port: {1}", uri.Host, uri.Port);
            socket.Connect(uri.Host, uri.Port);
            Console.WriteLine("Connection set up");

            string request = "GET / HTTP/1.1\r\n" +
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
            socket.Close();
            System.IO.File.WriteAllText(@"file.txt", headerString + "\n\n" + wholeBody);
            Console.ReadKey();
        }
    }
}