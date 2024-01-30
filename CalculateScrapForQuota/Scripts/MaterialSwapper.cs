using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using P = CalculateScrapForQuota.Plugin;

namespace CalculateScrapForQuota.Scripts;

/// <summary>
/// Non-intrusive way of "swapping materials" by using cloned objects in a pool.
/// </summary>
public static class MaterialSwapper
{
    public static Material swapMaterial = Materials.HighlightMaterial;
    public static Color swapColor = Color.green;
    
    private static Dictionary<GameObject, SwapObject> pool = new();

    public static void SwapOn(List<GameObject> gameObjects)
    {
        foreach (var gameObject in gameObjects)
        {
            Add(gameObject);
            pool[gameObject].Swap(true);
        }
    }

    public static void SwapOff()
    {
        foreach (var kvp in pool)
        {
            if (kvp.Key == null)
            {
                kvp.Value.Dispose();
                pool.Remove(kvp.Key);
            }
            else
            {
                kvp.Value.Swap(false);
            }
        }
    }
    
    private static void Add(GameObject original)
    {
        if (pool.ContainsKey(original))
            return;
        
        pool.Add(original, new(original));
    }
    
    public static void Clear()
    {
        SwapOff();
        foreach (var swapObject in pool.Values)
        {
            swapObject.Dispose();
        }
        pool.Clear();
    }

    private static GameObject Clone(GameObject parent)
    {
        var clone = Object.Instantiate(parent, parent.transform.position, parent.transform.rotation);
        clone.transform.localScale = parent.transform.localScale;
        clone.transform.SetParent(parent.transform);
        clone.name = $"{parent.name} (Clone for swap)";
        P.Log($"Made '{clone.name}' \nAnd parented it to {parent.name}");
        return clone;
    }

    private class Visuals
    {
        private readonly List<Component> _rootOtherComponents;
        private List<Transform> Transforms { get; } = new();
        private List<MeshFilter> MeshFilters { get; } = new();
        private List<MeshRenderer> MeshRenderers { get; } = new();
        private List<SkinnedMeshRenderer> SkinnedMeshRenderers { get; } = new();
        public Visuals(GameObject gameObject)
        {
            _rootOtherComponents = gameObject.GetComponentsInChildren<Component>().ToList();
            foreach (var transform in gameObject.GetComponentsInChildren<Transform>())
            {
                Transforms.Add(transform);
                _rootOtherComponents.Remove(transform);
            }
            foreach (var meshFilter in gameObject.GetComponentsInChildren<MeshFilter>())
            {
                MeshFilters.Add(meshFilter);
                _rootOtherComponents.Remove(meshFilter);
            }
            foreach (var meshRenderer in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                MeshRenderers.Add(meshRenderer);
                _rootOtherComponents.Remove(meshRenderer);
            }
            foreach (var skinnedMeshRenderer in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                SkinnedMeshRenderers.Add(skinnedMeshRenderer);
                _rootOtherComponents.Remove(skinnedMeshRenderer);
            }
            foreach (var mf in MeshFilters)
            {
                P.Log($"Preserving mesh '{mf.mesh.name}'");
            }
        }

        public List<Renderer> Renderers
        {
            get
            {
                List<Renderer> result = new(MeshRenderers);
                result.AddRange(SkinnedMeshRenderers);
                return result;
            }
        }
        
        public void DestroyNonVisuals()
        {
            foreach (var component in _rootOtherComponents) 
                Object.Destroy(component);
            _rootOtherComponents.Clear();
        }
    }
    
    private class SwapObject
    {
        private readonly Visuals _rootVisuals;
        private readonly GameObject _cloneGO;
        private readonly Visuals _cloneVisuals;
        
        public SwapObject(GameObject original)
        {
            _cloneGO = Clone(original);
            _rootVisuals = new(original);
            _cloneVisuals = new(_cloneGO);
            CleanUpClone();
        }
        private void CleanUpClone()
        {
            _cloneVisuals.DestroyNonVisuals();
            foreach (var renderer in _cloneVisuals.Renderers)
            {
                for (var i = 0; i < renderer.materials.Length; i++)
                {
                    renderer.materials[i] = swapMaterial;
                    renderer.materials[i].color = new(swapColor.r, swapColor.g, swapColor.b);
                }
            }
        }
        
        public void Swap(bool on)
        {
            foreach (var renderer in _rootVisuals.Renderers)
            {
                renderer.enabled = !on;
            }
            foreach (var renderer in _cloneVisuals.Renderers)
            {
                renderer.enabled = on;
                renderer.rendererPriority = on ? 69420 : -69420;
            }
        }
        public void Dispose()
        {
            if (_cloneGO != null)
                Object.Destroy(_cloneGO);
        }
    }
}