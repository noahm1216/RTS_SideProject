using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;


public class NavMesh_Handler : MonoBehaviour
{
    public NavMeshSurface[] navMeshSurfaces;
    public bool bakeNavMeshNow;

    public bool autoBakeOnSchedule;
    public float autoBakeWaitTime = 12;
    private float autoBakeTimeStamp;
   
    // Update is called once per frame
    void LateUpdate()
    {
        if(autoBakeOnSchedule && Time.time > autoBakeTimeStamp + autoBakeWaitTime)
        {
            bakeNavMeshNow = true;
            autoBakeTimeStamp = Time.time;
        }

        if (bakeNavMeshNow && navMeshSurfaces.Length > 0)
        {
            BakeNaveMeshSurfaces();
            bakeNavMeshNow = false;
        }        
    }

    public void BakeNaveMeshSurfaces()
    {
        for (int n = 0; n < navMeshSurfaces.Length; n++)
            navMeshSurfaces[n].BuildNavMesh();
    }
}
