using UnityEngine;

public class VoxelMesh {
    public Mesh mesh;
    public int optimizationLevel;
    public bool waitOptimization = false;

    public VoxelMesh(int optimizationLevel = 0) {
        mesh = new Mesh();
        this.optimizationLevel = optimizationLevel;
    }

    public VoxelMesh(Mesh mesh, int optimizationLevel = 0) {
        this.mesh = mesh;
        this.optimizationLevel = optimizationLevel;
    }
}