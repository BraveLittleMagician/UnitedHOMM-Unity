#nullable enable

using System;

public interface ISequence : ICopyable<ISequence> 
{
    bool GuaranteesAtLeastOneStayable { get; }
}

public interface ISequence<TPosition, TSelf> : ISequence, IEquatable<TSelf> where TSelf : notnull, ISequence where TPosition : struct
{
    public ISequenceEnumerator<TPosition> GetEnumerator(TPosition start);
}