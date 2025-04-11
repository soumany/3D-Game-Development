using UnityEngine;

public class Interactable : MonoBehaviour
{
    protected MeshRenderer mesh;


    [SerializeField] private Material highlightMaterial;
    private Material defaultMaterial;

    private void Start()
    {
        if (mesh == null)
            mesh = GetComponentInChildren<MeshRenderer>();

        defaultMaterial = mesh.sharedMaterial;
    }

    protected void UpdateMeshAndMaterial(MeshRenderer newMesh)
    {
        mesh = newMesh;
        defaultMaterial = newMesh.sharedMaterial;
    }

    public virtual void Interaction()
    {
        Debug.Log("Interacted with " + gameObject.name);
    }


    public void HighlightActive(bool active)
    {
        if (active)
            mesh.material = highlightMaterial;
        else
            mesh.material = defaultMaterial;
    }
    protected virtual void OnTriggerEnter(Collider other)
    {
        PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();

        if (playerInteraction == null)
            return;

        playerInteraction.GetInteractables().Add(this);
        playerInteraction.UpdateCloseInteractable();
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();

        if (playerInteraction == null)
            return;
        
        playerInteraction.GetInteractables().Remove(this);
        playerInteraction.UpdateCloseInteractable();
    }
}
