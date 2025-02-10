using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class InputTestManager : MonoBehaviour
{
    // InputActions to be set via the Inspector.
    [SerializeField] private InputAction drawAction;       // Action for drawing cards.
    [SerializeField] private InputAction suitSortAction;     // Action for performing suit-based sorting (CalculateSet).
    [SerializeField] private InputAction numberSortAction;   // Action for performing number-based sorting (CalculateRun).
    [SerializeField] private InputAction smartSortAction;    // Action for computing optimal melds (ComputeOptimalMelds).

    // Thresholds (as a percentage) for safe area regions.
    [SerializeField] private float safeAreaTopThreshold = 0.8f;    // Upper 20% of safe area.
    [SerializeField] private float safeAreaBottomThreshold = 0.3f; // Lower 30% of safe area.

    // Cached safe area rectangle obtained from Screen.safeArea.
    private Rect safeArea;

    private void OnEnable()
    {
        // Retrieve the safe area from the current screen settings.
        safeArea = Screen.safeArea;

        // Enable the input actions.
        drawAction.Enable();
        suitSortAction.Enable();
        numberSortAction.Enable();
        smartSortAction.Enable();

        // Subscribe to the performed events of each InputAction.
        drawAction.performed += OnDrawPerformed;
        suitSortAction.performed += OnSuitSortPerformed;
        numberSortAction.performed += OnNumberSortPerformed;
        smartSortAction.performed += OnSmartSortPerformed;
    }

    private void OnDisable()
    {
        // Unsubscribe from the InputAction events.
        drawAction.performed -= OnDrawPerformed;
        suitSortAction.performed -= OnSuitSortPerformed;
        numberSortAction.performed -= OnNumberSortPerformed;
        smartSortAction.performed -= OnSmartSortPerformed;

        // Disable the input actions.
        drawAction.Disable();
        suitSortAction.Disable();
        numberSortAction.Disable();
        smartSortAction.Disable();
    }

    /// <summary>
    /// Called when the draw action is performed.
    /// Reads the input position and processes it.
    /// </summary>
    private void OnDrawPerformed(InputAction.CallbackContext context)
    {
        Vector2 pos = context.ReadValue<Vector2>();
        ProcessInput(pos);
    }

    /// <summary>
    /// Called when the suit sort action is performed.
    /// Reads the input position and processes it.
    /// </summary>
    private void OnSuitSortPerformed(InputAction.CallbackContext context)
    {
        Vector2 pos = context.ReadValue<Vector2>();
        ProcessInput(pos);
    }

    /// <summary>
    /// Called when the number sort action is performed.
    /// Reads the input position and processes it.
    /// </summary>
    private void OnNumberSortPerformed(InputAction.CallbackContext context)
    {
        Vector2 pos = context.ReadValue<Vector2>();
        ProcessInput(pos);
    }

    /// <summary>
    /// Called when the smart sort action is performed.
    /// Reads the input position and processes it.
    /// </summary>
    private void OnSmartSortPerformed(InputAction.CallbackContext context)
    {
        Vector2 pos = context.ReadValue<Vector2>();
        ProcessInput(pos);
    }

    /// <summary>
    /// Processes the input based on its position relative to the safe area.
    /// - If the input is in the upper safe area, 10 cards are drawn.
    /// - If in the lower safe area, the action depends on the horizontal position:
    ///   left side for suit sort, right side for smart sort, and middle for number sort.
    /// </summary>
    /// <param name="inputPos">The input position (from touch or mouse) in screen coordinates.</param>
    private void ProcessInput(Vector2 inputPos)
    {
        // Only process input that falls within the safe area.
        if (!safeArea.Contains(inputPos))
            return;

        // Optionally, you can get the screen width and height if needed.
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Check if the input is in the upper safe area (top threshold region).
        if (inputPos.y >= safeArea.yMin + safeArea.height * safeAreaTopThreshold)
        {
            // In the upper region, draw 10 cards (similar to pressing the Space key).
            for (int i = 0; i < 10; i++)
            {
                DrawAndLogCard();
            }
        }
        // Check if the input is in the lower safe area (bottom threshold region).
        else if (inputPos.y <= safeArea.yMin + safeArea.height * safeAreaBottomThreshold)
        {
            // Determine horizontal region within the lower safe area.
            // Left third: Suit sort action.
            if (inputPos.x < safeArea.xMin + safeArea.width / 3f)
            {
                SuitSortTest();
            }
            // Right third: Smart sort action.
            else if (inputPos.x > safeArea.xMin + safeArea.width * 2f / 3f)
            {
                SmartSort();
            }
            // Middle: Number sort action.
            else
            {
                NumberSortTest();
            }
        }
        // You can add more conditions for other regions if needed.
    }

    /// <summary>
    /// Triggers suit sort functionality, calling the CalculateSet method on MyDeckManager.
    /// </summary>
    public void SuitSortTest()
    {
        MyDeckManager.Instance.CalculateSet();
    }

    /// <summary>
    /// Triggers number sort functionality, calling the CalculateRun method on MyDeckManager.
    /// </summary>
    public void NumberSortTest()
    {
        MyDeckManager.Instance.CalculateRun();
    }

    /// <summary>
    /// Triggers smart sort functionality, calling the ComputeOptimalMelds method on MyDeckManager.
    /// </summary>
    public void SmartSort()
    {
        MyDeckManager.Instance.ComputeOptimalMelds();
    }

    /// <summary>
    /// Draws a card using the BoardManager.
    /// </summary>
    public void DrawAndLogCard()
    {
        BoardManager.Instance.DrawCard();
    }
}
