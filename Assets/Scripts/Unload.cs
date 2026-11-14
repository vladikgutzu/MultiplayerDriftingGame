using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unload : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Resources.UnloadUnusedAssets();
    }

}
