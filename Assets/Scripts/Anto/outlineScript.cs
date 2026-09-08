using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class OutlineOnHover : MonoBehaviour
{
    [Header("Material d'outline ajouté au survol")]
    public Material outlineMaterial;

    private MeshRenderer meshRenderer;
    private Material[] baseMaterials;
    private Material[] materialsWithOutline;

    private bool mouseEntered = false;

    private Transform cameraTransform;
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask playerMask;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        baseMaterials = meshRenderer.materials;

        materialsWithOutline = new Material[baseMaterials.Length + 1];
        baseMaterials.CopyTo(materialsWithOutline, 0);
        materialsWithOutline[materialsWithOutline.Length - 1] = outlineMaterial;
    }

    void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (cameraTransform == null)
            return;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, interactRange, ~playerMask))
        {
            meshRenderer.materials = mouseEntered ? materialsWithOutline : baseMaterials;
        }
        else
        {
            meshRenderer.materials = baseMaterials;
        }
    }

    void OnMouseEnter()
    {
        mouseEntered = true;
    }

    void OnMouseExit()
    {
        mouseEntered = false;
    }
}