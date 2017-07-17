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
        string mapname;
        List<AgentApplication.Agent> agentlist =  new List<AgentApplication.Agent>();
        int agentSerial = 0;
        public Form1(string mapname = null, string imagePath = null, int res = 15, int thr = 600, int[,] data = null)
        {
            InitializeComponent();
            if (mapname == null)
            {
                this.Text = "New Map";
            }
            else {
                this.Text = "Map - " + mapname;
                this.mapname = mapname;
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
                Pen p = new Pen(comboBox2.SelectedIndex == agentlist.IndexOf(a) ? Color.Red : Color.Orange);
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
                g.FillRectangle(new SolidBrush(Color.FromArgb(128, Color.Yellow)), currentX * imageResolution, currentY * imageResolution, imageResolution, imageResolution);
                g.DrawString(a._serialNumber.ToString(), new Font("Arial", (float)10), comboBox2.SelectedIndex == agentlist.IndexOf(a) ? Brushes.Red : Brushes.Orange, (float)(0.5 + currentX) * imageResolution - 10, (float)(0.5 + currentY) * imageResolution - 10);
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
            if (agentlist.Count != 0)
            {
                switch (agentlist[comboBox2.SelectedIndex].status)
                {
                    case AgentApplication.AgentStatus.IDLE:
                        statusLabel.Text = "Idle";
                        statusLabel.ForeColor = Color.Orange;
                        break;
                    case AgentApplication.AgentStatus.ACTIVE:
                        statusLabel.Text = "On Duty";
                        statusLabel.ForeColor = Color.DarkGreen;
                        break;
                    case AgentApplication.AgentStatus.AVOIDING:
                        statusLabel.Text = "Avoiding";
                        statusLabel.ForeColor = Color.Red;
                        break;
                    case AgentApplication.AgentStatus.ERROR:
                        statusLabel.Text = "Avoiding";
                        statusLabel.ForeColor = Color.Red;
                        break;
                    case AgentApplication.AgentStatus.RETURNING:
                        statusLabel.Text = "Returning";
                        statusLabel.ForeColor = Color.Orange;
                        break;
                    case AgentApplication.AgentStatus.TRANSFERING:
                        statusLabel.Text = "Transfering";
                        statusLabel.ForeColor = Color.Blue;
                        break;
                }
            }
            else {
                statusLabel.Text = "Null";
                statusLabel.ForeColor = Color.DarkGray;
            }
            pictureBox1.Image = map;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (imageFileName != null)
            {
                if (checkBox3.Checked) {
                    agentlist.Add(new AgentApplication.Agent(agentSerial + 1, new RoutingApplication.Coordinate((int)(e.X / imageResolution), (int)(e.Y / imageResolution))));
                    RoutingApplication.RouteNode[] route = Mapping.FindPath(mapdata, (int)(e.X / imageResolution), (int)(e.Y / imageResolution), selectedOnly: true);
                    if (route == null) { agentlist.RemoveAt(agentlist.Count - 1); }
                    else
                    {
                        agentlist[agentlist.Count - 1].UpdateRoute(route);
                        agentlist[agentlist.Count - 1].status = AgentApplication.AgentStatus.IDLE;
                        comboBox2.Enabled = true;
                        button4.Enabled = true; button5.Enabled = true; button6.Enabled = true;
                        comboBox2.Items.Add(agentlist[agentlist.Count - 1]._serialNumber.ToString());
                        comboBox2.SelectedIndex = agentlist.Count - 1;
                        agentSerial++;
                    }
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
            if (imageFileName == null)
            {
                MessageBox.Show(this, "Please create a map before saving.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string input = Microsoft.VisualBasic.Interaction.InputBox("Input a map name:", "Save as...", mapname == null ? "New Map 1" : mapname, -1, -1);
            if (input == "") { return; }
            try {
                List<string> textToSend = new List<string>();
                textToSend.Add("uploadmapdata:" + input);
                textToSend.Add(imageFileName.Substring(imageFileName.LastIndexOf(@"\") + 1));
                textToSend.Add(imageResolution.ToString());
                textToSend.Add(threshold.ToString());
                for (int i = 0; i < mapdata.GetLength(0); ++i)
                {
                    String sb = "";
                    for (int j = 0; j < mapdata.GetLength(1); ++j)
                    {
                        sb += String.Format(" {0}", mapdata[i, j]);
                    }
                    sb += "\n";
                    textToSend.Add(sb);
                }
                textToSend.Add("END");
                foreach (AgentApplication.Agent a in agentlist)
                {
                    textToSend.Add(a.Transport() + a.facingDirection);
                }
                textToSend.Add("END");
                NetUtil.SendParagraph(textToSend);
            } catch(SocketException err)
            {
                MessageBox.Show(this, "Connection denied by the server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == MessageBox.Show(this, "This action will send intructions to available agents. Continue?", "Notice", MessageBoxButtons.OKCancel, MessageBoxIcon.Question)) {
                if (agentlist.Count == 0) {
                    MessageBox.Show(this, "No agent has been assigned.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                try
                {
                    NetUtil.SendLineWithReceipt("applytransport:" + agentlist[0].Transport());
                }
                catch (SocketException err)
                {
                    MessageBox.Show(this, "Connection denied by the server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void checkBox3_CheckedChanged_1(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            panel2.Visible = true;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            panel2.Visible = false;
            imageResolution = Int32.Parse(textBox1.Text);
            threshold = Int32.Parse(textBox2.Text);
            if (imageFileName != null) {
                mapdata = Mapping.Execute(imageFileName, imageResolution, threshold);
                dragarea = new MapNode[mapdata.GetLength(0), mapdata.GetLength(1)];
                RefreshImage();
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            panel2.Visible = false;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshImage();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            agentlist.RemoveAt(comboBox2.SelectedIndex);
            comboBox2.Items.RemoveAt(comboBox2.SelectedIndex);
            if (agentlist.Count == 0)
            {
                comboBox2.Enabled = false;
                button4.Enabled = false; button5.Enabled = false; button6.Enabled = false;
            }
            else {
                comboBox2.SelectedIndex = 0;
            }
            RefreshImage();
        }
    }
}
