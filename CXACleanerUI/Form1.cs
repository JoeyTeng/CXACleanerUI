using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace CXACleanerUI
{
    using System.Net.Sockets;
    using MapNode = System.Int32;
    public partial class Form1 : Form
    {
        MapNode[,] mapdata;
        Image map;
        Point mouseStart = new Point(-1, -1);
        int[,] dragarea;
        string imageFileName;
        int imageResolution;
        int threshold;
        List<AgentApplication.Agent> agentlist =  new List<AgentApplication.Agent>();
        public Form1(string mapname = null, string imagePath = null, int res = 15, int thr = 600, int[,] data = null)
        {
            InitializeComponent();
            if (mapname == null)
            {
                this.Text = "New Map";
            }
            else {
                this.Text = "Map - " + mapname;
            }
            imageFileName = imagePath;
            textBox1.Text = res.ToString();
            imageResolution = res;
            textBox2.Text = thr.ToString();
            threshold = thr;
            mapdata = data;
            RefreshImage();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RefreshImage();
            comboBox1.SelectedIndex = 0;
            toolStripStatusLabel1.Text = "Ready.";
        }

        private void RefreshImage()
        {
            if (imageFileName == null)
            {
                return;
            }
            var imagefromfile = new Bitmap(imageFileName);
            map = new Bitmap(imagefromfile.Width + 2 * imageResolution, imagefromfile.Height + 2 * imageResolution, PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(map);
            g.FillRectangle(Brushes.White, 0, 0, imagefromfile.Width + 2 * imageResolution, imagefromfile.Height + 2 * imageResolution);
            g.DrawImage(imagefromfile, imageResolution, imageResolution, imagefromfile.Width, imagefromfile.Height);
            for (int i = 0; i < mapdata.GetLength(0); i++)
            {
                for (int j = 0; j < mapdata.GetLength(1); j++)
                {
                    if (checkBox2.Checked == true)
                    {
                        if (Constants.MappingConstants.Unblocked(mapdata, new RoutingApplication.Coordinate(i, j)))
                        {
                            g.DrawRectangle(new Pen(Color.DarkGray, (float)1), i * imageResolution, j * imageResolution, imageResolution, imageResolution);
                        }
                    }
                    if (Constants.MappingConstants.Selected(mapdata, new RoutingApplication.Coordinate(i, j)))
                    {
                        g.FillRectangle(new SolidBrush(Color.FromArgb(128, Color.LightGreen)), i * imageResolution, j * imageResolution, imageResolution, imageResolution);
                        if (checkBox2.Checked == true) { g.DrawRectangle(new Pen(Color.DarkGray, (float)1), i * imageResolution, j * imageResolution, imageResolution, imageResolution); }
                    }
                }
            }
            foreach (AgentApplication.Agent a in agentlist) {
                RoutingApplication.RouteNode[] route = a.route;
                Pen p = new Pen(Color.Orange);
                GraphicsPath capPath = new GraphicsPath();
                capPath.AddLine(0, 0, -4, -8);
                capPath.AddLine(0, 0, 4, -8);
                //capPath.AddLine(4, -4, 0, 5);
                p.CustomEndCap = new CustomLineCap(null, capPath);
                int currentX = a.chargerPosition.x; int currentY = a.chargerPosition.y;
                if (route == null)
                {
                    pictureBox1.Image = map;
                    continue;
                }
                g.DrawString(a._serialNumber.ToString(), new Font("Arial", (float)10), Brushes.Orange, (float)(0.5 + currentX) * imageResolution - 10, (float)(0.5 + currentY) * imageResolution - 10);
                foreach (RoutingApplication.RouteNode i in route)
                {
                    g.DrawLine(p, (float)(0.5 + currentX) * imageResolution, (float)(0.5 + currentY) * imageResolution,
                        (float)(0.5 + currentX + Constants.RoutingConstants.MOVE_INCREMENT[i.direction].x * i.steps) * imageResolution,
                        (float)(0.5 + currentY + Constants.RoutingConstants.MOVE_INCREMENT[i.direction].y * i.steps) * imageResolution);
                    currentX += Constants.RoutingConstants.MOVE_INCREMENT[i.direction].x * i.steps;
                    currentY += Constants.RoutingConstants.MOVE_INCREMENT[i.direction].y * i.steps;
                }
                //pictureBox1.Image = map;
            }
            pictureBox1.Image = map;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (imageFileName != null)
            {
                if (checkBox3.Checked) {
                    agentlist.Add(new AgentApplication.Agent(agentlist.Count + 1, new RoutingApplication.Coordinate((int)(e.X / imageResolution), (int)(e.Y / imageResolution))));
                    RoutingApplication.RouteNode[] route = Mapping.FindPath(mapdata, (int)(e.X / imageResolution), (int)(e.Y / imageResolution), selectedOnly: true);
                    if (route == null) { agentlist.RemoveAt(agentlist.Count - 1); }
                    else { agentlist[agentlist.Count - 1].UpdateRoute(route); }
                    RefreshImage();
                }
                else if (radioButton1.Checked)
                {
                    int X = (int)(e.X / imageResolution);
                    int Y = (int)(e.Y / imageResolution);
                    if (Constants.MappingConstants.Unblocked(mapdata, new RoutingApplication.Coordinate(X, Y)) && Constants.MappingConstants.Deselected(mapdata, new RoutingApplication.Coordinate(X, Y))) {
                        Constants.MappingConstants.Select(mapdata, new RoutingApplication.Coordinate(X, Y));
                    }
                    else if (Constants.MappingConstants.Selected(mapdata, new RoutingApplication.Coordinate(X, Y))) {
                        Constants.MappingConstants.Deselect(mapdata, new RoutingApplication.Coordinate(X, Y));
                    }
                    RefreshImage();
                }
                else if (radioButton2.Checked)
                {
                    mouseStart = new Point(e.X, e.Y);
                }
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripStatusLabel1.Text = "X: " + ((int)(e.X / imageResolution)).ToString() + ", Y: " + ((int)(e.Y / imageResolution)).ToString();
            if (imageFileName != null)
            {
                if (radioButton2.Checked && mouseStart != new Point(-1, -1))
                {
                    RefreshImage();
                    Graphics g = Graphics.FromImage(map);
                    Point topleft = new Point(Math.Min(mouseStart.X, e.X), (Math.Min(mouseStart.Y, e.Y)));
                    Point bottomright = new Point(Math.Max(mouseStart.X, e.X), (Math.Max(mouseStart.Y, e.Y)));
                    g.DrawRectangle(new Pen(Color.Red, (float)1), topleft.X, topleft.Y, bottomright.X - topleft.X, bottomright.Y - topleft.Y);
                    pictureBox1.Image = map;
                }
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (imageFileName != null)
            {
                if (radioButton2.Checked && mouseStart != new Point(-1, -1))
                {
                    Point topleft = new Point(Math.Min(mouseStart.X, e.X), (Math.Min(mouseStart.Y, e.Y)));
                    Point bottomright = new Point(Math.Max(mouseStart.X, e.X), (Math.Max(mouseStart.Y, e.Y)));
                    var startPoint = new Point((int)(topleft.X / imageResolution), (int)(topleft.Y / imageResolution));
                    var endPoint = new Point((int)(bottomright.X / imageResolution), (int)(bottomright.Y / imageResolution));
                    for (int i = startPoint.X; i < endPoint.X + 1; i++)
                    {
                        for (int j = startPoint.Y; j < endPoint.Y + 1; j++)
                        {
                            if (comboBox1.SelectedIndex == 0 && Constants.MappingConstants.Unblocked(mapdata, new RoutingApplication.Coordinate(i, j))) {
                                Constants.MappingConstants.Select(mapdata, new RoutingApplication.Coordinate(i, j));
                            }
                            else if (comboBox1.SelectedIndex == 1 && Constants.MappingConstants.Unblocked(mapdata, new RoutingApplication.Coordinate(i, j))) {
                                Constants.MappingConstants.Deselect(mapdata, new RoutingApplication.Coordinate(i, j));
                            }
                        }
                    }
                    RefreshImage();
                    mouseStart = new Point(-1, -1);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            imageResolution = Int32.Parse(textBox1.Text);
            threshold = Int32.Parse(textBox2.Text);
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK) {
                var filename = ofd.FileName;
                imageFileName = filename;
                mapdata = Mapping.Execute(imageFileName, imageResolution, threshold);
                dragarea = new MapNode[mapdata.GetLength(0), mapdata.GetLength(1)];
                RefreshImage();
            }
        }
        private void radioButton1_Click(object sender, EventArgs e)
        {
            radioButton2.Checked = false;
            comboBox1.Enabled = false;
        }

        private void radioButton2_Click(object sender, EventArgs e)
        {
            radioButton1.Checked = false;
            comboBox1.Enabled = true;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            RefreshImage();
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Ready.";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            new Form2(mapdata).Visible = true;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox("Input a map name:", "Save as...", "New Map 1", -1, -1);
            TcpClient client = new TcpClient();
            client.Connect("192.168.1.101", 1234);
            NetworkStream stream = client.GetStream();
            byte[] sendText = System.Text.Encoding.ASCII.GetBytes("uploadmapdata:" + input);
            stream.Write(sendText, 0, sendText.Length);
            stream.Flush();
            byte[] inText = new byte[1024];
            stream.Read(inText, 0, inText.Length);
            sendText = System.Text.Encoding.ASCII.GetBytes(imageFileName.Substring(imageFileName.LastIndexOf(@"\") + 1));
            stream.Write(sendText, 0, sendText.Length);
            stream.Flush();
            inText = new byte[1024];
            stream.Read(inText, 0, inText.Length);
            sendText = System.Text.Encoding.ASCII.GetBytes(imageResolution.ToString());
            stream.Write(sendText, 0, sendText.Length);
            stream.Flush();
            inText = new byte[1024];
            stream.Read(inText, 0, inText.Length);
            sendText = System.Text.Encoding.ASCII.GetBytes(threshold.ToString());
            stream.Write(sendText, 0, sendText.Length);
            stream.Flush();
            inText = new byte[1024];
            stream.Read(inText, 0, inText.Length);
            for (int i = 0; i < mapdata.GetLength(0); ++i)
            {
                String sb = "";
                for (int j = 0; j < mapdata.GetLength(1); ++j)
                {
                    sb += String.Format(" {0}", mapdata[i, j]);
                }
                sb += "\n";
                sendText = System.Text.Encoding.ASCII.GetBytes(sb);
                stream.Write(sendText, 0, sendText.Length);
                stream.Flush();
                inText = new byte[1024];
                stream.Read(inText, 0, inText.Length);
            }
            sendText = System.Text.Encoding.ASCII.GetBytes("END");
            stream.Write(sendText, 0, sendText.Length);
            stream.Flush();
            foreach (AgentApplication.Agent a in agentlist)
            {
                sendText = System.Text.Encoding.ASCII.GetBytes(a.Transport() + a.facingDirection);
                stream.Write(sendText, 0, sendText.Length);
                stream.Flush();
                inText = new byte[1024];
                stream.Read(inText, 0, inText.Length);
            }
            sendText = System.Text.Encoding.ASCII.GetBytes("END");
            stream.Write(sendText, 0, sendText.Length);
            stream.Flush();
            byte[] exitText = System.Text.Encoding.ASCII.GetBytes("exit");
            stream.Write(exitText, 0, exitText.Length);
            stream.Flush();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == MessageBox.Show(this, "This action will send intructions to available agents. Continue?", "Notice", MessageBoxButtons.OKCancel, MessageBoxIcon.Question)) {
                if (agentlist.Count == 0) {
                    MessageBox.Show(this, "No agent has been assigned.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                TcpClient client = new TcpClient();
                client.Connect("192.168.1.101", 1234);
                NetworkStream stream = client.GetStream();
                byte[] sendText = System.Text.Encoding.ASCII.GetBytes("applytransport:" + agentlist[0].Transport());
                stream.Write(sendText, 0, sendText.Length);
                stream.Flush();
                byte[] inText = new byte[1024];
                stream.Read(inText, 0, inText.Length);
                byte[] exitText = System.Text.Encoding.ASCII.GetBytes("exit");
                stream.Write(exitText, 0, exitText.Length);
                stream.Flush();
            }
        }
    }
}
