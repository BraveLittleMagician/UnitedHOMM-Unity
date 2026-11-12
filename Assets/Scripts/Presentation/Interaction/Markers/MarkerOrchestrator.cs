#nullable enable

using System;

public sealed class MarkerOrchestrator
{
    private readonly IMarkerPresenter[] _presenters;

    public MarkerOrchestrator(params IMarkerPresenter[] presenters)
        => _presenters = presenters ?? throw new ArgumentNullException(nameof(presenters));

    public void ClearAll()
    {
        for (int i = 0; i < _presenters.Length; i++)
            _presenters[i].Clear();
    }

    public void RepositionAll()
    {
        for (int i = 0; i < _presenters.Length; i++)
            _presenters[i].Reposition();
    }
}