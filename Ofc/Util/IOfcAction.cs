namespace Ofc.Util
{
    internal interface IOfcAction : IAction
    {
        string Code { get; }

        bool Force { get; set; }

        string Message { get; }

        string Path { get; }

        int Status { get; }

        OfcActionResult Result { get; }
    }
}