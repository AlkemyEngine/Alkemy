using Unity.Jobs;
using UnityEngine;
using Unity.Collections;
using System;

public enum FaceDirection {
    UP,
    DOWN,
    LEFT,
    RIGHT,
    FRONT,
    BACK
}

public enum MeshingAlgorithm {
    NAIVE = 1,
    GREEDY = 2,
    DONE = 3, // Used to indicate that there is no more optimisation to be done
}

public struct VoxelMeshJob : IJob {
    [ReadOnly]
    public NativeArray<int> voxels;

    [ReadOnly]
    public Vector3 origin;

    [ReadOnly]
    public int width;

    [ReadOnly]
    public int height;

    [ReadOnly]
    public int depth;

    [ReadOnly]
    public MeshingAlgorithm algorithm;

    public NativeList<Vector3> vertices;
    public NativeList<int> triangles;
    public NativeList<Color32> colors;

    public void Execute() {
        DateTime start = DateTime.Now;
        if (algorithm == MeshingAlgorithm.NAIVE) {
            NaiveMeshing();
        } else if (algorithm == MeshingAlgorithm.GREEDY) {
            GreedyMeshing();
        }
    }

    void NaiveMeshing() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < depth; z++) {
                    int color = voxels[GetIndex(x, y, z)];
                    // Extract the Alpha channel from the color, if the alpha is 0, then there is no voxel
                    if ((color & 0xFF) == 0) continue;

                    // Only create a face if the adjacent voxel is empty
                    // otherwise the face will be hidden under the adjacent voxel
                    if (IsVoxelEmpty(x, y + 1, z)) PushFace(x - origin.x, y - origin.y, z - origin.z, color, FaceDirection.UP);
                    if (IsVoxelEmpty(x, y - 1, z)) PushFace(x - origin.x, y - origin.y, z - origin.z, color, FaceDirection.DOWN);
                    if (IsVoxelEmpty(x - 1, y, z)) PushFace(x - origin.x, y - origin.y, z - origin.z, color, FaceDirection.LEFT);
                    if (IsVoxelEmpty(x + 1, y, z)) PushFace(x - origin.x, y - origin.y, z - origin.z, color, FaceDirection.RIGHT);
                    if (IsVoxelEmpty(x, y, z - 1)) PushFace(x - origin.x, y - origin.y, z - origin.z, color, FaceDirection.BACK);
                    if (IsVoxelEmpty(x, y, z + 1)) PushFace(x - origin.x, y - origin.y, z - origin.z, color, FaceDirection.FRONT);
                }
            }
        }
    }

    void GreedyMeshing() {
        GenerateSlicedVoxelMesh(FaceDirection.UP);
        GenerateSlicedVoxelMesh(FaceDirection.DOWN);
        GenerateSlicedVoxelMesh(FaceDirection.LEFT);
        GenerateSlicedVoxelMesh(FaceDirection.RIGHT);
        GenerateSlicedVoxelMesh(FaceDirection.BACK);
        GenerateSlicedVoxelMesh(FaceDirection.FRONT);
    }

    Vector3Int ConvertToFaceCoordinate(int x, int y, int z, FaceDirection direction) {
        switch (direction) {
            case FaceDirection.UP:
            case FaceDirection.DOWN:
                return new Vector3Int(x, z, y);
            case FaceDirection.LEFT:
            case FaceDirection.RIGHT:
                return new Vector3Int(z, y, x);
            case FaceDirection.BACK:
            case FaceDirection.FRONT:
                return new Vector3Int(x, y, z);
            default:
                return Vector3Int.zero;
        }
    }

    bool IsVoxelOccluded(int x, int y, int z, FaceDirection direction) {
        switch (direction) {
            case FaceDirection.UP:
                return !IsVoxelEmpty(x, y + 1, z);
            case FaceDirection.DOWN:
                return !IsVoxelEmpty(x, y - 1, z);
            case FaceDirection.LEFT:
                return !IsVoxelEmpty(x - 1, y, z);
            case FaceDirection.RIGHT:
                return !IsVoxelEmpty(x + 1, y, z);
            case FaceDirection.BACK:
                return !IsVoxelEmpty(x, y, z - 1);
            case FaceDirection.FRONT:
                return !IsVoxelEmpty(x, y, z + 1);
            default:
                return false;
        }
    }

    void GenerateSlicedVoxelMesh(FaceDirection direction) {
        Vector3Int size = ConvertToFaceCoordinate(width, height, depth, direction);
        byte[] voxelsMask = new byte[size.x * size.y];

        /**
         * Slice the voxel data into 2D slices
         */
        for (int z = 0; z < size.z; z++) {
            Array.Fill<byte>(voxelsMask, 0);

            int faceWidth = 0;
            int faceHeight = 0;
            int color = 0;
            for (int x = 0; x < size.x; x++) {
                for (int y = 0; y < size.y; y++) {
                    Vector3Int pos = ConvertToFaceCoordinate(x, y, z, direction);
                    int index = x + y * size.x;
                    int globalIndex = GetIndex(pos.x, pos.y, pos.z);

                    /**
                     * Find the first voxel that has not been visited yet and that is not empty and not occluded
                     */
                    if (voxelsMask[index] == 0 && !IsVoxelEmpty(pos.x, pos.y, pos.z) && !IsVoxelOccluded(pos.x, pos.y, pos.z, direction)) {
                        color = voxels[globalIndex];
                        faceWidth = 1;
                        faceHeight = 1;

                        /**
                         * Expand along the Column as long as the voxel is not empty and has not been visited yet
                         * Compute the expanded height
                         */
                        for (int ny = y + 1; ny < size.y; ny++) {
                            Vector3Int npos = ConvertToFaceCoordinate(x, ny, z, direction);
                            int nindex = x + ny * size.x;
                            int nglobalIndex = GetIndex(npos.x, npos.y, npos.z);
                            if (voxelsMask[nindex] != 0 || voxels[nglobalIndex] != color || IsVoxelOccluded(npos.x, npos.y, npos.z, direction)) {
                                break;
                            }
                            faceHeight += 1;
                        }

                        /**
                         * Expand along the Rows as long as all the voxel in the column have the same color and have not been visited yet
                         */
                        for (int nx = x + 1; nx < size.x; nx++) {
                            bool columnValid = true;
                            for (int ny = y; ny < y + faceHeight; ny++) {
                                Vector3Int npos = ConvertToFaceCoordinate(nx, ny, z, direction);
                                int nindex = nx + ny * size.x;
                                int nglobalIndex = GetIndex(npos.x, npos.y, npos.z);
                                if (voxelsMask[nindex] != 0 || voxels[nglobalIndex] != color || IsVoxelOccluded(npos.x, npos.y, npos.z, direction)) {
                                    columnValid = false;
                                    break;
                                }
                            }

                            if (!columnValid) {
                                break;
                            }

                            faceWidth += 1;
                        }

                        /**
                         * Push the face to the mesh
                         */
                        if (faceWidth > 0 && faceHeight > 0) {
                            Vector3Int npos = ConvertToFaceCoordinate(x, y, z, direction);
                            PushFace(npos.x - origin.x, npos.y - origin.y, npos.z - origin.z, color, direction, faceWidth, faceHeight);
                            for (int nx = x; nx < x + faceWidth; nx++) {
                                for (int ny = y; ny < y + faceHeight; ny++) {
                                    int nindex = nx + ny * size.x;
                                    voxelsMask[nindex] = 1;
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    bool IsVoxelEmpty(int x, int y, int z) {
        // The voxel is empty if out of bounds
        if (x < 0 || x >= width) return true;
        if (y < 0 || y >= height) return true;
        if (z < 0 || z >= depth) return true;

        // Extract the Alpha channel from the color, if the alpha is 0 then the voxel is empty
        if ((voxels[GetIndex(x, y, z)] & 0xFF) == 0) return true;
        return false;
    }

    private int GetIndex(int x, int y, int z) {
        return x + y * width + z * width * height;
    }

    /**
     * Push a face into the mesh
     * Creates 4 vertices and 6 indices and 4 colors (one for each vertex)
     */
    private void PushFace(float x, float y, float z, int color, FaceDirection dir, int w = 1, int h = 1) {
        int start = vertices.Length;

        byte A = (byte)(color & 0xFF);
        byte R = (byte)((color >> 8) & 0xFF);
        byte G = (byte)((color >> 16) & 0xFF);
        byte B = (byte)((color >> 24) & 0xFF);

        switch (dir) {
            case FaceDirection.UP:
                vertices.Add(new Vector3(x, y + 1, z));
                vertices.Add(new Vector3(x + w, y + 1, z));
                vertices.Add(new Vector3(x, y + 1, z + h));
                vertices.Add(new Vector3(x + w, y + 1, z + h));
                break;
            case FaceDirection.DOWN:
                vertices.Add(new Vector3(x, y, z + h));
                vertices.Add(new Vector3(x + w, y, z + h));
                vertices.Add(new Vector3(x, y, z));
                vertices.Add(new Vector3(x + w, y, z));
                break;
            case FaceDirection.LEFT:
                vertices.Add(new Vector3(x, y + h, z));
                vertices.Add(new Vector3(x, y + h, z + w));
                vertices.Add(new Vector3(x, y, z));
                vertices.Add(new Vector3(x, y, z + w));
                break;
            case FaceDirection.RIGHT:
                vertices.Add(new Vector3(x + 1, y + h, z + w));
                vertices.Add(new Vector3(x + 1, y + h, z));
                vertices.Add(new Vector3(x + 1, y, z + w));
                vertices.Add(new Vector3(x + 1, y, z));
                break;
            case FaceDirection.BACK:
                vertices.Add(new Vector3(x + w, y + h, z));
                vertices.Add(new Vector3(x, y + h, z));
                vertices.Add(new Vector3(x + w, y, z));
                vertices.Add(new Vector3(x, y, z));
                break;
            case FaceDirection.FRONT:
                vertices.Add(new Vector3(x, y + h, z + 1));
                vertices.Add(new Vector3(x + w, y + h, z + 1));
                vertices.Add(new Vector3(x, y, z + 1));
                vertices.Add(new Vector3(x + w, y, z + 1));
                break;
            default:
                break;
        }

        colors.Add(new Color32(R, G, B, A));
        colors.Add(new Color32(R, G, B, A));
        colors.Add(new Color32(R, G, B, A));
        colors.Add(new Color32(R, G, B, A));

        triangles.Add(start);
        triangles.Add(start + 2);
        triangles.Add(start + 1);
        triangles.Add(start + 2);
        triangles.Add(start + 3);
        triangles.Add(start + 1);
    }
}