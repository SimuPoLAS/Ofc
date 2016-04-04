namespace Ofc.IO
{
    internal interface IOutputStream<in T>
    {
        void Write(T[] buffer, int offset, int count);
    }
}