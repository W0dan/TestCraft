namespace TestCraft
{
    public interface IOutputProvider
    {
        void OutputLine(string s, params object[] name);
        void OutputLine();
        void Output(string s);

        void SetStylePass();
        void SetStyleFail();
        void SetStyleBadLabel();
        void SetStyleExceptionMessage();
        void SetStyleExceptionStacktrace();

        void ResetStyle();
    }
}