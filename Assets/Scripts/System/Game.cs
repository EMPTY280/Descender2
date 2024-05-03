using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    static private Game instance = null;

    private GameInput input = null;

    static public Game Instance
    {
        get
        {
            if (instance == null)
                Initialize();
            return instance;
        }
    }

    static private void Initialize()
    {
        GameObject inst = new GameObject("!==== GAME ====!");
        DontDestroyOnLoad(inst);

        instance = inst.AddComponent<Game>();
        instance.input = inst.AddComponent<GameInput>();
    }
}
