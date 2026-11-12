#nullable enable
using System;
using UnityEngine.InputSystem;

public static class InputModule
{
    public static InputContext Initialize(
        MouseEventer mouseEventer,
        InputActionAsset inputActions,
        IClickLeftHub clickLeftHub,
        IClickRightHub clickRightHub,
        IHoldRightHub holdRightHub,
        IHoldMiddleHub holdMiddleHub,
        IScrollHub scrollHub,
        IPointerHub pointerHub,
        IDeltaHub deltaHub)
    {
        if (mouseEventer == null) throw new ArgumentNullException(nameof(mouseEventer));
        if (inputActions == null) throw new ArgumentNullException(nameof(inputActions));

        mouseEventer.Initialize(
            inputActions,
            clickLeftHub,
            clickRightHub,
            holdRightHub,
            holdMiddleHub,
            scrollHub,
            pointerHub,
            deltaHub);

        return new InputContext(mouseEventer);
    }
}