using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using TMPro;
using UnityEngine;

public class DialogueBox : MonoBehaviour
{
    public Animator animator;
    public TextMeshProUGUI TextBox;

    private CharacterPlayerInput[] inputs;

    private static int hashOpen = Animator.StringToHash("Opened");

    private static DialogueBox instance;
    private static DialogueBox Instance
    {
        get { return (instance ?? (instance = FindObjectOfType<DialogueBox>())); }
    }

    public static void Show(string message)
    {
        Instance.IShow(message);
    }

    public static void Close()
    {
        Instance.IClose();
    }

    public void IShow(string message)
    {
        TextBox.text = message;
        animator.SetBool(hashOpen, true);

        inputs = FindObjectsOfType<CharacterPlayerInput>();
        foreach (var input in inputs)
        {
            input.enabled = false;
            input.RewiredInput.AddInputEventDelegate(OnRewiredInput, Rewired.UpdateLoopType.Update, Rewired.InputActionEventType.ButtonJustPressed, "Submit");
        }
    }

    private void OnRewiredInput(InputActionEventData obj)
    {
        IClose();
    }

    public void IClose()
    {
        animator.SetBool(hashOpen, false);

        foreach (var input in inputs)
        {
            input.enabled = true;
            input.RewiredInput.RemoveInputEventDelegate(OnRewiredInput);
        }
    }

    

}
