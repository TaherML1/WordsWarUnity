using UnityEngine;

public class GameObjectActivator : MonoBehaviour
{
    public GameObject gameObjectToActivate;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("ActivateGameObject", 0.5f); // Invoke the method after one second
    }

    void ActivateGameObject()
    {
        gameObjectToActivate.SetActive(true);
    }
}
