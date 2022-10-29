using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;


public class VoxelParser : MonoBehaviour
{
    public TextAsset file;

    public VoxelData voxelData;

    [System.NonSerialized]
    public JobHandle parserJobHandle;

    private bool parserRunning = false;
    private PlyVoxelParseJob parseJob;

    // Start is called before the first frame update
    void Start()
    {
        if (file == null) return;

        voxelData = VoxelDataStore.GetVoxelData(file.name);

        if (voxelData == null) {
            if (file.text.StartsWith("ply")) {
                voxelData = new VoxelData();
                voxelData.name = file.name;

                VoxelDataStore.SetVoxelData(file.name, voxelData);
                parseJob = new PlyVoxelParseJob();
                parseJob.fileData = new NativeArray<char>(file.text.ToCharArray(), Allocator.Persistent);
                parseJob.voxelData = new NativeArray<JobVoxelData>(1, Allocator.Persistent);
                parseJob.voxels = new NativeList<int>(Allocator.Persistent);
                parserJobHandle = parseJob.Schedule();
                parserRunning = true;
            }
        }
    }

    void LateUpdate() {
        if (parserRunning && parserJobHandle.IsCompleted) {
            parserJobHandle.Complete();
            parserRunning = false;

            voxelData.canExplode = parseJob.voxelData[0].canExplode;
            voxelData.origin = parseJob.voxelData[0].origin;
            voxelData.width = parseJob.voxelData[0].width;
            voxelData.height = parseJob.voxelData[0].height;
            voxelData.depth = parseJob.voxelData[0].depth;
            voxelData.voxels = parseJob.voxels.ToArray();

            parseJob.fileData.Dispose();
            parseJob.voxelData.Dispose();
            parseJob.voxels.Dispose();

            voxelData.MarkReady();
        }
    }
}
