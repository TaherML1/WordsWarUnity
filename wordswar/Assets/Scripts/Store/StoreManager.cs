using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    public GameObject itemPrefab;  // Reference to the item prefab
    public RectTransform content;  // Reference to the content panel
    public int totalItems = 100;   // Total number of items in the store
    public float itemHeight = 100f; // Height of each item

    void Start()
    {
        LoadItems();
    }

    void LoadItems()
    {
        for (int i = 0; i < totalItems; i++)
        {
            GameObject newItem = Instantiate(itemPrefab, content);
            RectTransform itemRect = newItem.GetComponent<RectTransform>();
            itemRect.anchoredPosition = new Vector2(0, -i * itemHeight);
        }

        // Adjust content height based on the number of items
        content.sizeDelta = new Vector2(content.sizeDelta.x, totalItems * itemHeight);
    }
}
