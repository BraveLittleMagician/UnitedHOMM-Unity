#nullable enable

using UnityEngine;
using UnityEngine.UI;

public static class ExtensionsInUnity
{
    public static Selectable.Transition ToSelectable(this TransitionType transition) => transition switch
    {
        TransitionType.None => Selectable.Transition.None,
        TransitionType.Animation => Selectable.Transition.Animation,
        TransitionType.ColorTint => Selectable.Transition.ColorTint,
        TransitionType.SpriteSwap => Selectable.Transition.SpriteSwap,
        _ => Selectable.Transition.None
    };
}