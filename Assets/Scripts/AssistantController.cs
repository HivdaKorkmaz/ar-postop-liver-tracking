using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Sağ alt köşede beliren asistan baloncuğu kontrolcüsü.
///
/// CANVAS HİYERARŞİSİ:
/// Canvas (Screen Space - Overlay)
///   └── AssistantPanel (Bu script buraya eklenir)
///       ├── AssistantIcon   [Image — asistan ikonu/avatar]
///       └── SpeechBubble
///           └── SpeechText  [TextMeshProUGUI — tavsiye mesajı]
///
/// TÜM GÖRSEl DETAYLAR RUNTIME'DA OTOMATİK OLUŞTURULUR:
///   - Konuşma balonu kuyruğu (AutoCreateTail)
///   - TMP gölge/outline (ApplyTextShadow)
///   - Pop animasyonu, daktilo efekti, ikon süzülme
///
/// KULLANIM:
///   - LiverAssistantManager, Beslenme paneli açıldığında
///     AssistantController.ShowAdvice() çağırır.
///   - Mesaj daktilo efektiyle yazılır, 5 sn görünür, sonra kapanır.
///   - Her çağrıda listeden sırayla farklı bir tavsiye gösterilir.
/// </summary>
public class AssistantController : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // Inspector Referansları
    // ─────────────────────────────────────────────

    [Header("UI Bileşenleri")]
    [Tooltip("Asistan panelinin ana GameObject'i (sağ alt köşe)")]
    [SerializeField] private GameObject _assistantPanel;

    [Tooltip("Tavsiye metninin gösterileceği TextMeshProUGUI")]
    [SerializeField] private TextMeshProUGUI _speechText;

    [Tooltip("Asistan ikon/avatar Image bileşeni (opsiyonel)")]
    [SerializeField] private Image _assistantIcon;

    [Header("Zamanlama Ayarları")]
    [Tooltip("Mesajın ekranda kalma süresi (saniye)")]
    [SerializeField] private float _displayDuration = 5f;

    [Tooltip("Fade-in/out animasyon süresi (saniye)")]
    [SerializeField] private float _fadeDuration = 0.3f;

    [Tooltip("Daktilo efekti: harfler arası bekleme süresi (saniye)")]
    [SerializeField] private float _typewriterSpeed = 0.03f;

    [Tooltip("Pop animasyon süresi (saniye)")]
    [SerializeField] private float _popDuration = 0.25f;

    [Tooltip("İkon süzülme genliği (piksel)")]
    [SerializeField] private float _iconBobAmplitude = 4f;

    [Tooltip("İkon süzülme hızı")]
    [SerializeField] private float _iconBobSpeed = 2f;

    // ─────────────────────────────────────────────
    // Özel Alanlar
    // ─────────────────────────────────────────────

    private CanvasGroup _canvasGroup;
    private Coroutine _activeRoutine;
    private int _currentAdviceIndex = 0;

    // İkon süzülme için başlangıç pozisyonu
    private RectTransform _iconRect;
    private Vector2 _iconBasePos;
    private bool _isIconBobbing = false;

    // Runtime'da oluşturulan kuyruk referansı
    private GameObject _tailObject;

    // Su Takip Sistemi
    private int _currentWaterCount = 0;
    private const int MaxWaterCount = 8;
    private Image _waterFillImage;
    private TextMeshProUGUI _waterText;
    private Coroutine _waterLerpRoutine;

    private static readonly string[] WaterAdvices = new string[]
    {
        "Harika! 1 bardak daha gitti. Vucudunuz size tesekkur ediyor.",
        "Su hayattir! Karacigeriniz su an cok mutlu.",
        "Aynen boyle devam! Toksinleri atmak icin en iyi yol su icmektir."
    };
    private const string WaterGoalReached = "TEBRIKLER! Gunluk 8 bardak su hedefine ulastiniz. Karacigeriniz detoks modunda!";

    /// <summary>
    /// Beslenme kategorisi için sabit tavsiye listesi.
    /// Her çağrıda sırayla bir sonraki mesaj gösterilir.
    /// </summary>
    private static readonly string[] BeslenmeAdvices = new string[]
    {
        "[ONEMLI] Gunde en az 8 bardak su icmeyi unutmayin! Karacigerinizin iyilesmesi icin hidrasyon cok onemli.",
        "[IPUCU] Ogunlerinizi kucuk porsiyonlara bolun. Gunde 5-6 kucuk ogun, karacigerinizi yormuyor.",
        "[YASAK] Ilk 3 ay kizartilmis ve yagli yiyeceklerden tamamen uzak durun.",
        "[VITAMIN] Taze meyve ve sebzeler antioksidan deposu! Her ogune bir porsiyon ekleyin.",
        "[PROTEIN] Protein ihtiyaciniz icin haslanmis yumurta, tavuk gogsu ve mercimek tuketin.",
        "[UYARI] Alkol kesinlikle yasak — karacigeriniz yeniden yapilaniyor, ona zaman verin.",
        "[DIKKAT] Tuz tuketimini azaltin. Gunluk 5 gramin altinda kalmaya calisin.",
        "[ICECEK] Yesil cay ve papatya cayi karaciger dostu iceceklerdir. Sekersiz icin!",
        "[SEBZE] Brokoli, karnabahar ve lahana karaciger detoksunu destekleyen super gidalardir.",
        "[ZAMANLAMA] Gece yatmadan 2-3 saat once son ogununuzu yiyin. Gec yemek karacigeri yorar."
    };

    // ─────────────────────────────────────────────
    // Unity Lifecycle
    // ─────────────────────────────────────────────

    private void Awake()
    {
        // CanvasGroup yoksa ekle (fade animasyonu için gerekli)
        if (_assistantPanel != null)
        {
            _canvasGroup = _assistantPanel.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = _assistantPanel.AddComponent<CanvasGroup>();
        }

        // İkon süzülme referanslarını hazırla
        if (_assistantIcon != null)
        {
            _iconRect = _assistantIcon.GetComponent<RectTransform>();
            if (_iconRect != null)
                _iconBasePos = _iconRect.anchoredPosition;
        }

        // Runtime görsel kurulumları
        AutoCreateTail();
        ApplyTextShadow();
        CreateWaterTrackerWidget();

        // Başlangıçta gizle
        HideImmediate();
    }

    private void Update()
    {
        // İkon süzülme animasyonu (nefes alıyormuş gibi)
        if (_isIconBobbing && _iconRect != null)
        {
            float offsetY = Mathf.Sin(Time.time * _iconBobSpeed) * _iconBobAmplitude;
            _iconRect.anchoredPosition = _iconBasePos + new Vector2(0f, offsetY);
        }
    }

    // ─────────────────────────────────────────────
    // Runtime Görsel Kurulum (Inspector'da Manuel İşlem Gerektirmez)
    // ─────────────────────────────────────────────

    /// <summary>
    /// AssistantPanel'in sol kenarına 20x20, 45° döndürülmüş,
    /// panel ile aynı renkte bir konuşma balonu kuyruğu oluşturur.
    /// </summary>
    private void AutoCreateTail()
    {
        if (_assistantPanel == null) return;

        // Zaten oluşturulmuşsa tekrar oluşturma
        if (_tailObject != null) return;

        // Panel'in arka plan rengini al
        Color panelColor = Color.white;
        Image panelImage = _assistantPanel.GetComponent<Image>();
        if (panelImage != null)
            panelColor = panelImage.color;

        // Kuyruk GameObject oluştur
        _tailObject = new GameObject("BubbleTail");
        _tailObject.transform.SetParent(_assistantPanel.transform, false);

        // Image bileşeni ekle
        Image tailImage = _tailObject.AddComponent<Image>();
        tailImage.color = panelColor;
        tailImage.raycastTarget = false;

        // RectTransform ayarla: sol kenarda, dikey ortada
        RectTransform tailRect = _tailObject.GetComponent<RectTransform>();
        tailRect.sizeDelta = new Vector2(20f, 20f);

        // Sol-orta anchor
        tailRect.anchorMin = new Vector2(0f, 0.5f);
        tailRect.anchorMax = new Vector2(0f, 0.5f);
        tailRect.pivot = new Vector2(0.5f, 0.5f);
        tailRect.anchoredPosition = new Vector2(-7f, 0f); // Sol kenara hafif taşsın

        // 45 derece döndür → kare → baklava şekli (kuyruk görünümü)
        tailRect.localRotation = Quaternion.Euler(0f, 0f, 45f);

        // Panelin arkasına at (sıralama)
        _tailObject.transform.SetAsFirstSibling();
    }

    /// <summary>
    /// SpeechText'e runtime'da TMP Underlay (gölge) ve Outline ekler.
    /// Inspector'da manuel ayar gerektirmez.
    /// </summary>
    private void ApplyTextShadow()
    {
        if (_speechText == null) return;

        // Font material'ın instance'ını al (orijinali bozmamak için)
        Material mat = _speechText.fontMaterial;

        // ── Underlay (Alt Gölge) ──
        // ShaderUtilities.ID_UnderlayColor vb. yerine string keyword kullanıyoruz
        mat.EnableKeyword("UNDERLAY_ON");
        mat.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.45f));
        mat.SetFloat("_UnderlayOffsetX", 1.0f);
        mat.SetFloat("_UnderlayOffsetY", -1.0f);
        mat.SetFloat("_UnderlayDilate", 0.2f);
        mat.SetFloat("_UnderlaySoftness", 0.4f);

        // ── Outline ──
        mat.SetFloat("_OutlineWidth", 0.12f);
        mat.SetColor("_OutlineColor", new Color(0f, 0f, 0f, 0.3f));

        // Değişiklikleri uygula
        _speechText.fontMaterial = mat;
        _speechText.UpdateMeshPadding();
        _speechText.ForceMeshUpdate();
    }

    // ─────────────────────────────────────────────
    // Su Takip Sistemi (Water Tracker)
    // ─────────────────────────────────────────────

    /// <summary>
    /// Ana canvas'in alt-orta kismina kod uzerinden dinamik olarak bir su takip bari ekler.
    /// Manuel Inspector ayari gerektirmez.
    /// </summary>
    private void CreateWaterTrackerWidget()
    {
        if (_assistantPanel == null) return;

        // AssistantPanel'in parent'ini (genelde Canvas'i) buluyoruz ki bagimsiz olsun
        Transform canvasTransform = _assistantPanel.transform.parent;
        if (canvasTransform == null) return;

        // Panelin kendi arkaplan sprite'ini (genelde UISprite) yuvarlak koseler icin aliyoruz
        Sprite defaultSprite = null;
        Image pImg = _assistantPanel.GetComponent<Image>();
        if (pImg != null) defaultSprite = pImg.sprite;

        // 1. Ana Kapsayici (Gorunmez, yatay duzen saglar)
        GameObject widgetGO = new GameObject("WaterTrackerWidget");
        widgetGO.transform.SetParent(canvasTransform, false);
        
        RectTransform widgetRT = widgetGO.AddComponent<RectTransform>();
        widgetRT.anchorMin = new Vector2(0.5f, 0f);
        widgetRT.anchorMax = new Vector2(0.5f, 0f);
        widgetRT.pivot = new Vector2(0.5f, 0f);
        widgetRT.anchoredPosition = new Vector2(0f, 60f); 
        widgetRT.sizeDelta = new Vector2(650f, 80f);

        HorizontalLayoutGroup mainHlg = widgetGO.AddComponent<HorizontalLayoutGroup>();
        mainHlg.spacing = 20f;
        mainHlg.childAlignment = TextAnchor.MiddleCenter;
        mainHlg.childControlHeight = true;
        mainHlg.childControlWidth = true;
        mainHlg.childForceExpandHeight = true;
        mainHlg.childForceExpandWidth = false;

        // 2. Bar Kapsayicisi (Acik gri kapsul)
        GameObject barGO = new GameObject("BarContainer");
        barGO.transform.SetParent(widgetGO.transform, false);
        
        LayoutElement barLE = barGO.AddComponent<LayoutElement>();
        barLE.preferredWidth = 540f;
        barLE.flexibleWidth = 0f;

        Image bgImg = barGO.AddComponent<Image>();
        bgImg.color = new Color(0.65f, 0.65f, 0.68f, 0.9f); // Daha acik gri
        if (defaultSprite != null)
        {
            bgImg.sprite = defaultSprite;
            bgImg.type = Image.Type.Sliced;
        }

        // 3. Iceri Dolan Parlak Mavi Bar (Fill)
        GameObject fillGO = new GameObject("WaterFill");
        fillGO.transform.SetParent(barGO.transform, false);
        
        RectTransform fillRT = fillGO.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        _waterFillImage = fillGO.AddComponent<Image>();
        _waterFillImage.color = new Color(0.161f, 0.714f, 0.965f, 1f); // #29B6F6
        if (defaultSprite != null)
        {
            _waterFillImage.sprite = defaultSprite;
            _waterFillImage.type = Image.Type.Filled;
            _waterFillImage.fillMethod = Image.FillMethod.Horizontal;
        }
        else
        {
            _waterFillImage.type = Image.Type.Filled;
            _waterFillImage.fillMethod = Image.FillMethod.Horizontal;
        }
        _waterFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        _waterFillImage.fillAmount = 0f;

        // 4. Metin (Gunluk Su: 0/8) -> Barin icinde ortali
        GameObject textGO = new GameObject("WaterText");
        textGO.transform.SetParent(barGO.transform, false);
        
        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        _waterText = textGO.AddComponent<TextMeshProUGUI>();
        _waterText.text = "Gunluk Su: 0/8";
        _waterText.fontSize = 24f;
        _waterText.fontStyle = FontStyles.Bold;
        _waterText.color = Color.white;
        _waterText.alignment = TextAlignmentOptions.Center;
        _waterText.enableWordWrapping = false;
        
        Material mat = _waterText.fontMaterial;
        mat.EnableKeyword("UNDERLAY_ON");
        mat.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.5f));
        mat.SetFloat("_UnderlayOffsetX", 0.5f);
        mat.SetFloat("_UnderlayOffsetY", -0.5f);
        _waterText.fontMaterial = mat;

        // 5. Buton (+) -> Barin disinda, widget'in icinde
        GameObject btnGO = new GameObject("AddWaterButton");
        btnGO.transform.SetParent(widgetGO.transform, false);
        
        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.8f, 0.35f, 1f); // Modern canli yesil
        if (defaultSprite != null)
        {
            btnImg.sprite = defaultSprite;
            btnImg.type = Image.Type.Sliced;
        }

        LayoutElement btnLE = btnGO.AddComponent<LayoutElement>();
        btnLE.preferredWidth = 80f; // Sadece arti isareti olacagi icin kare/kucuk
        
        Button btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(OnAddWaterClicked);

        GameObject btnTextGO = new GameObject("BtnText");
        btnTextGO.transform.SetParent(btnGO.transform, false);
        
        RectTransform btnTextRT = btnTextGO.AddComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero;
        btnTextRT.anchorMax = Vector2.one;
        btnTextRT.offsetMin = Vector2.zero;
        btnTextRT.offsetMax = new Vector2(0f, 3f); // Yaziyi dikeyde hafif ortalamak icin

        TextMeshProUGUI btnTMP = btnTextGO.AddComponent<TextMeshProUGUI>();
        btnTMP.text = "+";
        btnTMP.fontSize = 42f;
        btnTMP.fontStyle = FontStyles.Bold;
        btnTMP.color = Color.white;
        btnTMP.alignment = TextAlignmentOptions.Center;
    }

    private void OnAddWaterClicked()
    {
        if (_currentWaterCount >= MaxWaterCount) return;

        _currentWaterCount++;
        if (_waterText != null)
            _waterText.text = $"Gunluk Su: {_currentWaterCount}/{MaxWaterCount}";

        if (_waterLerpRoutine != null)
            StopCoroutine(_waterLerpRoutine);
        
        _waterLerpRoutine = StartCoroutine(LerpWaterFill((float)_currentWaterCount / MaxWaterCount));

        // Asistan mesaji goster (eger baska bir asistan mesaji aktif degilse karismamasi icin)
        if (_activeRoutine == null)
        {
            if (_currentWaterCount == MaxWaterCount)
            {
                ShowMessage(WaterGoalReached);
            }
            else
            {
                string msg = WaterAdvices[Random.Range(0, WaterAdvices.Length)];
                ShowMessage(msg);
            }
        }
    }

    private IEnumerator LerpWaterFill(float targetFill)
    {
        if (_waterFillImage == null) yield break;
        
        float startFill = _waterFillImage.fillAmount;
        float elapsed = 0f;
        float duration = 0.4f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            _waterFillImage.fillAmount = Mathf.Lerp(startFill, targetFill, t);
            yield return null;
        }
        _waterFillImage.fillAmount = targetFill;
    }
    
    // ─────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────

    /// <summary>
    /// Varsayılan beslenme tavsiyesi listesinden sıradaki mesajı gösterir.
    /// Her çağrıda bir sonraki tavsiye gösterilir (döngüsel).
    /// Event Trigger'dan parametresiz çağrılabilir.
    /// </summary>
    public void ShowAdvice()
    {
        string message = BeslenmeAdvices[_currentAdviceIndex];
        _currentAdviceIndex = (_currentAdviceIndex + 1) % BeslenmeAdvices.Length;

        ShowMessage(message);
    }

    /// <summary>
    /// Belirtilen mesajı asistan balonunda gösterir.
    /// Daktilo efekti + Pop animasyonu ile.
    /// Event Trigger'dan string parametresiyle çağrılabilir.
    /// </summary>
    public void ShowMessage(string message)
    {
        if (_assistantPanel == null || _speechText == null)
        {
            Debug.LogWarning("[Asistan] AssistantPanel veya SpeechText atanmamış!");
            return;
        }

        // Zaten çalışan bir gösterim varsa iptal et
        if (_activeRoutine != null)
            StopCoroutine(_activeRoutine);

        // Coroutine başlatmadan önce GameObject'i kesinlikle aktif et!
        _assistantPanel.SetActive(true);
        _activeRoutine = StartCoroutine(ShowAndAutoHide(message));
    }

    /// <summary>
    /// Asistan panelini anında gizler.
    /// Event Trigger veya CloseButton'dan çağrılabilir.
    /// </summary>
    public void HideAssistant()
    {
        if (_activeRoutine != null)
        {
            StopCoroutine(_activeRoutine);
            _activeRoutine = null;
        }

        _isIconBobbing = false;
        HideImmediate();
    }

    // ─────────────────────────────────────────────
    // Ana Coroutine — Göster, Yaz, Bekle, Gizle
    // ─────────────────────────────────────────────

    private IEnumerator ShowAndAutoHide(string message)
    {
        // 1. Metni temizle (daktilo efekti için boş başlayacak)
        _speechText.text = "";

        // 2. Paneli aktif et ve scale sıfırla (Pop için)
        _assistantPanel.SetActive(true);
        _assistantPanel.transform.localScale = Vector3.zero;

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = true;
        }

        // 3. İkon süzülme animasyonunu başlat
        _isIconBobbing = true;

        // 4. Pop animasyonu (0 → 1.1 → 1.0)
        yield return StartCoroutine(PopAnimation());

        // 5. Daktilo efekti ile metni yaz
        yield return StartCoroutine(TypewriterEffect(message));

        // 6. Etkileşimi aç
        if (_canvasGroup != null)
            _canvasGroup.interactable = true;

        // 7. Belirlenen süre kadar bekle
        yield return new WaitForSeconds(_displayDuration);

        // 8. İkon süzülmeyi durdur
        _isIconBobbing = false;
        if (_iconRect != null)
            _iconRect.anchoredPosition = _iconBasePos; // Orijinal pozisyona dön

        // 9. Fade-out animasyonu
        yield return StartCoroutine(FadeCanvas(1f, 0f, _fadeDuration));

        // 10. Paneli kapat
        _assistantPanel.SetActive(false);
        _activeRoutine = null;
    }

    // ─────────────────────────────────────────────
    // Daktilo (Typewriter) Efekti
    // ─────────────────────────────────────────────

    /// <summary>
    /// Metni harf harf 0.03 sn aralıklarla ekrana yazdırır.
    /// </summary>
    private IEnumerator TypewriterEffect(string fullMessage)
    {
        _speechText.text = "";

        for (int i = 0; i < fullMessage.Length; i++)
        {
            _speechText.text = fullMessage.Substring(0, i + 1);
            yield return new WaitForSeconds(_typewriterSpeed);
        }
    }

    // ─────────────────────────────────────────────
    // Pop (Büyüyerek Açılma) Animasyonu
    // ─────────────────────────────────────────────

    /// <summary>
    /// Panel scale: 0 → 1.1 (esneme) → 1.0 (oturma).
    /// EaseOut hissi verir.
    /// </summary>
    private IEnumerator PopAnimation()
    {
        Transform panelTransform = _assistantPanel.transform;
        float halfDuration = _popDuration * 0.6f;   // İlk yarı: 0 → 1.1
        float settleDuration = _popDuration * 0.4f;  // İkinci yarı: 1.1 → 1.0

        // Faz 1: 0 → 1.1 (hızlı büyüme)
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / halfDuration);
            float scale = Mathf.Lerp(0f, 1.1f, t);
            panelTransform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        // Faz 2: 1.1 → 1.0 (yumuşak oturma)
        elapsed = 0f;
        while (elapsed < settleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / settleDuration);
            float scale = Mathf.Lerp(1.1f, 1.0f, t);
            panelTransform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        panelTransform.localScale = Vector3.one;
    }

    // ─────────────────────────────────────────────
    // Fade Animasyonu
    // ─────────────────────────────────────────────

    private IEnumerator FadeCanvas(float from, float to, float duration)
    {
        if (_canvasGroup == null) yield break;

        _canvasGroup.alpha = from;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            _canvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        _canvasGroup.alpha = to;
        _canvasGroup.interactable = to > 0.5f;
        _canvasGroup.blocksRaycasts = to > 0.5f;
    }

    // ─────────────────────────────────────────────
    // Yardımcı
    // ─────────────────────────────────────────────

    private void HideImmediate()
    {
        if (_assistantPanel != null)
        {
            _assistantPanel.SetActive(false);
            _assistantPanel.transform.localScale = Vector3.one; // Scale sıfırla
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        // İkon pozisyonunu sıfırla
        if (_iconRect != null)
            _iconRect.anchoredPosition = _iconBasePos;
    }
}
