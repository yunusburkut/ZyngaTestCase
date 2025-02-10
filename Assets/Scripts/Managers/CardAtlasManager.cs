using UnityEngine;
using UnityEngine.U2D;

public class CardAtlasManager : MonoBehaviour
{
    // Singleton instance for global access.
    public static CardAtlasManager Instance { get; private set; }

    // Reference to the SpriteAtlas that contains all card sprites.
    [SerializeField] private SpriteAtlas cardAtlas;

    private void Awake()
    {
        // If no instance exists, assign this one; otherwise, destroy the duplicate.
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Retrieves a card sprite from the atlas based on the card's suit and number.
    /// The sprite names in the atlas should follow the naming convention: "card_{suit}_{number}".
    /// For example, for suit 1 and number 5, the sprite name should be "card_1_5".
    /// </summary>
    /// <param name="suit">The suit of the card (e.g., 1, 2, 3, or 4).</param>
    /// <param name="number">The number of the card (e.g., 1 to 13).</param>
    /// <returns>The Sprite corresponding to the given suit and number. If not found, returns null and logs a warning.</returns>
    public Sprite GetCardSprite(byte suit, byte number)
    {
        // Construct the sprite name using the specified naming convention.
        string spriteName = $"card_{suit}_{number}";

        // Retrieve the sprite from the atlas.
        Sprite sprite = cardAtlas.GetSprite(spriteName);

        // Log a warning if the sprite was not found in the atlas.
        if (sprite == null)
            Debug.LogWarning("Sprite not found in atlas: " + spriteName);

        return sprite;
    }
}