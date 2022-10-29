using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;

struct VoxelInfo {
    public byte x;
    public byte y;
    public byte z;
    public byte red;
    public byte green;
    public byte blue;
    public byte alpha;
}

public struct PlyVoxelParseJob : IJob {
    [ReadOnly]
    public NativeArray<char> fileData;

    public NativeArray<JobVoxelData> voxelData;
    public NativeList<int> voxels;

    public void Execute() {
        string text = new string(fileData.ToArray());
        JobVoxelData voxelData = new JobVoxelData();
        voxelData.canExplode = false;

        bool endHeader = false;
        List<string> properties = new List<string>();
        List<VoxelInfo> voxelsInfo = new List<VoxelInfo>();

        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;
        int minZ = int.MaxValue;
        int maxZ = int.MinValue;

        foreach (string line in text.Split('\n')) {
            if (line == "end_header") {
                endHeader = true;
                continue;
            }

            if (!endHeader) {
                string[] parts = line.Split(' ');
                switch (parts[0]) {
                    case "property":
                        properties.Add(parts[2]);
                        break;
                    case "explode":
                        voxelData.canExplode = true;
                        break;
                    case "origin":
                        voxelData.origin = new Vector3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
                        break;
                    default:
                        break;
                }
            } else if (line.Length > 0) {
                string[] parts = line.Split(' ');
                VoxelInfo voxelInfo = new VoxelInfo();
                voxelInfo.alpha = 255;
                for (int i = 0; i < parts.Length; i++) {
                    switch (properties[i]) {
                        case "x":
                            voxelInfo.x = byte.Parse(parts[i]);
                            minX = Mathf.Min(minX, voxelInfo.x);
                            maxX = Mathf.Max(maxX, voxelInfo.x);
                            break;
                        case "y":
                            voxelInfo.y = byte.Parse(parts[i]);
                            minY = Mathf.Min(minY, voxelInfo.y);
                            maxY = Mathf.Max(maxY, voxelInfo.y);
                            break;
                        case "z":
                            voxelInfo.z = byte.Parse(parts[i]);
                            minZ = Mathf.Min(minZ, voxelInfo.z);
                            maxZ = Mathf.Max(maxZ, voxelInfo.z);
                            break;
                        case "red":
                            voxelInfo.red = byte.Parse(parts[i]);
                            break;
                        case "green":
                            voxelInfo.green = byte.Parse(parts[i]);
                            break;
                        case "blue":
                            voxelInfo.blue = byte.Parse(parts[i]);
                            break;
                        case "alpha":
                            voxelInfo.alpha = byte.Parse(parts[i]);
                            break;
                    }
                }
                voxelsInfo.Add(voxelInfo);
            }
        }
        // Compute the size of the voxel grid
        voxelData.width = maxX - minX + 1;
        voxelData.height = maxY - minY + 1;
        voxelData.depth = maxZ - minZ + 1;

        // Resize the capacity of the voxel array to avoid reallocations
        voxels.Resize(voxelData.width * voxelData.height * voxelData.depth, NativeArrayOptions.ClearMemory);

        for (int i = 0; i < voxelsInfo.Count; i++) {
            VoxelInfo voxelInfo = voxelsInfo[i];
            int index = (voxelInfo.x - minX) + (voxelInfo.y - minY) * voxelData.width + (voxelInfo.z - minZ) * voxelData.width * voxelData.height;
            // Encode the color in the voxel
            voxels[index] = (voxelInfo.alpha) | (voxelInfo.red << 8) | (voxelInfo.green << 16) | (voxelInfo.blue << 24);
        }

        this.voxelData[0] = voxelData;
    }
}