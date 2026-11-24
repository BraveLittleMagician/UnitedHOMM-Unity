#nullable enable

using System;

public interface ISequence : ICopyable<ISequence> 
{
    int Count { get; }
    int CountOfAbles { get; }
}

public interface ISequence<TPosition, TPurpose, TSelf> : ISequence where TPosition : struct where TPurpose : struct, Enum
{
    public ISequenceEnumerator<TPosition, TPurpose> GetEnumerator(TPosition start);
}