using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TreeGenerator : MonoBehaviour
{
    // Constants
    private const int DefaultSegments = 8;
    private const float ClusterProximityRadiusMultiplier = 3f;
    private const float DensityNormalizationFactor = 10f;
    private const float ProximitySizeWeight = 0.7f;
    private const float RandomSizeVariation = 0.1f;
    private const int MaxProximityBranchCount = 8;
    private const int TransparentRenderQueue = 3000;
    private const float TransparentModeValue = 3f;
    private const int ClusterSeedMultiplier = 7919;
    private const int OuterShellSeedOffset = 1000;
    private const float OuterShellNoiseMultiplier = 1.5f;
    private const float OuterShellNoiseScale = 0.8f;

    public enum LeafGenerationMode
    {
        Planes,  // Traditional quad-based leaves
        Clusters     // Spherical leaf clusters around branch groups
    }

    private struct BranchPoint
    {
        public Vector3 pos;
        public float radius;

        public BranchPoint(Vector3 pos, float radius)
        {
            this.pos = pos;
            this.radius = radius;
        }
    }

    [Header("Tree Materials")]
    [Tooltip("Material applied to the tree trunk and branches")]
    [SerializeField] private Material barkMaterial;
    [Tooltip("Material applied to the leaves")]
    [SerializeField] private Material leafMaterial;
    [Tooltip("How many times the bark texture repeats around the branch circumference")]
    [Range(0.1f, 10f)]
    [SerializeField] private float barkTilingHorizontal = 1f;
    [Tooltip("How many times the bark texture repeats per unit of branch length")]
    [Range(0.1f, 10f)]
    [SerializeField] private float barkTilingVertical = 1f;
    [Tooltip("Random UV offset per branch to break up repetition (0 = none, 1 = full randomization)")]
    [Range(0f, 1f)]
    [SerializeField] private float barkUVRandomness = 0.3f;
    [Tooltip("Noise-based UV distortion strength to add organic variation")]
    [Range(0f, 0.5f)]
    [SerializeField] private float barkUVNoiseStrength = 0.1f;
    [Tooltip("Scale of the noise pattern used for UV distortion")]
    [Range(0.1f, 5f)]
    [SerializeField] private float barkUVNoiseScale = 1f;

    [Header("Tree Generation")]
    [Tooltip("Starting symbol for the L-System (B = Branch, F = Forward). Leave as 'FB' for standard trees")]
    [SerializeField] private string lSystemSeed = "B";
    [Tooltip("Number of times to expand the tree structure. Higher values create more complex trees")]
    [Range(1, 10)]
    [SerializeField] private int complexity = 3;
    [Tooltip("Length of each branch segment in units")]
    [Range(0.1f, 5f)]
    [SerializeField] private float segmentLength = 1f;

    [Header("Thickness")]
    [Tooltip("Starting thickness of the tree trunk at the base")]
    [Range(0.1f, 2f)]
    [SerializeField] private float baseThickness = 0.5f;
    [Tooltip("How much branches thin out per segment (1.0 = no thinning, 0.5 = rapid thinning)")]
    [Range(0.5f, 1.0f)]
    [SerializeField] private float branchThinningRate = 0.9f; // Radius multiplier per segment
    [Tooltip("How much thinner child branches are compared to their parent branch")]
    [Range(0.3f, 0.95f)]
    [SerializeField] private float childBranchThickness = 0.7f; // Radius multiplier for new branches

    [Header("Leaves")]
    [Tooltip("Style of foliage generation")]
    [SerializeField] private LeafGenerationMode leafMode = LeafGenerationMode.Clusters;
    [Tooltip("Width of each plane leaf quad (Plane mode only)")]
    [Range(0.05f, 10f)]
    [SerializeField] private float leafWidth = 0.25f;
    [Tooltip("Length of each plane leaf quad (Plane mode only)")]
    [Range(0.05f, 10f)]
    [SerializeField] private float leafLength = 0.4f;
    [Tooltip("Average number of leaves per branch segment (Plane mode) or leaf cluster density (Cluster mode)")]
    [Range(0f, 20f)]
    [SerializeField] private float leafDensity = 1.2f;
    [Tooltip("Where leaves start appearing on the tree (0 = bottom, 1 = top only)")]
    [Range(0f, 1f)]
    [SerializeField] private float leafStartHeight = 0.5f;
    [Tooltip("Random variation in leaf/cluster sizes (0 = uniform, 1 = highly varied)")]
    [Range(0f, 1f)]
    [SerializeField] private float leafSizeVariation = 0.2f;
    [Tooltip("Makes plane leaves visible from both sides (Plane mode only)")]
    [SerializeField] private bool doubleSidedLeaves = false;
    [Tooltip("How far plane leaves extend from the branch surface (Plane mode only)")]
    [Range(-0.5f, 1f)]
    [SerializeField] private float leafDistanceFromBranch = 0.1f;
    
    [Header("Leaf Appearance")]
    [Tooltip("Transparency of the leaf material (0 = fully transparent, 1 = fully opaque). Note: Material must support transparency")]
    [Range(0f, 1f)]
    [SerializeField] private float leafTransparency = 1f;
    [Tooltip("Random UV offset per leaf/cluster to break up repetition (0 = none, 1 = full randomization)")]
    [Range(0f, 1f)]
    [SerializeField] private float leafUVRandomness = 0.2f;
    [Tooltip("Noise-based UV distortion strength to add organic variation to leaves")]
    [Range(0f, 0.5f)]
    [SerializeField] private float leafUVNoiseStrength = 0.05f;
    [Tooltip("Scale of the noise pattern used for leaf UV distortion")]
    [Range(0.1f, 5f)]
    [SerializeField] private float leafUVNoiseScale = 2f;
    
    [Header("Leaf Clusters (Cluster Mode)")]
    [Tooltip("Base size of each spherical leaf cluster")]
    [Range(0.2f, 10f)]
    [SerializeField] private float clusterRadius = 0.8f;
    [Tooltip("Minimum cluster size as a multiplier of base radius (for randomization)")]
    [Range(0.5f, 1f)]
    [SerializeField] private float clusterSizeMin = 0.8f;
    [Tooltip("Maximum cluster size as a multiplier of base radius (for randomization)")]
    [Range(1f, 2f)]
    [SerializeField] private float clusterSizeMax = 1.2f;
    [Tooltip("Horizontal stretch of cluster shape (1 = sphere, <1 = compressed, >1 = stretched)")]
    [Range(0.5f, 2f)]
    [SerializeField] private float clusterShapeX = 1.2f;
    [Tooltip("Vertical stretch of cluster shape (1 = sphere, <1 = flattened, >1 = elongated)")]
    [Range(0.5f, 2f)]
    [SerializeField] private float clusterShapeY = 0.8f;
    [Tooltip("Depth stretch of cluster shape (1 = sphere, <1 = compressed, >1 = stretched)")]
    [Range(0.5f, 2f)]
    [SerializeField] private float clusterShapeZ = 1.1f;
    [Tooltip("Add organic irregularity to cluster surface (0 = perfect ellipsoid, higher = more bumpy)")]
    [Range(0f, 0.5f)]
    [SerializeField] private float clusterNoiseStrength = 0.15f;
    [Tooltip("Scale of the noise pattern for cluster irregularity")]
    [Range(0.5f, 5f)]
    [SerializeField] private float clusterNoiseScale = 2f;
    [Tooltip("Add a transparent outer shell layer for depth and texture")]
    [SerializeField] private bool enableOuterShell = true;
    [Tooltip("Thickness of the outer transparent shell as a multiplier of cluster radius")]
    [Range(1.05f, 1.5f)]
    [SerializeField] private float outerShellThickness = 1.2f;
    [Tooltip("Transparency of the outer shell layer (lower = more transparent)")]
    [Range(0f, 1f)]
    [SerializeField] private float outerShellTransparency = 0.3f;
    [Tooltip("Number of quads per cluster sphere (higher = smoother but more expensive)")]
    [Range(4, 32)]
    [SerializeField] private int clusterSegments = 12;
    [Tooltip("How many times to tile the texture on clusters (higher = more leaves visible)")]
    [Range(0.5f, 10f)]
    [SerializeField] private float clusterTextureTiling = 1f;
    [Tooltip("Randomize cluster rotation for more natural variation")]
    [SerializeField] private bool randomizeClusterRotation = true;
    [Tooltip("How far clusters extend from branch tips")]
    [Range(-1f, 2f)]
    [SerializeField] private float clusterOffset = 0.3f;
    [Tooltip("Minimum branches needed in an area to create a cluster")]
    [Range(1, 5)]
    [SerializeField] private int minBranchesPerCluster = 1;
    
    [Header("Leaf Performance")]
    [Tooltip("Maximum number of leaf elements to generate (quads or clusters). Lower values improve performance (0 = unlimited)")]
    [Range(0, 10000)]
    [SerializeField] private int maxLeafCount = 2000;
    [Tooltip("Reduce leaf/cluster density on thinner branches for better performance")]
    [SerializeField] private bool optimizeLeafDistribution = true;
    [Tooltip("Minimum branch radius to generate leaves (helps reduce leaves on tiny branches)")]
    [Range(0f, 0.5f)]
    [SerializeField] private float minBranchRadiusForLeaves = 0.05f;

    [Header("Branch Connection")]
    [Tooltip("How far branches extend back into their parent branch for smoother connections")]
    [Range(0f, 1f)]
    [SerializeField] private float branchBlendDistance = 0.2f; // How far to extrude child ring back along parent

    [Header("Advanced Growth Parameters")]
    [Tooltip("Probability (0-100) that branch segments grow longer. Higher values create taller trees")]
    [Range(0f, 100f)]
    [SerializeField] private float segmentGrowthChance = 50f; // Probability (0-100) of branch growth
    [Tooltip("Probability (0-100) affecting branch pattern distribution. Experiment for different shapes")]
    [Range(0f, 100f)]
    [SerializeField] private float branchPatternVariation = 50f; // Probability (0-100) of branch type selection
    [Tooltip("Minimum angle for branch rotation (affects how spread out branches are)")]
    [Range(0f, 90f)]
    [SerializeField] private float minBranchAngle = 15f; // Minimum angle for left rotation
    [Tooltip("Maximum angle for branch rotation (affects how spread out branches are)")]
    [Range(0f, 90f)]
    [SerializeField] private float maxBranchAngle = 45f; // Maximum angle for left rotation
    [Tooltip("Minimum vertical angle for branches (negative = downward, positive = upward)")]
    [Range(-180f, 0f)]
    [SerializeField] private float minVerticalAngle = -30f; // Minimum angle for right rotation
    [Tooltip("Maximum vertical angle for branches (negative = downward, positive = upward)")]
    [Range(0f, 180f)]
    [SerializeField] private float maxVerticalAngle = 30f; // Maximum angle for right rotation

    private string expandedTree;
    private Stack<TransformInfoHelper> transformStack;
    private GameObject generatedTree; // Reference to the tree created by this generator

    // Get the Y position of the generator
    private float generatorY => transform.position.y;


    void Start()
    {
        // Only generate a tree if one doesn't already exist
        if (transform.Find("Tree") == null)
        {
            RegenerateTree();
        }
    }

    public void RegenerateTree()
    {
        ClearTree();
        expandedTree = lSystemSeed;
        ExpandTreeString();
        CreateMesh();
    }

    private void ClearTree()
    {
        // Destroy the tree generated by this specific generator
        if (generatedTree != null)
        {
            if (Application.isPlaying)
            {
                Destroy(generatedTree);
            }
            else
            {
                DestroyImmediate(generatedTree);
            }
            generatedTree = null;
        }
        else
        {
            // Fallback: Check for any existing "Tree" child (handles play mode transitions)
            Transform existingTree = transform.Find("Tree");
            if (existingTree != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(existingTree.gameObject);
                }
                else
                {
                    DestroyImmediate(existingTree.gameObject);
                }
            }
        }
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
        StringBuilder expandedTreeBuilder = new StringBuilder(lSystemSeed);

        for (int i = 0; i < complexity; i++)
        {
            StringBuilder nextIteration = new StringBuilder();
            
            foreach (char character in expandedTreeBuilder.ToString())
            {
                switch (character)
                {
                    case 'F':
                        // Randomly decide between single or double forward segment
                        nextIteration.Append(Random.Range(0f, 100f) > segmentGrowthChance ? "F" : "FF");
                        break;

                    case 'B':
                        // Randomly choose branch configuration
                        nextIteration.Append(Random.Range(0f, 100f) > branchPatternVariation 
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
        if (barkMaterial == null)
        {
            Debug.LogError("Tree Material is not assigned!");
            return;
        }

        // Create GameObject and components
        GameObject treeObject = CreateMeshObject("Tree", barkMaterial, null, out Mesh mesh);
        generatedTree = treeObject; // Store reference to this generator's tree

        // Initialize mesh data
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // Leaf mesh data
        List<Vector3> leafVertices = new List<Vector3>();
        List<int> leafTriangles = new List<int>();
        List<Vector2> leafUvs = new List<Vector2>();
        List<Color> leafColors = new List<Color>(); // For transparency

        // Initialize stacks
        transformStack = new Stack<TransformInfoHelper>();
        Stack<float> radiusStack = new Stack<float>();
        Stack<List<BranchPoint>> branchStack = new Stack<List<BranchPoint>>();
        Stack<bool> firstSegmentStack = new Stack<bool>();
        Stack<Vector3> branchConnectionDirStack = new Stack<Vector3>();
        List<List<BranchPoint>> allBranches = new List<List<BranchPoint>>();
        int branchCounter = 0; // For unique branch seeds

        // Create temporary transform for tree generation
        Transform treeTransform = treeObject.transform;
        treeTransform.position = transform.position;
        treeTransform.rotation = transform.rotation;

        float currentRadius = baseThickness;
        List<BranchPoint> currentBranch = new List<BranchPoint>();
        bool isFirstSegmentInBranch = false;
        Vector3 currentConnectionDirection = Vector3.zero;
        currentBranch.Add(new BranchPoint(treeTransform.position, currentRadius));

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
                        ? currentRadius * childBranchThickness * branchThinningRate
                        : currentRadius * branchThinningRate;
                    currentRadius = endRadius;
                    currentBranch.Add(new BranchPoint(treeTransform.position, currentRadius));
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
                    currentBranch = new List<BranchPoint>();
                    currentBranch.Add(new BranchPoint(treeTransform.position, currentRadius));
                    break;

                case ']':
                    // Generate mesh for the branch
                    Vector3 parentConnectionDir = branchConnectionDirStack.Peek();
                    AddTube(vertices, triangles, uvs, currentBranch, DefaultSegments, parentConnectionDir, branchCounter++);
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
                    // Left rotation - rotate around z-axis for horizontal spread
                    treeTransform.Rotate(Vector3.back, Random.Range(minBranchAngle, maxBranchAngle));
                    // Add vertical tilt and depth rotation for 3D branching
                    treeTransform.Rotate(Vector3.up, Random.Range(minVerticalAngle, maxVerticalAngle));
                    treeTransform.Rotate(Vector3.right, Random.Range(-15f, 15f)); // Add depth variation
                    break;

                case 'r':
                    // Right rotation - rotate around z-axis for horizontal spread
                    treeTransform.Rotate(Vector3.forward, Random.Range(minBranchAngle, maxBranchAngle));
                    // Add vertical tilt and depth rotation for 3D branching
                    treeTransform.Rotate(Vector3.up, Random.Range(minVerticalAngle, maxVerticalAngle));
                    treeTransform.Rotate(Vector3.right, Random.Range(-15f, 15f)); // Add depth variation
                    break;
            }
        }

        // Generate mesh for the main trunk
        AddTube(vertices, triangles, uvs, currentBranch, DefaultSegments, Vector3.zero, branchCounter++);
        allBranches.Add(currentBranch);

        // Assign mesh data
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Create leaves based on selected mode
        if (leafMaterial != null && leafDensity > 0f)
        {
            if (leafMode == LeafGenerationMode.Clusters)
            {
                CreateLeafClusters(leafVertices, leafTriangles, leafUvs, leafColors, allBranches);
            }
            else
            {
                CreatePlaneLeaves(leafVertices, leafTriangles, leafUvs, leafColors, allBranches);
            }
            GameObject leafObject = CreateMeshObject("Leaves", leafMaterial, treeObject.transform, out Mesh leafMesh);
            leafMesh.vertices = leafVertices.ToArray();
            leafMesh.triangles = leafTriangles.ToArray();
            leafMesh.uv = leafUvs.ToArray();
            leafMesh.colors = leafColors.ToArray();
            leafMesh.RecalculateNormals();
            leafMesh.RecalculateBounds();
            
            // Configure material transparency
            ConfigureLeafMaterialTransparency(leafObject);
        }

        // Make tree a child of the generator and set position
        treeObject.transform.SetParent(transform, true);
        treeObject.transform.position = new Vector3(treeObject.transform.position.x, generatorY, treeObject.transform.position.z);
    }

    private void ConfigureLeafMaterialTransparency(GameObject leafObject)
    {
        MeshRenderer leafRenderer = leafObject.GetComponent<MeshRenderer>();
        if (leafRenderer == null || leafRenderer.sharedMaterial == null)
            return;
            
        // Create a material instance (avoid leaks in edit mode)
        Material leafMatInstance = new Material(leafRenderer.sharedMaterial);
        leafRenderer.sharedMaterial = leafMatInstance;
        
        // Set transparency via material properties
        if (leafMatInstance.HasProperty("_Color"))
        {
            Color matColor = leafMatInstance.GetColor("_Color");
            matColor.a = leafTransparency;
            leafMatInstance.SetColor("_Color", matColor);
        }
        
        // Enable transparent rendering mode if needed
        if (leafTransparency < 1f)
        {
            SetupTransparentRenderMode(leafMatInstance);
        }
    }

    private void SetupTransparentRenderMode(Material material)
    {
        material.SetFloat("_Mode", TransparentModeValue);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = TransparentRenderQueue;
    }

    private (float minY, float maxY, float heightRange) CalculateHeightRange(List<List<BranchPoint>> branches)
    {
        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;
        
        foreach (var branch in branches)
        {
            foreach (BranchPoint point in branch)
            {
                minY = Mathf.Min(minY, point.pos.y);
                maxY = Mathf.Max(maxY, point.pos.y);
            }
        }
        
        float heightRange = Mathf.Max(0.0001f, maxY - minY);
        return (minY, maxY, heightRange);
    }

    private void AddTube(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<BranchPoint> points, int segments, Vector3 parentConnectionDir, int branchSeed)
    {
        if (points.Count < 2) return;

        int startVertexIndex = vertices.Count;
        int pointCount = points.Count;
        
        // Calculate cumulative distances along the branch for proper UV mapping
        float[] cumulativeDistances = new float[pointCount];
        cumulativeDistances[0] = 0f;
        for (int i = 1; i < pointCount; i++)
        {
            cumulativeDistances[i] = cumulativeDistances[i - 1] + Vector3.Distance(points[i - 1].pos, points[i].pos);
        }
        float totalLength = cumulativeDistances[pointCount - 1];
        
        // If this is a child branch, add an extra ring extruded back along parent direction
        int ringOffset = 0;
        float connectionDistance = 0f;
        if (parentConnectionDir != Vector3.zero && branchBlendDistance > 0f)
        {
            ringOffset = 1;
            connectionDistance = branchBlendDistance;
            Vector3 connectionPos = points[0].pos - parentConnectionDir * branchBlendDistance;
            float radius = points[0].radius;
            
            // Generate the connection ring at the extruded position
            Vector3 perpendicular = GetPerpendicular(parentConnectionDir);
            
            for (int j = 0; j < segments; j++)
            {
                float angle = j * Mathf.PI * 2 / segments;
                Vector3 offset = (Mathf.Cos(angle) * perpendicular + Mathf.Sin(angle) * Vector3.Cross(parentConnectionDir, perpendicular)) * radius;
                Vector3 vertexWorldPos = connectionPos + offset;
                vertices.Add(vertexWorldPos);
                
                float u = (j / (float)segments) * barkTilingHorizontal;
                float v = (-connectionDistance / totalLength) * totalLength * barkTilingVertical;
                Vector2 baseUV = new Vector2(u, v);
                
                // Apply texture variation
                Vector2 finalUV = BarkTextureUtility.ApplyAllUVVariation(
                    baseUV,
                    vertexWorldPos,
                    branchSeed,
                    barkUVRandomness,
                    barkUVNoiseScale,
                    barkUVNoiseStrength
                );
                uvs.Add(finalUV);
            }
        }

        // Add all ring vertices for the main branch
        for (int i = 0; i < pointCount; i++)
        {
            BranchPoint point = points[i];
            Vector3 pos = point.pos;
            float radius = point.radius;
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
            Vector3 perpendicular = GetPerpendicular(direction);

            for (int j = 0; j < segments; j++)
            {
                float angle = j * Mathf.PI * 2 / segments;
                Vector3 offset = (Mathf.Cos(angle) * perpendicular + Mathf.Sin(angle) * Vector3.Cross(direction, perpendicular)) * radius;
                Vector3 vertexWorldPos = pos + offset;
                vertices.Add(vertexWorldPos);
                
                // Calculate UVs based on actual distance and circumference
                float u = (j / (float)segments) * barkTilingHorizontal;
                float v = (cumulativeDistances[i] / totalLength) * totalLength * barkTilingVertical;
                Vector2 baseUV = new Vector2(u, v);
                
                // Apply texture variation
                Vector2 finalUV = BarkTextureUtility.ApplyAllUVVariation(
                    baseUV,
                    vertexWorldPos,
                    branchSeed,
                    barkUVRandomness,
                    barkUVNoiseScale,
                    barkUVNoiseStrength
                );
                uvs.Add(finalUV);
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

        // Add end cap to close off the branch
        BranchPoint lastPoint = points[pointCount - 1];
        int centerVertexIndex = vertices.Count;
        vertices.Add(lastPoint.pos);
        uvs.Add(new Vector2(0.5f, 0.5f));

        int lastRingStart = startVertexIndex + (totalRings - 1) * segments;
        for (int j = 0; j < segments; j++)
        {
            int nextJ = (j + 1) % segments;
            triangles.Add(centerVertexIndex);
            triangles.Add(lastRingStart + j);
            triangles.Add(lastRingStart + nextJ);
        }
    }

    private void CreatePlaneLeaves(List<Vector3> leafVertices, List<int> leafTriangles, List<Vector2> leafUvs, List<Color> leafColors, List<List<BranchPoint>> branches)
    {
        if (branches.Count == 0) return;

        (float minY, float maxY, float heightRange) = CalculateHeightRange(branches);
        
        // Track total leaf count for performance limiting
        int totalLeavesGenerated = 0;
        bool reachedMaxLeaves = false;
        int leafSeed = 0; // For UV randomization

        foreach (var branch in branches)
        {
            if (branch.Count < 2) continue;
            if (reachedMaxLeaves) break;

            int segmentCount = branch.Count - 1;
            for (int i = 0; i < segmentCount; i++)
            {
                if (reachedMaxLeaves) break;
                
                float segmentHeightNormalized = (branch[i].pos.y - minY) / heightRange;
                if (segmentHeightNormalized < leafStartHeight) continue;
                
                float radius = branch[i].radius;
                
                // Skip leaves on very thin branches for performance
                if (radius < minBranchRadiusForLeaves) continue;
                
                // Calculate leaf density based on branch thickness if optimization is enabled
                float densityMultiplier = 1f;
                if (optimizeLeafDistribution)
                {
                    // Reduce leaves on thinner branches (radius-based scaling)
                    densityMultiplier = Mathf.Clamp01(radius / baseThickness);
                }

                float leavesForSegment = leafDensity * densityMultiplier;
                int leafCount = Mathf.FloorToInt(leavesForSegment);
                if (Random.value < (leavesForSegment - leafCount)) leafCount++;
                
                // Check max leaf limit
                if (maxLeafCount > 0 && totalLeavesGenerated + leafCount > maxLeafCount)
                {
                    leafCount = maxLeafCount - totalLeavesGenerated;
                    reachedMaxLeaves = true;
                }

                Vector3 start = branch[i].pos;
                Vector3 end = branch[i + 1].pos;
                Vector3 direction = (end - start).normalized;
                float branchRadius = branch[i].radius;

                for (int l = 0; l < leafCount; l++)
                {
                    float along = Random.value;
                    Vector3 pos = Vector3.Lerp(start, end, along);

                    Vector3 perpendicular = GetPerpendicular(direction);
                    Vector3 binormal = Vector3.Cross(direction, perpendicular).normalized;
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    Vector3 radial = (Mathf.Cos(angle) * perpendicular + Mathf.Sin(angle) * binormal).normalized;
                    pos += radial * (branchRadius + leafDistanceFromBranch);

                    // Add rotation diversity
                    Quaternion rotation = Quaternion.LookRotation(radial, direction) * Quaternion.Euler(Random.Range(-45f, 45f), Random.Range(0f, 360f), Random.Range(-45f, 45f));
                    
                    // Add size variation
                    float sizeVariationMultiplier = 1f + Random.Range(-leafSizeVariation, leafSizeVariation);
                    float variedWidth = leafWidth * sizeVariationMultiplier;
                    float variedLength = leafLength * sizeVariationMultiplier;
                    
                    AddLeafQuad(leafVertices, leafTriangles, leafUvs, leafColors, pos, rotation, variedWidth, variedLength, doubleSidedLeaves, leafSeed++);
                    totalLeavesGenerated++;
                }
            }
        }
    }

    private void CreateLeafClusters(List<Vector3> leafVertices, List<int> leafTriangles, List<Vector2> leafUvs, List<Color> leafColors, List<List<BranchPoint>> branches)
    {
        if (branches.Count == 0) return;

        (float minY, float maxY, float heightRange) = CalculateHeightRange(branches);

        // Collect branch endpoints that qualify for clusters with nearby branch counts
        List<Vector3> clusterPositions = new List<Vector3>();
        List<int> nearbyBranchCounts = new List<int>(); // Track how many branches are near each cluster
        int clusterSeed = 0; // For UV randomization
        
        // First pass: collect all potential cluster positions
        List<Vector3> allEndpoints = new List<Vector3>();
        
        foreach (var branch in branches)
        {
            if (branch.Count < 2) continue;
            
            // Get the endpoint of this branch
            BranchPoint endpoint = branch[branch.Count - 1];
            float endpointHeight = (endpoint.pos.y - minY) / heightRange;
            
            // Check if this endpoint qualifies for a cluster
            if (endpointHeight >= leafStartHeight && endpoint.radius >= minBranchRadiusForLeaves)
            {
                // Position cluster to cover the branch endpoint
                // Offset inward slightly to ensure branch tip is inside the cluster
                Vector3 branchDir = (branch[branch.Count - 1].pos - branch[branch.Count - 2].pos).normalized;
                Vector3 clusterPos = endpoint.pos - branchDir * (clusterRadius * 0.2f) + branchDir * clusterOffset;
                allEndpoints.Add(clusterPos);
            }
        }
        
        // Calculate center of all endpoints for distance-based sizing
        Vector3 treeCenter = Vector3.zero;
        if (allEndpoints.Count > 0)
        {
            foreach (Vector3 pos in allEndpoints)
            {
                treeCenter += pos;
            }
            treeCenter /= allEndpoints.Count;
        }
        
        // Second pass: count nearby branches for each endpoint
        float clusterProximityRadius = clusterRadius * ClusterProximityRadiusMultiplier;
        
        // In cluster mode, leafDensity directly controls the number of clusters
        int targetClusterCount = Mathf.RoundToInt(leafDensity);
        
        for (int i = 0; i < allEndpoints.Count; i++)
        {
            Vector3 pos = allEndpoints[i];
            
            // Count how many other endpoints are nearby
            int nearbyCount = CountNearbyBranches(pos, allEndpoints, clusterProximityRadius);
            
            clusterPositions.Add(pos);
            nearbyBranchCounts.Add(nearbyCount);
        }
        
        // Sort clusters by proximity count (ascending) to prioritize isolated/naked branches first
        // This ensures single branches get covered before adding density to grouped areas
        var clusterData = new List<(Vector3 pos, int count)>();
        for (int i = 0; i < clusterPositions.Count; i++)
        {
            clusterData.Add((clusterPositions[i], nearbyBranchCounts[i]));
        }
        clusterData.Sort((a, b) => a.count.CompareTo(b.count));
        
        // Take only the target number of clusters
        clusterPositions.Clear();
        nearbyBranchCounts.Clear();
        int clustersToGenerate = Mathf.Min(targetClusterCount, clusterData.Count);
        for (int i = 0; i < clustersToGenerate; i++)
        {
            clusterPositions.Add(clusterData[i].pos);
            nearbyBranchCounts.Add(clusterData[i].count);
        }

        // Generate clusters with size based on branch proximity and distance from center
        int clustersGenerated = 0;
        int maxClusters = maxLeafCount > 0 ? Mathf.Min(maxLeafCount, clusterPositions.Count) : clusterPositions.Count;
        float maxDistance = CalculateMaxDistanceFromCenter(clusterPositions, treeCenter);
        
        for (int i = 0; i < clusterPositions.Count && clustersGenerated < maxClusters; i++)
        {
            Vector3 clusterCenter = clusterPositions[i];
            int nearbyCount = nearbyBranchCounts[i];
            
            // Calculate cluster size based on proximity and distance from center
            float sizeMultiplier = CalculateClusterSize(clusterCenter, nearbyCount, treeCenter, maxDistance);
            
            float radius = clusterRadius * sizeMultiplier;
            
            // Generate random rotation if enabled
            Quaternion rotation = randomizeClusterRotation 
                ? Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f))
                : Quaternion.identity;
            
            AddLeafCluster(leafVertices, leafTriangles, leafUvs, leafColors, clusterCenter, radius, clusterSegments, rotation, clusterTextureTiling, clusterSeed++);
            clustersGenerated++;
        }
    }

    private void AddLeafCluster(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Color> colors, Vector3 center, float radius, int segments, Quaternion rotation, float uvTiling, int clusterSeed)
    {
        // Create an ellipsoidal cluster with organic noise variation
        // This creates horizontal and vertical rings of quads
        
        int rings = segments / 2;
        int segmentsPerRing = segments;
        
        int startIndex = vertices.Count;
        Color leafColor = new Color(1f, 1f, 1f, leafTransparency);
        
        // Seed for consistent noise per cluster
        Random.InitState(clusterSeed * ClusterSeedMultiplier);
        
        // Generate inner cluster vertices
        for (int ring = 0; ring <= rings; ring++)
        {
            float phi = Mathf.PI * ring / rings; // 0 to PI
            float y = Mathf.Cos(phi);
            float ringRadius = Mathf.Sin(phi);
            
            for (int seg = 0; seg <= segmentsPerRing; seg++)
            {
                float theta = 2f * Mathf.PI * seg / segmentsPerRing; // 0 to 2PI
                float x = ringRadius * Mathf.Cos(theta);
                float z = ringRadius * Mathf.Sin(theta);
                
                // Create ellipsoid shape by scaling each axis differently
                Vector3 ellipsoidPos = new Vector3(x * clusterShapeX, y * clusterShapeY, z * clusterShapeZ);
                
                // Add organic noise variation
                if (clusterNoiseStrength > 0f)
                {
                    Vector3 noisePos = ellipsoidPos * clusterNoiseScale;
                    float noise = Mathf.PerlinNoise(noisePos.x + clusterSeed, noisePos.y + clusterSeed) * 2f - 1f;
                    noise += Mathf.PerlinNoise(noisePos.z + clusterSeed * 0.5f, noisePos.x + clusterSeed * 0.5f) * 2f - 1f;
                    ellipsoidPos *= (1f + noise * clusterNoiseStrength);
                }
                
                // Apply rotation to the cluster vertex
                Vector3 localPos = ellipsoidPos * radius;
                Vector3 rotatedPos = rotation * localPos;
                Vector3 pos = center + rotatedPos;
                vertices.Add(pos);
                colors.Add(leafColor);
                
                // UV coordinates with tiling and variation
                float u = (float)seg / segmentsPerRing * uvTiling;
                float v = (float)ring / rings * uvTiling;
                Vector2 baseUV = new Vector2(u, v);
                
                // Apply UV variation using BarkTextureUtility
                Vector2 finalUV = BarkTextureUtility.ApplyAllUVVariation(
                    baseUV,
                    pos,
                    clusterSeed,
                    leafUVRandomness,
                    leafUVNoiseScale,
                    leafUVNoiseStrength
                );
                uvs.Add(finalUV);
            }
        }
        
        // Generate triangles
        for (int ring = 0; ring < rings; ring++)
        {
            for (int seg = 0; seg < segmentsPerRing; seg++)
            {
                int current = startIndex + ring * (segmentsPerRing + 1) + seg;
                int next = current + segmentsPerRing + 1;
                
                // First triangle (counter-clockwise for outward normals)
                triangles.Add(current);
                triangles.Add(current + 1);
                triangles.Add(next);
                
                // Second triangle (counter-clockwise for outward normals)
                triangles.Add(current + 1);
                triangles.Add(next + 1);
                triangles.Add(next);
            }
        }
        
        // Generate outer transparent shell for depth and texture
        if (enableOuterShell && outerShellThickness > 1f)
        {
            int outerStartIndex = vertices.Count;
            Color outerColor = new Color(1f, 1f, 1f, outerShellTransparency);
            float outerRadius = radius * outerShellThickness;
            
            // Generate outer shell vertices
            for (int ring = 0; ring <= rings; ring++)
            {
                float phi = Mathf.PI * ring / rings;
                float y = Mathf.Cos(phi);
                float ringRadius = Mathf.Sin(phi);
                
                for (int seg = 0; seg <= segmentsPerRing; seg++)
                {
                    float theta = 2f * Mathf.PI * seg / segmentsPerRing;
                    float x = ringRadius * Mathf.Cos(theta);
                    float z = ringRadius * Mathf.Sin(theta);
                    
                    // Create ellipsoid shape with slightly more variation on outer shell
                    Vector3 ellipsoidPos = new Vector3(x * clusterShapeX, y * clusterShapeY, z * clusterShapeZ);
                    
                    // Add more pronounced noise to outer shell for wispy effect
                    if (clusterNoiseStrength > 0f)
                    {
                        Vector3 noisePos = ellipsoidPos * clusterNoiseScale * OuterShellNoiseScale;
                        float noise = Mathf.PerlinNoise(noisePos.x + clusterSeed + OuterShellSeedOffset, noisePos.y + clusterSeed + OuterShellSeedOffset) * 2f - 1f;
                        noise += Mathf.PerlinNoise(noisePos.z + clusterSeed * 0.5f + OuterShellSeedOffset, noisePos.x + clusterSeed * 0.5f + OuterShellSeedOffset) * 2f - 1f;
                        ellipsoidPos *= (1f + noise * clusterNoiseStrength * OuterShellNoiseMultiplier);
                    }
                    
                    Vector3 localPos = ellipsoidPos * outerRadius;
                    Vector3 rotatedPos = rotation * localPos;
                    Vector3 pos = center + rotatedPos;
                    vertices.Add(pos);
                    colors.Add(outerColor);
                    
                    // UV coordinates for outer shell
                    float u = (float)seg / segmentsPerRing * uvTiling;
                    float v = (float)ring / rings * uvTiling;
                    Vector2 baseUV = new Vector2(u, v);
                    Vector2 finalUV = BarkTextureUtility.ApplyAllUVVariation(
                        baseUV,
                        pos,
                        clusterSeed + OuterShellSeedOffset,
                        leafUVRandomness,
                        leafUVNoiseScale,
                        leafUVNoiseStrength
                    );
                    uvs.Add(finalUV);
                }
            }
            
            // Generate triangles for outer shell
            for (int ring = 0; ring < rings; ring++)
            {
                for (int seg = 0; seg < segmentsPerRing; seg++)
                {
                    int current = outerStartIndex + ring * (segmentsPerRing + 1) + seg;
                    int next = current + segmentsPerRing + 1;
                    
                    triangles.Add(current);
                    triangles.Add(current + 1);
                    triangles.Add(next);
                    
                    triangles.Add(current + 1);
                    triangles.Add(next + 1);
                    triangles.Add(next);
                }
            }
        }
    }

    private void AddLeafQuad(List<Vector3> leafVertices, List<int> leafTriangles, List<Vector2> leafUvs, List<Color> leafColors, Vector3 center, Quaternion rotation, float width, float length, bool doubleSided, int leafSeed)
    {
        int startIndex = leafVertices.Count;
        float halfWidth = width * 0.5f;
        float halfLength = length * 0.5f;

        Vector3 right = rotation * Vector3.right * halfWidth;
        Vector3 up = rotation * Vector3.up * halfLength;
        
        Color leafColor = new Color(1f, 1f, 1f, leafTransparency);

        leafVertices.Add(center - right - up);
        leafVertices.Add(center + right - up);
        leafVertices.Add(center + right + up);
        leafVertices.Add(center - right + up);
        
        // Add colors for all vertices
        leafColors.Add(leafColor);
        leafColors.Add(leafColor);
        leafColors.Add(leafColor);
        leafColors.Add(leafColor);
        
        // Apply UV variation to each corner
        Vector2[] baseUVs = new Vector2[]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f)
        };
        
        for (int i = 0; i < 4; i++)
        {
            Vector3 vertexPos = leafVertices[startIndex + i];
            Vector2 finalUV = BarkTextureUtility.ApplyAllUVVariation(
                baseUVs[i],
                vertexPos,
                leafSeed,
                leafUVRandomness,
                leafUVNoiseScale,
                leafUVNoiseStrength
            );
            leafUvs.Add(finalUV);
        }

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

    private static Vector3 GetPerpendicular(Vector3 direction)
    {
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
        if (perpendicular == Vector3.zero)
        {
            perpendicular = Vector3.Cross(direction, Vector3.right).normalized;
        }
        return perpendicular;
    }

    private static GameObject CreateMeshObject(string name, Material material, Transform parent, out Mesh mesh)
    {
        GameObject obj = new GameObject(name);
        if (parent != null)
        {
            obj.transform.SetParent(parent, false);
        }

        var meshFilter = obj.AddComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        var meshRenderer = obj.AddComponent<MeshRenderer>();
        meshRenderer.material = material;

        return obj;
    }

    // Helper methods for cluster generation
    private int CountNearbyBranches(Vector3 position, List<Vector3> allPositions, float radius)
    {
        int count = 0;
        foreach (Vector3 other in allPositions)
        {
            if (Vector3.Distance(position, other) < radius)
            {
                count++;
            }
        }
        return count;
    }

    private float CalculateMaxDistanceFromCenter(List<Vector3> positions, Vector3 center)
    {
        float maxDistance = 0f;
        foreach (Vector3 pos in positions)
        {
            float dist = Vector3.Distance(center, pos);
            if (dist > maxDistance) maxDistance = dist;
        }
        return maxDistance;
    }

    private float CalculateClusterSize(Vector3 clusterPosition, int nearbyCount, Vector3 treeCenter, float maxDistance)
    {
        // Ensure isolated branches (low nearby count) get reasonable size to cover tips
        // More branches = larger cluster for main canopy
        float minIsolatedSize = (clusterSizeMin + clusterSizeMax) * 0.5f; // Mid-range for isolated branches
        float proximitySize = nearbyCount <= 2 
            ? minIsolatedSize 
            : Mathf.Lerp(minIsolatedSize, clusterSizeMax, Mathf.Clamp01((nearbyCount - 2) / (float)(MaxProximityBranchCount - 2)));
        
        // Size based on distance from center (center = large, edges = small)
        float distanceFromCenter = maxDistance > 0 ? Vector3.Distance(treeCenter, clusterPosition) / maxDistance : 0f;
        float centerDistanceSize = Mathf.Lerp(clusterSizeMax, clusterSizeMin, distanceFromCenter);
        
        // Combine both factors, but give more weight to proximity for isolated branches
        float sizeMultiplier = Mathf.Lerp(centerDistanceSize, proximitySize, ProximitySizeWeight);
        
        // Add random variation
        sizeMultiplier *= Random.Range(1f - RandomSizeVariation, 1f + RandomSizeVariation);
        
        return sizeMultiplier;
    }

    private Vector3 CalculateTreeCenter(List<Vector3> positions)
    {
        if (positions.Count == 0)
            return Vector3.zero;
            
        Vector3 center = Vector3.zero;
        foreach (Vector3 pos in positions)
        {
            center += pos;
        }
        return center / positions.Count;
    }
}
