using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(MeshRenderer))]
public class BuildPreview_Behavior : MonoBehaviour
{
    public Material matIsBuildable, matIsNotBuildable;

    public bool shouldBeBuildable;
    private bool wasBuildable;

    private MeshRenderer meshRen;

    // Start is called before the first frame update
    void Start()
    {
        TryGetComponent(out meshRen);       
    }

    private void LateUpdate()
    {
        if(wasBuildable != shouldBeBuildable)
            UpdateMaterialDisplay();
    }

    private void UpdateMaterialDisplay()
    {
        print("Mesh Needs To Change");
        Material newMat = matIsBuildable;
        if (!shouldBeBuildable)
            newMat = matIsNotBuildable;

        for (int i = 0; i < meshRen.materials.Length; i++)
            meshRen.materials[i] = newMat;

        wasBuildable = shouldBeBuildable;
    }

}
