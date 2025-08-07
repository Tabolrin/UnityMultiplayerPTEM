using UnityEngine;

[CreateAssetMenu(fileName = "MaterialsContainer", menuName = "Data/MaterialsContainer", order = 1)]
public class PlayerMaterialsContainer : ScriptableObject
{
    [SerializeField] public Material blackMaterial;
    [SerializeField] public Material redMaterial;
    [SerializeField] public Material blueMaterial;
    [SerializeField] public Material greenMaterial;
    [SerializeField] public Material yellowMaterial;
    [SerializeField] public Material purpleMaterial;
    [SerializeField] public Material orangeMaterial;
    [SerializeField] public Material whiteMaterial;
    [SerializeField] public Material grayMaterial;
    [SerializeField] public Material pinkMaterial;
}