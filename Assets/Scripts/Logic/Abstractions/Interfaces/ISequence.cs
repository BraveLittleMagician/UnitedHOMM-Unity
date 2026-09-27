#nullable enable

using System;

public interface ISequence : ICopyable<ISequence> 
{
    bool GuaranteesAtLeastOneStayable { get; }
}

public interface ISequence<TPosition, TSelf> : ISequence where TPosition : struct
{
    public ISequenceEnumerator<TPosition> GetEnumerator(TPosition start);
}