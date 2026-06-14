using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AREgzersizOyunu : MonoBehaviour
{
    private GameObject _egzersizPaneli;
    private Text _skorMetni;
    private Camera _anaKamera;
    
    private int _skor = 0;
    private bool _oyunAktif = false;
    private GameObject _oyunCanvas;

    private GameObject _mevcutKure = null; // Ekranda tek kure olacak
    private bool _sonrakiSagda = true;     // Sag-sol degisimi icin
    private GameObject _nisangahObj;

    private GameObject _kapatButonuObj;

    // Gunluk oynanma limiti sistemi
    public int OynanmaSayisi { get; private set; } = 0;
    public const int MaxOynanma = 10;

    private void Start()
    {
        _anaKamera = Camera.main;

        // Varsa sahnede bul
        _egzersizPaneli = GameObject.Find("ExercisePanel");

        GameObject skorObj = GameObject.Find("SkorMetni");
        if (skorObj != null) _skorMetni = skorObj.GetComponent<Text>();

        // Eger skor metni atanmamissa uret
        if (_skorMetni == null)
        {
            OtomatikUIOlustur();
        }
    }

    private void OtomatikUIOlustur()
    {
        _oyunCanvas = new GameObject("AR_Oyun_Canvas");
        Canvas canvas = _oyunCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        
        CanvasScaler scaler = _oyunCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        
        _oyunCanvas.AddComponent<GraphicRaycaster>();

        GameObject skorObj = new GameObject("SkorMetni");
        skorObj.transform.SetParent(_oyunCanvas.transform, false);
        
        RectTransform skorRT = skorObj.AddComponent<RectTransform>();
        skorRT.anchorMin = new Vector2(0.5f, 1f);
        skorRT.anchorMax = new Vector2(0.5f, 1f);
        skorRT.anchoredPosition = new Vector2(0, -100);
        skorRT.sizeDelta = new Vector2(1000, 100);

        _skorMetni = skorObj.AddComponent<Text>();
        _skorMetni.text = "";
        _skorMetni.alignment = TextAnchor.MiddleCenter;
        _skorMetni.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _skorMetni.fontSize = 54;
        _skorMetni.color = Color.yellow;

        // --- Nisangah (Crosshair) Olusturma ---
        _nisangahObj = new GameObject("Nisangah");
        _nisangahObj.transform.SetParent(_oyunCanvas.transform, false);
        
        // Yatay Cizgi
        GameObject yatayCizgi = new GameObject("Yatay");
        yatayCizgi.transform.SetParent(_nisangahObj.transform, false);
        RectTransform yatayRT = yatayCizgi.AddComponent<RectTransform>();
        yatayRT.anchorMin = new Vector2(0.5f, 0.5f);
        yatayRT.anchorMax = new Vector2(0.5f, 0.5f);
        yatayRT.anchoredPosition = Vector2.zero;
        yatayRT.sizeDelta = new Vector2(60f, 4f);
        Image yatayImg = yatayCizgi.AddComponent<Image>();
        yatayImg.color = new Color(1f, 1f, 1f, 0.7f); // Yari saydam beyaz
        
        // Dikey Cizgi
        GameObject dikeyCizgi = new GameObject("Dikey");
        dikeyCizgi.transform.SetParent(_nisangahObj.transform, false);
        RectTransform dikeyRT = dikeyCizgi.AddComponent<RectTransform>();
        dikeyRT.anchorMin = new Vector2(0.5f, 0.5f);
        dikeyRT.anchorMax = new Vector2(0.5f, 0.5f);
        dikeyRT.anchoredPosition = Vector2.zero;
        dikeyRT.sizeDelta = new Vector2(4f, 60f);
        Image dikeyImg = dikeyCizgi.AddComponent<Image>();
        dikeyImg.color = new Color(1f, 1f, 1f, 0.7f); // Yari saydam beyaz
        
        _nisangahObj.SetActive(false); // Oyun baslayana kadar gizli kalsin

        // --- Kapat Butonu Olusturma ---
        _kapatButonuObj = new GameObject("KapatButonu");
        _kapatButonuObj.transform.SetParent(_oyunCanvas.transform, false);
        RectTransform btnRT = _kapatButonuObj.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.5f, 0.5f);
        btnRT.anchorMax = new Vector2(0.5f, 0.5f);
        btnRT.anchoredPosition = new Vector2(0, -150);
        btnRT.sizeDelta = new Vector2(400, 120);
        
        Image btnImg = _kapatButonuObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        
        Button btn = _kapatButonuObj.AddComponent<Button>();
        btn.onClick.AddListener(KapatButonunaTiklandi);

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(_kapatButonuObj.transform, false);
        RectTransform btnTextRT = btnTextObj.AddComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero;
        btnTextRT.anchorMax = Vector2.one;
        btnTextRT.sizeDelta = Vector2.zero;
        
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.text = "Kapat";
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 48;
        btnText.color = Color.white;

        _kapatButonuObj.SetActive(false);
    }

    private void Update()
    {
        if (!_oyunAktif) return;
        if (_anaKamera == null) return;
        if (_mevcutKure == null) return;

        // Kurenin ekrandaki pozisyonunu viewport koordinatlarina cevir (0-1 arasi)
        Vector3 viewPos = _anaKamera.WorldToViewportPoint(_mevcutKure.transform.position);

        // Kure kameranin onunde mi kontrol et (z > 0)
        if (viewPos.z <= 0f) return;

        // Ekranin tam merkezi (0.5, 0.5) - nisangahin oldugu yer
        float merkezX = 0.5f;
        float merkezY = 0.5f;

        // Kure ile nisangah arasindaki mesafeyi hesapla (viewport koordinatlarinda)
        float mesafe = Vector2.Distance(
            new Vector2(viewPos.x, viewPos.y),
            new Vector2(merkezX, merkezY)
        );

        // Eger kure nisangaha yeterince yakinsa (ekranin %12'si tolerans) -> patlat!
        if (mesafe < 0.12f)
        {
            PatlatKure(_mevcutKure);
        }
    }

    /// <summary>
    /// Kureyi patlat, skoru artir ve siradaki kureyi uret
    /// </summary>
    private void PatlatKure(GameObject kure)
    {
        Destroy(kure);
        _mevcutKure = null;
        SkoruArtir();

        // Oyun hala devam ediyorsa yeni kure uret (kisa bir gecikmeyle)
        if (_oyunAktif && _skor < 5)
        {
            StartCoroutine(GecikmeliKureUret());
        }
    }

    /// <summary>
    /// Kure patladiktan sonra kisa bir bekleme ile yeni kure uretir.
    /// Bu sayede oyuncu bir sonraki kureye hazirlanir.
    /// </summary>
    private IEnumerator GecikmeliKureUret()
    {
        GuncelleMetin("Vurulan: " + _skor + " / 5");
        yield return new WaitForSeconds(1.0f);

        if (_oyunAktif)
        {
            SiradakiKureyiUret();
        }
    }

    /// <summary>
    /// Sag/Sol degisimli olarak bir sonraki kureyi uretir.
    /// </summary>
    private void SiradakiKureyiUret()
    {
        // Sag ve sol yonleri belirle - kamera onune gore
        Vector3 yon;
        if (_sonrakiSagda)
        {
            yon = new Vector3(1f, 0f, 0.3f).normalized; // Sag on
        }
        else
        {
            yon = new Vector3(-1f, 0f, 0.3f).normalized; // Sol on
        }
        _sonrakiSagda = !_sonrakiSagda; // Bir sonraki icin taraf degistir

        KureUret(yon);
    }

    public void OyunuBaslat()
    {
        if (OynanmaSayisi >= MaxOynanma) return; // Limit dolduysa baslatma

        // Paneli kapat
        _egzersizPaneli = GameObject.Find("ExercisePanel"); // Dinamik oldugu icin tekrar arayalim
        if (_egzersizPaneli != null)
            _egzersizPaneli.SetActive(false);

        _skor = 0;
        _mevcutKure = null;
        _sonrakiSagda = true; // Ilk kure sagda baslasin
        GuncelleMetin("Vurulan: 0 / 5");
        _oyunAktif = true;

        if (_nisangahObj != null) _nisangahObj.SetActive(true); // Nisangahi goster

        // Eski kureleri temizle
        foreach(GameObject obj in GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if(obj.name == "AR_Hedef_Kure") Destroy(obj);
        }

        // Ilk kureyi uret (sagda)
        SiradakiKureyiUret();
    }

    private void KureUret(Vector3 yon)
    {
        GameObject kure = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        kure.name = "AR_Hedef_Kure";
        
        kure.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

        Renderer rend = kure.GetComponent<Renderer>();
        if (rend != null)
        {
            Material kirmiziMat = new Material(Shader.Find("Standard"));
            kirmiziMat.color = Color.red;
            rend.material = kirmiziMat;
        }

        if (_anaKamera != null)
        {
            float mesafe = Random.Range(2f, 3f);
            
            Vector3 spawnDir = _anaKamera.transform.rotation * yon;
            spawnDir.y = 0f; // Boy hizasinda sabit kalsin
            spawnDir.Normalize();

            Vector3 dogumNoktasi = _anaKamera.transform.position + (spawnDir * mesafe);
            dogumNoktasi.y += Random.Range(-0.2f, 0.2f);
            kure.transform.position = dogumNoktasi;
        }

        _mevcutKure = kure;
    }

    private void SkoruArtir()
    {
        _skor++;
        GuncelleMetin("Vurulan: " + _skor + " / 5");

        if (_skor >= 5)
        {
            StartCoroutine(OyunBitisRutini());
        }
    }

    private void GuncelleMetin(string metin)
    {
        if (_skorMetni != null)
            _skorMetni.text = metin;
    }

    private IEnumerator OyunBitisRutini()
    {
        _oyunAktif = false;
        if (_nisangahObj != null) _nisangahObj.SetActive(false); // Nisangahi gizle
        
        OynanmaSayisi++; // Oynanma sayisini artir
        
        GuncelleMetin($"Karaciğer Gövde Egzersizi Tamamlandı!\nGünlük İlerleme: {OynanmaSayisi} / {MaxOynanma}");

        yield return new WaitForSeconds(1f);

        // Kapat butonunu goster, kullanici tiklayana kadar bekle
        if (_kapatButonuObj != null) _kapatButonuObj.SetActive(true);
    }

    private void KapatButonunaTiklandi()
    {
        if (_kapatButonuObj != null) _kapatButonuObj.SetActive(false);
        GuncelleMetin(""); // Yaziyi temizle

        // Egzersiz panelini geri ac
        if (_egzersizPaneli != null)
        {
            _egzersizPaneli.SetActive(true);
            
            LiverAssistantManager lam = FindFirstObjectByType<LiverAssistantManager>();
            if (lam != null)
            {
                lam.OpenPanel("Egzersiz");
            }
        }
    }
}
