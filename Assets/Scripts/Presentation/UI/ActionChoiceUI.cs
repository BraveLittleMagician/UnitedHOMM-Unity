#nullable enable
using System;
using System.Text;
using UnityEngine;

public sealed class ActionChoiceUI : EventSubscriberBehaviour
{
    private CellActionHandler _cellActionHandler = null!;
    private PendingModalManager _pendingModalManager = null!;
    private UIPointerProbe _uiPointerProbe = null!;

    private bool _isBlocking;
    private Rect _blockingRect;

    public void Initialize(
        CellActionHandler cellActionHandler,
        PendingModalManager pendingModalManager,
        UIPointerProbe uiPointerProbe)
    {
        _cellActionHandler = cellActionHandler ?? throw new ArgumentNullException(nameof(cellActionHandler));
        _pendingModalManager = pendingModalManager ?? throw new ArgumentNullException(nameof(pendingModalManager));
        _uiPointerProbe = uiPointerProbe ?? throw new ArgumentNullException(nameof(uiPointerProbe));
    }

    private void OnGUI()
    {
        var pending = _pendingModalManager.CurrentChoice;
        bool shouldBlock = pending != null;
        UpdateBlocking(shouldBlock);

        if (pending == null) return;
        var options = pending.Options;
        if (options.Count == 0) return;

        var labels = new string[options.Count];
        float buttonWidth = 0f;
        for (int i = 0; i < options.Count; i++)
        {
            labels[i] = BuildLabel(options[i]);
            float estimated = labels[i].Length * 8f + 20f;
            if (estimated > buttonWidth) buttonWidth = estimated;
        }
        if (buttonWidth < 260f) buttonWidth = 260f;
        if (buttonWidth > 600f) buttonWidth = 600f;

        const float buttonHeight = 50f;
        const float gap = 6f;
        const float cancelHeight = 34f;

        float width = 40f + buttonWidth;
        float height = 40f + options.Count * (buttonHeight + gap) + cancelHeight + 20f;
        float x = (Screen.width - width) * 0.5f;
        float y = (Screen.height - height) * 0.5f;

        GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);

        var c = pending.Cell;
        GUILayout.Label($"Actions on cell ({c.X}, {c.Y}, {c.Z}, {c.W})");
        GUILayout.Space(4);

        for (int i = 0; i < options.Count; i++)
        {
            if (GUILayout.Button(labels[i], GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
                _cellActionHandler.HandleChoiceSelection(i);
        }

        GUILayout.Space(4);
        if (GUILayout.Button("Cancel", GUILayout.Width(buttonWidth), GUILayout.Height(cancelHeight)))
            _cellActionHandler.CancelChoice();

        GUILayout.EndArea();
    }

    protected override void OnCleanup()
    {
        if (_isBlocking && _uiPointerProbe != null)
        {
            _uiPointerProbe.UnregisterRegion(_blockingRect);
            _isBlocking = false;
        }
    }

    private void UpdateBlocking(bool shouldBlock)
    {
        if (shouldBlock)
        {
            var screenRect = new Rect(0, 0, Screen.width, Screen.height);
            if (!_isBlocking || _blockingRect != screenRect)
            {
                if (_isBlocking) _uiPointerProbe.UnregisterRegion(_blockingRect);
                _uiPointerProbe.RegisterRegion(screenRect);
                _blockingRect = screenRect;
                _isBlocking = true;
            }
        }
        else if (_isBlocking)
        {
            _uiPointerProbe.UnregisterRegion(_blockingRect);
            _isBlocking = false;
        }
    }

    private static string BuildLabel(CellActionOption option)
    {
        var sb = new StringBuilder();
        sb.Append(option.Label);
        if (option.MaxCharges >= 0)
        {
            sb.Append(" (");
            sb.Append(option.CurrentCharges);
            sb.Append('/');
            sb.Append(option.MaxCharges);
            sb.Append(')');
        }
        if (!string.IsNullOrEmpty(option.Description))
        {
            sb.Append(" — ");
            sb.Append(option.Description);
        }
        return sb.ToString();
    }
}