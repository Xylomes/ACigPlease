using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class OutlineOnHover : MonoBehaviour
{
    [Header("Material d'outline ajouté au survol")]
    public Material outlineMaterial;

    private MeshRenderer meshRenderer;
    private Material[] baseMaterials;      // juste le/les material(s) d'origine
    private Material[] materialsWithOutline; // base + outline

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactMask = ~0;
    [SerializeField] private LayerMask playerMask;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        // Récupère les materials d'origine (instance propre à cet objet)
        baseMaterials = meshRenderer.materials;

        // Prépare le tableau avec l'outline en plus
        materialsWithOutline = new Material[baseMaterials.Length + 1];
        baseMaterials.CopyTo(materialsWithOutline, 0);
        materialsWithOutline[materialsWithOutline.Length - 1] = outlineMaterial;
    }

    private void Update()
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, interactRange, ~playerMask))
        {
            meshRenderer.materials = materialsWithOutline;
        }
        else
        {
            meshRenderer.materials = baseMaterials;
        }
    }

    void OnMouseEnter()
    {
        
    }

    void OnMouseExit()
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, interactRange, ~playerMask))
        {
            
        }
    }
}