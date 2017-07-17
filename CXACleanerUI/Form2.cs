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
        string host = "192.168.1.100";
        int port = 1234;
        int[,] mapdata;
        public Form2(int[,] data)
        {
            InitializeComponent();
            mapdata = data;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            try
            {
                var r = NetUtil.SendLineWithLineResponse("fetchmaplist");
                //Console.Write(r);
                while (r.IndexOf("|") != -1) {
                    listBox1.Items.Add(r.Substring(0, r.IndexOf("|")));
                    r = r.Substring(r.IndexOf("|") + 1);
                }
            }
            catch (SocketException err)
            {
                MessageBox.Show(this, "Connection denied by the server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) {
                MessageBox.Show(this, "Please select a map first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                var r = NetUtil.SendLineWithLongResponse("fetchmapinfo:" + listBox1.Items[listBox1.SelectedIndex]);
                var tmp = r.Split('\n');
                var mapname = listBox1.Items[listBox1.SelectedIndex].ToString();
                var imagepath = tmp[0];
                var resolution = Int32.Parse(tmp[1]);
                var threshold = Int32.Parse(tmp[2]);
                var scale = tmp[3];
                Console.WriteLine("Scale=" + scale);
                r = NetUtil.SendLineWithLongResponse("fetchmapdata:" + listBox1.Items[listBox1.SelectedIndex]);
                string[] lines = r.Split('\n');
                int[,] mapdata = new int[lines.Length, lines[0].Split(' ').Length];
                for (int i = 0; i < lines.Length - 1; i++) {
                    string[] elements = lines[i].Split(' ');
                    for (int j = 0; j < elements.Length - 1; j++) {
                        mapdata[i, j] = Int32.Parse(elements[j]);
                    }
                }
                new Form1(mapname, Directory.GetCurrentDirectory() + "/res/" + imagepath, resolution, threshold, scale, mapdata).Visible = true;
            }
            catch (SocketException err)
            {
                MessageBox.Show(this, "Connection denied by the server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
