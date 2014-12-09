using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Metrics
{
    public partial class Form1 : Form
    {
        private FormMcClure mcClureForm;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Parser.Program program = Parser.FindProgram(textBoxSourceCode.Text.ToLower());
            var programs = new List<Parser.Program>();
            AddProgramToList(ref programs, program);
            programs.Remove(program);
            var mcCabe = new McCabeMetrics();
            mcCabe.SetSubprograms(programs);
            int mc = mcCabe.CalculateMcCabeMetrics(program.BlockBeginEnd) + 1;
            MessageBox.Show("Цикломатическая сложность программы = " + mc);
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxSourceCode.Text = File.ReadAllText(openFileDialog.FileName);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            dataGridView.Columns.Clear();
            dataGridView.Columns.Add("c1", "Name");
            dataGridView.Columns.Add("c2", "fp");
            dataGridView.Columns.Add("c3", "gp");
            dataGridView.Columns.Add("c4", "Xp");
            dataGridView.Columns.Add("c5", "Yp");
            dataGridView.Columns.Add("c6", "M(p)");
            dataGridView.Rows.Clear();
            dataGridView.RowCount = 1;

            Parser.Program program = Parser.FindProgram(textBoxSourceCode.Text.ToLower());
            if (program != null)
            {
                McClureMetrics.CalculateComplexity(program);
                FillMcClure1(program);
                mcClureForm = new FormMcClure();
                mcClureForm.Show();
                mcClureForm.DrawGraph(program);
            }
        }

        private void FillMcClure1(Parser.Program program)
        {
            FillDataGridMcClure1(program);
            foreach (Parser.Program subprogram in program.Subprograms)
            {
                if (program != null)
                {
                    FillMcClure1(subprogram);
                }
            }
        }

        private void FillDataGridMcClure1(Parser.Program program)
        {
            dataGridView.Rows.Add(program.Declaration, program.fp, program.gp, program.Xp, program.Yp,
                program.Complexity);
        }

        private void AddProgramToList(ref List<Parser.Program> listPrograms, Parser.Program program)
        {
            if (program != null)
            {
                listPrograms.Add(program);
                foreach (Parser.Program subprogram in program.Subprograms)
                {
                    AddProgramToList(ref listPrograms, subprogram);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            dataGridView.Columns.Clear();

            dataGridView.Columns.Add("c0", "Owner Name");
            dataGridView.Columns.Add("c1", "Name");
            dataGridView.Columns.Add("c2", "D");
            dataGridView.Columns.Add("c3", "J");
            dataGridView.Columns.Add("c4", "n");
            dataGridView.Columns.Add("c5", "C");
            dataGridView.Rows.Clear();
            dataGridView.RowCount = 1;

            Parser.Program program = Parser.FindProgram(textBoxSourceCode.Text.ToLower());
            var programs = new List<Parser.Program>();
            AddProgramToList(ref programs, program);
            var mcClure = new McClureMetrics();
            mcClure.CalculateMcClure(programs);
            FillMcClure2(mcClure.GetVariables());
        }

        private void FillMcClure2(List<McClureMetrics.McClureVariable> variables)
        {
            foreach (McClureMetrics.McClureVariable variable in variables)
            {
                if (variable.Comlexity > 0)
                {
                    dataGridView.Rows.Add(variable.OwnerProgram.Name, variable.Name, variable.D, variable.J, variable.N,
                        variable.Comlexity);
                }
            }
        }
    }
}