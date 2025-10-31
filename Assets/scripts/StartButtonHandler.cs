using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class StartButtonHandler : MonoBehaviour
{
    public void Test()
    {
        SceneManager.LoadScene("Game");
    }

    public void gamer() {
        combatManager.gamer = true;

        SceneManager.LoadScene("Game");
    }
}
