namespace Ofc.Util
{
    internal interface IActionManager<in T> where T : IAction
    {
        bool Empty { get; }

        bool Finished { get; }


        void Enqueue(T action);

        void Handle();
    }
}