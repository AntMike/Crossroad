using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalController : MonoBehaviour
{
    public static GlobalController Instance;

    public List<Path> allPaths;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);        
    }
}
