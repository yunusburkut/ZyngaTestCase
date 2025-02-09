using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class InputTestManager : MonoBehaviour
{
    [SerializeField] private InputAction drawAction;
    [SerializeField] private InputAction suitSortAction;
    [SerializeField] private InputAction numberSortAction;
    [SerializeField] private InputAction smartSortAction;

    // Safe area ayarları; bu değerleri Inspector'dan ayarlayabilirsiniz.
    [SerializeField] private float safeAreaTopThreshold = 0.8f;   // Üst safe area başlangıcı (yüzde)
    [SerializeField] private float safeAreaBottomThreshold = 0.3f; // Alt safe area bitişi (yüzde)

    private Rect safeArea;

    private void OnEnable()
    {
        // Safe area bilgisini alıyoruz.
        safeArea = Screen.safeArea;

        // Input aksiyonlarını etkinleştiriyoruz.
        drawAction.Enable();
        suitSortAction.Enable();
        numberSortAction.Enable();
        smartSortAction.Enable();

        // Callback'leri atıyoruz.
        drawAction.performed += OnDrawPerformed;
        suitSortAction.performed += OnSuitSortPerformed;
        numberSortAction.performed += OnNumberSortPerformed;
        smartSortAction.performed += OnSmartSortPerformed;
    }

    private void OnDisable()
    {
        // Callback'leri kaldırıyoruz.
        drawAction.performed -= OnDrawPerformed;
        suitSortAction.performed -= OnSuitSortPerformed;
        numberSortAction.performed -= OnNumberSortPerformed;
        smartSortAction.performed -= OnSmartSortPerformed;

        // Input aksiyonlarını devre dışı bırakıyoruz.
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

    /// <summary>
    /// Giriş pozisyonunu safe area içinde kontrol eder ve pozisyona göre ilgili aksiyonu tetikler.
    /// Hem mobil hem PC girişleri için çalışır.
    /// </summary>
    /// <param name="inputPos">Giriş pozisyonu (dokunma veya fare)</param>
    private void ProcessInput(Vector2 inputPos)
    {
        // Dokunmanın safe area içinde olup olmadığını kontrol ediyoruz.
        if (!safeArea.Contains(inputPos))
            return;

        // Ekranın boyutlarını alalım.
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Örnek: Ekranın üst safe area bölgesi, safeArea'nın üst %20'si;
        // alt safe area bölgesi, safeArea'nın alt %30'u.
        if (inputPos.y >= safeArea.yMin + safeArea.height * safeAreaTopThreshold)
        {
            // Üst safe area: 10 kart çiz (Space tuşuna denk).
            for (int i = 0; i < 10; i++)
            {
                DrawAndLogCard();
            }
        }
        else if (inputPos.y <= safeArea.yMin + safeArea.height * safeAreaBottomThreshold)
        {
            // Alt safe area: Sol alt, orta alt, veya sağ alt bölgelere göre farklı aksiyonlar.
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
        else
        {
            // Orta bölgede isterseniz başka aksiyonlar tanımlayabilirsiniz.
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
