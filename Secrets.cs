using UnityEngine;

[CreateAssetMenu(fileName = "Secrets", menuName = "MyGame/Secrets")]
public class Secrets : ScriptableObject
{
    public string gistId;
    public string token;
}
