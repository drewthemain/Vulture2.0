using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RagdollBalancer : EditorWindow
{
    public GameObject normalParent;
    public GameObject ragdollParent;

    [MenuItem("Window/Ragdoll Balancer")]
    public static void ShowWindow()
    {
        GetWindow<RagdollBalancer>("Ragdoll Balancer");
    }

    private void OnGUI()
    {
        normalParent = (GameObject) EditorGUILayout.ObjectField(normalParent, typeof(Object), true);
        ragdollParent = (GameObject) EditorGUILayout.ObjectField(ragdollParent, typeof(Object), true);
        if (GUILayout.Button("Balance"))
        {
            Balancer();
        }
    }

    /// <summary>
    /// Sets all rotations of normal rig equal to the ragdoll rig
    /// Should not be used during runtime, an editor tool
    /// </summary>
    private void Balancer()
    {
        if (normalParent == null || ragdollParent == null)
        {
            Debug.Log("Missing parent objects!");
        }

        Transform[] normalTransforms = normalParent.GetComponentsInChildren<Transform>();
        Transform[] ragTransforms = ragdollParent.GetComponentsInChildren<Transform>();

        foreach (Transform child in normalTransforms)
        {
            foreach (Transform ragChild in ragTransforms)
            {
                if (child.name == ragChild.name)
                {
                    ragChild.position = child.position;
                    ragChild.rotation = child.rotation;
                    ragChild.localScale = child.localScale;
                }
            }
        }

        Debug.Log("Done!");
    }
}
