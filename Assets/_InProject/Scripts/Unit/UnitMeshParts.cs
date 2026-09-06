using System;
using System.Linq;
using DevelopKit;
using UnityEngine;

public class UnitMeshParts : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] meshRenderParts;
    public Material[] matVariations;

    public void SetRender(Transform parents, int index = 0)
    {
        this.SetParents(parents);

        if (meshRenderParts.Length == 0 || matVariations.Length == 0)
            return;

        foreach(var part in meshRenderParts)
            part.gameObject.SetActive(false);

        meshRenderParts[index].gameObject.SetActive(true);
        meshRenderParts[index].material = matVariations[index];
    }
}
