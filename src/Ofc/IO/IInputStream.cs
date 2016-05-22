namespace Ofc.IO
{
    internal interface IInputStream<in T>
    {
        int Read(T[] buffer, int offset, int count);
    }
}