using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Environment.Instancing
{
    [Serializable]
    public class InstanceConfiguration
    {
        public Mesh Mesh;
        public Material Material;
        public float Probability = 1f;
        public float Scale = 1f;
        public float NormalOffset = 0.1f;   // is the instance position pushed towards its normal

        // public Vector3 Scale = new Vector3(1, 1, 1);             // These 3 are useful features but rarely used. You can implement them by commentting in stuff here and elsewhere.
        // public Vector3 Offset = Vector3.zero;
        // public string Layer = "";
    };
    public abstract class InstancesBehaviour : MonoBehaviour
    {
        Dictionary<InstanceConfiguration, InstancingInfo> InstancingDictionary;
        bool drawInstances = false;
        
        Bounds instanceBounds = new Bounds(Vector3.zero, Vector3.one * 1000f); // Could figure out something with this later for optimization
        void Update()
        {
            if (drawInstances)
            {
                foreach (KeyValuePair<InstanceConfiguration, InstancingInfo> entry in InstancingDictionary)
                {
                    var (data, info) = entry;
                    Graphics.DrawMeshInstancedIndirect(data.Mesh, 0, data.Material, instanceBounds, info.argsBuffer, 0, info.materialPropertyBlock, ShadowCastingMode.Off, false, LayerMask.NameToLayer("Default")); //LayerMask.NameToLayer(data.Layer));
                }
                if ((transform.hasChanged) && InstancingDictionary != null && InstancingDictionary.Count > 0)
                {
                    UpdateMaterialParentTransformChange();
                    transform.hasChanged = false;
                }
            }
        }
        void UpdateMaterialParentTransformChange()
        {
            foreach (KeyValuePair<InstanceConfiguration, InstancingInfo> entry in InstancingDictionary)
            {
                var (_, instancingInfo) = entry;
                instancingInfo.materialPropertyBlock.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
            }
        }
        public abstract Dictionary<InstanceConfiguration, List<InstanceData>> GetInstanceData();
        void OnEnable()
        {
            var instanceData = GetInstanceData();
            EnableInstances(instanceData);
            // Update local position when starting
            if (InstancingDictionary != null && InstancingDictionary.Count > 0)
            {
                UpdateMaterialParentTransformChange();
            }
        }
        void EnableInstances(Dictionary<InstanceConfiguration, List<InstanceData>> instanceData)
        {
            static Dictionary<InstanceConfiguration, InstancingInfo> InitializeInstancingInfo(Dictionary<InstanceConfiguration, List<InstanceData>> instancingData)
            {
                var instacingInfo = new Dictionary<InstanceConfiguration, InstancingInfo>();
                foreach (var entry in instancingData)
                {
                    InstanceConfiguration toBeInstanced = entry.Key;
                    InstanceData[] data = entry.Value.ToArray();

                    if(data.Length == 0)
                    {
                        Debug.LogWarning("The generated data is empty for mesh: " + entry.Key.Mesh.name + ". Maybe the probability weight is too low?");
                        continue;
                    }

                    instacingInfo.Add(toBeInstanced, Instancing<InstanceData>.DataToInstancingInfo(toBeInstanced.Mesh, data, "_InstanceData"));
                }
                return instacingInfo;
            }

            if (drawInstances) { Debug.LogError("Instances are already enabled."); return; }
            if (instanceData == null || instanceData.Count == 0) { Debug.LogWarning("No objects to be instanced."); return; }
    ;
            InstancingDictionary = InitializeInstancingInfo(instanceData);

            drawInstances = InstancingDictionary.Count > 0;
        }
        void OnDisable()
        {
            DisableInstances();
        }
        void DisableInstances()
        {
            if (!drawInstances) { return; }

            Instancing<InstanceData>.ReleaseGPUMemory(InstancingDictionary.Values.ToArray());

            drawInstances = false;
        }
    }

}