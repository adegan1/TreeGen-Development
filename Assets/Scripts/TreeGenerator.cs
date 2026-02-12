using System.Collections.Generic;
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
    private const string TreeObjectName = "Tree";
    private static readonly Color CanopyGizmoColor = new Color(0.2f, 0.8f, 0.3f, 1f);

    public enum LeafGenerationMode
    {
        Planes,  // Traditional quad-based leaves
        Clusters,     // Spherical leaf clusters around branch groups
        Domes   // Open-bottom domes for soft canopy shapes
    }

    public enum TreeStructureMode
    {
        LSystem,
        GuidedGrowth
    }

    [System.Serializable]
    public struct CanopyVolumeSettings
    {
        public Vector3 centerOffset;
        public Vector3 radii;
        [Range(0f, 1f)]
        public float attraction;
        public bool surfaceTarget;
        [Range(0f, 1f)]
        public float heightStart;
        [Range(0f, 1f)]
        public float heightEnd;
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

    [Header("Structure")]
    [Tooltip("Length of each branch segment in units")]
    [Range(0.1f, 5f)]
    [SerializeField] private float segmentLength = 1f;
    [Tooltip("Seed for deterministic growth (0 = random)")]
    [SerializeField] private int randomSeed = 0;
    [Tooltip("Overall trunk height in units")]
    [Range(1f, 20f)]
    [SerializeField] private float trunkHeight = 6f;
    [Tooltip("Randomized variation applied to trunk height")]
    [Range(0f, 0.5f)]
    [SerializeField] private float trunkHeightVariation = 0.15f;
    [Tooltip("How much the trunk leans as it grows")]
    [Range(0f, 1f)]
    [SerializeField] private float trunkLeanStrength = 0.2f;
    [Tooltip("Noise scale for trunk direction variation")]
    [Range(0.05f, 3f)]
    [SerializeField] private float trunkNoiseScale = 0.35f;
    [Tooltip("Noise strength for trunk direction variation")]
    [Range(0f, 0.8f)]
    [SerializeField] private float trunkNoiseStrength = 0.2f;

    [Header("Branches")]
    [Tooltip("How many branching levels to generate (0 = no branches, 1 = only primary branches)")]
    [Range(0, 5)]
    [SerializeField] private int branchLevels = 3;
    [Tooltip("Branches spawned per parent at each level")]
    [Range(1, 12)]
    [SerializeField] private int branchesPerLevel = 3;
    [Tooltip("Reduce branch counts at higher levels (0.5 = fewer at higher levels)")]
    [Range(0.2f, 1f)]
    [SerializeField] private float branchLevelDensityFalloff = 0.7f;
    [Tooltip("Primary branch length as a fraction of trunk height")]
    [Range(0.2f, 1.2f)]
    [SerializeField] private float branchLengthFactor = 0.75f;
    [Tooltip("Length reduction per branching level")]
    [Range(0.3f, 0.9f)]
    [SerializeField] private float branchLengthFalloff = 0.7f;
    [Tooltip("Minimum branch angle away from parent direction")]
    [Range(0f, 90f)]
    [SerializeField] private float branchAngleMin = 20f;
    [Tooltip("Maximum branch angle away from parent direction")]
    [Range(0f, 90f)]
    [SerializeField] private float branchAngleMax = 55f;
    [Tooltip("Bias branches upward (0 = none, 1 = strong)")]
    [Range(0f, 1f)]
    [SerializeField] private float branchUpwardBias = 0.2f;
    [Tooltip("How much branches droop toward the tips")]
    [Range(0f, 1f)]
    [SerializeField] private float branchDroop = 0.25f;
    [Tooltip("Noise scale for branch direction variation")]
    [Range(0.05f, 3f)]
    [SerializeField] private float branchNoiseScale = 0.6f;
    [Tooltip("Noise strength for branch direction variation")]
    [Range(0f, 0.8f)]
    [SerializeField] private float branchNoiseStrength = 0.2f;
    [Tooltip("Azimuth jitter for branch distribution around the parent")]
    [Range(0f, 60f)]
    [SerializeField] private float branchTwistJitter = 12f;
    [Tooltip("Hard cap on total generated branches (0 = unlimited)")]
    [Range(0, 500)]
    [SerializeField] private int maxGeneratedBranches = 120;
    [Tooltip("Minimum upward component for branch direction (prevents downward spikes)")]
    [Range(-0.2f, 0.5f)]
    [SerializeField] private float minBranchUpward = 0.02f;
    [Tooltip("Keep branch points above the base height")]
    [SerializeField] private bool clampBranchesAboveBase = false;
    [Tooltip("Minimum clearance above base when clamping branches")]
    [Range(0f, 2f)]
    [SerializeField] private float branchGroundClearance = 0.05f;

    [Header("Canopy Targeting")]
    [Tooltip("Bias branches toward a canopy volume")]
    [SerializeField] private bool canopyTargetEnabled = false;
    [Tooltip("Center of the canopy volume relative to the generator")]
    [SerializeField] private Vector3 canopyCenterOffset = new Vector3(0f, 5f, 0f);
    [Tooltip("Canopy volume radii (ellipsoid) in local space")]
    [SerializeField] private Vector3 canopyRadii = new Vector3(3f, 2.5f, 3f);
    [Tooltip("Strength of branch attraction toward the canopy")]
    [Range(0f, 1f)]
    [SerializeField] private float canopyAttraction = 0.35f;
    [Tooltip("Use canopy surface instead of center as the target")]
    [SerializeField] private bool canopySurfaceTarget = true;
    [Tooltip("Height range for applying canopy attraction (0 = base, 1 = top)")]
    [Range(0f, 1f)]
    [SerializeField] private float canopyHeightStart = 0.35f;
    [Range(0f, 1f)]
    [SerializeField] private float canopyHeightEnd = 1f;

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

    [Header("Plane Leaf UVs")]
    [Tooltip("Texture tiling for plane leaves")]
    [Range(0.1f, 10f)]
    [SerializeField] private float planeLeafTextureTiling = 1f;

    [Header("Plane Leaf Size By Height")]
    [Tooltip("Scale plane leaf size from bottom to top")]
    [SerializeField] private bool enablePlaneLeafSizeByHeight = false;
    [Tooltip("Size multiplier at the bottom of the tree (Plane mode)")]
    [Range(0.1f, 10f)]
    [SerializeField] private float planeLeafSizeBottom = 1f;
    [Tooltip("Size multiplier at the top of the tree (Plane mode)")]
    [Range(0.1f, 10f)]
    [SerializeField] private float planeLeafSizeTop = 1f;

    [HideInInspector]
    [Range(0f, 1f)]
    [SerializeField] private float leafSizeByHeight = 0.3f;
    [HideInInspector]
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

    private GameObject generatedTree; // Reference to the tree created by this generator

    // Get the Y position of the generator
    private float generatorY => transform.position.y;


    private void Start()
    {
        // Only generate a tree if one doesn't already exist
        if (transform.Find(TreeObjectName) == null)
        {
            RegenerateTree();
        }
    }

    public void RegenerateTree()
    {
        ClearTree();
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
            Transform existingTree = transform.Find(TreeObjectName);
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
        DrawCanopyGizmos();
    }

    private void DrawCanopyGizmos()
    {
        if (!canopyTargetEnabled)
        {
            return;
        }

        Quaternion baseRotation = transform.rotation;
        DrawCanopyVolumeGizmo(canopyCenterOffset, canopyRadii, baseRotation, CanopyGizmoColor);
    }

    private void DrawCanopyVolumeGizmo(Vector3 centerOffset, Vector3 radii, Quaternion baseRotation, Color color)
    {
        Vector3 center = transform.position + baseRotation * centerOffset;
        Gizmos.color = color;
        Matrix4x4 prev = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, baseRotation, radii);
        Gizmos.DrawWireSphere(Vector3.zero, 1f);
        Gizmos.matrix = prev;
    }

    private void CreateMesh()
    {
        // Validate material
        if (barkMaterial == null)
        {
            Debug.LogError("Tree Material is not assigned!");
            return;
        }

        // Create GameObject and components
        GameObject treeObject = CreateMeshObject(TreeObjectName, barkMaterial, null, out Mesh mesh);
        generatedTree = treeObject; // Store reference to this generator's tree

        List<List<BranchPoint>> allBranches = BuildBranchMesh(mesh);
        BuildLeafMesh(treeObject, allBranches);

        // Make tree a child of the generator and set position
        treeObject.transform.SetParent(transform, true);
        treeObject.transform.position = new Vector3(treeObject.transform.position.x, generatorY, treeObject.transform.position.z);
    }

}
