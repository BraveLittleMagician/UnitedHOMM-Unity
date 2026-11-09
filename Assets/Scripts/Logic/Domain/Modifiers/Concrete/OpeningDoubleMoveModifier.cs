#nullable enable

using System;

public sealed class OpeningDoubleMoveModifier : IModifier, IShapeTransformModifier<Pattern<Square>, Square>
{
    private PieceModel _owner = null!;
    private PieceEventBus _eventBus = null!;
    private Action<IModifier> _selfRemove = null!;
    private int _maxCharges;
    private int _currentCharges;

    public string ModifierName => "DoubleMove";
    public int MaxDuration => -1;
    public int CurrentDuration => -1;
    public int MaxCharges => _maxCharges;
    public int CurrentCharges => _currentCharges;
    public ShapeTarget Target => ShapeTarget.Movement;

    public void Attach(ModifierContext context)
    {
        _owner = context.Owner;
        _eventBus = context.EventBus;
        _selfRemove = context.SelfRemove;
        _maxCharges = context.InitialCharges;
        _currentCharges = context.InitialCharges;
        _eventBus.PieceStartedAction += OnPieceStarted;
    }
    public void Detach()
    {
        if (_eventBus != null)
            _eventBus.PieceStartedAction -= OnPieceStarted;
    }
    public bool TryConsumeCharge()
    {
        if (_currentCharges < 0) return true;
        if (_currentCharges == 0) return false;
        _currentCharges--;
        return true;
    }
    public void RestoreCharges(int charges)
    {
        if (_maxCharges < 0) { _currentCharges = -1; return; }
        if (charges < 0 || charges > _maxCharges)
            throw new ArgumentOutOfRangeException(nameof(charges), $"Charges must be in [0..{_maxCharges}].");
        _currentCharges = charges;
    }
    public void RestoreDuration(int duration) { }

    public Pattern<Square> Apply(Pattern<Square> sequence)
    {
        if (_currentCharges == 0) return sequence;
        return sequence.WithAdded(new Square { X = 0, Y = 2, Z = 0 }, Stayable.Stay);
    }

    private void OnPieceStarted(PieceModel model, Activity activity)
    {
        if (model != _owner) return;
        if (!TryConsumeCharge()) return;
        if (_currentCharges == 0)
            _selfRemove(this);
    }
}