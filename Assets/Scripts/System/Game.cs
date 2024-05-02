using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    static private Game instance = null;
    static public Game Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject inst = new GameObject("!==== GAME ====!");
                DontDestroyOnLoad(inst);

                instance = inst.AddComponent<Game>();
            }
            return instance;
        }
    }
}
