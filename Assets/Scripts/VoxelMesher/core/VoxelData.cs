using UnityEngine;

public class VoxelData {
    public string name;
    public int[] voxels;
    public int width;
    public int height;
    public int depth;
    public Vector3 origin;
    public bool canExplode;

    private bool _ready = false;

    public delegate void VoxelDataReady(VoxelData self);
    public event VoxelDataReady OnVoxelDataReady;

    public void MarkReady() {
        if (!_ready) {
            _ready = true;
            OnVoxelDataReady.Invoke(this);
        }
    }

    public bool IsReady() {
        return _ready;
    }
}

public struct JobVoxelData {
    public int width;
    public int height;
    public int depth;
    public Vector3 origin;
    public bool canExplode;
}