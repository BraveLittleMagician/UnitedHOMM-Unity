#nullable enable


using System;

public interface ISequence : ICopyable<ISequence>, ICanHaveNotStayable { }

public interface ISequence<TPosition, TSelf> : ISequence, IEquatable<TSelf> where TSelf : notnull, ISequence, new() where TPosition : struct
{
    public ISequenceEnumerator<TPosition> GetEnumerator(TPosition start);
}