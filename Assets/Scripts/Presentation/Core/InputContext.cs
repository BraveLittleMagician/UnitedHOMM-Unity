#nullable enable
public sealed record InputContext(MouseEventer MouseEventer)
{
    public void Run() => MouseEventer.Run();
}