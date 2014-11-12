using System;
using System.Drawing;
using System.Windows.Forms;

namespace Metrics
{
    public partial class FormMcClure : Form
    {
        private const int Dy = 100;
        private const int rectWidth = 140;
        private const int rectHeight = 40;
        private readonly Pen pen = new Pen(Color.Black);
        private Bitmap bitMap;

        public FormMcClure()
        {
            InitializeComponent();
        }

        public void DrawGraph(Parser.Program program)
        {
            bitMap = new Bitmap(pictureBox.Width, pictureBox.Height);
            Graphics graphics = Graphics.FromImage(bitMap);
            graphics.Clear(Color.White);
            Draw(graphics, program, pictureBox.Width, 0, 10);
            pictureBox.Image = bitMap;
            pen.Width = 4;
        }

        public void Draw(Graphics graphics, Parser.Program program, int width, int x, int y)
        {
            if (program != null)
            {
                graphics.DrawRectangle(pen, x + (width - rectWidth)/2, y, rectWidth, rectHeight);
                graphics.DrawString(program.Name, new Font(FontFamily.GenericMonospace, 10),
                    new SolidBrush(Color.Black), x + (width - rectWidth)/2 + 3, y + 10);
                if (program.Subprograms.Count > 0)
                {
                    int dx = width/(program.Subprograms.Count);
                    int subWidth = width/(program.Subprograms.Count);
                    for (int i = 0; i < program.Subprograms.Count; i++)
                    {
                        graphics.DrawLine(pen, x + width/2, y + rectHeight, x + dx*i + subWidth/2, y + Dy);
                        Parser.Program subprogram = program.Subprograms[i];
                        Draw(graphics, subprogram, subWidth, x + dx*i, y + Dy);
                    }
                }
            }
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
        }
    }
}