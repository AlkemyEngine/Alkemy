using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VoxelMeshFilter : MonoBehaviour
{
    private MeshFilter meshFilter;
    private VoxelParser parser;
    private VoxelMesh voxelMesh;
    private VoxelMeshJob meshJob;
    private JobHandle meshJobHandle;
    private bool meshJobRunning = false;
    void Start() {
        parser = GetComponent<VoxelParser>();

        if (parser == null || parser.voxelData == null) return;

        if (parser.voxelData.IsReady()) {
            SetupVoxelMesh(parser.voxelData);
        } else {
            parser.voxelData.OnVoxelDataReady += SetupVoxelMesh;
        }
    }

    private void SetupVoxelMesh(VoxelData voxelData) {
        voxelMesh = VoxelMeshStore.GetVoxelMesh(voxelData.name);
        meshFilter = GetComponent<MeshFilter>();

        if (voxelMesh != null) {
            meshFilter.mesh = voxelMesh.mesh;
        } else {
            voxelMesh = VoxelMeshStore.SetVoxelMesh(voxelData.name, new VoxelMesh(1));
            voxelMesh.waitOptimization = true;

            meshJob = new VoxelMeshJob();
            meshJob.algorithm = MeshingAlgorithm.NAIVE;
            meshJob.voxels = new NativeArray<int>(voxelData.voxels, Allocator.Persistent);
            meshJob.origin = voxelData.origin;
            meshJob.width = voxelData.width;
            meshJob.height = voxelData.height;
            meshJob.depth = voxelData.depth;
            meshJob.vertices = new NativeList<Vector3>(Allocator.Persistent);
            meshJob.colors = new NativeList<Color32>(Allocator.Persistent);
            meshJob.triangles = new NativeList<int>(Allocator.Persistent);

            meshJobHandle = meshJob.Schedule(parser.parserJobHandle);
            meshJobRunning = true;
        }
    }

    void LateUpdate() {
        /**
         * If no optimisation or meshing is currently running on the mesh
         * we start the optimisation process by incrementing the algorithm level that is going to be used
         * and creating a new job to optimise the mesh.
         */
        if (voxelMesh != null && !voxelMesh.waitOptimization && !meshJobRunning && voxelMesh.optimizationLevel < ((int)MeshingAlgorithm.DONE - 1)) {
            voxelMesh.waitOptimization = true; // Prevent other jobs from running on the mesh
            voxelMesh.optimizationLevel++; // Increment the optimisation level

            meshJob = new VoxelMeshJob();
            meshJob.algorithm = (MeshingAlgorithm)voxelMesh.optimizationLevel; // Select the proper algorithm based on the optimisation level
            meshJob.voxels = new NativeArray<int>(parser.voxelData.voxels, Allocator.Persistent);
            meshJob.origin = parser.voxelData.origin;
            meshJob.width = parser.voxelData.width;
            meshJob.height = parser.voxelData.height;
            meshJob.depth = parser.voxelData.depth;
            meshJob.vertices = new NativeList<Vector3>(Allocator.Persistent);
            meshJob.colors = new NativeList<Color32>(Allocator.Persistent);
            meshJob.triangles = new NativeList<int>(Allocator.Persistent);

            meshJobHandle = meshJob.Schedule(parser.parserJobHandle);
            meshJobRunning = true;
        }

        /**
         * Wait for the current running job to be done
         * and then update the mesh information with the results of the job
         */
        if (meshJobRunning && meshJobHandle.IsCompleted) {
            meshJobHandle.Complete();

            voxelMesh.waitOptimization = false;
            meshJobRunning = false;

            voxelMesh.mesh.Clear();
            voxelMesh.mesh.SetVertices(meshJob.vertices.ToArray());
            voxelMesh.mesh.SetColors(meshJob.colors.ToArray());
            voxelMesh.mesh.SetTriangles(meshJob.triangles.ToArray(), 0);
            voxelMesh.mesh.Optimize();
            voxelMesh.mesh.RecalculateNormals();
            voxelMesh.mesh.RecalculateBounds();


            meshFilter.mesh = voxelMesh.mesh;

            meshJob.voxels.Dispose();
            meshJob.vertices.Dispose();
            meshJob.colors.Dispose();
            meshJob.triangles.Dispose();
        }
    }
}
