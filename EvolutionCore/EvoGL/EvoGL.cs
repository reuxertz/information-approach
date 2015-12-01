using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using Tao.OpenGl;
using Tao.Platform.Windows;

using EvolutionTools;

namespace EvolutionGL
{
    using Point = EvolutionTools.Point;
    using Color = EvolutionTools.Color;
    using Timer = System.Windows.Forms.Timer;

    public class EvoGL
    {
        //Sub Object
        public class DrawObject
        {
            private Object LOCK = new Object();
            public bool RemoveMe;
            public List<Color> MyColors = new List<Color>() { Color.White };

            private int type; //0 Line, 1 Circle
            private double val1; // x1
            private double val2; // y1
            private double val3; // x2 , r
            private double val4; // y2 , null

            private DrawObject()
            { }
            private DrawObject(int type, int val1, int val2, int val3, int val4)
            {
                this.type = type;
                this.val1 = val1;
                this.val2 = val2;
                this.val3 = val3; 
                this.val4 = val4;
            }

            public static DrawObject CreateCircle(Point p, double r)
            {
                var o = new DrawObject();
                o.SetCircle(p, r);
                return o;
            }
            public static DrawObject CreateCircle(double x, double y, double r)
            {
                var o = new DrawObject();
                o.SetCircle(x, y, r);
                return o;
            }
            public void SetCircle(double x, double y, double r)
            {
                lock (this.LOCK)
                {
                    this.type = 1;
                    this.val1 = x;
                    this.val2 = y;
                    this.val3 = r;
                }
            }
            public void SetCircle(Point p, double r)
            {
                lock (this.LOCK)
                {
                    this.type = 1;
                    this.val1 = p.X;
                    this.val2 = p.Y;
                    this.val3 = r;
                }
            }                
        }

        //Real Draw Objects
        public class GraphObject
        {
            //Private fields
            public Point[][] BarGraphPoints;
            public List<double> Verticals = new List<double>();
            public double BarWidth = .01;
            public bool DrawDistribution = true, DrawGrid = true;
            public List<Color> MyColors;

            //Private Fields
            private Point _startPoint, _graphSize;
            private Color _baseColor;
            private double _radius;
            private bool _isCircle;

            //Getter
            public Color GetColor(int index)
            {
                if (index < this.MyColors.Count)
                    return this.MyColors[index];

                return Color.White;
            }
            
            //Constructor
            public GraphObject(Point startPoint, Point graphSize, Color c)
            {
                this._startPoint = startPoint;
                this._graphSize = graphSize;
                this.MyColors = new List<Color> { c };
            }
            public GraphObject(Point start, Point size)
                : this(start, size, Color.White)
            {
            }

            //private constructors
            private GraphObject(Point center, double radius, Color c, string type)
            {
                if (type == "circle")
                {
                    this._startPoint = center;
                    this._radius = radius;
                    this._baseColor = c;
                    this._isCircle = true;
                    return;
                }

                throw new Exception("Unhandled region");
            }

            //Creators
            public static GraphObject CreateGraph(Point start, Point size)
            {
                return new GraphObject(start, size);
            }
            public static GraphObject CreateCircle(Point center, double radius, Color c)
            {
                return new GraphObject(center, radius, c, "circle");
            }

            //Draw Function
            public void DrawSelf(EvoGL drawer)
            {
                //If Circle
                if (this._isCircle)
                {
                    this._DrawCircle(drawer);
                    return;
                }

                //Base
                if (this.DrawGrid)
                    drawer.DrawGrid(this._startPoint, this._graphSize, 5.0, 1.0);

                //Draw BarGraph objects
                if (this.BarGraphPoints != null)
                {
                    for (int i = 0; i < this.BarGraphPoints.Length; i++)
                    {
                        if (DrawDistribution)
                        {
                            var bw = (this.BarWidth * .99) / 2.0;
                            for (int j = 0; j < this.BarGraphPoints[i].Length; j++)
                            {
                                var p = this.BarGraphPoints[i][j].Add(this._startPoint);
                                var np = new Point[4];

                                np[0] = new Point(p.X - bw, p.Y * _graphSize.Y);
                                np[1] = new Point(p.X + bw, p.Y * _graphSize.Y);
                                np[2] = new Point(p.X + bw, 0);
                                np[3] = new Point(p.X - bw, 0);

                                drawer.DrawPolygonShadow(np, this.GetColor(i), 1.0, .25);
                                //drawer.DrawLine(new Point(p.X, 0), new Point(p.X, p.Y * _graphSize.Y), Color.White, 1.0);
                            }
                        }
                        else
                        {
                           drawer.DrawLines(this.BarGraphPoints[i].ToList<Point>(), this.GetColor(i), 1.0);

                            /*
                            var sp = new SortedValueList<Point>("<=");
                            for (int j = 0; j < this.BarGraphPoints[i].Length; j++)
                                sp.Add(this.BarGraphPoints[i][j], this.BarGraphPoints[i][j].X);

                            var np = new Point[sp.Count + 3];
                            np[0] = new Point(sp[0].Add(this._startPoint).X, this._startPoint.Y);
                            for (int j = 0; i < sp.Count; j++)
                                np[j + 1] = new Point(sp[j].X, sp[j].Y * this._graphSize.Y).Add(this._startPoint);
                            np[sp.Count + 1] = new Point(np[sp.Count].X, this._startPoint.Y);
                            np[sp.Count + 2] = new Point(np[0].X, this._startPoint.Y);

                            //drawer.DrawLines(np.ToList<Point>(), this.MyColor, 1.0);
                            drawer.DrawPolygonShadow(np.ToArray<Point>(), Color.White, 1.0, .5);
                             */
                        }
                    }
                }

                //Draw Horizontals
                for (int i = 0; i < this.Verticals.Count; i++)
                {
                    var px = this.Verticals[i] + this._startPoint.X;
                    //drawer.DrawLine(new Point(px, this._startPoint.Y), new Point(px, this._startPoint.Y + this._graphSize.Y), this.MyColor, 1.0);
                }

                Gl.glEnd(); 
            }
        
            //Private draw functions
            private void _DrawCircle(EvoGL drawer)
            {
                drawer.DrawCircle(this._startPoint, this._radius, this._baseColor);
            }
        
        }

        //Contents
        protected List<GraphObject> _graphObjects = new List<GraphObject>();
        protected List<IDrawObject> _evoDrawObjects = new List<IDrawObject>();
        protected Timer _myTimer = new Timer();
        protected SimpleOpenGlControl _myOpenGlControl;
        protected Clock _fpsClock;
        protected Point _windowSize, _windowPos, _mousePos = new Point();
        protected Point _OGLSize;
        protected bool _abort = false, _drawFrame = true, _enabled = true;
        protected double _zoom = 1, _reqFPS = 100;
        protected bool _inverseView = false, _drawOverride;

        //Properties
        public SimpleOpenGlControl MyOpenGlControl
        {
            get
            { return _myOpenGlControl; }
        }
        public Point MousePosition
        {
            get
            { return _mousePos; }
        }
        public double FramesPerSecond
        {
            get
            {
                return this._fpsClock.GetTicksPerElapsedTime(true, 0);
            }
        }
        public bool Enabled
        {
            get
            { return _enabled; }
            set
            {
                _myOpenGlControl.Enabled = value;
            }           
        }
        public bool DisableCenterOnRightClick
        {
            get;
            private set;
        }
        public bool InverseView
        {
            get
            {
                return this._inverseView;
            }
            set
            {
                this._inverseView = value;
            }
        }
        public Point WindowPosition
        {
            get
            {
                return this._windowPos;
            }
        }
        public Point WindowSize
        {
            get
            {
                return this._windowSize;
            }
        }
        public bool DrawOverride
        {
            get
            {
                return this._drawOverride;
            }
            set
            {
                this._drawOverride = value;
            }
        }
            
        //Getters
        public Point DesktopToContentPoint(Point pt)
        {
            return new Point(
                _windowPos.X + (pt.X / _myOpenGlControl.Size.Width) * _windowSize.X,
                _windowPos.Y + (pt.Y / _myOpenGlControl.Size.Height) * _windowSize.Y);
        }
        public Point ContentToDesktopPoint(Point pt)
        {
            return new Point(
                (_myOpenGlControl.Size.Width * (pt.X - _windowPos.X)) / _windowSize.X,
                (_myOpenGlControl.Size.Height * (pt.Y - _windowPos.Y)) / _windowSize.Y);
        }   

        //Constructor
        public EvoGL(Form f, SimpleOpenGlControl glc, double width, double height) : 
            this(f, glc, new Point(width, height), new Point())
        {
        }
        public EvoGL(Form f, SimpleOpenGlControl mySimpleGlControl, Point windowSize, Point windowPosition)
        {
            //Setup Thread && start
            //_evoGlThread = new Thread(DrawThread);
            //_evoGlThread.Start();

            //Wait for OpenGL To Set
            //while (_myOpenGlControl == null)
            //{ }

            //Set Gl Control
            _myOpenGlControl = mySimpleGlControl;

            //var d = delegate(object o) {
            //    int x = 5;
            //};

            //Set Draw
            _myTimer.Interval = (int)(1000.0 / this._reqFPS);

            if (this._myTimer.Interval == 0)
                this._myTimer.Interval = 1;

            _myTimer.Tick += new EventHandler(delegate(object o, EventArgs ea) {
                this._myOpenGlControl.Refresh(); });
            _myOpenGlControl.Paint += new System.Windows.Forms.PaintEventHandler(this.openGlControl_Paint);

            //Set Gl MouesControls
            _myOpenGlControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.openGlControl_MouseDown);
            _myOpenGlControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.openGlControl_MouseMove);
            _myOpenGlControl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.openGlControl_MouseUp);
            _myOpenGlControl.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.openGlControl_MouseWheel);
            _myOpenGlControl.MouseHover += new System.EventHandler(this.openGlControl_MouseHover);

            //Set Form
            f.Resize += new EventHandler(this.MainForm_ResizeEnd);

            //Set Window
            setWindow(windowSize, windowPosition);

            //Initialize
            InitializeContexts();

            //Set OGLSize
            this._myOpenGlControl.Width = (int)this._windowSize.X;
            this._myOpenGlControl.Height = (int)this._windowSize.Y;
            _OGLSize = new Point(_myOpenGlControl.Size.Width, _myOpenGlControl.Size.Height);
            this.Resize();

            //Start FPS clock
            this._fpsClock = new Clock(true, 0, 333);
        }

        //DrawObjects
        public GraphObject AddGraph(GraphObject go)
        {
            this._graphObjects.Add(go);
            return go;
        }
        public void AddEvoDrawObject(IDrawObject drawobj)
        {
            this._evoDrawObjects.Add(drawobj);
        }
        public void RemoveEvoDrawObject(IDrawObject drawobj)
        {
            this._evoDrawObjects.Remove(drawobj);
        }


        //Control Functions
        public void Render(bool b)
        {
            this._myTimer.Enabled = b;
        }
        public void InitializeProperties(int sizeX, int sizeY, DockStyle dockStyle)
        {
            this._myOpenGlControl.AccumBits = ((byte)(0));
            this._myOpenGlControl.AutoCheckErrors = false;
            this._myOpenGlControl.AutoFinish = false;
            this._myOpenGlControl.AutoMakeCurrent = true;
            this._myOpenGlControl.AutoSwapBuffers = true;
            this._myOpenGlControl.BackColor = System.Drawing.Color.Black;
            this._myOpenGlControl.ColorBits = ((byte)(32));
            this._myOpenGlControl.DepthBits = ((byte)(16));
            this._myOpenGlControl.Dock = dockStyle;
            this._myOpenGlControl.Location = new System.Drawing.Point(0, 0);
            this._myOpenGlControl.Name = "openGlControl";
            this._myOpenGlControl.Size = new System.Drawing.Size(sizeX, sizeY);
            this._myOpenGlControl.StencilBits = ((byte)(0));
            this._myOpenGlControl.TabIndex = 0;
        }
        public void ResetProperties()
        {
            this._myOpenGlControl.Location = new System.Drawing.Point(0, 0);
            this._myOpenGlControl.Parent.PerformLayout();
            this._myOpenGlControl.Size = this._myOpenGlControl.Parent.Size;
        }

        //Moust Functions
        public virtual void LeftClick()
        {

        }
        public virtual void RightClick()
        {
            if (!this.DisableCenterOnRightClick)
                this.CenterScreen(_mousePos);
        }
        public virtual void LeftRelease()
        {

        }
        public virtual void RightRelease()
        {

        }

        //Mouse Event Functions
        protected virtual void openGlControl_MouseMove(object sender, MouseEventArgs e)
        {
            //Set Mouse-Universe Position
            _mousePos = DesktopToContentPoint(new Point(e.X, e.Y));
        }
        protected void openGlControl_MouseDown(object sender, MouseEventArgs e)
        {
            //Left Click
            if (e.Button == MouseButtons.Left)
            { 
                //leftClick
                this.LeftClick();
            }

            //Right Click
            if (e.Button == MouseButtons.Right)
            {
                //rightClick
                this.RightClick();
            }

        }
        protected void openGlControl_MouseUp(object sender, MouseEventArgs e)
        {
            //Left Click
            if (e.Button == MouseButtons.Left)
            {
                //leftClick
                this.LeftRelease();
            }

            //Right Click
            if (e.Button == MouseButtons.Right)
            {
                //rightClick
                this.RightRelease();
            }

        }
        protected void openGlControl_MouseWheel(object sender, MouseEventArgs e)
        {
            //If Universe Exists, Zoom Appropriately
            Zoom(.1, e.Delta, new Point(e.Location.X, e.Location.Y));
        }
        protected virtual void openGlControl_MouseHover(object sender, EventArgs e)
        {
        }

        //Form
        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            //this._myGraphics.MainFormResize();
            this.ResetContexts();

            this.Resize();
        }

        //Private Gl Functions
        protected void InitializeContexts()
        {
            //Activate OpenGl
            _myOpenGlControl.InitializeContexts();

            //Set OpenGl
            setOpenGL();
        }
        public void ResetContexts()
        {
            //Destroy Contexts
            _myOpenGlControl.DestroyContexts();

            //Reset Window
            this._windowPos = new Point();

            //Resize Windowsize
            this.ResizeWindowSize();

            //ReInitialize
            InitializeContexts();            
        }
        protected void ResizeWindowSize()
        {
            double nx = _windowSize.X;
            double ny = _windowSize.Y;

            //If X change
            if (_myOpenGlControl.Size.Width != _OGLSize.X)
            {
                double xchange = _myOpenGlControl.Size.Width / _OGLSize.X;

                nx = nx * xchange;
            }

            //If Y change
            if (_myOpenGlControl.Size.Height != _OGLSize.Y)
            {
                double ychange = _myOpenGlControl.Size.Height / _OGLSize.Y;

                ny = ny * ychange;
            }

            //Set Window Size
            _windowSize = new Point(nx, ny);

            //Set OGLSize
            _OGLSize = new Point(_myOpenGlControl.Size.Width, _myOpenGlControl.Size.Height);
        }
        protected void setWindow(Point windowSize, Point windowPosition)
        {
            _windowSize = windowSize;
            _windowPos = windowPosition;
        }
        protected void setOpenGL()
        {
            var inverse = 1.0;
            if (this._inverseView)
                inverse = -1.0;

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Gl.glOrtho(_windowPos.X, _windowPos.X + _windowSize.X,
                inverse * (_windowPos.Y + _windowSize.Y), inverse * _windowPos.Y, 0, 1);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
        }

        //Public Gl Functions
        public void CenterScreen(Point p)
        {
            Point center = new Point(_windowPos.X + _windowSize.X / 2, _windowPos.Y + _windowSize.Y / 2);
            _windowPos = new Point(_windowPos.X + (p.X - center.X), (_windowPos.Y + p.Y - center.Y));
            setOpenGL();
        }
        public void Zoom(double scrollMag, double scrollUnits, Point zpt)
        {
            //Get Old window position for zoom point adj
            Point owps = _windowPos.Add(_windowSize.Multiply(.5));

            //Get new Window size
            double nx = _windowSize.X + (_windowSize.X * scrollMag * Math.Sign(-1 * scrollUnits));
            double ny = _windowSize.Y + (_windowSize.Y * scrollMag * Math.Sign(-1 * scrollUnits));

            //Get new window Pos
            _windowPos = new Point(_windowPos.X + ((_windowSize.X - nx) / 2), _windowPos.Y + ((_windowSize.Y - ny) / 2));

            if (scrollUnits > 0)
            {
                //_windowPos = _windowPos
                //.Add(DesktopToContentPoint(zpt).Subtract(owps).Multiply(
                //Math.Sqrt(
                //mag)
                //)
                //.Multiply(Math.Sign(ed))
                //);
            }

            //Get new WindowSize
            _windowSize = new Point(nx, ny);

            //Set Zoom
            _zoom = _zoom * (1 - (scrollMag * Math.Sign(-1 * scrollUnits)));

            //SetOpenGl
            setOpenGL();
        }
        public void Resize()
        {
            //this._myOpenGlControl.PerformAutoScale();
            this._myOpenGlControl.PerformLayout();

            ResizeWindowSize();

            ResetContexts();
        }

        //Draw Functions
        private void openGlControl_Paint(object sender, PaintEventArgs e)
        {
            //Paint
            Paint();
        }
        protected virtual void Paint()
        {
            //Clear Screen
            this.Clear();

            //Base Draw
            this.Draw();

            //Add Frame to FPS Counter
            this._fpsClock.AddTick();
        }
        public virtual void Draw()
        { 
            //Draw Graph Objects
            for (int i = 0; i < this._graphObjects.Count; i++)
                this._graphObjects[i].DrawSelf(this);

            //Draw EvoDraw objects
            for (int i = 0; i < this._evoDrawObjects.Count; i++)
            {
                if (this._evoDrawObjects[i].RemoveMe())
                {
                    this._evoDrawObjects.RemoveAt(i);
                    i--;
                }

                this._evoDrawObjects[i].DrawSelf(this);
            }
        }
        
        //public Draw Functions
        public void Clear()
        { Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT); ;}
        public void DrawGrid(Point start, Point size, double gridDepth, double gridSep)
        {
            Gl.glColor3d(1, 1, 1);
            Gl.glLineWidth(1f);
            Gl.glBegin(Gl.GL_LINES);

            float darker = .05f, lighter = .15f;

            Point end = start.Add(size);

            for (float x = (float)start.X; x <= end.X; x += Convert.ToSingle(Math.Sign(end.X - start.X) * gridSep))
            {
                if (x % gridDepth == 0) { Gl.glColor3f(lighter, lighter, lighter); }
                else { Gl.glColor3f(darker, darker, darker); }

                Gl.glVertex2f(x, (float)start.Y);
                Gl.glVertex2f(x, (float)end.Y);
            }
            for (float y = (float)start.Y; y <= end.Y; y += Convert.ToSingle(Math.Sign(end.Y - start.Y) * gridSep))
            {
                if (y % gridDepth == 0) { Gl.glColor3f(lighter, lighter, lighter); }
                else { Gl.glColor3f(darker, darker, darker); }

                Gl.glVertex2f((float)start.X, y);
                Gl.glVertex2f((float)end.X, y);
            }

            Gl.glEnd();            
        }
        public void DrawGrid(Point size, double gridDepth, double gridSep)
        { DrawGrid(new Point(), size, gridDepth, gridSep); }
        public void DrawGrid(Point size, double gridDepth)
        { DrawGrid(new Point(), size, gridDepth, 1); }
        public void DrawCircleShadow(Point p, Double r, Color c, Color fc)
        {
            DrawCircle(p, r, fc.Multiply(.25), true, 1);
            DrawCircle(p, r, c, false, r);
        }
        public void DrawCircleShadow(Point p, Double r, Color c)
        { DrawCircleShadow(p, r, c, c); }
        public void DrawCircle(Point p, Double r, Color c)
        { DrawCircle(p, r, c, false, r); ;}
        public void DrawCircle(Point p, Double r, Color c, bool fill, double rfill)
        {
            //If is worth Drawing
            //if (!p.IsWithin(this._windowPos, this._windowPos.Add(this._windowSize)))
            //{ return; }

            //Set radius
            double radius = r;

            //set Color
            Gl.glColor3d(c.RedScaled, c.GreenScaled, c.BlueScaled);
            //Gl.glColor4f(c.redf(), c.greenf(), c.bluef(), .1f);

            //Set Thickness
            if (rfill >= 1)
            { Gl.glLineWidth((float)rfill); }

            //Set Fill
            if (!fill)
            { Gl.glBegin(Gl.GL_LINE_STRIP); }
            else
            { Gl.glBegin(Gl.GL_TRIANGLE_FAN); }

            //Get Start Points
            double nx = p.X; double ny = p.Y;

            //Cycle Through Points around circle
            for (double angle = 0; angle <= 360; angle += 3)
            {
                Angle da = new Angle(angle);
                nx = (p.X + Math.Cos(da.Radians) * radius);
                ny = (p.Y + Math.Sin(da.Radians) * radius);
                Gl.glVertex2d(nx, ny);
            }

            Gl.glEnd();

            Gl.glLineWidth(1f);
        }
        public void DrawLine(Point p1, Point p2, Color c, double thick)
        {
            //If is worth Drawing
            if (!p1.IsWithin(this._windowPos, this._windowPos.Add(this._windowSize)) &&
                !p2.IsWithin(this._windowPos, this._windowPos.Add(this._windowSize)))
            { return; }

            //SEt Color
            Gl.glColor3d(c.RedScaled, c.GreenScaled, c.BlueScaled);

            //Set Line Width
            Gl.glLineWidth((float)thick);

            //Set GlDrawType
            Gl.glBegin(Gl.GL_LINE_STRIP);

            //Add Line Points
            Gl.glVertex2d(p1.X, p1.Y);
            Gl.glVertex2d(p2.X, p2.Y);
            
            //End Draw
            Gl.glEnd();            
        }
        public void DrawLines(List<Point> ps, Color c, double thick)
        {
            this.DrawLines(ps, c, thick, false);
        }
        public void DrawLines(List<Point> ps, Color c, double thick, bool overideDrawCheck)
        {
            //If not enough Poitns, return
            if (ps.Count < 2)
            { return; }

            //SEt Color
            Gl.glColor3d(c.RedScaled, c.GreenScaled, c.BlueScaled);

            //Set Line Width
            Gl.glLineWidth((float)thick);

            //Set GlDrawType
            Gl.glBegin(Gl.GL_LINE_STRIP);

            //Cycle
            for (int i = 0; i < ps.Count - 1; i++)
            {
                //If is worth Drawing
                if (!overideDrawCheck)
                {
                    if (!ps[i].IsWithin(this._windowPos, this._windowPos.Add(this._windowSize)) &&
                        !(ps[i + 1].IsWithin(this._windowPos, this._windowPos.Add(this._windowSize))))
                    {
                        if (!this._drawOverride)
                            continue;
                    }
                }

                //Add Line Points
                Gl.glVertex2d(ps[i].X, ps[i].Y);
                Gl.glVertex2d(ps[i + 1].X, ps[i + 1].Y);
            }

            //End Draw
            Gl.glEnd();
        }
        public void DrawPolygon(Point[] pts, Color c, double thick, bool fill, double rfill)
        {
            //set Color
            Gl.glColor3d(c.RedScaled, c.GreenScaled, c.BlueScaled);
            //Gl.glColor4f(c.redf(), c.greenf(), c.bluef(), .1f);

            //Set Thickness
            if (thick >= 1)
            { Gl.glLineWidth((float)thick); }

            //Set Fill
            if (!fill)
            { Gl.glBegin(Gl.GL_LINE_STRIP); }
            else
            { 
                Gl.glBegin(Gl.GL_TRIANGLE_FAN);
                c = c.Multiply(rfill);
            }


            Gl.glColor3d(c.RedScaled, c.GreenScaled, c.BlueScaled);

            //Cycle Through Points around circle
            for (int i = 0; i < pts.Length; i++)
                Gl.glVertex2d(pts[i].X, pts[i].Y);                
            Gl.glVertex2d(pts[0].X, pts[0].Y);
            

            Gl.glEnd();

            Gl.glLineWidth(1f);
        }
        public void DrawPolygonShadow(Point[] pts, Color c, double thick, double rfill)
        {
            this.DrawPolygon(pts, c, thick, true, rfill);
            this.DrawPolygon(pts, c, thick, false, 0);
        }
        public void DrawSquare(Point upperLeft, double length, Color c)
        {
            //SEt Color
            Gl.glColor3d(c.RedScaled, c.GreenScaled, c.BlueScaled);

            //Set Line Width
            Gl.glLineWidth((float)1.0);

            //Set GlDrawType
            //Gl.glBegin(Gl.GL_LINE_STRIP);
            Gl.glBegin(Gl.GL_TRIANGLE_FAN);

            //Cycle
            //for (int i = 0; i < ps.Count - 1; i++)
            //{
                var p1 = new Point(upperLeft.X, upperLeft.Y);
                var p2 = new Point(upperLeft.X + length, upperLeft.Y);
                var p3 = new Point(upperLeft.X + length, upperLeft.Y - length);
                var p4 = new Point(upperLeft.X, upperLeft.Y - length);


                //If is worth Drawing
                // (!ps[i].IsWithin(this._windowPos, this._windowPos.Add(this._windowSize)) &&
                //    !(ps[i + 1].IsWithin(this._windowPos, this._windowPos.Add(this._windowSize))))
                //{ continue; }

                //Add Line Points
                Gl.glVertex2d(upperLeft.X, upperLeft.Y);
                Gl.glVertex2d(upperLeft.X + length, upperLeft.Y);
                Gl.glVertex2d(upperLeft.X + length, upperLeft.Y - length);
                Gl.glVertex2d(upperLeft.X, upperLeft.Y - length);
            //}

            //End Draw
            Gl.glEnd();


        }


    }
}
