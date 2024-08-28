using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Environment.Instancing
{
    // If you a large amount of instances behaviours, they will all create a material instance and instancing call. This causes a performance hit.
    // This script calls each InstanceBehaviour in their child objects using their input variables and combines them into a single call.

    // As a drawback the parent node that the objects rotate and move with is not the gameobject but the parent instead.
    public class ParentInstancesBehaviour : InstancesBehaviour
    {
        public override Dictionary<InstanceConfiguration, List<InstanceData>> GetInstanceData()
        {
            var parentInstanceData = new Dictionary<InstanceConfiguration, List<InstanceData>>();
            var childrenAndThis = transform.GetComponentsInChildren<InstancesBehaviour>(true);

            if (childrenAndThis.Length <= 1)
            {
                Debug.LogWarning("The ParentInstancesBehaviour could not find any InstancesBehaviours in child objects.");
            }
            foreach (var child in childrenAndThis)
            {
                if (child == this)
                {
                    continue;
                }

                var instanceData = child.GetInstanceData();
                foreach (var (configuration, data) in instanceData)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        var instance = data[i];
                        instance.TRS = transform.worldToLocalMatrix * child.transform.localToWorldMatrix * instance.TRS;
                        data[i] = instance;
                    }
                    var found = parentInstanceData.Keys.FirstOrDefault(key => key.Material == configuration.Material && key.Mesh == configuration.Mesh);
                    if (found == null)
                    {
                        parentInstanceData[configuration] = data;
                    }
                    else
                    {
                        parentInstanceData[found].AddRange(data);
                    }
                }
                child.enabled = false;
            }

            return parentInstanceData;
        }
    }
}
