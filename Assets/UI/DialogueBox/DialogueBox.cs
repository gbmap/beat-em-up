using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using TMPro;
using UnityEngine;
using Catacumba.Entity;

public class DialogueBox : MonoBehaviour
{
    public Animator animator;
    public TextMeshProUGUI TextBox;

    private CharacterPlayerInput[] inputs;

    private static int hashOpen = Animator.StringToHash("Opened");

    private static System.Action OnClosedEvent;

    private static DialogueBox instance;
    private static DialogueBox Instance
    {
        get { return instance == null ? (instance = FindObjectOfType<DialogueBox>()) : instance; }
    }

    public static void Show(string message, System.Action OnClose = null)
    {
        OnClosedEvent = OnClose;
        Instance.IShow(message);
    }

    public static void Close()
    {
        Instance.IClose();
    }

    public void IShow(string message)
    {
        animator.gameObject.SetActive(true);
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

        OnClosedEvent?.Invoke();
    }

    

}
