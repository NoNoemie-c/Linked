using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using game;
using System;

public sealed class combatMenuManager : MonoBehaviour
{
    public  static GameObject moveDisplayPrefab;
    public static new RectTransform transform;
    public static bool removal;

    void Awake() {
        moveDisplayPrefab = Resources.Load<GameObject>("prefabs/move");
        transform = GetComponent<RectTransform>();
    }

    public static void DisplayPlayer(List<move> moves) {
        foreach(moveDisplay m in transform.GetComponentsInChildren<moveDisplay>())
            Destroy(m.gameObject);

        moves.Sort(new Comparison<move>((m1, m2) => m1.type.CompareTo(m2.type)));

        for (int i = 0; i < moves.Count; i++) {
            moveDisplay m = Instantiate(moveDisplayPrefab, new Vector3(), Quaternion.identity, transform).GetComponent<moveDisplay>();
            m.transform.localPosition = new Vector3(-195 + m.GetComponent<RectTransform>().rect.width * Mathf.Floor(i / 4f), 49 - m.GetComponent<RectTransform>().rect.height * (i % 4), 0);
            m.move = moves[i];
            m.user = combatManager.player;

            if (removal)
                m.GetComponentInChildren<RightClick>().rightClick.AddListener(() => combatManager.removeMove(m.move));
            else
                m.GetComponentInChildren<RightClick>().leftClick.AddListener(() => combatManager.used(m.move, combatManager.player));
        }
    }
}
