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
    public partial class Form1 : Form
    {
        int[,] mapdata;
        Image map;
        Point mouseStart = new Point(-1, -1);
        int[,] dragarea;
        string imageFileName;
        int imageResolution;
        public Form1()
        {
            InitializeComponent();
            imageFileName = null;
            imageResolution = 20;
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
                        if (mapdata[i, j] == 1)
                        {
                            g.DrawRectangle(new Pen(Color.Black, (float)1), i * imageResolution, j * imageResolution, imageResolution, imageResolution);
                        }
                    }
                    if (mapdata[i, j] == 2)
                    {
                        g.FillRectangle(new SolidBrush(Color.FromArgb(128, Color.LightGreen)), i * imageResolution, j * imageResolution, imageResolution, imageResolution);
                        if (checkBox2.Checked == true) { g.DrawRectangle(new Pen(Color.Black, (float)1), i * imageResolution, j * imageResolution, imageResolution, imageResolution); }
                    }
                }
            }
            pictureBox1.Image = map;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (imageFileName != null)
            {
                if (radioButton1.Checked)
                {
                    int X = (int)(e.X / imageResolution);
                    int Y = (int)(e.Y / imageResolution);
                    if (mapdata[X, Y] == 1) { mapdata[X, Y] = 2; }
                    else if (mapdata[X, Y] == 2) { mapdata[X, Y] = 1; }
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
            /*
            if (mousedown == true) {
                int Y = (int)(e.X / imageResolution);
                int X = (int)(e.Y / imageResolution);
                if (dragarea[Y, X] == 0) {
                    dragarea[Y, X] = 1;
                }
                Graphics g = Graphics.FromImage(map);
                g.FillRectangle(Brushes.Red, e.X, e.Y, 3, 3);
                pictureBox1.Image = map;
            }
            */
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
                            if (comboBox1.SelectedIndex == 0 && mapdata[i, j] != 0) { mapdata[i, j] = 2; }
                            else if (comboBox1.SelectedIndex == 1 && mapdata[i, j] != 0) { mapdata[i, j] = 1; }
                        }
                    }
                    RefreshImage();
                    mouseStart = new Point(-1, -1);
                }
            }
            /*
                mousedown = false;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (dragarea[i, j] == 1)
                        {
                            if (mapdata[i, j] == 1) { mapdata[i, j] = 2; }
                            else if (mapdata[i, j] == 2) { mapdata[i, j] = 1; }
                        }
                    }
                }
                dragarea = new int[3, 4];
                RefreshImage();
                */
        }

        private void button1_Click(object sender, EventArgs e)
        {
            imageResolution = Int32.Parse(textBox1.Text);
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK) {
                var filename = ofd.FileName;
                imageFileName = filename;
                mapdata = Mapping.Execute(imageFileName, imageResolution, Int32.Parse(textBox2.Text));
                dragarea = new int[mapdata.GetLength(0), mapdata.GetLength(1)];
                RefreshImage();
            }
            //var imagefromfile = new Bitmap(imageFileName);
            //pictureBox1.Size = new Size(imagefromfile.Width + 2 * imageResolution, imagefromfile.Height + 2 * imageResolution);
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
            new Form2().Visible = true;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            Image pic = pictureBox1.Image;
            Graphics g = Graphics.FromImage(pic);
            Pen p = new Pen(Color.Orange);
            GraphicsPath capPath = new GraphicsPath();
            capPath.AddLine(-4, -4, 4, -4);
            capPath.AddLine(-4, -4, 0, 5);
            capPath.AddLine(4, -4, 0, 5);
            p.CustomEndCap = new System.Drawing.Drawing2D.CustomLineCap(null, capPath);
            RoutingApplication.RouteNode[] route = Mapping.FindPath(mapdata, 1, 1);
            int currentX = 1; int currentY = 1;
            foreach (RoutingApplication.RouteNode i in route) {
                g.DrawLine(p, (float)(0.5 + currentX) * imageResolution, (float)(0.5 + currentY) * imageResolution, 
                    (float)(0.5 + currentX + Constants.RoutingConstants.MOVE_INCREMENT[i.direction].x * i.steps) * imageResolution,
                    (float)(0.5 + currentY + Constants.RoutingConstants.MOVE_INCREMENT[i.direction].y * i.steps) * imageResolution);
                currentX += Constants.RoutingConstants.MOVE_INCREMENT[i.direction].x * i.steps;
                currentY += Constants.RoutingConstants.MOVE_INCREMENT[i.direction].y * i.steps;
            }
            pictureBox1.Image = pic;
        }
    }
}
