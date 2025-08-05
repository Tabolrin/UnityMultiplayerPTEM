using UnityEngine;

[CreateAssetMenu(fileName = "MaterialsContainer", menuName = "Data/MaterialsContainer", order = 1)]
public class PlayerMaterialsContainer : ScriptableObject
{
    [SerializeField] private Material blackMaterial;
    [SerializeField] private Material orangeMaterial;
    [SerializeField] private Material greenMaterial;
    [SerializeField] private Material blueMaterial;
    [SerializeField] private Material redMaterial;
    [SerializeField] private Material pinkMaterial;
    [SerializeField] private Material yellowMaterial;
    [SerializeField] private Material whiteMaterial;
    [SerializeField] private Material greyMaterial;
}