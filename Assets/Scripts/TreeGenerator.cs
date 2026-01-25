using System.Collections.Generic;
using System.Text;
using UnityEngine.Splines;
using UnityEngine;

public class TreeGenerator : MonoBehaviour
{
    [SerializeField] private string axiom = "B";
    [SerializeField] private int iterations = 3;
    [SerializeField] private float segmentLength = 1f;
    [SerializeField] private Material treeMaterial;

    // L-system parameters
    [SerializeField] private float growthProbability = 50f; // Probability (0-100) of branch growth
    [SerializeField] private float branchProbability = 50f; // Probability (0-100) of branch type selection
    [SerializeField] private float angleXMin = 15f; // Minimum angle for left rotation
    [SerializeField] private float angleXMax = 45f; // Maximum angle for left rotation
    [SerializeField] private float angleYMin = -30f; // Minimum angle for right rotation
    [SerializeField] private float angleYMax = 30f; // Maximum angle for right rotation

    private string expandedTree;
    private Stack<TransformInfoHelper> transformStack;
    private Stack<int> splineIndexStack;

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
        meshFilter.mesh = new Mesh();
        var meshRenderer = treeObject.AddComponent<MeshRenderer>();
        meshRenderer.material = treeMaterial;

        // Setup spline container
        var container = treeObject.AddComponent<SplineContainer>();
        container.RemoveSplineAt(0);
        var extrude = treeObject.AddComponent<SplineExtrude>();
        extrude.Container = container;

        // Initialize stacks and first spline
        transformStack = new Stack<TransformInfoHelper>();
        splineIndexStack = new Stack<int>();
        
        var currentSpline = container.AddSpline();
        var splineIndex = container.Splines.FindIndex(currentSpline);
        
        // Create temporary transform for tree generation
        Transform treeTransform = treeObject.transform;
        treeTransform.position = transform.position;
        treeTransform.rotation = transform.rotation;
        
        currentSpline.Add(new BezierKnot(treeTransform.position), TangentMode.AutoSmooth);

        // Process L-system string
        foreach (char instruction in expandedTree)
        {
            switch (instruction)
            {
                case 'F':
                    treeTransform.Translate(Vector3.up * segmentLength);
                    currentSpline.Add(new BezierKnot(treeTransform.position), TangentMode.AutoSmooth);
                    break;

                case 'B':
                    // Branch marker (handled implicitly by [ and ])
                    break;

                case '[':
                    // Save current transform state
                    transformStack.Push(new TransformInfoHelper()
                    {
                        position = treeTransform.position,
                        rotation = treeTransform.rotation
                    });
                    splineIndexStack.Push(splineIndex);

                    // Create new branch spline
                    int knnotCount = currentSpline.Count;
                    int previousSplineIndex = splineIndex;
                    currentSpline = container.AddSpline();
                    splineIndex = container.Splines.FindIndex(currentSpline);
                    
                    currentSpline.Add(new BezierKnot(treeTransform.position), TangentMode.AutoSmooth);
                    
                    // Link splines at branch point
                    container.LinkKnots(
                        new SplineKnotIndex(previousSplineIndex, knnotCount - 1),
                        new SplineKnotIndex(splineIndex, 0)
                    );
                    break;

                case ']':
                    // Restore transform state
                    TransformInfoHelper savedTransform = transformStack.Pop();
                    treeTransform.position = savedTransform.position;
                    treeTransform.rotation = savedTransform.rotation;
                    splineIndex = splineIndexStack.Pop();
                    currentSpline = container.Splines[splineIndex];
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
        // Set tree object's Y position to generator's Y position
        treeObject.transform.position = new Vector3(treeObject.transform.position.x, generatorY, treeObject.transform.position.z);
    }
}
