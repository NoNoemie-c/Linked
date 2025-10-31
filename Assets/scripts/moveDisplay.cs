using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using game;
using UnityEngine.UI;
using TMPro;

public sealed class moveDisplay : MonoBehaviour
{
    public move move;
    public actor user;

    private Button button;
    private TextMeshProUGUI Name;
    private Image icon;

    void Start() {
        if (move is null) {
            enabled = false;
            return;
        }
        icon = GetComponentInChildren<Image>();
        icon.sprite = move.typeImages[move.type];
        Name = GetComponentInChildren<TextMeshProUGUI>();
        Name.text = $" {move.Name}";
        button = GetComponentInChildren<Button>();
        RightClick r = GetComponent<RightClick>();
        r.moveDisplay = move;
        r.displayType = RightClick.DisplayType.move;
    }

    void Update() {
        if (user is enemy) {
            if (move.Usable(user.energy)) {
                if ((user as enemy).nextMoves.Contains(move)) {
                    Name.color = Color.yellow;
                    icon.color = Color.white;

                    Name.text = $"{(user as enemy).nextMoves.IndexOf(move) + 1} {move.Name}";
                } else {
                    Name.color = Color.white;
                    icon.color = Color.white;
                }
            } else {
                Name.color = new Color(1, 1, 1, .5f);
                icon.color = new Color(1, 1, 1, .5f);
            }
        } else {
            if (move.Usable(user.energy)) {
                Name.color = Color.white;
                icon.color = Color.white;
                button.interactable = true;
            } else {
                Name.color = new Color(1, 1, 1, .5f);
                icon.color = new Color(1, 1, 1, .5f);
                button.interactable = false;
             }
        }
    } 
}
