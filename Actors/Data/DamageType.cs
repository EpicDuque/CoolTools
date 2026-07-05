using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Damage Type", menuName = "Damage Type", order = 1)]
public class DamageType : ScriptableObject
{
    [FormerlySerializedAs("typeName")]
    [SerializeField] private string _typeName;
    [SerializeField, TextArea] private string _description;

    public string TypeName => _typeName;

    public string Description => _description;
}
