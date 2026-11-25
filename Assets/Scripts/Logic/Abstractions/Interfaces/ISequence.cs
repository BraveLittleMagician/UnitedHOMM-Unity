#nullable enable

public interface ISequence : ICopyable<ISequence> 
{
    int Count { get; }
    int CountOfAbles { get; }
}

public interface ISequence<TPosition, TSelf> : ISequence where TPosition : struct
{
    public ISequenceEnumerator<TPosition> GetEnumerator(TPosition start);
}