namespace Ofc.Parsing.Hooks
{
    internal class EmptyHook<T> : IParserHook<T>
    {
        private static EmptyHook<T> instance;

        internal static EmptyHook<T> Instance => instance ?? (instance = new EmptyHook<T>());


        private EmptyHook()
        {
        }


        public void EnterDictionary(string name)
        {
        }

        public void LeaveDictionary()
        {
        }

        public void EnterCodeStreamDictionary(string name)
        {
        }

        public void LeaveCodeStreamDictionary()
        {
        }

        public void EnterEntry(string name)
        {
        }

        public void LeaveEntry()
        {
        }

        public void EnterList(OfcListType type, int capacity)
        {
        }

        public void HandleListEntry(T value)
        {
        }

        public void HandleListEntries(T[] values)
        {
        }

        public void LeaveList()
        {
        }


        public void HandleMacro(OfcDirectiveType directive, string data)
        {
        }

        public void HandleDimension(T[] values)
        {
        }

        public void HandleScalar(T value)
        {
        }

        public void HandleKeyword(string value)
        {
        }

        public void HandleString(string data)
        {
        }

        public void Flush()
        {
        }
    }
}