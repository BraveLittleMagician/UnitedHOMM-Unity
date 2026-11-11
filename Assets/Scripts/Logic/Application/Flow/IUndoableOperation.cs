#nullable enable

internal interface IUndoableOperation
{
    bool Undo();
    bool Redo();
}