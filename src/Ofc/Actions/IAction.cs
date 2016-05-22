namespace Ofc.Actions
{
    internal interface IAction
    {
        bool Faulty { get; }


        void Conduction();

        void Cleanup();

        void Preperation();
    }
}