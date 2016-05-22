namespace Ofc.Parsing.Hooks
{
    using System;

    internal class DebugHook<T> : IParserHook<T>
    {
        public void EnterDictionary(string name)
        {
            Console.WriteLine($"{name} {{");
        }

        public void LeaveDictionary()
        {
            Console.WriteLine("}");
        }

        public void EnterCodeStreamDictionary(string name)
        {
            Console.WriteLine($"{name} #codeStream {{");
        }

        public void LeaveCodeStreamDictionary()
        {
            Console.WriteLine("}");
        }

        public void EnterEntry(string name)
        {
            Console.Write($"{name}");
        }

        public void LeaveEntry()
        {
            Console.WriteLine(";");
        }

        public void EnterList(OfcListType type, int capacity)
        {
            switch (type)
            {
                case OfcListType.Scalar:
                    Console.WriteLine($" List<scalar> {capacity} (");
                    break;
                case OfcListType.Vector:
                    Console.WriteLine($" List<vector> {capacity} (");
                    break;
                case OfcListType.Tensor:
                    Console.WriteLine($" List<tensor> {capacity} (");
                    break;
                case OfcListType.Anonymous:
                    Console.WriteLine($" {capacity} (");
                    break;
            }
        }

        public void HandleListEntry(T value)
        {
            Console.WriteLine(value);
        }

        public void HandleListEntries(T[] values)
        {
            throw new NotImplementedException();
        }

        public void LeaveList()
        {
            Console.WriteLine(")");
        }


        public void HandleMacro(OfcDirectiveType directive, string data)
        {
            Console.WriteLine($"#{directive} {$"\"{data}\""}");
        }

        public void HandleDimension(T[] values)
        {
            Console.Write($" [{string.Join(", ", values)}]");
        }

        public void HandleScalar(T value)
        {
            Console.Write($" {value}");
        }

        public void HandleKeyword(string value)
        {
            Console.Write($" {value}");
        }

        public void HandleString(string data)
        {
            Console.Write($" \"{data}\"");
        }

        public void Flush()
        {
        }
    }
}