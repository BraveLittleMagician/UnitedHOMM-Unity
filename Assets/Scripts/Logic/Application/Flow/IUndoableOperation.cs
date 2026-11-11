#nullable enable

internal interface IUndoableOperation
{
    bool Undo(Flow flow);
    bool Redo(Flow flow);
}