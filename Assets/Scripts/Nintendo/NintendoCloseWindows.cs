using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class NintendoCloseWindows : MonoBehaviour
{
    [SerializeField] Button currentButton;

    [SerializeField] private ButtonControl nintendoButton;
    //[SerializeField] private GamepadButton _gamepadButton;

    private enum NintendoButtons
    {
        A = GamepadButton.East,
        B = GamepadButton.South,
        X = GamepadButton.North,
        Y = GamepadButton.West,
        ZL = GamepadButton.RightTrigger,
        ZR = GamepadButton.LeftTrigger,
        L = GamepadButton.LeftShoulder,
        R = GamepadButton.RightShoulder,
        DUp = GamepadButton.DpadUp,
        DDown = GamepadButton.DpadDown,
        DLeft = GamepadButton.DpadLeft,
        DRight = GamepadButton.DpadRight,
        Plus = GamepadButton.Start
    }

    [SerializeField] private NintendoButtons buttonToPress;

    [SerializeField] private KeyCode buttonToPressOnKeyboard;


    private void Awake()
    {
        if (currentButton == null)
        {
            currentButton = GetComponent<Button>();
        }
        //AssignButton();
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy) return;
#if UNITY_SWITCH && !UNITY_EDITOR
        NintendoInteractions();
#else
        UnityInteractions();
#endif
    }

    private void UnityInteractions()
    {
        if (Input.GetKeyDown(buttonToPressOnKeyboard))
        {
            if (currentButton != null)
            {
                currentButton.onClick.Invoke();
            }
        }
    }


    private void NintendoInteractions()
    {
        UnityEngine.InputSystem.Gamepad gamepad = UnityEngine.InputSystem.Gamepad.current;
        if (gamepad != null && gamepad[(GamepadButton)buttonToPress].wasPressedThisFrame)
        {
            if (currentButton != null)
            {
                currentButton.onClick.Invoke();
            }
            // currentButton.onClick.Invoke();
        }
    }
}
