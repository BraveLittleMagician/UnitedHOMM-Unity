#nullable enable


public interface ICopyable<out T>
{
    T Copy();
}