using System.Diagnostics;
using System.IO;

namespace Metrics
{
    internal class Compile
    {
        private static string compilerExePath = "C:\\FPC\\2.6.4\\bin\\i386-win32\\fp.exe";

        private static string shellCompilationOptions = "/c /EHsc ";
        private static string shellCompilationOutFile = " /Fo: ";
        private static readonly string shellCompilationOutFileName = Directory.GetCurrentDirectory() + "\\test.obj";

        public static int tryCompile(string filePath)
        {
            var compilerProccess = new Process();
            compilerProccess.StartInfo.FileName = compilerExePath;
            compilerProccess.StartInfo.Arguments = GenerateShellArguments(filePath);
            compilerProccess.StartInfo.UseShellExecute = false;
            compilerProccess.StartInfo.RedirectStandardOutput = true;

            compilerProccess.Start();
            compilerProccess.WaitForExit();
            Log(compilerProccess.StandardOutput);
            return compilerProccess.ExitCode;
        }

        private static string GenerateShellArguments(string filePath)
        {
            return shellCompilationOptions + filePath + shellCompilationOutFile + shellCompilationOutFileName;
        }

        private static void Log(StreamReader outStream)
        {
            string output = outStream.ReadToEnd();
            var writer = new StreamWriter("D:\\log.txt", false, outStream.CurrentEncoding);
            writer.Write(output);
            writer.Dispose();
        }
    }
}