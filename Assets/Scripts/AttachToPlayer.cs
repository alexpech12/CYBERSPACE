using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachToPlayer : MonoBehaviour
{

    public Transform player;
    public bool X = true;
    public bool Y = true;
    public bool Z = true;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(
            X ? player.position.x : transform.position.x,
            Y ? player.position.y : transform.position.y,
            Z ? player.position.z : transform.position.z);
    }
}
