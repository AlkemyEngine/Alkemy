using Unity.Collections;
using System.Collections;
using System.Collections.Generic;

public class VoxelDataStore {
    private static Dictionary<string, VoxelData> store = new Dictionary<string, VoxelData>();

    public static VoxelData SetVoxelData(string instanceID, VoxelData voxelData) {
        store.TryAdd(instanceID, voxelData);
        return store[instanceID];
    }

    public static VoxelData GetVoxelData(string instanceID) {
        if (store.ContainsKey(instanceID)) {
            return store[instanceID];
        }
        return null;
    }
}