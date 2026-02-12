using System.Collections.Generic;
using System.Text;
using UnityEngine;

public partial class TreeGenerator : MonoBehaviour
{
    // Constants
    private const int DefaultSegments = 8;
    private const float ClusterProximityRadiusMultiplier = 3f;
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
        Clusters,     // Spherical leaf clusters around branch groups
        Domes   // Open-bottom domes for soft canopy shapes
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

    [Header("Presets")]
    [SerializeField] private TreePreset preset;

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
    [Tooltip("How strongly leaves form clumps along each branch segment (Plane mode only)")]
    [Range(0f, 1f)]
    [SerializeField] private float leafClumpiness = 0.5f;
    [Tooltip("How far leaf positions can drift from a clump center, as a fraction of segment length (Plane mode only)")]
    [Range(0.01f, 1f)]
    [SerializeField] private float leafClumpSpread = 0.25f;
    [Tooltip("Bias leaf placement toward branch tips (Plane mode only)")]
    [Range(0f, 1f)]
    [SerializeField] private float leafTipBias = 0.6f;
    [Tooltip("Align leaf up direction toward world up (Plane mode only)")]
    [Range(0f, 1f)]
    [SerializeField] private float leafUpAlignment = 0.7f;
    [Tooltip("Reduce leaf size toward the top of the tree (Plane mode only)")]
    [Range(0f, 1f)]
    [SerializeField] private float leafSizeByHeight = 0.3f;
    [Tooltip("Random radial jitter for leaf distance from the branch (Plane mode only)")]
    [Range(0f, 0.2f)]
    [SerializeField] private float leafRadialJitter = 0.04f;
    
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

    [Header("Leaf Domes (Dome Mode)")]
    [Tooltip("Base radius of each leaf dome")]
    [Range(0.2f, 10f)]
    [SerializeField] private float domeRadius = 1.2f;
    [Tooltip("Horizontal stretch of dome shape (1 = sphere)")]
    [Range(0.5f, 2f)]
    [SerializeField] private float domeShapeX = 1.15f;
    [Tooltip("Vertical stretch of dome shape (1 = sphere)")]
    [Range(0.5f, 2f)]
    [SerializeField] private float domeShapeY = 0.75f;
    [Tooltip("Depth stretch of dome shape (1 = sphere)")]
    [Range(0.5f, 2f)]
    [SerializeField] private float domeShapeZ = 1.05f;
    [Tooltip("How much to push domes outward from branch tips")]
    [Range(-1f, 2f)]
    [SerializeField] private float domeOffset = 0.5f;
    [Tooltip("Number of segments around the dome (higher = smoother)")]
    [Range(4, 32)]
    [SerializeField] private int domeSegments = 12;
    [Tooltip("Scale of the noise pattern used for dome irregularity")]
    [Range(0.5f, 5f)]
    [SerializeField] private float domeNoiseScale = 2f;
    [Tooltip("Add organic irregularity to dome surface (0 = perfect)")]
    [Range(0f, 0.5f)]
    [SerializeField] private float domeNoiseStrength = 0.12f;
    [Tooltip("Randomize dome rotation for more natural variation")]
    [SerializeField] private bool randomizeDomeRotation = true;
    [Tooltip("How many times to tile the texture on domes")]
    [Range(0.5f, 10f)]
    [SerializeField] private float domeTextureTiling = 1f;
    
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

    [Header("Branch Curvature")]
    [Tooltip("Chance that a segment slightly bends to create natural curvature")]
    [Range(0f, 1f)]
    [SerializeField] private float branchBendChance = 0.35f;
    [Tooltip("Maximum bend angle per segment")]
    [Range(0f, 10f)]
    [SerializeField] private float branchBendStrength = 3f;

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
                    if (branchBendStrength > 0f && Random.value < branchBendChance)
                    {
                        float bendPitch = Random.Range(-branchBendStrength, branchBendStrength);
                        float bendYaw = Random.Range(-branchBendStrength, branchBendStrength);
                        treeTransform.Rotate(Vector3.right, bendPitch, Space.Self);
                        treeTransform.Rotate(Vector3.forward, bendYaw, Space.Self);
                    }
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
            else if (leafMode == LeafGenerationMode.Domes)
            {
                CreateLeafDomes(leafVertices, leafTriangles, leafUvs, leafColors, allBranches);
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

}
