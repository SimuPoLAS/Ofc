namespace OfcAlgorithm.Rounding
{
    public interface IUnrandomizerConfig
    {
        double Min { get; }
        double Max { get; }
        double Epsilon { get; }
    }
}