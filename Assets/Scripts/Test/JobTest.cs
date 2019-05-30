using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

struct JobOutput
{
    public bool a;
    public int b;
    public float c;
    public List<int> dd;
}

struct MyJob : IJob
{
    public int a;
    public NativeArray<int> values;
    public void Execute()
    {
        
        values[0] = 0;
        for (int i = 1; i < 100000; i++)
        {
            for (int j = 0; j < values.Length; j++)
            {
                values[j] = values[j] + a;
            }
        }
        
    }
}

public class JobTest : MonoBehaviour
{
    List<JobHandle> handles;

    List<NativeArray<int>> valuesList;

    NativeArray<JobOutput> output;

    // Start is called before the first frame update
    void Start()
    {
        valuesList = new List<NativeArray<int>>();
        handles = new List<JobHandle>();

        for (int i = 0; i < 10; i++)
        {
            valuesList.Add(new NativeArray<int>((new List<int> { i * 1, i * 2, i * 3, i * 4, i * 5 }).ToArray(), Allocator.TempJob));

            MyJob job = new MyJob();
            job.a = i;
            job.values = valuesList[valuesList.Count - 1];
            handles.Add(job.Schedule());
        }

    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < handles.Count; i++)
        {
            JobHandle handle = handles[i];
            NativeArray<int> values = valuesList[i];
            if (handle.IsCompleted && values.IsCreated)
            {
                Debug.Log("Job " + i + " is Complete!");
                handle.Complete();

                int count = 0;
                foreach (var v in values)
                {
                    Debug.Log("Job " + i + ": v["+count+"] = " + v);
                    count++;
                }

                values.Dispose();
            }
        }
    }
}
