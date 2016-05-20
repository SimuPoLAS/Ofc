namespace Ofc.Util
{
    internal interface IAction
    {
        bool Faulty { get; }


        void Conduction();

        void Cleanup();

        void Preperation();
    }
}