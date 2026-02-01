using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TreeGenerator : MonoBehaviour
{
    [SerializeField] private string axiom = "B";
    [SerializeField] private int iterations = 3;
    [SerializeField] private float segmentLength = 1f;
    [SerializeField] private Material treeMaterial;

    // Thickness parameters
    [SerializeField] private float initialRadius = 0.5f;
    [SerializeField] private float taperFactor = 0.9f; // Radius multiplier per segment
    [SerializeField] private float branchRadiusFactor = 0.7f; // Radius multiplier for new branches

    // Leaf parameters
    [SerializeField] private Material leafMaterial;
    [SerializeField] private float leafWidth = 0.25f;
    [SerializeField] private float leafLength = 0.4f;
    [SerializeField] private float leafDensity = 1.2f; // Leaves per segment
    [SerializeField] private float leafStartHeightNormalized = 0.5f; // 0 = trunk base height, 1 = highest point
    [SerializeField] private float leafRadiusOffset = 0.1f; // Push leaves away from branch surface
    [SerializeField] private float leafSizeVariation = 0.2f; // Range of size randomization (0-1)
    [SerializeField] private bool doubleSidedLeaves = true;

    // Branch connection parameters
    [SerializeField] private float branchConnectionExtrusionLength = 0.2f; // How far to extrude child ring back along parent

    // L-system parameters
    [SerializeField] private float growthProbability = 50f; // Probability (0-100) of branch growth
    [SerializeField] private float branchProbability = 50f; // Probability (0-100) of branch type selection
    [SerializeField] private float angleXMin = 15f; // Minimum angle for left rotation
    [SerializeField] private float angleXMax = 45f; // Maximum angle for left rotation
    [SerializeField] private float angleYMin = -30f; // Minimum angle for right rotation
    [SerializeField] private float angleYMax = 30f; // Maximum angle for right rotation

    private string expandedTree;
    private Stack<TransformInfoHelper> transformStack;

    // Get the Y position of the generator
    private float generatorY => transform.position.y;


    void Start()
    {
        expandedTree = axiom;
        ExpandTreeString();
        CreateMesh();
    }

    private void OnDrawGizmos()
    {
        if (string.IsNullOrEmpty(expandedTree))
            return;

        // Optional: Visualize tree structure in editor
        // foreach(List<Vector3> line in LineList)
        // {
        //     Gizmos.DrawLine(line[0], line[1]);
        // }
    }

    void ExpandTreeString()
    {
        StringBuilder expandedTreeBuilder = new StringBuilder(axiom);

        for (int i = 0; i < iterations; i++)
        {
            StringBuilder nextIteration = new StringBuilder();
            
            foreach (char character in expandedTreeBuilder.ToString())
            {
                switch (character)
                {
                    case 'F':
                        // Randomly decide between single or double forward segment
                        nextIteration.Append(Random.Range(0f, 100f) > growthProbability ? "F" : "FF");
                        break;

                    case 'B':
                        // Randomly choose branch configuration
                        nextIteration.Append(Random.Range(0f, 100f) > branchProbability 
                            ? "[llFB][rFB]" 
                            : "[lFB][rrFB]");
                        break;

                    case 'l': // Left rotation marker
                    case 'r': // Right rotation marker
                    case '[': // Push transform state
                    case ']': // Pop transform state
                        nextIteration.Append(character);
                        break;

                    default:
                        nextIteration.Append(character);
                        break;
                }
            }
            
            expandedTreeBuilder = nextIteration;
        }

        expandedTree = expandedTreeBuilder.ToString();
    }

    void CreateMesh()
    {
        // Validate material
        if (treeMaterial == null)
        {
            Debug.LogError("Tree Material is not assigned!");
            return;
        }

        // Create GameObject and components
        GameObject treeObject = new GameObject("Tree");
        var meshFilter = treeObject.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;
        var meshRenderer = treeObject.AddComponent<MeshRenderer>();
        meshRenderer.material = treeMaterial;

        // Initialize mesh data
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // Leaf mesh data
        List<Vector3> leafVertices = new List<Vector3>();
        List<int> leafTriangles = new List<int>();
        List<Vector2> leafUvs = new List<Vector2>();

        // Initialize stacks
        transformStack = new Stack<TransformInfoHelper>();
        Stack<float> radiusStack = new Stack<float>();
        Stack<List<(Vector3 pos, float radius)>> branchStack = new Stack<List<(Vector3 pos, float radius)>>();
        Stack<bool> firstSegmentStack = new Stack<bool>();
        Stack<Vector3> branchConnectionDirStack = new Stack<Vector3>();
        List<List<(Vector3 pos, float radius)>> allBranches = new List<List<(Vector3 pos, float radius)>>();

        // Create temporary transform for tree generation
        Transform treeTransform = treeObject.transform;
        treeTransform.position = transform.position;
        treeTransform.rotation = transform.rotation;

        float currentRadius = initialRadius;
        List<(Vector3 pos, float radius)> currentBranch = new List<(Vector3 pos, float radius)>();
        bool isFirstSegmentInBranch = false;
        Vector3 currentConnectionDirection = Vector3.zero;
        currentBranch.Add((treeTransform.position, currentRadius));

        // Process L-system string
        foreach (char instruction in expandedTree)
        {
            switch (instruction)
            {
                case 'F':
                    Vector3 prevPos = treeTransform.position;
                    treeTransform.Translate(Vector3.up * segmentLength);
                    currentConnectionDirection = (treeTransform.position - prevPos).normalized;
                    float endRadius = isFirstSegmentInBranch
                        ? currentRadius * branchRadiusFactor * taperFactor
                        : currentRadius * taperFactor;
                    currentRadius = endRadius;
                    currentBranch.Add((treeTransform.position, currentRadius));
                    isFirstSegmentInBranch = false;
                    break;

                case 'B':
                    // Branch marker (handled implicitly by [ and ])
                    break;

                case '[':
                    // Save current transform and radius state
                    transformStack.Push(new TransformInfoHelper()
                    {
                        position = treeTransform.position,
                        rotation = treeTransform.rotation
                    });
                    radiusStack.Push(currentRadius);
                    branchStack.Push(currentBranch);
                    firstSegmentStack.Push(isFirstSegmentInBranch);
                    branchConnectionDirStack.Push(currentConnectionDirection);
                    isFirstSegmentInBranch = true;
                    currentBranch = new List<(Vector3 pos, float radius)>();
                    currentBranch.Add((treeTransform.position, currentRadius));
                    break;

                case ']':
                    // Generate mesh for the branch
                    Vector3 parentConnectionDir = branchConnectionDirStack.Peek();
                    AddTube(vertices, triangles, uvs, currentBranch, 8, parentConnectionDir);
                    allBranches.Add(currentBranch);
                    // Restore transform and radius state
                    TransformInfoHelper savedTransform = transformStack.Pop();
                    treeTransform.position = savedTransform.position;
                    treeTransform.rotation = savedTransform.rotation;
                    currentRadius = radiusStack.Pop();
                    currentBranch = branchStack.Pop();
                    isFirstSegmentInBranch = firstSegmentStack.Pop();
                    currentConnectionDirection = branchConnectionDirStack.Pop();
                    break;

                case 'l':
                    // Left rotation
                    treeTransform.Rotate(Vector3.back, Random.Range(angleXMin, angleXMax));
                    treeTransform.Rotate(Vector3.up, Random.Range(angleYMin, angleYMax));
                    break;

                case 'r':
                    // Right rotation
                    treeTransform.Rotate(Vector3.forward, Random.Range(angleYMin, angleYMax));
                    treeTransform.Rotate(Vector3.up, Random.Range(angleYMin, angleYMax));
                    break;
            }
        }

        // Generate mesh for the main trunk
        AddTube(vertices, triangles, uvs, currentBranch, 8, Vector3.zero);
        allBranches.Add(currentBranch);

        // Assign mesh data
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Create leaves
        if (leafMaterial != null && leafDensity > 0f)
        {
            CreateLeaves(leafVertices, leafTriangles, leafUvs, allBranches);
            GameObject leafObject = new GameObject("Leaves");
            leafObject.transform.SetParent(treeObject.transform, false);
            var leafFilter = leafObject.AddComponent<MeshFilter>();
            var leafMesh = new Mesh();
            leafFilter.mesh = leafMesh;
            var leafRenderer = leafObject.AddComponent<MeshRenderer>();
            leafRenderer.material = leafMaterial;
            leafMesh.vertices = leafVertices.ToArray();
            leafMesh.triangles = leafTriangles.ToArray();
            leafMesh.uv = leafUvs.ToArray();
            leafMesh.RecalculateNormals();
            leafMesh.RecalculateBounds();
        }

        // Set tree object's Y position to generator's Y position
        treeObject.transform.position = new Vector3(treeObject.transform.position.x, generatorY, treeObject.transform.position.z);
    }

    private void AddTube(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<(Vector3 pos, float radius)> points, int segments, Vector3 parentConnectionDir)
    {
        if (points.Count < 2) return;

        int startVertexIndex = vertices.Count;
        int pointCount = points.Count;
        
        // If this is a child branch, add an extra ring extruded back along parent direction
        int ringOffset = 0;
        if (parentConnectionDir != Vector3.zero && branchConnectionExtrusionLength > 0f)
        {
            ringOffset = 1;
            Vector3 connectionPos = points[0].pos - parentConnectionDir * branchConnectionExtrusionLength;
            float radius = points[0].radius;
            
            // Generate the connection ring at the extruded position
            Vector3 perpendicular = Vector3.Cross(parentConnectionDir, Vector3.up).normalized;
            if (perpendicular == Vector3.zero) perpendicular = Vector3.Cross(parentConnectionDir, Vector3.right).normalized;
            
            for (int j = 0; j < segments; j++)
            {
                float angle = j * Mathf.PI * 2 / segments;
                Vector3 offset = (Mathf.Cos(angle) * perpendicular + Mathf.Sin(angle) * Vector3.Cross(parentConnectionDir, perpendicular)) * radius;
                vertices.Add(connectionPos + offset);
                uvs.Add(new Vector2(j / (float)segments, -0.1f));
            }
        }

        // Add all ring vertices for the main branch
        for (int i = 0; i < pointCount; i++)
        {
            var (pos, radius) = points[i];
            Vector3 direction;
            if (i < pointCount - 1)
            {
                direction = (points[i + 1].pos - pos).normalized;
            }
            else
            {
                direction = (pos - points[i - 1].pos).normalized;
            }
            
            // Use branch's own direction for all rings
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
            if (perpendicular == Vector3.zero) perpendicular = Vector3.Cross(direction, Vector3.right).normalized;

            for (int j = 0; j < segments; j++)
            {
                float angle = j * Mathf.PI * 2 / segments;
                Vector3 offset = (Mathf.Cos(angle) * perpendicular + Mathf.Sin(angle) * Vector3.Cross(direction, perpendicular)) * radius;
                vertices.Add(pos + offset);
                uvs.Add(new Vector2(j / (float)segments, (i + ringOffset) / (float)(pointCount + ringOffset - 1)));
            }
        }

        // Add triangles, including connection segment if present
        int totalRings = pointCount + ringOffset;
        for (int i = 0; i < totalRings - 1; i++)
        {
            int ringStart = startVertexIndex + i * segments;
            int nextRingStart = startVertexIndex + (i + 1) * segments;
            for (int j = 0; j < segments; j++)
            {
                int nextJ = (j + 1) % segments;
                // First triangle (reversed for outward normals)
                triangles.Add(ringStart + j);
                triangles.Add(nextRingStart + nextJ);
                triangles.Add(nextRingStart + j);
                // Second triangle
                triangles.Add(ringStart + j);
                triangles.Add(ringStart + nextJ);
                triangles.Add(nextRingStart + nextJ);
            }
        }
    }

    private void CreateLeaves(List<Vector3> leafVertices, List<int> leafTriangles, List<Vector2> leafUvs, List<List<(Vector3 pos, float radius)>> branches)
    {
        if (branches.Count == 0) return;

        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;
        foreach (var branch in branches)
        {
            foreach (var point in branch)
            {
                minY = Mathf.Min(minY, point.pos.y);
                maxY = Mathf.Max(maxY, point.pos.y);
            }
        }
        float heightRange = Mathf.Max(0.0001f, maxY - minY);

        foreach (var branch in branches)
        {
            if (branch.Count < 2) continue;

            int segmentCount = branch.Count - 1;
            for (int i = 0; i < segmentCount; i++)
            {
                float segmentHeightNormalized = (branch[i].pos.y - minY) / heightRange;
                if (segmentHeightNormalized < leafStartHeightNormalized) continue;

                float leavesForSegment = leafDensity;
                int leafCount = Mathf.FloorToInt(leavesForSegment);
                if (Random.value < (leavesForSegment - leafCount)) leafCount++;

                Vector3 start = branch[i].pos;
                Vector3 end = branch[i + 1].pos;
                Vector3 direction = (end - start).normalized;
                float radius = branch[i].radius;

                for (int l = 0; l < leafCount; l++)
                {
                    float along = Random.value;
                    Vector3 pos = Vector3.Lerp(start, end, along);

                    Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
                    if (perpendicular == Vector3.zero) perpendicular = Vector3.Cross(direction, Vector3.right).normalized;
                    Vector3 binormal = Vector3.Cross(direction, perpendicular).normalized;
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    Vector3 radial = (Mathf.Cos(angle) * perpendicular + Mathf.Sin(angle) * binormal).normalized;
                    pos += radial * (radius + leafRadiusOffset);

                    // Add rotation diversity
                    Quaternion rotation = Quaternion.LookRotation(radial, direction) * Quaternion.Euler(Random.Range(-45f, 45f), Random.Range(0f, 360f), Random.Range(-45f, 45f));
                    
                    // Add size variation
                    float sizeVariationMultiplier = 1f + Random.Range(-leafSizeVariation, leafSizeVariation);
                    float variedWidth = leafWidth * sizeVariationMultiplier;
                    float variedLength = leafLength * sizeVariationMultiplier;
                    
                    AddLeafQuad(leafVertices, leafTriangles, leafUvs, pos, rotation, variedWidth, variedLength, doubleSidedLeaves);
                }
            }
        }
    }

    private void AddLeafQuad(List<Vector3> leafVertices, List<int> leafTriangles, List<Vector2> leafUvs, Vector3 center, Quaternion rotation, float width, float length, bool doubleSided)
    {
        int startIndex = leafVertices.Count;
        float halfWidth = width * 0.5f;
        float halfLength = length * 0.5f;

        Vector3 right = rotation * Vector3.right * halfWidth;
        Vector3 up = rotation * Vector3.up * halfLength;

        leafVertices.Add(center - right - up);
        leafVertices.Add(center + right - up);
        leafVertices.Add(center + right + up);
        leafVertices.Add(center - right + up);

        leafUvs.Add(new Vector2(0f, 0f));
        leafUvs.Add(new Vector2(1f, 0f));
        leafUvs.Add(new Vector2(1f, 1f));
        leafUvs.Add(new Vector2(0f, 1f));

        leafTriangles.Add(startIndex + 0);
        leafTriangles.Add(startIndex + 1);
        leafTriangles.Add(startIndex + 2);
        leafTriangles.Add(startIndex + 0);
        leafTriangles.Add(startIndex + 2);
        leafTriangles.Add(startIndex + 3);

        if (doubleSided)
        {
            leafTriangles.Add(startIndex + 2);
            leafTriangles.Add(startIndex + 1);
            leafTriangles.Add(startIndex + 0);
            leafTriangles.Add(startIndex + 3);
            leafTriangles.Add(startIndex + 2);
            leafTriangles.Add(startIndex + 0);
        }
    }
}
