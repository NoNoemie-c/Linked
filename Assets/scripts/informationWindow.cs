using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using game;

public sealed class informationWindow : MonoBehaviour, ISerializationCallbackReceiver
{
    // texts references
    private static TextMeshProUGUI Name, description, energyCost;
    private static Image moveType, energyImg;
    public static new RectTransform transform;

    private static Dictionary<string, string> statusEffectDescriptions;
    [SerializeField] private List<string> StatusEffectNames;
    [SerializeField] [TextArea(3, 5)] private List<string> StatusEffectDescriptions;

    public void OnBeforeSerialize() {}
    public void OnAfterDeserialize() {
        statusEffectDescriptions = new Dictionary<string, string>();

        for (int i = 0; i < StatusEffectNames.Count; i++)
            statusEffectDescriptions.Add(StatusEffectNames[i], StatusEffectDescriptions[i]);
    }

    void Awake() {
        move.Awake();

        transform = GetComponent<RectTransform>();

        TextMeshProUGUI[] texts = transform.GetComponentsInChildren<TextMeshProUGUI>(true);
        Name = Array.Find(texts, i => i.gameObject.name == "name");
        description = Array.Find(texts, i => i.gameObject.name == "description");
        energyCost = Array.Find(texts, i => i.gameObject.name == "energyCost");
        
        var imgs = GetComponentsInChildren<Image>();
        energyImg = imgs[0];
        moveType = imgs[1];
    }

    public static void Display(move m) {
        Name.text = m.Name; 

        description.text = m.description;

        energyCost.text = m.energyCost.ToString();
        energyImg.enabled = true;
        
        moveType.sprite = move.typeImages[m.type];
    }
    public static void Display(statusEffect s) {
        energyCost.text = s.value.ToString();
        energyImg.enabled = false;

        moveType.sprite = s.sprite;

        Name.text = s.name;

        description.text = statusEffectDescriptions[s.name];
    }
}