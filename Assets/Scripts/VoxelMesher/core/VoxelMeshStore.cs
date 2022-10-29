using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using Unity.Collections;

public class VoxelMeshStore {
    private static Dictionary<string, VoxelMesh> store = new Dictionary<string, VoxelMesh>();

    public static VoxelMesh SetVoxelMesh(string instanceID, VoxelMesh mesh) {
        store.TryAdd(instanceID, mesh);
        return store[instanceID];
    }

    public static VoxelMesh GetVoxelMesh(string instanceID) {
        if (store.ContainsKey(instanceID)) {
            return store[instanceID];
        }
        return null;
    }
}