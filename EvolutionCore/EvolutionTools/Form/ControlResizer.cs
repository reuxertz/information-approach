using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EvolutionTools
{
    public static class ControlResizer
    {
        public enum Direction
        {
            Horizontal,
            Vertical
        }
        
        public static void ResizeForm(Form resizer, Form controlToResize, Direction direction, Cursor cursor)
        {
            bool dragging = false;
            System.Drawing.Point dragStart = System.Drawing.Point.Empty;
            int maxBound;
            int minBound;

            resizer.MouseHover += delegate(object sender, EventArgs e)
            {
                resizer.Cursor = cursor;
            };

            resizer.MouseDown += delegate(object sender, MouseEventArgs e)
            {
                dragging = true;
                dragStart = new System.Drawing.Point(e.X, e.Y);
                resizer.Capture = true;
            };

            resizer.MouseUp += delegate(object sender, MouseEventArgs e)
            {
                dragging = false;
                resizer.Capture = false;
            };

            resizer.MouseMove += delegate(object sender, MouseEventArgs e)
            {
                if (dragging)
                {
                    if (direction == Direction.Vertical)
                    {
                        minBound = resizer.Height;
                        maxBound = controlToResize.Parent.Height - controlToResize.Top - 20;
                        controlToResize.Height = (int)Math.Min(maxBound, Math.Max(minBound, controlToResize.Height + (e.Y - dragStart.Y)));
                    }
                    if (direction == Direction.Horizontal)
                    {
                        minBound = resizer.Width;
                        maxBound = controlToResize.Parent.Width - controlToResize.Left - 20;
                        controlToResize.Width = (int)Math.Min(maxBound, Math.Max(minBound, controlToResize.Width + (e.X - dragStart.X)));
                    }
                }
            };
        }
        public static void Init(Control resizer, Control controlToResize, Direction direction, Cursor cursor)
        {
            bool dragging = false;
            System.Drawing.Point dragStart = System.Drawing.Point.Empty;
            int maxBound;
            int minBound;

            resizer.MouseHover += delegate(object sender, EventArgs e)
            {
                resizer.Cursor = cursor;
            };

            resizer.MouseDown += delegate(object sender, MouseEventArgs e)
            {
                dragging = true;
                dragStart = new System.Drawing.Point(e.X, e.Y);
                resizer.Capture = true;
            };

            resizer.MouseUp += delegate(object sender, MouseEventArgs e)
            {
                dragging = false;
                resizer.Capture = false;
            };

            resizer.MouseMove += delegate(object sender, MouseEventArgs e)
            {
                if (dragging)
                {
                    if (direction == Direction.Vertical)
                    {
                        minBound = resizer.Height;
                        maxBound = controlToResize.Parent.Height - controlToResize.Top - 20;
                        controlToResize.Height = (int)Math.Min(maxBound, Math.Max(minBound, controlToResize.Height + (e.Y - dragStart.Y)));
                    }
                    if (direction == Direction.Horizontal)
                    {
                        minBound = resizer.Width;
                        maxBound = controlToResize.Parent.Width - controlToResize.Left - 20;
                        controlToResize.Width = (int)Math.Min(maxBound, Math.Max(minBound, controlToResize.Width + (e.X - dragStart.X)));
                    }
                }
            };
        }
    }
}
