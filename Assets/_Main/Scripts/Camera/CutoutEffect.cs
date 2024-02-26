using System;
using System.Collections.Generic;
using UnityEngine;

// Note: Setup object with box collider of chosen width/height with length extending from camera to target object
[RequireComponent(typeof(Collider))]
public class CutoutEffect : MonoBehaviour {
    // Set cutout params on material directly. Possibly add runtime override here
    [SerializeField] float size;
    // [SerializeField] float smoothness;
    // [SerializeField] float opacity;
    
    [Tooltip("Cutout shader on material applied to occluding objects, such as walls.")]
    [SerializeField] Shader cutOutShader;
    [Tooltip("LayerMask applied to occluding objects")]
    [SerializeField] LayerMask cutOutMask;

    [Tooltip("Target object to always show through occluding objects.")]
    [SerializeField] Transform targetObject;

    [SerializeField] List<Material> occludingObjectsMaterials = new();

    Camera mainCamera;

    void Awake() {
        mainCamera = Camera.main;
        
        
    }

    void Update() {
        for (int i = 0; i < occludingObjectsMaterials.Count; i++) {
            // Set position of cutout in material
            Vector2 cutOutPos = mainCamera.WorldToViewportPoint(targetObject.position);
            occludingObjectsMaterials[i].SetVector("_TargetPosition", cutOutPos);
        }
    }
    
    void EnableEffect(Material material) {
        material.SetFloat("_Size", size);
    }
    void DisableEffect(Material material) {
        material.SetFloat("_Size", 0);
    }

    void OnTriggerEnter(Collider col) {
        if ((cutOutMask & (1 << col.gameObject.layer)) != 0) {
            if (col.TryGetComponent(out Renderer r)) {
                Material mat = r.material;
                if (mat.shader.Equals(cutOutShader))
                {
                    occludingObjectsMaterials.Add(mat);
                    EnableEffect(mat);
                    
                }
            }
        }
    }
    
    void OnTriggerExit(Collider col) {
        if ((cutOutMask & (1 << col.gameObject.layer)) != 0) {
            if (col.TryGetComponent(out Renderer r)) {
                Material mat = r.material;
                occludingObjectsMaterials.Remove(mat);
                DisableEffect(mat);
            }
        }
    }
}