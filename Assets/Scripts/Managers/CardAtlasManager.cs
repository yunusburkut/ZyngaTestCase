using UnityEngine;
using UnityEngine.U2D;

public class CardAtlasManager : MonoBehaviour
{
    public static CardAtlasManager Instance { get; private set; }

    [SerializeField] private SpriteAtlas cardAtlas;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public Sprite GetCardSprite(byte suit, byte number)
    {
        string spriteName = $"card_{suit}_{number}";
        Sprite sprite = cardAtlas.GetSprite(spriteName);
        if (sprite == null)
            Debug.LogWarning("Sprite not found in atlas: " + spriteName);
        return sprite;
    }
}