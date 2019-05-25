using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryPark : MonoBehaviour
{
    public List<Transform> shapes;
    public float maxTorque = 5.0f;

    public int shapeNum = 10;
    public float size = 20f;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < shapeNum; ++i)
        {
            Vector3 pos = new Vector3(Random.Range(-size / 2, size / 2), Random.Range(1f, 2f), Random.Range(-size / 2, size / 2));
            var newShape = Instantiate(shapes[Random.Range(0,shapes.Count)], transform);
            newShape.localPosition = pos;
            float scale = Random.Range(0.8f, 1.2f);
            newShape.localScale = new Vector3(scale, scale, scale);
            Rigidbody rb = newShape.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 torque = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                torque *= Random.Range(0, maxTorque);
                newShape.GetComponent<Rigidbody>().AddTorque(torque);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
