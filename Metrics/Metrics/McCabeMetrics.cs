namespace Metrics
{
    public class McCabeMetrics
    {
        private int McCabeIf(ref string sourceCode)
        {
            int result = 1;
            string code = sourceCode.Substring(sourceCode.IndexOf("then") + 4);

            if (code.Trim().IndexOf("begin") == 0)
            {
                sourceCode = sourceCode.Substring(Parser.GetIndexEndOfBegin(sourceCode));
                result += McCabe(code.Substring(0, Parser.GetIndexEndOfBegin(code)));
                code = code.Substring(Parser.GetIndexEndOfBegin(code)).Trim();
                if (code.Trim().IndexOf("else") == 0)
                {
                    code = code.Substring(4);
                    if (code.Trim().IndexOf("begin") == 0)
                    {
                        sourceCode = sourceCode.Substring(Parser.GetIndexEndOfBegin(sourceCode));
                        result += McCabe(code.Substring(0, Parser.GetIndexEndOfBegin(code)));
                    }
                    else
                    {
                        sourceCode = sourceCode.Substring(sourceCode.IndexOf(";") + 1);
                        result += McCabe(code.Substring(0, code.IndexOf(";") + 1));
                    }
                }
            }
            else
            {
                if (code.IndexOf("else") != -1 && code.IndexOf("else") < code.IndexOf(";"))
                {
                    result += McCabe(code.Substring(0, code.IndexOf("else")));
                    code.Substring(code.IndexOf("else") + 4);
                    sourceCode = sourceCode.Substring(sourceCode.IndexOf("else") + 4);
                    if (code.Trim().IndexOf("begin") == 0)
                    {
                        sourceCode = sourceCode.Substring(Parser.GetIndexEndOfBegin(sourceCode));
                        result += McCabe(code.Substring(0, Parser.GetIndexEndOfBegin(code)));
                    }
                    else
                    {
                        sourceCode = sourceCode.Substring(sourceCode.IndexOf(";") + 1);
                        result += McCabe(code.Substring(0, code.IndexOf(";") + 1));
                    }
                }
                else
                {
                    sourceCode = sourceCode.Substring(sourceCode.IndexOf(";"));
                    result += McCabe(code.Substring(0, code.IndexOf(";") + 1));
                }
            }
            return result;
        }

        private int McCabeWhile(ref string sourceCode)
        {
            int result = 1;
            string code = sourceCode.Substring(sourceCode.IndexOf("do") + 2);

            if (code.Trim().IndexOf("begin") == 0)
            {
                result += McCabe(code.Substring(0, Parser.GetIndexEndOfBegin(code)));
                sourceCode = sourceCode.Substring(Parser.GetIndexEndOfBegin(sourceCode));
            }
            else
            {
                result += McCabe(code.Substring(0, code.IndexOf(";")));
                sourceCode = sourceCode.Substring(sourceCode.IndexOf(";"));
            }
            return result;
        }

        private int McCabeRepeat(ref string sourceCode)
        {
            int result = 1;
            string code = sourceCode.Substring(sourceCode.IndexOf("repeat"));

            result += McCabe(code.Substring(6).Remove(GetIndexEndOfRepeat(code)));
            sourceCode = sourceCode.Substring(GetIndexEndOfRepeat(sourceCode));
            return result;
        }

        public static int GetIndexEndOfRepeat(string code)
        {
            int result = -1;
            int repeatCount = 0;
            int untilCount = 0;

            for (int i = code.IndexOf("repeat"); i < code.Length; i++)
            {
                if (code.Substring(i).IndexOf("repeat") == 0)
                {
                    repeatCount++;
                }
                else if (code.Substring(i).IndexOf("until") == 0)
                {
                    untilCount++;
                }
                if (repeatCount == untilCount)
                {
                    result = i;
                    break;
                }
            }
            return result;
        }

        private int McCabeFor(ref string sourceCode)
        {
            int result = 1;
            string code = sourceCode.Substring(sourceCode.IndexOf("do") + 2);

            if (code.Trim().IndexOf("begin") == 0)
            {
                result += McCabe(code.Substring(0, Parser.GetIndexEndOfBegin(code)));
                sourceCode = sourceCode.Substring(Parser.GetIndexEndOfBegin(sourceCode));
            }
            else
            {
                result += McCabe(code.Substring(0, code.IndexOf(";")));
                sourceCode = sourceCode.Substring(sourceCode.IndexOf(";"));
            }
            return result;
        }

        private int McCabeCase(ref string sourceCode)
        {
            int result = 0;
            string code = sourceCode;
            int i = code.IndexOf("of") + 2;

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
                            codeAfterValue = codeAfterValue.Substring(0, Parser.GetIndexEndOfBegin(codeAfterValue));
                            i += Parser.GetIndexEndOfBegin(currentCode);
                        }
                        else
                        {
                            if ((codeAfterValue.IndexOf("else") != -1) &&
                                (codeAfterValue.IndexOf("else") < codeAfterValue.IndexOf(";")))
                            {
                                codeAfterValue = codeAfterValue.Substring(0, codeAfterValue.IndexOf("else"));
                                i += currentCode.IndexOf("else");
                            }
                            else
                            {
                                codeAfterValue = codeAfterValue.Substring(0, codeAfterValue.IndexOf(";"));
                                i += currentCode.IndexOf(";");
                            }
                        }
                        result += McCabe(codeAfterValue) + 1;
                    }
                    else
                    {
                        if (currentCode.Trim().IndexOf("else") == 0)
                        {
                            string codeAfterElse = currentCode.Substring(currentCode.IndexOf("else") + 4);
                            if (codeAfterElse.Trim().IndexOf("begin") == 0)
                            {
                                codeAfterElse = codeAfterElse.Substring(0, Parser.GetIndexEndOfBegin(codeAfterElse));
                                i += Parser.GetIndexEndOfBegin(currentCode);
                            }
                            else
                            {
                                codeAfterElse = codeAfterElse.Substring(0, codeAfterElse.IndexOf(";"));
                                i += currentCode.IndexOf(";");
                            }
                            result += McCabe(codeAfterElse) + 1;
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
                else
                {
                    sourceCode = sourceCode.Substring(i + 3);
                    break;
                }
            }
            return result - 1;
        }

        public int McCabe(string sourceCode)
        {
            int result = 0;
            int i = 0;
            while (i < sourceCode.Length)
            {
                if (sourceCode.Substring(i).Trim().IndexOf("if") == 0)
                {
                    sourceCode = sourceCode.Substring(sourceCode.IndexOf("if") + 2);
                    result += McCabeIf(ref sourceCode);
                    i = 0;
                }
                else if (sourceCode.Substring(i).Trim().IndexOf("while") == 0)
                {
                    sourceCode = sourceCode.Substring(sourceCode.IndexOf("while"));
                    result += McCabeWhile(ref sourceCode);
                    i = 0;
                }
                else if (sourceCode.Substring(i).Trim().IndexOf("repeat") == 0)
                {
                    sourceCode = sourceCode.Substring(sourceCode.IndexOf("repeat"));
                    result += McCabeRepeat(ref sourceCode);
                    i = 0;
                }
                else if (sourceCode.Substring(i).Trim().IndexOf("for") == 0)
                {
                    sourceCode = sourceCode.Substring(sourceCode.IndexOf("for"));
                    result += McCabeFor(ref sourceCode);
                    i = 0;
                }
                else if (sourceCode.Substring(i).Trim().IndexOf("case") == 0)
                {
                    sourceCode = sourceCode.Substring(sourceCode.IndexOf("case"));
                    result += McCabeCase(ref sourceCode);
                    i = 0;
                }
                else
                {
                    i++;
                }
            }
            return result;
        }
    }
}