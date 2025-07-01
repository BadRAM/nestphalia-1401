using WrenSharp;

namespace nestphalia;

public class WrenBufferOutput : IWrenWriteOutput, IWrenErrorOutput
{
    private string buffer;
    
    public void OutputWrite(WrenVM vm, string text)
    {
        buffer += text;
    }

    public void OutputError(WrenVM vm, WrenErrorType errorType, string moduleName, int lineNumber, string message)
    {
        switch (errorType)
        {
            case WrenErrorType.Compile:
                buffer += ($"Wren compile error in {moduleName}:{lineNumber} : {message}") + "\n";
                break;

            case WrenErrorType.StackTrace:
                buffer += ($"at {message} in {moduleName}:{lineNumber}") + "\n";
                break;

            case WrenErrorType.Runtime:
                buffer += (string.IsNullOrEmpty(moduleName)
                    ? $"Wren error: {message}"
                    : $"Wren error in {moduleName}: {message}") + "\n";
                break;
        }
    }

    public string GetBuffer()
    {
        string ret = buffer;
        buffer = "";
        return ret;
    }
}