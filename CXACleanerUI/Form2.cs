using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;

namespace CXACleanerUI
{
    public partial class Form2 : Form
    {
        int[,] mapdata;
        string host = "192.168.1.101";
        int port = 1234;
        public Form2()
        {
            InitializeComponent();
        }

        string GetResponse(string text) {
            TcpClient client = new TcpClient();
            client.Connect(host, port);
            NetworkStream stream = client.GetStream();
            byte[] sendText = System.Text.Encoding.ASCII.GetBytes(text);
            stream.Write(sendText, 0, sendText.Length);
            stream.Flush();
            byte[] inText = new byte[1024];
            stream.Read(inText, 0, inText.Length);
            string returndata = System.Text.Encoding.ASCII.GetString(inText);
            byte[] exitText = System.Text.Encoding.ASCII.GetBytes("exit");
            stream.Write(exitText, 0, exitText.Length);
            stream.Flush();
            //client.Close();
            return returndata;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            /*try {
                string host = "192.168.1.101";
                int port = 1234;
                textBox1.Text += String.Format("Host: {0} Port: {1}\r\nTrying to connect to the server...\r\n", host, port);
                TcpClient client = new TcpClient();
                client.Connect(host, port);
                textBox1.Text += "Server connected!\r\n";
                textBox1.Text += "Sending data...\r\n";
                NetworkStream stream = client.GetStream();
                //String sb = "\n";
                for (int i = 0; i < mapdata.GetLength(0); ++i)
                {
                    String sb = "";
                    for (int j = 0; j < mapdata.GetLength(1); ++j)
                    {
                        sb += String.Format(" {0}", mapdata[i, j]);
                    }
                    sb += "\n";
                    textBox1.Text += "Send: " + sb + "\r\n";
                    byte[] sendText = System.Text.Encoding.ASCII.GetBytes(sb);
                    stream.Write(sendText, 0, sendText.Length);
                    stream.Flush();
                    byte[] inText = new byte[1024];
                    stream.Read(inText, 0, inText.Length);
                    string returndata = System.Text.Encoding.ASCII.GetString(inText);
                    textBox1.Text += ">> " + returndata + "\r\n";
                }
            } catch (SocketException err){
                textBox1.Text += "Connection denied by the server.\r\n";
                MessageBox.Show("Connection denied by the server.");
            }*/
            try
            {
                var r = GetResponse("checkmaps");
                Console.Write(r);
                while (r.IndexOf("|") != -1) {
                    listBox1.Items.Add(r.Substring(0, r.IndexOf("|")));
                    r = r.Substring(r.IndexOf("|") + 1);
                }
            }
            catch (SocketException err)
            {
                MessageBox.Show(this, "Connection denied by the server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }
    }
}
