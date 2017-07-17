using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace CXACleanerUI
{
    class NetUtil
    {
        static string host = "192.168.1.100";
        static int port = 1234;
        static int timeout = 2000;
        public static void SendText(NetworkStream stream, string textToSend)
        {
            byte[] sendText = System.Text.Encoding.ASCII.GetBytes(textToSend);
            stream.Write(sendText, 0, sendText.Length);
            stream.Flush();
            Console.WriteLine(string.Format("Message sent: {0}", textToSend));
        }
        public static string ReceiveText(NetworkStream stream)
        {
            byte[] inText = new byte[1024];
            stream.Read(inText, 0, inText.Length);
            string returndata = System.Text.Encoding.ASCII.GetString(inText);
            returndata = returndata.TrimEnd('\0');
            Console.WriteLine(string.Format("Message received: {0}", returndata));
            return returndata;
        }
        public static void EndConnection(NetworkStream stream)
        {
            SendText(stream, "exit");
        }
        public static void SendLineWithReceipt(string textToSend)
        {
            TcpClient client = new TcpClient();
            IAsyncResult result = client.BeginConnect(host, port, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(timeout, true);
            if (!success) { client.Close(); throw new SocketException(); }
            Console.WriteLine(string.Format("Connected to {0}:{1}", host, port));
            NetworkStream stream = client.GetStream();
            SendText(stream, textToSend);
            ReceiveText(stream);
            EndConnection(stream);
        }
        public static string SendLineWithLineResponse(string textToSend)
        {
            TcpClient client = new TcpClient();
            IAsyncResult result = client.BeginConnect(host, port, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(timeout, true);
            if (!success) { client.Close(); throw new SocketException(); }
            Console.WriteLine(string.Format("Connected to {0}:{1}", host, port));
            NetworkStream stream = client.GetStream();
            SendText(stream, textToSend);
            string returndata = ReceiveText(stream);
            EndConnection(stream);
            return returndata;
        }
        public static string SendLineWithLongResponse(string textToSend)
        {
            TcpClient client = new TcpClient();
            IAsyncResult result = client.BeginConnect(host, port, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(timeout, true);
            if (!success) { client.Close(); throw new SocketException(); }
            NetworkStream stream = client.GetStream();
            SendText(stream, textToSend);
            string stringbuilder = "";
            while (true)
            {
                string returndata = ReceiveText(stream);
                if (returndata == "END")
                {
                    break;
                }
                stringbuilder += returndata + "\n";
                SendText(stream, "Done");
            }
            EndConnection(stream);
            return stringbuilder;
        }
        public static void SendParagraph(List<string> textToSend) {
            TcpClient client = new TcpClient();
            IAsyncResult result = client.BeginConnect(host, port, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(timeout, true);
            if (!success) { client.Close(); throw new SocketException(); }
            NetworkStream stream = client.GetStream();
            foreach (string t in textToSend) { SendText(stream, t); if (t != "END") { ReceiveText(stream); } }
            EndConnection(stream);
        }
    }
}
