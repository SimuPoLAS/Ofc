namespace Ofc.Parsing
{
    using JetBrains.Annotations;

    internal interface IParserHook<in T>
    {
        void EnterDictionary(string name);

        void LeaveDictionary();

        void EnterCodeStreamDictionary(string name);

        void LeaveCodeStreamDictionary();

        void EnterEntry(string name);

        void LeaveEntry();

        void EnterList(OfcListType type, int capacity);

        void HandleListEntry(T value);

        void HandleListEntries(T[] values);

        void LeaveList();


        void HandleMacro(OfcDirectiveType directive, [CanBeNull] string data);

        void HandleDimension(T[] values);

        void HandleScalar(T value);

        void HandleKeyword(string value);

        void HandleString(string data);


        void Flush();
    }
}