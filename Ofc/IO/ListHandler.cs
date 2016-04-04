namespace Ofc.IO
{
    using OfcCore;

    internal abstract class ListHandler : IHandler<string>
    {
        public IConfiguaration Configuaration { get; }


        public void End()
        {
            throw new System.NotImplementedException();
        }

        public void HandleEntry(string value)
        {
            throw new System.NotImplementedException();
        }

        public void HandleEntries(string[] values, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public void Start()
        {
            throw new System.NotImplementedException();
        }
    }
}