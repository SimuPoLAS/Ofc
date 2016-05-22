namespace Ofc.LZMA.Common
{
    public class CommandForm
    {
        public string IdString = "";
        public bool PostStringMode;
        public CommandForm(string idString, bool postStringMode)
        {
            IdString = idString;
            PostStringMode = postStringMode;
        }
    }
}