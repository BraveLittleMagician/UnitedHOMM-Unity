#nullable enable
public sealed record EmptyActivity : Activity
{
    public static readonly EmptyActivity Instance = new();
    private EmptyActivity() { }
}
