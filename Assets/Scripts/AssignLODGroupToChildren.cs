using UnityEngine;

[RequireComponent(typeof(LODGroup))]
public class AssignLODGroupToChildren : MonoBehaviour
{
    public void OnValidate()
    {
        // Get the LODGroup component of the parent GameObject
        LODGroup parentLODGroup = GetComponent<LODGroup>();

        if (parentLODGroup == null)
        {
            Debug.LogError("Parent GameObject does not have a LODGroup component.");
            return;
        }

        // Get all the mesh renderers of children
        MeshRenderer[] childMeshRenderers = GetComponentsInChildren<MeshRenderer>();

        // Create LODs for the parent LODGroup
        LOD[] lods = new LOD[childMeshRenderers.Length]; // No need to include the parent in LODs
        float[] screenRelativeTransitionHeights = new float[lods.Length];

        // Create LODs for each child renderer
        for (int i = 0; i < childMeshRenderers.Length; i++)
        {
            Renderer renderer = childMeshRenderers[i];

            // Set LODs to use the mesh of each child renderer
            Renderer[] renderers = new Renderer[] { renderer };
            lods[i] = new LOD(1.0f / (i + 2), renderers); // Divide by (i + 2) to create decreasing LODs
            screenRelativeTransitionHeights[i] = (i + 1) / (float)(childMeshRenderers.Length + 1); // Transition heights
        }

        // Assign the LODs to the parent's LODGroup
        parentLODGroup.SetLODs(lods);
        parentLODGroup.RecalculateBounds(); // Recalculate bounds after changing LODs

        // Set the fade mode to CrossFade to ensure smooth transitions
        parentLODGroup.fadeMode = LODFadeMode.CrossFade;
    }
}