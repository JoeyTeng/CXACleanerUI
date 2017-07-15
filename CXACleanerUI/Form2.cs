using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace CXACleanerUI
{
    public partial class Form2 : Form
    {
        string host = "192.168.1.101";
        int port = 1234;
        int[,] mapdata;
        public Form2(int[,] data)
        {
            InitializeComponent();
            mapdata = data;
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
            return returndata;
        }

        string GetLongResponse(string text) {
            TcpClient client = new TcpClient();
            client.Connect(host, port);
            NetworkStream stream = client.GetStream();
            byte[] sendText = System.Text.Encoding.ASCII.GetBytes(text);
            stream.Write(sendText, 0, sendText.Length);
            stream.Flush();
            string sb = "";
            while (true) {
                byte[] inText = new byte[1024];
                stream.Read(inText, 0, inText.Length);
                string returndata = System.Text.Encoding.ASCII.GetString(inText);
                returndata = returndata.TrimEnd('\0');
                if (returndata == "END") {
                    break;
                }
                sb += returndata + "\n";
                byte[] doneText = System.Text.Encoding.ASCII.GetBytes("Done");
                stream.Write(sendText, 0, sendText.Length);
                stream.Flush();
            }
            byte[] exitText = System.Text.Encoding.ASCII.GetBytes("exit");
            stream.Write(exitText, 0, exitText.Length);
            stream.Flush();
            return sb;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            /*try {
                //textBox1.Text += String.Format("Host: {0} Port: {1}\r\nTrying to connect to the server...\r\n", host, port);
                TcpClient client = new TcpClient();
                client.Connect(host, port);
                //textBox1.Text += "Server connected!\r\n";
                //textBox1.Text += "Sending data...\r\n";
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
                    //textBox1.Text += "Send: " + sb + "\r\n";
                    byte[] sendText = System.Text.Encoding.ASCII.GetBytes(sb);
                    stream.Write(sendText, 0, sendText.Length);
                    stream.Flush();
                    byte[] inText = new byte[1024];
                    stream.Read(inText, 0, inText.Length);
                    string returndata = System.Text.Encoding.ASCII.GetString(inText);
                    //textBox1.Text += ">> " + returndata + "\r\n";
                }
            } catch (SocketException err){
                //textBox1.Text += "Connection denied by the server.\r\n";
                MessageBox.Show("Connection denied by the server.");
            }*/
            try
            {
                var r = GetResponse("fetchmaplist");
                //Console.Write(r);
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

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) {
                MessageBox.Show(this, "Please select a map first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            try
            {
                var r = GetLongResponse("fetchmapinfo:" + listBox1.Items[listBox1.SelectedIndex]);
                var tmp = r.Split('\n');
                //MessageBox.Show(this, r, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                var mapname = listBox1.Items[listBox1.SelectedIndex].ToString();
                var imagepath = tmp[0];
                var resolution = Int32.Parse(tmp[1]);
                var threshold = Int32.Parse(tmp[2]);
                r = GetLongResponse("fetchmapdata:" + listBox1.Items[listBox1.SelectedIndex]);
                //MessageBox.Show(this, r, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                string[] lines = r.Split('\n');
                int[,] mapdata = new int[lines.Length, lines[0].Split(' ').Length];
                for (int i = 0; i < lines.Length - 1; i++) {
                    string[] elements = lines[i].Split(' ');
                    for (int j = 0; j < elements.Length - 1; j++) {
                        mapdata[i, j] = Int32.Parse(elements[j]);
                    }
                }
                new Form1(mapname, Directory.GetCurrentDirectory() + "/res/" + imagepath, resolution, threshold, mapdata).Visible = true;
            }
            catch (SocketException err)
            {
                MessageBox.Show(this, "Connection denied by the server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
