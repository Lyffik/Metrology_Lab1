using System.Collections.Generic;

namespace Metrics
{
    public class Parser
    {
        private List<Program> Subprograms;

        public static Program FindProgram(string sourceCode)
        {
            Program result = null;
            string code = sourceCode;
            if (code.IndexOf("program") != -1)
            {
                result = new Program();
                code = code.Substring(code.IndexOf("program"));
                result.Code = code;
                result.Declaration = GetDeclaration(code);
                result.Name = GetName(result.Declaration);
                result.Arguments = FindArgmunets(result.Declaration);
                code = code.Substring(code.IndexOf("begin"));
                while (GetIndexEndOfBegin(code) != -1)
                {
                    code = code.Substring(code.IndexOf("begin"));
                    result.BlockBeginEnd = code.Substring(0, GetIndexEndOfBegin(code));
                    code = code.Substring(GetIndexEndOfBegin(code));
                }

                result.Subprograms = FindSubprograms(result.Code);
            }
            return result;
        }


        public static List<Program> FindSubprograms(string sourceCode)
        {
            var result = new List<Program>();
            string code = sourceCode;
            string codeAfterDeclaration;
            while (code.IndexOf("procedure") != -1 || code.IndexOf("function") != -1)
            {
                var subprogram = new Program();
                code = code.Substring(code.IndexOf(FindSubprogramType(code)));
                subprogram.Declaration =
                    GetDeclaration(code);
                subprogram.Arguments = FindArgmunets(subprogram.Declaration);
                subprogram.Name = GetName(subprogram.Declaration);
                code = code.Substring(code.IndexOf(";"));
                codeAfterDeclaration = code;
                code = RemoveSubprograms(code);
                codeAfterDeclaration = codeAfterDeclaration.Remove(codeAfterDeclaration.Length - code.Length);
                subprogram.BlockBeginEnd = code.Substring(0, GetIndexEndOfBegin(code));
                subprogram.Subprograms = FindSubprograms(codeAfterDeclaration);
                result.Add(subprogram);
            }
            return result;
        }

        private static string RemoveSubprograms(string code)
        {
            int subprogramCount = 0;
            while ((code.IndexOf("procedure") != -1 &&
                    code.IndexOf("begin") > code.IndexOf("procedure")) ||
                   (code.IndexOf("function") != -1 &&
                    code.IndexOf("begin") > code.IndexOf("function")))
            {
                subprogramCount++;
                string type = FindSubprogramType(code);
                code = code.Substring(code.IndexOf(type) + type.Length);
            }
            for (int i = 0; i < subprogramCount; i++)
            {
                code = code.Substring(GetIndexEndOfBegin(code));
            }
            code = code.Substring(code.IndexOf("begin"));
            return code;
        }

        private static List<string> FindArgmunets(string declaration)
        {
            string[] notArguments = {"var", "out"};
            var result = new List<string>();
            if (declaration.IndexOf("(") != -1)
            {
                declaration = declaration.Remove(declaration.IndexOf(")")).Substring(declaration.IndexOf("(") + 1);
                if (declaration.IndexOf(",") != -1)
                {
                    string[] arguments = declaration.Split(',');
                    foreach (string argument in arguments)
                    {
                        string name = argument.Remove(argument.IndexOf(":"));
                        foreach (string notArgument in notArguments)
                        {
                            if (name.IndexOf(notArgument) != -1)
                            {
                                name = name.Replace(notArgument, "");
                            }
                        }
                        result.Add(name.Trim());
                    }
                }
                else
                {
                    if (declaration.IndexOf(":") != -1)
                    {
                        string name = declaration.Remove(declaration.IndexOf(":"));
                        foreach (string notArgument in notArguments)
                        {
                            if (name.IndexOf(notArgument) != -1)
                            {
                                name = name.Replace(notArgument, "");
                            }
                        }
                        result.Add(name.Trim());
                    }
                }
            }

            return result;
        }

        private static string FindSubprogramType(string code)
        {
            string result;
            if ((code.IndexOf("function") == -1))
            {
                result = "procedure";
            }
            else if ((code.IndexOf("procedure") == -1))
            {
                result = "function";
            }
            else if (code.IndexOf("procedure") < code.IndexOf("function"))
            {
                result = "procedure";
            }
            else
            {
                result = "function";
            }
            return result;
        }


        public static int GetIndexEndOfBegin(string code)
        {
            int result = -1;
            int beginCount = 0;
            int endCount = 0;
            if (code.IndexOf("begin") != -1)
            {
                for (int i = code.IndexOf("begin"); i < code.Length; i++)
                {
                    if (code.Substring(i).IndexOf("begin") == 0)
                    {
                        beginCount++;
                    }
                    else if (code.Substring(i).IndexOf("end") == 0)
                    {
                        endCount++;
                    }
                    if (beginCount == endCount)
                    {
                        result = i + 3;
                        break;
                    }
                }
            }

            return result;
        }

        private static string GetDeclaration(string code)
        {
            return code.Remove(code.IndexOf(";") + 1).Trim();
        }

        private static string GetName(string code)
        {
            string result = "";
            string subprogramType;
            code = code.Trim();
            if (code.IndexOf("procedure") == 0)
            {
                subprogramType = "procedure";
            }
            else if (code.IndexOf("function") == 0)
            {
                subprogramType = "function";
            }
            else
            {
                subprogramType = "program";
            }
            if ((code.IndexOf("(") != -1) && (code.IndexOf("(") < code.IndexOf(";")))
            {
                result = code.Remove(code.IndexOf("(")).Substring(subprogramType.Length).Trim();
            }
            else
            {
                result = code.Remove(code.IndexOf(";")).Substring(subprogramType.Length).Trim();
            }
            return result;
        }

        public class Program
        {
            public List<string> Arguments;
            public string BlockBeginEnd;
            public string Code;
            public string Declaration;
            public string Name;
            public List<Program> Subprograms;
            public int Xp;
            public int Yp;
            public int fp;
            public int gp;

            public int Complexity
            {
                get { return Xp + Yp + fp + gp; }
            }
        }
    }
}