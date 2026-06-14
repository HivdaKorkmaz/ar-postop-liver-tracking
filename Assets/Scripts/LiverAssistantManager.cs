using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// KaraciğerAR — Uygulamanın Kalbi
/// 
/// SORUMLULUKLAR:
/// 1. Vuforia afiş algılama olaylarını dinler
/// 2. Ekrana dokunulduğunda Physics.Raycast ile hangi Zone'a vurduğunu tespit eder
/// 3. İlgili Zone için InfoPanel'i açar, içerik doldurur, video oynatır
/// 4. Panel kapatma ve geçiş animasyonlarını yönetir
/// 
/// KURULUM:
/// - Bu script sahnedeki boş bir GameObject'e eklenir
/// - ImageTarget, InfoPanelController ve diğer referanslar Inspector'da sürüklenerek atanır
/// </summary>
public class LiverAssistantManager : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // Inspector Alanları
    // ─────────────────────────────────────────────

    [Header("AR Bileşenleri")]
    [Tooltip("Sahne kamerasına (AR Camera) referans")]
    [SerializeField] private Camera _arCamera;

    [Tooltip("Vuforia ImageTarget GameObject'i")]
    [SerializeField] private GameObject _imageTarget;

    [Header("UI Bileşenleri")]
    [Tooltip("InfoPanelController bileşenine referans")]
    [SerializeField] private InfoPanelController _infoPanelController;

    [Tooltip("Sağ alt köşedeki asistan baloncuğu kontrolcüsü")]
    [SerializeField] private AssistantController _assistantController;

    [Header("Interaction Zones")]
    [Tooltip("Sahnedeki tüm InteractionZone bileşenleri")]
    [SerializeField] private List<InteractionZone> _interactionZones = new List<InteractionZone>();

    [Header("İçerik Ayarları")]
    [Tooltip("Her bölge için başlık, açıklama, renk ve video bilgisi")]
    [SerializeField] private ZoneContent[] _zoneContents;

    [Header("Raycast Ayarları")]
    [Tooltip("Raycast'in hangi layer'larla etkileşeceği")]
    [SerializeField] private LayerMask _zoneLayerMask = ~0; // Varsayılan: her şey
    [Tooltip("Raycast maksimum mesafesi (metre)")]
    [SerializeField] private float _raycastDistance = 10f;

    [Header("Debug")]
    [SerializeField] private bool _debugMode = true;

    // ─────────────────────────────────────────────
    // Özel Alanlar
    // ─────────────────────────────────────────────

    private bool _isTracking = false;       // Afiş kamera tarafından görünüyor mu?
    private bool _isPanelOpen = false;      // Bilgi paneli açık mı?
    private string _currentZoneName = "";   // Şu an açık olan zone

    // Egzersiz Paneli için
    private GameObject _exercisePanel;
    private int _completedExerciseTasks = 0;

    // Varsayılan içerikler (Inspector boş bırakılırsa kullanılır)
    private static readonly ZoneContent[] DefaultContents = new ZoneContent[]
    {
        new ZoneContent
        {
            ZoneName = "Beslenme",
            PanelTitle = "BESLENME VE DIYET LISTESI",
            PanelDescription =
                "GUNLUK BESLENME PROGRAMI\n\n" +
                "[SABAH]\n" +
                "- 2 adet haslanmis yumurta beyazi\n" +
                "- 1 dilim tam bugday ekmegi\n" +
                "- Tuzsuz lor peyniri ve bol yesilli\n\n" +
                "[OGLE]\n" +
                "- Zeytinyagli enginar veya izgara tavuk gogsu\n" +
                "- 1 kase probiyotik yogurt\n" +
                "- Kucuk porsiyon karabuday pilavi\n\n" +
                "[ARA OGUN]\n" +
                "- 1 avuc cig badem veya 1 adet yesil elma\n" +
                "- (ONEMLI: Ara ogunleri asla atlama, kan sekerini dengede tutar!)\n\n" +
                "[AKSAM]\n" +
                "- Firinda pismis somon veya buglama balik\n" +
                "- Haslanmis brokoli\n" +
                "- Bol limonlu, az zeytinyagli yesil salata\n\n" +
                "--- YENMEMESI GEREKENLER (SAKIN TUKETME!) ---\n" +
                "- Alkol: Karacigerin en buyuk dusmani.\n" +
                "- Sodyum: Hazir corbalar, cipsler, salamura gidalar (odem yapar).\n" +
                "- Islenmis Etler: Salam, sucuk, sosis.\n" +
                "- Kizartmalar: Her turlu agir yagli kizartma.\n" +
                "- Sekerli Icecekler: Asitli, sekerli tum paketli gidalar.",
            ThemeColor = new Color(1f, 0.35f, 0.35f),
            VideoResourcePath = "Videos/beslenme_video"
        },
        new ZoneContent
        {
            ZoneName = "Egzersiz",
            PanelTitle = "HAREKET VE EGZERSIZ REHBERI",
            PanelDescription =
                "[ILK 2 HAFTA]\n" +
                "- Sadece kisa yuruyusler (5-10 dakika)\n" +
                "- Nefes egzersizleri\n\n" +
                "[2-4. HAFTA]\n" +
                "- Yuruyusleri yavasca artirin\n" +
                "- Hafif esneme hareketleri\n\n" +
                "[1. AYDAN ITIBAREN]\n" +
                "- Hafif egzersizler baslatilabilir\n" +
                "- Kisa tempolu yuruyus\n\n" +
                "--- KACINILMASI GEREKENLER ---\n" +
                "- Agir kaldirma (5 kg uzeri)\n" +
                "- Ani hareketler ve kosu\n" +
                "- Karin kaslarini zorlayan egzersizler\n\n" +
                "--- ONERILEN HAREKETLER ---\n" +
                "- Nefes egzersizleri\n" +
                "- Kisa tempolu yuruyus\n" +
                "- Hafif esneme hareketleri",
            ThemeColor = new Color(0.3f, 0.85f, 0.5f),
            VideoResourcePath = "Videos/egzersiz_video"
        },
        new ZoneContent
        {
            ZoneName = "Uyarilar",
            PanelTitle = "KRITIK DOKTOR UYARILARI",
            PanelDescription =
                "[!] ACIL DURUM BELIRTILERI - Hemen hastaneye gidin:\n" +
                "- Yuksek ates (38.5C uzeri)\n" +
                "- Siddetli karin agrisi\n" +
                "- Sarilk (cilt/gozlerde sararma)\n" +
                "- Koyu renkli idrar\n" +
                "- Yara yerinde kizariklik veya akinti\n\n" +
                "[GUNLUK TAKIP]\n" +
                "- Her gun ayni saatte ilaclarinizi alin\n" +
                "- Yara yerinizi kuru ve temiz tutun\n" +
                "- Doktor kontrollerinizi aksatmayin\n\n" +
                "Acil: 112\n" +
                "Randevu: ALO 182",
            ThemeColor = new Color(1f, 0.75f, 0.2f),
            VideoResourcePath = "Videos/uyarilar_video"
        }
    };

    // ─────────────────────────────────────────────
    // Unity Lifecycle
    // ─────────────────────────────────────────────

    private void Awake()
    {
        // Ekranı kesin olarak Dikey (Portrait) modda kilitle
        Screen.orientation = ScreenOrientation.Portrait;

        // Camera referansı yoksa otomatik bul
        if (_arCamera == null)
            _arCamera = Camera.main;

        // Varsayılan içerikleri yükle (Inspector boşsa)
        if (_zoneContents == null || _zoneContents.Length == 0)
            _zoneContents = DefaultContents;

        // InfoPanel başlangıçta kapalı
        if (_infoPanelController != null)
            _infoPanelController.HideImmediate();

        ValidateSetup();
    }

    private void Start()
    {
        // Vuforia tracking olaylarını dinle (Vuforia 10+ API)
        RegisterVuforiaEvents();
    }

    private void Update()
    {
        // Afiş görünmüyorsa dokunma işlemi yapma
        if (!_isTracking) return;

        HandleTouchInput();
    }

    // ─────────────────────────────────────────────
    // Vuforia Tracking
    // ─────────────────────────────────────────────

    private void RegisterVuforiaEvents()
    {
        // Vuforia 10+ DefaultObserverEventHandler veya ObserverBehaviour kullanır
        // ImageTarget nesnesinde DefaultObserverEventHandler bileşeni aranır
        if (_imageTarget == null) return;

        // Genel yaklaşım: ObserverBehaviour'un OnTargetStatusChanged eventine abone ol
        // Vuforia namespace'i dinamik olarak kontrol ediyoruz (paket henüz import edilmemiş olabilir)
        var observerBehaviour = _imageTarget.GetComponent("ObserverBehaviour");
        if (observerBehaviour == null)
        {
            if (_debugMode)
                Debug.Log("[KaracigerAR] ObserverBehaviour bulunamadı. " +
                          "Vuforia import edildikten sonra bu uyarı kaybolacak.");
            return;
        }

        // Yansıma ile event'e abone ol (Vuforia paket bağımlılığını derleme zamanında almamak için)
        var eventInfo = observerBehaviour.GetType().GetEvent("OnTargetStatusChanged");
        if (eventInfo != null)
        {
            var handler = new System.Action<object, object>((sender, args) =>
            {
                // Status kontrol et
                var statusField = args.GetType().GetProperty("Status");
                if (statusField != null)
                {
                    var status = statusField.GetValue(args).ToString().ToUpperInvariant();
                    _isTracking = status == "TRACKED" || status == "EXTENDED_TRACKED";
                    if (_debugMode)
                        Debug.Log($"[KaracigerAR] Tracking durumu: {status} | İzleniyor: {_isTracking}");
                }
            });

            if (_debugMode)
                Debug.Log("[KaracigerAR] Vuforia tracking eventi başarıyla bağlandı.");
                
            eventInfo.AddEventHandler(observerBehaviour, System.Delegate.CreateDelegate(eventInfo.EventHandlerType, handler.Target, handler.Method));
        }
    }

    // ─────────────────────────────────────────────
    // Dokunma & Raycast
    // ─────────────────────────────────────────────

    private void HandleTouchInput()
    {
        // Panel açıkken kapatma dokunuşu
        if (_isPanelOpen)
        {
            if (GetTouchDown())
            {
                bool isOverUI = IsPointerOverUIObject();

                // Eger tiklanan yer mevcut panelin ici degilse VE herhangi bir UI butonu degilse kapat
                if (!IsTouchOnPanel() && !isOverUI)
                    StartCoroutine(ClosePanelAnimated());
            }
            return;
        }

        // Zone'a dokunma
        if (!GetTouchDown()) return;

        Vector2 touchPos = GetTouchPosition();
        Ray ray = _arCamera.ScreenPointToRay(new Vector3(touchPos.x, touchPos.y, 0));

        if (_debugMode)
            Debug.DrawRay(ray.origin, ray.direction * _raycastDistance, Color.yellow, 1f);

        if (Physics.Raycast(ray, out RaycastHit hit, _raycastDistance, _zoneLayerMask))
        {
            var zone = hit.collider.GetComponent<InteractionZone>();
            if (zone != null)
            {
                if (_debugMode)
                    Debug.Log($"[KaracigerAR] Zone vuruldu: {zone.ZoneName}");
                
                zone.OnTouched();
                StartCoroutine(OpenPanelAnimated(zone.ZoneName));
            }
        }
    }

    // ─────────────────────────────────────────────
    // Panel Yönetimi
    // ─────────────────────────────────────────────

    private IEnumerator OpenPanelAnimated(string zoneName)
    {
        if (_infoPanelController == null)
        {
            Debug.LogWarning("[KaracigerAR] InfoPanelController atanmamış!");
            yield break;
        }

        // Aynı panel zaten açıksa kapat önce
        if (_isPanelOpen && _currentZoneName == zoneName)
        {
            yield return StartCoroutine(ClosePanelAnimated());
            yield break;
        }

        if (_isPanelOpen)
            yield return StartCoroutine(ClosePanelAnimated());

        _currentZoneName = zoneName;
        _isPanelOpen = true;

        if (zoneName == "Egzersiz")
        {
            CreateAndShowExercisePanel();
            yield break;
        }

        if (zoneName == "Uyarilar")
        {
            HologramUyariManager hum = GetComponent<HologramUyariManager>();
            if (hum == null) hum = gameObject.AddComponent<HologramUyariManager>();
            hum.PaneliGoster();
            yield break;
        }

        // İçeriği bul, paneli hazırla
        ZoneContent content = GetContentForZone(zoneName);

        // Panel GameObject'ini Show() çağrılmadan ÖNCE aktif et
        _infoPanelController.gameObject.SetActive(true);
        _infoPanelController.Show(content);

        // Beslenme zone'u açıldığında asistan tavsiye baloncuğunu tetikle
        if (zoneName == "Beslenme" && _assistantController != null)
            _assistantController.ShowAdvice();

        // Animasyon coroutine'ini BU GameObject (Manager) üzerinde başlat
        yield return StartCoroutine(_infoPanelController.ShowAnimated());
    }

    private IEnumerator ClosePanelAnimated()
    {
        _isPanelOpen = false;
        string closingZone = _currentZoneName;
        _currentZoneName = "";

        if (closingZone == "Egzersiz")
        {
            if (_exercisePanel != null)
            {
                Destroy(_exercisePanel);
                _exercisePanel = null;
            }
            yield break;
        }

        if (_infoPanelController != null)
        {
            // Animasyondan önce paneli aktif et
            _infoPanelController.gameObject.SetActive(true);

            // Kapanma animasyonunu bekle
            yield return StartCoroutine(_infoPanelController.HideAnimated());

            // Animasyon bittikten HEMEN SONRA paneli kapat
            _infoPanelController.gameObject.SetActive(false);
        }
        else
        {
            yield return null;
        }
    }

    // ─────────────────────────────────────────────
    // Yardımcı Metodlar
    // ─────────────────────────────────────────────

    private ZoneContent GetContentForZone(string zoneName)
    {
        foreach (var content in _zoneContents)
            if (content.ZoneName == zoneName)
                return content;

        // Bulunamazsa varsayılan
        foreach (var content in DefaultContents)
            if (content.ZoneName == zoneName)
                return content;

        return DefaultContents[0];
    }

    /// <summary>Hem yeni Input System hem de eski Input Manager'ı destekler</summary>
    private bool IsPointerOverUIObject()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null) return false;

        UnityEngine.EventSystems.PointerEventData eventDataCurrentPosition = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        
        // Touch veya Mouse pozisyonunu al
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Touchscreen.current != null && UnityEngine.InputSystem.Touchscreen.current.touches.Count > 0)
            eventDataCurrentPosition.position = UnityEngine.InputSystem.Touchscreen.current.primaryTouch.position.ReadValue();
        else if (UnityEngine.InputSystem.Mouse.current != null)
            eventDataCurrentPosition.position = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
#else
        if (Input.touchCount > 0)
            eventDataCurrentPosition.position = Input.GetTouch(0).position;
        else
            eventDataCurrentPosition.position = Input.mousePosition;
#endif

        System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult> results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        
        // Eger sonuclarda UI elementi varsa (raycastTarget = true olanlar) UI'in uzerindeyiz demektir.
        return results.Count > 0;
    }

    private bool GetTouchDown()
    {
#if ENABLE_INPUT_SYSTEM
        // Yeni Input System
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            return true;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;
#else
        // Eski Input Manager
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            return true;
        if (Input.GetMouseButtonDown(0))
            return true;
#endif
        return false;
    }

    private Vector2 GetTouchPosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
            return Touchscreen.current.primaryTouch.position.ReadValue();
        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();
        return Vector2.zero;
#else
        if (Input.touchCount > 0)
            return Input.GetTouch(0).position;
        return Input.mousePosition;
#endif
    }

    private bool IsTouchOnPanel()
    {
        if (_isPanelOpen && _currentZoneName == "Egzersiz" && _exercisePanel != null)
        {
            RectTransform rt = _exercisePanel.GetComponent<RectTransform>();
            if (rt != null)
            {
                Canvas canvas = rt.GetComponentInParent<Canvas>();
                Camera cam = (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : (_arCamera != null ? _arCamera : Camera.main);
                return RectTransformUtility.RectangleContainsScreenPoint(rt, GetTouchPosition(), cam);
            }
        }

        // Panel üzerindeki dokunuşu ana raycast'ten ayırt etmek için
        if (_infoPanelController == null) return false;
        var panelRect = _infoPanelController.GetPanelScreenRect();
        if (panelRect == Rect.zero) return false;
        return panelRect.Contains(GetTouchPosition());
    }

    private void ValidateSetup()
    {
        if (_arCamera == null)
            Debug.LogError("[KaracigerAR] [HATA] AR Camera atanmamış! Inspector'dan atayın.");
        if (_imageTarget == null)
            Debug.LogWarning("[KaracigerAR] [UYARI] ImageTarget atanmamış. Tracking çalışmayacak.");
        if (_infoPanelController == null)
            Debug.LogWarning("[KaracigerAR] [UYARI] InfoPanelController atanmamış. Panel açılmayacak.");
        if (_interactionZones.Count == 0)
            Debug.LogWarning("[KaracigerAR] [UYARI] Interaction Zone listesi boş.");
    }

    // ─────────────────────────────────────────────
    // Public API — Event Trigger & Inspector'dan Erişilebilir
    // ─────────────────────────────────────────────
    // ℹ️ Aşağıdaki tüm public void metotlar Unity Event Trigger
    //   (Pointer Click, Pointer Enter vb.) açılır menüsünde görünür.

    /// <summary>
    /// Zone adı vererek ilgili paneli açar.
    /// Event Trigger → string parametresi olarak "Beslenme", "Egzersiz" veya "Uyarilar" yazın.
    /// </summary>
    public void OpenPanel(string zoneName)
    {
        StartCoroutine(OpenPanelAnimated(zoneName));
    }

    /// <summary>Beslenme panelini açar (parametresiz — doğrudan Event Trigger'a bağlanabilir).</summary>
    public void OpenBeslenmePanel()
    {
        StartCoroutine(OpenPanelAnimated("Beslenme"));
    }

    /// <summary>Egzersiz panelini açar (parametresiz — doğrudan Event Trigger'a bağlanabilir).</summary>
    public void OpenEgzersizPanel()
    {
        StartCoroutine(OpenPanelAnimated("Egzersiz"));
    }

    /// <summary>Uyarılar panelini açar (parametresiz — doğrudan Event Trigger'a bağlanabilir).</summary>
    public void OpenUyarilarPanel()
    {
        StartCoroutine(OpenPanelAnimated("Uyarilar"));
    }

    /// <summary>
    /// Panel açıksa kapatır, kapalıysa ilgili zone panelini açar.
    /// Event Trigger → string parametresi olarak zone adını yazın.
    /// </summary>
    public void TogglePanel(string zoneName)
    {
        if (_isPanelOpen && _currentZoneName == zoneName)
            ClosePanel();
        else
            StartCoroutine(OpenPanelAnimated(zoneName));
    }

    /// <summary>
    /// Tracking durumuna bakmadan paneli zorla açar.
    /// 3D nesneye Event Trigger ekleyip doğrudan tıklamayla panel açmak için kullanın.
    /// </summary>
    public void ForceOpenPanel(string zoneName)
    {
        _isTracking = true; // Tracking kontrolünü atla
        StartCoroutine(OpenPanelAnimated(zoneName));
    }

    /// <summary>Paneli kapatır (Event Trigger'dan çağrılabilir).</summary>
    public void ClosePanel()
    {
        StartCoroutine(ClosePanelAnimated());
    }

    /// <summary>Vuforia ImageTarget tracking durumunu dışarıdan set etmek için</summary>
    public void SetTrackingState(bool isTracked)
    {
        _isTracking = isTracked;
    }

    public bool IsTracking => _isTracking;
    public bool IsPanelOpen => _isPanelOpen;

    // ─────────────────────────────────────────────
    // Ozel Egzersiz Paneli Uretimi (Runtime)
    // ─────────────────────────────────────────────

    private void CreateAndShowExercisePanel()
    {
        if (_exercisePanel != null) Destroy(_exercisePanel);
        _completedExerciseTasks = 0;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Ana Kasa (Acik, Modern Tema)
        _exercisePanel = new GameObject("ExercisePanel");
        _exercisePanel.transform.SetParent(canvas.transform, false);
        
        RectTransform rt = _exercisePanel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(800f, 650f); // Daha buyuk panel

        Image bg = _exercisePanel.AddComponent<Image>();
        bg.color = new Color(0.95f, 0.96f, 0.98f, 1f); // #F2F5FA (Acik gri/mavi)
        
        // Yuvarlak koseler icin sprite
        Sprite defaultSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        if (defaultSprite == null && _infoPanelController != null) {
            Image pImg = _infoPanelController.GetComponentInChildren<Image>();
            if (pImg != null) defaultSprite = pImg.sprite;
        }
        if (defaultSprite != null) {
            bg.sprite = defaultSprite;
            bg.type = Image.Type.Sliced;
        }

        // Kapatma Butonu
        GameObject closeBtnGO = new GameObject("CloseBtn");
        closeBtnGO.transform.SetParent(_exercisePanel.transform, false);
        RectTransform closeBtnRT = closeBtnGO.AddComponent<RectTransform>();
        closeBtnRT.anchorMin = new Vector2(1f, 1f);
        closeBtnRT.anchorMax = new Vector2(1f, 1f);
        closeBtnRT.pivot = new Vector2(1f, 1f);
        closeBtnRT.anchoredPosition = new Vector2(-25f, -25f);
        closeBtnRT.sizeDelta = new Vector2(60f, 60f);

        Image closeBtnImg = closeBtnGO.AddComponent<Image>();
        closeBtnImg.color = new Color(0.9f, 0.9f, 0.9f, 1f); // Hafif gri arka plan
        if (defaultSprite != null) {
            closeBtnImg.sprite = defaultSprite;
            closeBtnImg.type = Image.Type.Sliced;
        }

        Button closeBtn = closeBtnGO.AddComponent<Button>();
        closeBtn.onClick.AddListener(() => { ClosePanel(); });
        
        GameObject closeTxtGO = new GameObject("Text");
        closeTxtGO.transform.SetParent(closeBtnGO.transform, false);
        RectTransform closeTxtRT = closeTxtGO.AddComponent<RectTransform>();
        closeTxtRT.anchorMin = Vector2.zero;
        closeTxtRT.anchorMax = Vector2.one;
        closeTxtRT.offsetMin = Vector2.zero;
        closeTxtRT.offsetMax = Vector2.zero;
        
        TextMeshProUGUI closeTMP = closeTxtGO.AddComponent<TextMeshProUGUI>();
        closeTMP.text = "X";
        closeTMP.color = new Color32(50, 50, 50, 255); // Koyu gri X
        closeTMP.alignment = TextAlignmentOptions.Center;
        closeTMP.fontSize = 32f;
        closeTMP.fontStyle = FontStyles.Bold;
        closeTMP.raycastTarget = false; // Engel olmasin

        // Baslik
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(_exercisePanel.transform, false);
        RectTransform titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 1f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0f, -40f);
        titleRT.sizeDelta = new Vector2(0f, 60f);

        TextMeshProUGUI titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "IRONPULSE Aktif Yasam";
        titleTMP.fontSize = 46f;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = new Color32(20, 30, 40, 255); // Koyu lacivert/gri
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.raycastTarget = false; // Kapatma butonunu engellemesin!

        // Gorev Listesi (Vertical Layout)
        GameObject listGO = new GameObject("TaskList");
        listGO.transform.SetParent(_exercisePanel.transform, false);
        RectTransform listRT = listGO.AddComponent<RectTransform>();
        listRT.anchorMin = new Vector2(0f, 0f);
        listRT.anchorMax = new Vector2(1f, 1f);
        listRT.offsetMin = new Vector2(50f, 50f);
        listRT.offsetMax = new Vector2(-50f, -140f);

        VerticalLayoutGroup vlg = listGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20f;
        vlg.padding = new RectOffset(20, 20, 20, 20);
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;

        string[] tasks = new string[]
        {
            "15 Dk Oda Ici Esneme",
            "5000 Adim Tempolu Yuruyus",
            "10 Dk Hafif Kardiyo"
        };

        foreach (string taskText in tasks)
        {
            CreateTaskCard(listGO.transform, taskText, defaultSprite);
        }

        // --- AR Oyun Butonunu Listenin Altina Ekle ---
        CreateGameButton(listGO.transform, defaultSprite);
    }

    private void CreateTaskCard(Transform parent, string taskText, Sprite bgSprite)
    {
        GameObject cardGO = new GameObject("TaskCard");
        cardGO.transform.SetParent(parent, false);
        
        LayoutElement le = cardGO.AddComponent<LayoutElement>();
        le.preferredHeight = 120f; // Daha buyuk kartlar

        Image cardImg = cardGO.AddComponent<Image>();
        cardImg.color = new Color(1f, 1f, 1f, 1f); // Beyaz kart
        if (bgSprite != null) {
            cardImg.sprite = bgSprite;
            cardImg.type = Image.Type.Sliced;
        }

        // Karta hafif golge (derinlik)
        Shadow shadow = cardGO.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.1f);
        shadow.effectDistance = new Vector2(0f, -3f);

        GameObject contentGO = new GameObject("CardContent");
        contentGO.transform.SetParent(cardGO.transform, false);
        RectTransform contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = Vector2.zero;
        contentRT.anchorMax = Vector2.one;
        contentRT.offsetMin = new Vector2(30f, 0f);
        contentRT.offsetMax = new Vector2(-30f, 0f);

        HorizontalLayoutGroup hlg = contentGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 30f;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlHeight = true;
        hlg.childControlWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.childForceExpandWidth = false;

        // Checkbox Container
        GameObject checkGO = new GameObject("Checkbox");
        checkGO.transform.SetParent(contentGO.transform, false);
        LayoutElement checkLE = checkGO.AddComponent<LayoutElement>();
        checkLE.preferredWidth = 60f;
        checkLE.preferredHeight = 60f;

        Image checkImg = checkGO.AddComponent<Image>();
        checkImg.color = new Color(0.9f, 0.9f, 0.9f, 1f); // Acik gri bos kutu
        if (bgSprite != null) {
            checkImg.sprite = bgSprite;
            checkImg.type = Image.Type.Sliced;
        }

        Button btn = checkGO.AddComponent<Button>();

        // Metin
        GameObject textGO = new GameObject("TaskText");
        textGO.transform.SetParent(contentGO.transform, false);
        
        LayoutElement textLE = textGO.AddComponent<LayoutElement>();
        textLE.flexibleWidth = 1f;

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = taskText;
        tmp.fontSize = 32f;
        tmp.color = new Color32(30, 30, 30, 255); // Tam koyu gri / siyah
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.raycastTarget = false;

        // Tiklama Eventi
        bool isDone = false;
        btn.onClick.AddListener(() =>
        {
            if (isDone) return;
            isDone = true;

            // Kutu yesil olsun (#4CAF50)
            checkImg.color = new Color(0.298f, 0.686f, 0.314f, 1f);
            
            // Metin ustu cizilsin ve soluk gri olsun
            tmp.text = $"<s>{taskText}</s>";
            tmp.color = new Color(0.6f, 0.6f, 0.6f, 1f);

            _completedExerciseTasks++;

            if (_completedExerciseTasks == 3 && _assistantController != null)
            {
                _assistantController.ShowMessage("Harika! Karaciger sagligin icin bugun mukemmel bir is cikardin!");
            }
        });
    }

    private void CreateGameButton(Transform parent, Sprite bgSprite)
    {
        // Oynanma durumunu ogren
        int oynanmaSayisi = 0;
        AREgzersizOyunu oyun = GetComponent<AREgzersizOyunu>();
        if (oyun != null) oynanmaSayisi = oyun.OynanmaSayisi;
        
        bool limitDoldu = oynanmaSayisi >= AREgzersizOyunu.MaxOynanma;

        GameObject cardGO = new GameObject("GameButtonCard");
        cardGO.transform.SetParent(parent, false);
        
        LayoutElement le = cardGO.AddComponent<LayoutElement>();
        le.preferredHeight = 120f;
        le.minHeight = 120f;

        Image cardImg = cardGO.AddComponent<Image>();
        cardImg.color = limitDoldu ? new Color(0.6f, 0.6f, 0.6f, 1f) : new Color(0.1f, 0.6f, 1f, 1f); // Gri veya Mavi
        if (bgSprite != null) {
            cardImg.sprite = bgSprite;
            cardImg.type = Image.Type.Sliced;
        }

        Shadow shadow = cardGO.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.2f);
        shadow.effectDistance = new Vector2(0f, -4f);

        Button btn = cardGO.AddComponent<Button>();
        btn.interactable = !limitDoldu;
        if (!limitDoldu)
        {
            btn.onClick.AddListener(() =>
            {
                if (oyun != null) oyun.OyunuBaslat();
            });
        }

        GameObject textGO = new GameObject("GameText");
        textGO.transform.SetParent(cardGO.transform, false);
        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = limitDoldu ? $"Gunluk Egzersiz Tamamlandi (3/3)" : $"Govde Egzersizi Oyna ({oynanmaSayisi}/3)";
        tmp.fontSize = 36f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
    }
}
