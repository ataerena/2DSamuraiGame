using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class KeyboardControls
{
    public enum Button
    {
        Left,
        Right,
        Jump
    }

    private static readonly Dictionary<Button, KeyCode> keyBindings = new Dictionary<Button, KeyCode>()
    {
        { Button.Left, KeyCode.A },
        { Button.Right, KeyCode.D },
        { Button.Jump, KeyCode.Space }
    };

    public static KeyCode GetKey(Button button)
    {
        return keyBindings[button];
    }

    public static bool IsKeyPressed(Button button)
    {
        return Input.GetKey(GetKey(button));
    }

    public static bool IsKeyHit(Button button)
    {
        return Input.GetKeyDown(GetKey(button));
    }

    public static void SetKeyBinding(Button button, KeyCode keyCode)
    {
        keyBindings[button] = keyCode;
    }
}