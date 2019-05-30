using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcTest : MonoBehaviour
{
    public Transform inst;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    int z = 0;
    void Update()
    {
        if (z < 100)
        {
            for (int x = -50; x <= 50; x += 10)
            {
                Instantiate(inst, new Vector3(x, 0, z), Quaternion.identity);
            }
            z += 10;
        }
    }
}
