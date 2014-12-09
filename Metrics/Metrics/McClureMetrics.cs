using System;
using System.Collections.Generic;
using System.Linq;

namespace Metrics
{
    public class McClureMetrics
    {
        private readonly List<McClureVariable> variables;
        private List<Parser.Program> subprograms;

        public McClureMetrics()
        {
            variables = new List<McClureVariable>();
        }

        public static void CalculateComplexity(Parser.Program program)
        {
            program.Xp = 0;
            program.fp = 0;
            program.gp = 0;
            Complexity(program, 0);
        }

        private static int Complexity(Parser.Program program, int lvl)
        {
            program.Xp = lvl;
            if (program.Subprograms.Count == 0)
            {
                program.Yp = 0;
            }
            else
            {
                for (int i = 0; i < program.Subprograms.Count; i++)
                {
                    program.Subprograms[i].fp = i;
                    program.Subprograms[i].gp = program.Subprograms.Count - 1;
                    int maxLvl = Complexity(program.Subprograms[i], lvl + 1) + 1;
                    if (program.Yp < maxLvl)
                    {
                        program.Yp = maxLvl;
                    }
                }
            }
            return program.Yp;
        }


        private static int FindArgumentIndex(McClureVariable variable, string currentName, string line,
            Parser.Program subprogram)
        {
            int result = -1;
            if (line.IndexOf(subprogram.Name) != -1)
            {
                if (line.IndexOf("(") != -1)
                {
                    line = line.Remove(line.IndexOf(")")).Substring(line.IndexOf("(") + 1);
                    string[] arguments = line.Split(',');
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        if (arguments[i].Trim() == currentName)
                        {
                            result = i;
                        }
                    }
                }
            }
            return result;
        }

        private void AnalyzeBlock(string code, McClureVariable variable, string currentName, int lvl,
            Parser.Program currentProgram)
        {
            if (variable.J < lvl)
            {
                variable.J = lvl;
            }
            int i = 0;
            string[] stringSeparators = {"\r\n"};
            string[] lines = code.Split(stringSeparators, StringSplitOptions.None);
            foreach (string line in lines)
            {
                foreach (Parser.Program subprogram in subprograms)
                {
                    if (subprogram != currentProgram)
                    {
                        int argumentIndex = FindArgumentIndex(variable, currentName, line, subprogram);
                        if (argumentIndex != -1)
                        {
                            variable.D++;
                            variable.AddSubprogram(subprogram);
                            if (subprogram.Arguments.Count > argumentIndex)
                            {
                                AnalyzeBlock(subprogram.BlockBeginEnd, variable, subprogram.Arguments[argumentIndex],
                                    lvl + 1, subprogram);
                            }
                        }
                    }
                }
            }
        }

        public void CalculateMcClure(List<Parser.Program> subPrograms)
        {
            subprograms = subPrograms;
            variables.Clear();
            foreach (Parser.Program subprogram in subprograms)
            {
                AnalyzeSubprogram(subprogram.BlockBeginEnd, subprogram);
            }
        }

        private void McClureFor(ref string sourceCode, Parser.Program ownerProgram)
        {
            string code = sourceCode.Substring(sourceCode.IndexOf("for") + 3).Trim();
            sourceCode = sourceCode.Substring(sourceCode.IndexOf("for") + 3);
            var variable = new McClureVariable();
            variable.OwnerProgram = ownerProgram;
            variable.Name = code.Substring(0, code.IndexOf(":")).Trim();
            code = code.Substring(code.IndexOf("do") + 2).Trim();
            if (code.IndexOf("begin") == 0)
            {
                AnalyzeBlock(code.Remove(Parser.GetIndexEndOfBegin(code)), variable, variable.Name, 0, ownerProgram);
                sourceCode = sourceCode.Substring(Parser.GetIndexEndOfBegin(sourceCode));
            }
            else
            {
                AnalyzeBlock(code.Remove(code.IndexOf(";") + 1), variable, variable.Name, 0, ownerProgram);
                sourceCode = sourceCode.Substring(sourceCode.IndexOf(";"));
            }
            variables.Add(variable);
        }

        private string[] GetNamesVariables(string line)
        {
            var result = new List<string>();
            string[] notVariable = {"endof", "<", "<>", ">", "=", "xor", "or", "not", "(", ")"};
            string lexem = "";
            line = line.Trim();
            foreach (char t in line)
            {
                if (!Char.IsSeparator(t) && !Char.IsPunctuation(t) && !Char.IsWhiteSpace(t) && Char.IsLetterOrDigit(t))
                {
                    lexem += t;
                }
                else
                {
                    if (!notVariable.Contains(lexem) && lexem.Trim().Length > 0)
                    {
                        bool isNumber = true;
                        foreach (char symbol in lexem)
                        {
                            if (Char.IsLetter(symbol))
                            {
                                isNumber = false;
                            }
                        }
                        if (!isNumber)
                        {
                            result.Add(lexem);
                        }
                    }
                    lexem = "";
                }
            }
            if (!notVariable.Contains(lexem) && lexem.Trim().Length > 0)
            {
                result.Add(lexem);
            }
            return result.ToArray();
        }

        private void McClureWhile(ref string sourceCode, Parser.Program ownerProgram)
        {
            string code = sourceCode.Substring(sourceCode.IndexOf("while") + 5).Trim();
            var clureVariables = new List<McClureVariable>();

            sourceCode = sourceCode.Substring(sourceCode.IndexOf("do") + 2);
            string[] variablesNames = GetNamesVariables(code.Remove(code.IndexOf("do")).Trim());
            code = code.Substring(code.IndexOf("do") + 2).Trim();
            foreach (string name in variablesNames)
            {
                var variable = new McClureVariable();
                variable.OwnerProgram = ownerProgram;
                variable.Name = name;
                clureVariables.Add(variable);
                variables.Add(variable);
            }

            if (code.IndexOf("begin") == 0)
            {
                foreach (McClureVariable variable in clureVariables)
                {
                    AnalyzeBlock(code.Remove(Parser.GetIndexEndOfBegin(code)), variable, variable.Name, 0, ownerProgram);
                }
                sourceCode = sourceCode.Substring(Parser.GetIndexEndOfBegin(sourceCode));
            }
            else
            {
                foreach (McClureVariable variable in clureVariables)
                {
                    AnalyzeBlock(code.Remove(code.IndexOf(";") + 1), variable, variable.Name, 0, ownerProgram);
                }
                sourceCode = sourceCode.Substring(sourceCode.IndexOf(";"));
            }
        }

        private void McClureRepeat(ref string sourceCode, Parser.Program ownerProgram)
        {
            string code = sourceCode.Substring(sourceCode.IndexOf("repeat")).Trim();
            var clureVariables = new List<McClureVariable>();
            string untilString = code.Substring(McCabeMetrics.GetIndexEndOfRepeat(code));

            sourceCode = sourceCode.Substring(McCabeMetrics.GetIndexEndOfRepeat(sourceCode));
            sourceCode = sourceCode.Substring(sourceCode.IndexOf(";") + 1);
            untilString =
                untilString.Remove(untilString.IndexOf(";")).Substring(untilString.IndexOf("until") + 5);
            string[] variablesNames = GetNamesVariables(untilString);
            foreach (string name in variablesNames)
            {
                var variable = new McClureVariable();
                variable.OwnerProgram = ownerProgram;
                variable.Name = name;
                clureVariables.Add(variable);
                variables.Add(variable);
            }
            foreach (McClureVariable variable in clureVariables)
            {
                AnalyzeBlock(code.Substring(6).Remove(McCabeMetrics.GetIndexEndOfRepeat(code)), variable, variable.Name,
                    0,
                    ownerProgram);
            }
        }

        private void McClureIf(ref string sourceCode, Parser.Program ownerProgram)
        {
            string code = sourceCode.Substring(sourceCode.IndexOf("if") + 2).Trim();
            var clureVariables = new List<McClureVariable>();

            sourceCode = sourceCode.Substring(sourceCode.IndexOf("then") + 4);
            string[] variablesNames = GetNamesVariables(code.Remove(code.IndexOf("then")).Trim());
            code = code.Substring(code.IndexOf("then") + 4).Trim();
            foreach (string name in variablesNames)
            {
                var variable = new McClureVariable();
                variable.OwnerProgram = ownerProgram;
                variable.Name = name;
                clureVariables.Add(variable);
                variables.Add(variable);
            }

            if (code.Trim().IndexOf("begin") == 0)
            {
                sourceCode = sourceCode.Substring(Parser.GetIndexEndOfBegin(sourceCode));
                foreach (McClureVariable variable in clureVariables)
                {
                    AnalyzeBlock(code.Substring(0, Parser.GetIndexEndOfBegin(code)), variable, variable.Name, 0,
                        ownerProgram);
                }
                code = code.Substring(Parser.GetIndexEndOfBegin(code)).Trim();
                if (code.Trim().IndexOf("else") == 0)
                {
                    code = code.Substring(4);
                    if (code.Trim().IndexOf("begin") == 0)
                    {
                        foreach (McClureVariable variable in clureVariables)
                        {
                            AnalyzeBlock(code.Substring(0, Parser.GetIndexEndOfBegin(code)), variable, variable.Name, 0,
                                ownerProgram);
                        }
                        sourceCode = sourceCode.Substring(0, Parser.GetIndexEndOfBegin(sourceCode));
                    }
                    else
                    {
                        foreach (McClureVariable variable in clureVariables)
                        {
                            AnalyzeBlock(code.Substring(0, code.IndexOf(";")), variable, variable.Name, 0,
                                ownerProgram);
                        }
                        sourceCode = sourceCode.Substring(sourceCode.IndexOf(";") + 1);
                    }
                }
            }
            else
            {
                if (code.IndexOf("else") != -1 && code.IndexOf("else") < code.IndexOf(";"))
                {
                    foreach (McClureVariable variable in clureVariables)
                    {
                        AnalyzeBlock(code.Substring(0, code.IndexOf("else")), variable, variable.Name, 0,
                            ownerProgram);
                    }
                    code.Substring(code.IndexOf("else") + 4);
                    sourceCode = sourceCode.Substring(sourceCode.IndexOf("else") + 4);
                    if (code.Trim().IndexOf("begin") == 0)
                    {
                        sourceCode = sourceCode.Substring(Parser.GetIndexEndOfBegin(sourceCode));
                        foreach (McClureVariable variable in clureVariables)
                        {
                            AnalyzeBlock(code.Substring(0, Parser.GetIndexEndOfBegin(code)), variable, variable.Name, 0,
                                ownerProgram);
                        }
                    }
                    else
                    {
                        sourceCode = sourceCode.Substring(sourceCode.IndexOf(";") + 1);
                        foreach (McClureVariable variable in clureVariables)
                        {
                            AnalyzeBlock(code.Substring(0, code.IndexOf(";") + 1), variable, variable.Name, 0,
                                ownerProgram);
                        }
                    }
                }
                else
                {
                    sourceCode = sourceCode.Substring(sourceCode.IndexOf(";"));
                    foreach (McClureVariable variable in clureVariables)
                    {
                        AnalyzeBlock(code.Substring(0, code.IndexOf(";") + 1), variable, variable.Name, 0,
                            ownerProgram);
                    }
                }
            }
        }

        private void McClureCase(ref string sourceCode, Parser.Program ownerProgram)
        {
            string code = sourceCode.Substring(sourceCode.IndexOf("case") + 4).Trim();

            var variable = new McClureVariable();
            variable.OwnerProgram = ownerProgram;
            variable.Name = code.Remove(code.IndexOf("of")).Trim();
            variables.Add(variable);
            sourceCode = sourceCode.Substring(sourceCode.IndexOf("of") + 2);
            code = code.Substring(code.IndexOf("of") + 2).Trim();
            int i = 0;
            while (i < code.Length)
            {
                string currentCode = code.Substring(i);
                if (currentCode.IndexOf("end") != 0)
                {
                    if (currentCode.Trim().IndexOf(":") == 0)
                    {
                        string codeAfterValue = currentCode.Substring(currentCode.IndexOf(":") + 1);
                        if (codeAfterValue.Trim().IndexOf("begin") == 0)
                        {
                            i += Parser.GetIndexEndOfBegin(currentCode);
                        }
                        else
                        {
                            if ((codeAfterValue.IndexOf("else") != -1) &&
                                (codeAfterValue.IndexOf("else") < codeAfterValue.IndexOf(";")))
                            {
                                i += currentCode.IndexOf("else");
                            }
                            else
                            {
                                i += currentCode.IndexOf(";");
                            }
                        }
                    }
                    else
                    {
                        if (currentCode.Trim().IndexOf("else") == 0)
                        {
                            string codeAfterElse = currentCode.Substring(currentCode.IndexOf("else") + 4);
                            if (codeAfterElse.Trim().IndexOf("begin") == 0)
                            {
                                i += Parser.GetIndexEndOfBegin(currentCode);
                            }
                            else
                            {
                                i += currentCode.IndexOf(";");
                            }
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
                else
                {
                    AnalyzeBlock(code.Substring(0, i + 3), variable, variable.Name, 0, ownerProgram);
                    sourceCode = sourceCode.Substring(i + 3);
                    break;
                }
            }
        }

        private void AnalyzeSubprogram(string sourceCode, Parser.Program ownerProgram)
        {
            int i = 0;
            while (i < sourceCode.Length)
            {
                string currentCode = sourceCode.Substring(i);
                
                 if (currentCode.Trim().IndexOf("if") == 0)
                {
                    McClureIf(ref sourceCode, ownerProgram);
                    i = 0;
                }
                else if (currentCode.Trim().IndexOf("for") == 0)
                {
                    McClureFor(ref sourceCode, ownerProgram);
                    i = 0;
                }
                else if (currentCode.Trim().IndexOf("while") == 0)
                {
                    McClureWhile(ref sourceCode, ownerProgram);
                    i = 0;
                }
                else if (currentCode.Trim().IndexOf("repeat") == 0)
                {
                    McClureRepeat(ref sourceCode, ownerProgram);
                    i = 0;
                }
                else if (currentCode.Trim().IndexOf("case") == 0)
                {
                    McClureCase(ref sourceCode, ownerProgram);
                    i = 0;
                }
                else
                {
                    i++;
                }
            }
        }

        public List<McClureVariable> GetVariables()
        {
            return variables;
        }

        public class McClureVariable
        {
            private readonly List<Parser.Program> subprograms;
            public int D;
            public int J;
            public string Name;
            public Parser.Program OwnerProgram;

            public McClureVariable()
            {
                subprograms = new List<Parser.Program>();
            }

            public int N
            {
                get { return subprograms.Count; }
            }


            public double Comlexity
            {
                get { return (double) D*J/N; }
            }

            public void AddSubprogram(Parser.Program program)
            {
                if (!subprograms.Contains(program))
                {
                    subprograms.Add(program);
                }
            }
        }
    }
}