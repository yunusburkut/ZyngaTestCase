using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class InputTestManager : MonoBehaviour
{
    [SerializeField] private InputAction drawAction;
    [SerializeField] private InputAction suitSortAction;
    [SerializeField] private InputAction numberSortAction;
    [SerializeField] private InputAction smartSortAction;

    [SerializeField] private float safeAreaTopThreshold = 0.8f;   
    [SerializeField] private float safeAreaBottomThreshold = 0.3f; 

    private Rect safeArea;

    private void OnEnable()
    {
        safeArea = Screen.safeArea;

        drawAction.Enable();
        suitSortAction.Enable();
        numberSortAction.Enable();
        smartSortAction.Enable();

        drawAction.performed += OnDrawPerformed;
        suitSortAction.performed += OnSuitSortPerformed;
        numberSortAction.performed += OnNumberSortPerformed;
        smartSortAction.performed += OnSmartSortPerformed;
    }

    private void OnDisable()
    {
        drawAction.performed -= OnDrawPerformed;
        suitSortAction.performed -= OnSuitSortPerformed;
        numberSortAction.performed -= OnNumberSortPerformed;
        smartSortAction.performed -= OnSmartSortPerformed;

        drawAction.Disable();
        suitSortAction.Disable();
        numberSortAction.Disable();
        smartSortAction.Disable();
    }

    
    private void OnDrawPerformed(InputAction.CallbackContext context)
    {
        Vector2 pos = context.ReadValue<Vector2>();
        ProcessInput(pos);
    }

    private void OnSuitSortPerformed(InputAction.CallbackContext context)
    {
        Vector2 pos = context.ReadValue<Vector2>();
        ProcessInput(pos);
    }

    private void OnNumberSortPerformed(InputAction.CallbackContext context)
    {
        Vector2 pos = context.ReadValue<Vector2>();
        ProcessInput(pos);
    }

    private void OnSmartSortPerformed(InputAction.CallbackContext context)
    {
        Vector2 pos = context.ReadValue<Vector2>();
        ProcessInput(pos);
    }

    private void ProcessInput(Vector2 inputPos)
    {
        if (!safeArea.Contains(inputPos))
            return;

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        if (inputPos.y >= safeArea.yMin + safeArea.height * safeAreaTopThreshold)
        {
            for (int i = 0; i < 10; i++)
            {
                DrawAndLogCard();
            }
        }
        else if (inputPos.y <= safeArea.yMin + safeArea.height * safeAreaBottomThreshold)
        {
            if (inputPos.x < safeArea.xMin + safeArea.width / 3f)
            {
                SuitSortTest();
            }
            else if (inputPos.x > safeArea.xMin + safeArea.width * 2f / 3f)
            {
                SmartSort();
            }
            else
            {
                NumberSortTest();
            }
        }
    }

    public void SuitSortTest()
    {
        MyDeckManager.Instance.CalculateSet();
    }

    public void NumberSortTest()
    {
        MyDeckManager.Instance.CalculateRun();
    }

    public void SmartSort()
    {
        MyDeckManager.Instance.ComputeOptimalMelds();
    }

    public void DrawAndLogCard()
    {
        BoardManager.Instance.DrawCard();
    }
}
