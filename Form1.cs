using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CP_PAINT_APPLICATION
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            bm = new Bitmap(pic.Width, pic.Height);
            g = Graphics.FromImage(bm);
            g.Clear(Color.White);
            pic.Image = bm;
            this.Resize += Form1_Resize;
            this.MinimumSize = new Size(950, 461);
            trackBar1.Scroll += trackBar1_Scroll;
            label1.Text = $"Thickness: {thickness}";
        }
        Bitmap bm;
        Graphics g;
        bool paint = false;
        Point px, py;
        Pen p = new Pen(Color.Black, 2);
        int index;
        Pen erase = new Pen(Color.White, 10);
        int x, y, sX, sY, cX, cY;
        ColorDialog cd = new ColorDialog();
        Color new_color;
        private Stack<Bitmap> undoStack = new Stack<Bitmap>();
        private Stack<Bitmap> redoStack = new Stack<Bitmap>();
        int thickness = 2;

        private void btn_rect_Click(object sender, EventArgs e)
        {
            index = 4;
        }

        private void btn_line_Click(object sender, EventArgs e)
        {
            index = 5;
        }

        private void pic_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (paint)
            {
                if (index == 3)
                {
                    g.DrawEllipse(p, cX, cY, sX, sY);
                }
                if (index == 4)
                {
                    g.DrawRectangle(p, cX, cY, sX, sY);
                }
                if (index == 5)
                {
                    g.DrawLine(p, cX, cY, x, y);
                }
            }
        }

        private void btn_clear_Click(object sender, EventArgs e)
        {
            SaveToUndoStack();
            g.Clear(Color.White);
            pic.Image = bm;
            index = 0;
        }

        private void btn_color_Click(object sender, EventArgs e)
        {
            cd.ShowDialog();
            new_color = cd.Color;
            pic_color.BackColor = cd.Color;
            p.Color = cd.Color;
        }

        private void btn_ellipse_Click(object sender, EventArgs e)
        {
            index = 3;
        }

        private void color_picker_MouseClick(object sender, MouseEventArgs e)
        {
            Point point = set_point(color_picker, e.Location);
            pic_color.BackColor = ((Bitmap)color_picker.Image).GetPixel(point.X, point.Y);
            new_color = pic_color.BackColor;
            p.Color = pic_color.BackColor;
        }

        private void pic_MouseMove(object sender, MouseEventArgs e)
        {
            if (paint)
            {
                if (index == 1)
                {
                    px = e.Location;
                    g.DrawLine(p, px, py);
                    py = px;
                }
                if (index == 2)
                {
                    px = e.Location;
                    g.DrawLine(erase, px, py);
                    py = px;
                }
            }
            pic.Refresh();
            x = e.X; y = e.Y;
            sX = e.X - cX; sY = e.Y - cY;
        }

        private void pic_MouseUp(object sender, MouseEventArgs e)
        {
            paint = false;
            sX = x - cX; sY = y - cY;
            if (index == 3)
            {
                g.DrawEllipse(p, cX, cY, sX, sY);
            }
            if (index == 4)
            {
                g.DrawRectangle(p, cX, cY, sX, sY);
            }
            if (index == 5)
            {
                g.DrawLine(p, cX, cY, x, y);
            }

        }

        private void pic_MouseClick(object sender, MouseEventArgs e)
        {
            if (index == 7)
            {
                Point point = set_point(pic, e.Location);
                Fill(bm, point.X, point.Y, new_color);
            }
        }

        private void btn_fill_Click(object sender, EventArgs e)
        {
            index = 7;
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "JPEG Image (*.jpg)|*.jpg|PNG Image (*.png)|*.png|Bitmap Image (*.bmp)|*.bmp|All files (*.*)|*.*";
                sfd.Title = "Save an Image File";
                sfd.DefaultExt = "jpg";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Ensure dimensions are valid
                        if (pic.Width <= 0 || pic.Height <= 0 || bm.Width <= 0 || bm.Height <= 0)
                        {
                            MessageBox.Show("Invalid image dimensions. Unable to save.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Create a new bitmap based on the PictureBox dimensions
                        Bitmap tempBitmap = new Bitmap(pic.Width, pic.Height);
                        using (Graphics tempGraphics = Graphics.FromImage(tempBitmap))
                        {
                            tempGraphics.DrawImage(bm, new Rectangle(0, 0, pic.Width, pic.Height));
                        }

                        // Determine the image format based on the file extension
                        ImageFormat format = ImageFormat.Jpeg;
                        string extension = System.IO.Path.GetExtension(sfd.FileName).ToLower();

                        switch (extension)
                        {
                            case ".png":
                                format = ImageFormat.Png;
                                break;
                            case ".bmp":
                                format = ImageFormat.Bmp;
                                break;
                            case ".jpg":
                            case ".jpeg":
                                format = ImageFormat.Jpeg;
                                break;
                            default:
                                MessageBox.Show("Unsupported file format. Saving as JPEG.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                format = ImageFormat.Jpeg;
                                break;
                        }

                        // Save the bitmap to the specified file
                        tempBitmap.Save(sfd.FileName, format);
                        MessageBox.Show("Image saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Dispose of the temporary bitmap
                        tempBitmap.Dispose();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void SaveToUndoStack()
        {
            undoStack.Push((Bitmap)bm.Clone());
            redoStack.Clear(); // Clear redoStack whenever a new action is performed
        }
        private void ScalePenWidth(float scaleFactor)
        {
            p.Width = p.Width * scaleFactor;
            erase.Width = erase.Width * scaleFactor;
        }

        private void btn_undo_Click(object sender, EventArgs e)
        {
            if (undoStack.Count > 0)
            {
                redoStack.Push((Bitmap)bm.Clone()); // Save the current state to redoStack
                bm = undoStack.Pop(); // Restore the last state from undoStack
                g = Graphics.FromImage(bm); // Reassign Graphics object
                pic.Image = bm;
                pic.Refresh();
            }
        }

        private void btn_redo_Click(object sender, EventArgs e)
        {
            if (redoStack.Count > 0)
            {
                undoStack.Push((Bitmap)bm.Clone()); // Save the current state to undoStack
                bm = redoStack.Pop(); // Restore the last state from redoStack
                g = Graphics.FromImage(bm); // Reassign Graphics object
                pic.Image = bm;
                pic.Refresh();
            }
        }

        private void btn_pencil_Click(object sender, EventArgs e)
        {
            index = 1;

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            thickness = trackBar1.Value; // Update thickness based on TrackBar value
            label1.Text = $"Thickness: {thickness}"; // Update label to show thickness
            p.Width = thickness; // Update pen thickness
            erase.Width = thickness;
        }

        private void btn_eraser_Click(object sender, EventArgs e)
        {
            index = 2;
        }

        private void pic_MouseDown(object sender, MouseEventArgs e)
        {
            SaveToUndoStack(); // Save the current state before starting to draw
            paint = true;
            py = e.Location;
            cX = e.X; cY = e.Y;
        }
        static Point set_point(PictureBox pb, Point pt)
        {
            float pX = 1f * pb.Image.Width / pb.Width;
            float pY = 1f * pb.Image.Height / pb.Height;
            return new Point((int)(pt.X * pX), (int)(pt.Y * pY));
        }
        private void validate(Bitmap bm, Stack<Point> sp, int x, int y, Color old_color, Color new_color)
        {
            Color cx = bm.GetPixel(x, y);
            if (cx == old_color)
            {
                sp.Push(new Point(x, y));
                bm.SetPixel(x, y, new_color);
            }
        }
        public void Fill(Bitmap bm, int x, int y, Color new_c1r)
        {
            Color old_color = bm.GetPixel(x, y);
            Stack<Point> pixel = new Stack<Point>();
            pixel.Push(new Point(x, y));
            bm.SetPixel(x, y, new_c1r);
            if (old_color == new_c1r) return;
            while (pixel.Count > 0)
            {
                Point pt = (Point)pixel.Pop();
                if (pt.X > 0 && pt.Y > 0 && pt.X < bm.Width - 1 && pt.Y < bm.Height - 1)
                {
                    validate(bm, pixel, pt.X - 1, pt.Y, old_color, new_c1r);
                    validate(bm, pixel, pt.X, pt.Y - 1, old_color, new_c1r);
                    validate(bm, pixel, pt.X + 1, pt.Y, old_color, new_c1r);
                    validate(bm, pixel, pt.X, pt.Y + 1, old_color, new_c1r);
                }
            }
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            float aspectRatio = (float)bm.Width / bm.Height;
            int newWidth = pic.Width;
            int newHeight = (int)(newWidth / aspectRatio);

            if (newHeight > pic.Height)
            {
                newHeight = pic.Height;
                newWidth = (int)(newHeight * aspectRatio);
            }

            Bitmap resizedBitmap = new Bitmap(newWidth, newHeight);
            using (Graphics gResized = Graphics.FromImage(resizedBitmap))
            {
                gResized.Clear(Color.White);
                gResized.DrawImage(bm, 0, 0, newWidth, newHeight); // Maintain aspect ratio
            }

            bm = resizedBitmap;
            g = Graphics.FromImage(bm);
            pic.Image = bm;
        }
    }
}
