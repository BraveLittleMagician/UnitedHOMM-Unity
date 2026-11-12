#nullable enable
using System;
using System.Text;
using UnityEngine;

public sealed class PromotionUI : EventSubscriberBehaviour
{
    private PromotionChoiceHandler _promotionChoiceHandler = null!;
    private PendingModalManager _pendingModalManager = null!;
    private UIPointerProbe _uiPointerProbe = null!;

    private bool _isBlocking;
    private Rect _blockingRect;

    public void Initialize(
        PromotionChoiceHandler promotionChoiceHandler,
        PendingModalManager pendingModalManager,
        UIPointerProbe uiPointerProbe)
    {
        _promotionChoiceHandler = promotionChoiceHandler ?? throw new ArgumentNullException(nameof(promotionChoiceHandler));
        _pendingModalManager = pendingModalManager ?? throw new ArgumentNullException(nameof(pendingModalManager));
        _uiPointerProbe = uiPointerProbe ?? throw new ArgumentNullException(nameof(uiPointerProbe));
    }

    private void OnGUI()
    {
        var pending = _pendingModalManager.CurrentPromotion;
        bool shouldBlock = pending != null;
        UpdateBlocking(shouldBlock);

        if (pending == null) return;
        var options = pending.Result.Options;
        if (options.Count == 0) return;

        var labels = new string[options.Count];
        float buttonWidth = 0f;
        for (int i = 0; i < options.Count; i++)
        {
            labels[i] = BuildOptionLabel(options[i]);
            float estimated = labels[i].Length * 8f + 20f;
            if (estimated > buttonWidth) buttonWidth = estimated;
        }
        if (buttonWidth < 200f) buttonWidth = 200f;
        if (buttonWidth > 600f) buttonWidth = 600f;

        const float buttonHeight = 42f;
        const float gap = 6f;

        float width = 40f + buttonWidth;
        float height = 60f + options.Count * (buttonHeight + gap) + 20f;
        float x = (Screen.width - width) * 0.5f;
        float y = (Screen.height - height) * 0.5f;

        GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);
        GUILayout.Label($"Promotion for {pending.Result.FromName}");
        GUILayout.Space(4);

        for (int i = 0; i < options.Count; i++)
        {
            var option = options[i];
            if (GUILayout.Button(labels[i], GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
                _promotionChoiceHandler.HandlePromotionChoice(option.TemplateIndex);
        }
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

    private static string BuildOptionLabel(PromotionOption option)
    {
        var sb = new StringBuilder();
        sb.Append(option.Name);
        sb.Append(" (HP ");
        sb.Append(option.MaxHp);
        sb.Append(", AC ");
        sb.Append(option.MaxAttackCharges);
        sb.Append(", dmg ");
        sb.Append(option.AttackDamage);
        foreach (var m in option.Modifiers)
        {
            sb.Append(", ");
            sb.Append(m.Name);
            sb.Append(' ');
            sb.Append(m.CurrentCharges);
            sb.Append('/');
            sb.Append(m.MaxCharges);
        }
        sb.Append(')');
        return sb.ToString();
    }
}