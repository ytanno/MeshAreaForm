using OpenTK;

//2015/11/20
//Lisense is same of reference

//Need OpenTK.dll and OpenTK.GLControl
//OpenTK.GLControl.dll is used "Item Selected" in toolbox
//You can download and install from http://www.opentk.com/

//other reference
//https://github.com/occar421/region_OpenTK/blob/master/01-02/01-02_main.cs#L99

using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MeshAreaForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //point sample
        private List<Vector3> PointList = new List<Vector3>();

        private Vector3 EyePosition = new Vector3(0.3f, 0.0f, 1.0f);

        //rotate matrix
        private Matrix4 FirstRotate = new Matrix4(-0.374569654f, -0.139827654f, 0.9165927f, 0,
                                     0.9260388f, -0.10581933f, 0.362286627f, 0,
                                     -0.0463347435f, 0.9845049f, 0.169122279f, 0,
                                     0, 0, 0, 1);

        //update param
        private Matrix4 Rotate;
        private float Zoom;
        private bool Loaded = false;
        private float WheelPrevious;
        private bool IsCameraRotating;      //Rotate Mode
        private Vector2 Current, Previous;  //Mouse point


        private void Form1_Load(object sender, EventArgs e)
        {
            Reset();

            //Update OpenGL
            timer1.Interval = 50;
            timer1.Tick += (sss, eee) => { glControl1.Invalidate(); };
            timer1.Start();

            //MouseEvent
            AddMouseEvnets();
        }

        private void Reset()
        {
            IsCameraRotating = false;
            Current = Vector2.Zero;
            Previous = Vector2.Zero;
            //rotate = Matrix4.Identity;
            WheelPrevious = 0.0f;
            Rotate = FirstRotate;
            Zoom = 1.6f;
        }

        private void AddMouseEvnets()
        {
            //zoom
            glControl1.MouseWheel += (sss, eee) =>
            {
                float delta = eee.Delta;
                Zoom += delta / (float)Math.Abs(delta) * 0.1f;
                if (Zoom > 5) Zoom = 5.0f;
                if (Zoom < 0.5f) Zoom = 0.5f;
                WheelPrevious = delta;
            };

            //Rotate
            glControl1.MouseMove += (sss, eee) =>
            {
                //if Rotate On
                if (IsCameraRotating)
                {
                    Previous = Current;
                    Current = new Vector2(eee.X, eee.Y);
                    Vector2 delta = Current - Previous;
                    delta /= (float)Math.Sqrt(this.Width * this.Width + this.Height * this.Height);
                    float length = delta.Length;
                    if (length > 0.0)
                    {
                        float rad = length * MathHelper.Pi;
                        float theta = (float)Math.Sin(rad) / length;
                        Quaternion after = new Quaternion(delta.Y * theta, delta.X * theta, 0.0f, (float)Math.Cos(rad));
                        Rotate = Rotate * Matrix4.CreateFromQuaternion(after);
                    }
                }
            };

            //Rotate Mode On
            glControl1.MouseDown += (sss, eee) =>
            {
                if (eee.Button == MouseButtons.Right)
                {
                    IsCameraRotating = true;
                    Current = new Vector2(eee.X, eee.Y);
                }
            };

            //Rotate Mode Off
            glControl1.MouseUp += (sss, eee) =>
            {
                if (eee.Button == MouseButtons.Right)
                {
                    IsCameraRotating = false;
                    Previous = Vector2.Zero;
                }
            };
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            GL.ClearColor(Color.Gray);
            Loaded = true;
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (!Loaded) return;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 modelView = Matrix4.LookAt(Vector3.UnitZ * 10 / Zoom, Vector3.Zero, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelView);
            GL.MultMatrix(ref Rotate);
            GL.Translate(0, 0, -1);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4 / Zoom, (float)this.Width / (float)this.Height, 1.0f, 64.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);

            DrawReticulation();
            DrawPoint();

            glControl1.SwapBuffers();
        }

        private void DrawPoint()
        {
            GL.PointSize(5.0F);
            GL.Begin(PrimitiveType.Points);
            GL.Color3(Color.Blue);

            foreach (var p in PointList)
                GL.Vertex3(p.X, p.Y, p.Z);
            GL.End();
        }

        private Random _rnd = new Random();

        private void button1_Click(object sender, EventArgs e)
        {
            var x = (float)(_rnd.NextDouble() * 2 - 1); //range -1 --- 1
            var y = (float)(_rnd.NextDouble() * 2 - 1); //range -1 --- 1
            var z = (float)(_rnd.NextDouble() * 2); //range  0 --- 2
            PointList.Add(new Vector3(x, y, z));
        }

        /// <summary>
        /// reset postion
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetButton_Click(object sender, EventArgs e)
        {
            Reset();
        }

        /// <summary>
        /// draw help viewer
        /// </summary>
        public void DrawReticulation()
        {
            bool frame = false;

            GL.Color3(Color.White);
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(-1.2, -1.2);
            GL.Vertex2(-1.2, 1.2);
            GL.Vertex2(1.2, 1.2);
            GL.Vertex2(1.2, -1.2);
            GL.End();

            GL.Begin(PrimitiveType.Quads);
            GL.Vertex3(1.2, -1, 2f);
            GL.Vertex3(-1.2, -1, 2f);
            GL.Vertex3(-1.2, -1, 0f);
            GL.Vertex3(1.2, -1, 0f);

            GL.End();

            GL.Begin(PrimitiveType.Quads);
            GL.Vertex3(-1, 1.2, 2f);
            GL.Vertex3(-1, -1.2, 2f);
            GL.Vertex3(-1, -1.2, 0f);
            GL.Vertex3(-1, 1.2, 0f);

            GL.End();

            for (var i = -10; i <= 10; i++)
            {
                //Console.WriteLine(i);
                var pos = (float)(i / 10.0f);
                if (i == 10 || i == -10)
                {
                    frame = true;
                    GL.Disable(EnableCap.LineStipple);
                }
                else
                {
                    frame = false;
                    GL.Enable(EnableCap.LineStipple);
                    GL.LineStipple(1, 0x0F0F0);
                }

                if (i % 2 == 0 || frame)
                {
                    GL.Color3(Color.Black);

                    if (frame) GL.Color3(Color.LightGreen);
                    GL.Begin(PrimitiveType.LineStrip);
                    GL.Vertex3(pos, -1.0f, 0f);
                    GL.Vertex3(pos, -1.0f, 2.0f);
                    GL.End();
                    GL.Begin(PrimitiveType.LineStrip);
                    GL.Vertex3(-1.0f, pos, 0f);
                    GL.Vertex3(-1.0f, pos, 2.0f);
                    GL.End();

                    GL.Begin(PrimitiveType.LineStrip);
                    GL.Vertex3(-1.0f, -1.0f, pos + 1);
                    GL.Vertex3(1.0f, -1.0f, pos + 1);
                    GL.End();

                    GL.Begin(PrimitiveType.LineStrip);
                    GL.Vertex3(-1.0f, -1.0f, pos + 1);
                    GL.Vertex3(-1.0f, 1.0f, pos + 1);
                    GL.End();

                    if (frame) GL.Color3(Color.Blue);
                    GL.Begin(PrimitiveType.LineLoop);
                    GL.Vertex3(pos, -1.0f, 0);
                    GL.Vertex3(pos, 1.0f, 0);
                    GL.End();

                    if (frame) GL.Color3(Color.Red);
                    GL.Begin(PrimitiveType.LineStrip);
                    GL.Vertex3(-1.0f, pos, 0);
                    GL.Vertex3(1.0f, pos, 0);
                    GL.End();
                }
            }
        }
    }
}