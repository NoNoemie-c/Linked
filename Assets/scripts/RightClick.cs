using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;
using game;
 
public sealed class RightClick : MonoBehaviour, IPointerClickHandler {
    public UnityEvent leftClick, rightClick, hoverStart, hoverEnd;
    public enum DisplayType : int {
        move,
        statusEffect,
        none
    };
    public DisplayType displayType;
    public move moveDisplay;
    public statusEffect statusEffectDisplay;
    private bool wasHovered;

    private static RightClick lastClicked;
    private static List<RightClick> allInstances = new List<RightClick>();

    private void Awake() {
        if (allInstances.Count > 0)
            allInstances.Clear();
    }

    private void Start() =>
        allInstances.Add(this);
    
    public void OnPointerClick(PointerEventData eventData) {
        if (leftClick is not null && eventData.button == PointerEventData.InputButton.Left) {
            if (SystemInfo.deviceType == DeviceType.Handheld) {
                foreach (RightClick r in allInstances)
                    if (r.wasHovered)
                        unhover();

                if (lastClicked == this) 
                    leftClick.Invoke();
                else
                    hover();
            } else
                leftClick.Invoke();
        } if (rightClick is not null && eventData.button == PointerEventData.InputButton.Right)
            rightClick.Invoke();
            
        lastClicked = this;
    }

    void Update() {
        if (Mouse.current.delta.ReadValue() == Vector2.zero)
            return;

        if (GetComponent<RectTransform>().rect.Contains(transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()))))
            hover();
        else if (wasHovered)
            unhover();     
    }

    private void hover() {
        if (displayType != DisplayType.none)
            Display();
        else if (!wasHovered)
            hoverStart.Invoke();
            
        wasHovered = true;
    }

    private void unhover() {
        wasHovered = false;
        hoverEnd.Invoke();
    }

    public void Display() {
        if (displayType == DisplayType.move)
            informationWindow.Display(moveDisplay);
        if (displayType == DisplayType.statusEffect)
            informationWindow.Display(statusEffectDisplay);
    }
}