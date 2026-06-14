using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BeslenmeVeIlacManager : MonoBehaviour
{
    private GameObject _ilacKutusu;
    private GameObject _ilacPaneli;
    
    private TextMeshProUGUI _sayacText;
    private string _hedefZamanKey = "Ursofalk_HedefZaman";
    
    private int _suSayaci = 0;
    private TextMeshProUGUI _suSayaciText;
    private bool _ilacIcimeHazir = false;


    private void Start()
    {
        // 1. Otomatik UI Butonu: BeslenmePaneli isimli objeyi arayıp bulma
        // (Panelin oyunun basinda kapali/aktif olmama ihtimaline karsi Coroutine ile ariyoruz)
        StartCoroutine(BeslenmePaneliAramaRutini());
    }

    private IEnumerator BeslenmePaneliAramaRutini()
    {
        InfoPanelController infoPanel = null;
        while (infoPanel == null)
        {
            // InfoPanelController bilesenini bul
            infoPanel = FindFirstObjectByType<InfoPanelController>();
            yield return new WaitForSeconds(1f); 
        }

        GameObject beslenmePaneli = infoPanel.gameObject;

        // === Panel Bulundu, "İlaçlar" Butonunu Yarat ===
        GameObject btnGO = new GameObject("IlaclarBtn");
        btnGO.transform.SetParent(beslenmePaneli.transform, false);
        
        RectTransform rt = btnGO.AddComponent<RectTransform>();
        // Sol üst köşe (Kapatma tusuyla cakisacagi icin sola alindi)
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(20f, -20f); 
        rt.sizeDelta = new Vector2(180f, 55f);

        Image img = btnGO.AddComponent<Image>();
        img.color = new Color(0.15f, 0.55f, 0.75f);
        Sprite bgSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        if (bgSprite != null) { img.sprite = bgSprite; img.type = Image.Type.Sliced; }

        Button btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(() => { IlacSisteminiBaslat(); });

        GameObject txtGO = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform, false);
        RectTransform txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero; txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero; txtRT.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "💊 İlaçlar";
        tmp.fontSize = 22f; // Buton kuculdugu icin fontu da biraz kuculttuk
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        
        Material mat = new Material(tmp.font.material);
        mat.SetColor("_FaceColor", Color.white);
        tmp.fontMaterial = mat;
    }

    private void IlacSisteminiBaslat()
    {
        // Önceki açık paneli veya kutuyu temizle
        if (_ilacKutusu != null) Destroy(_ilacKutusu);
        if (_ilacPaneli != null) Destroy(_ilacPaneli);
        
        _suSayaci = 0;
        _ilacIcimeHazir = false;



        // === 2. 3D Kutu ve Panel Tetikleyicisi ===
        _ilacKutusu = GameObject.CreatePrimitive(PrimitiveType.Quad);
        _ilacKutusu.name = "KlinikIlacKutusu";
        // Resmin oranlarina uygun boyutlandirma
        _ilacKutusu.transform.localScale = new Vector3(0.45f, 0.33f, 1f);

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            _ilacKutusu.transform.position = mainCam.transform.position + mainCam.transform.forward * 1.5f;
            _ilacKutusu.transform.LookAt(mainCam.transform);
            _ilacKutusu.transform.Rotate(0, 180f, 0); // Quad ters baktigi icin dondur
        }

        // Texture Kaplama (Arkaplani saydamlastirip ata)
        Renderer rend = _ilacKutusu.GetComponent<Renderer>();
        if (rend != null)
        {
            Material kutuMat = new Material(Shader.Find("Unlit/Transparent"));
            Texture2D kutuTex = Resources.Load<Texture2D>("ursofalkutu");

            #if UNITY_EDITOR
            if (kutuTex == null)
            {
                string[] guids = AssetDatabase.FindAssets("ursofalkutu t:Texture2D");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    kutuTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
            }
            #endif

            if (kutuTex != null)
            {
                Texture2D transparanKutuTex = BeyazArkaplaniSil(kutuTex);
                kutuMat.mainTexture = transparanKutuTex;
            }
            rend.material = kutuMat;
        }

        // Ana Canvas
        Canvas anaCanvas = FindFirstObjectByType<Canvas>();
        if (anaCanvas == null) { Debug.LogWarning("Sahne icinde Canvas bulunamadi!"); return; }

        _ilacPaneli = new GameObject("IlacBilgiPaneli");
        _ilacPaneli.transform.SetParent(anaCanvas.transform, false);
        RectTransform pRT = _ilacPaneli.AddComponent<RectTransform>();
        pRT.anchorMin = new Vector2(0.5f, 0.5f);
        pRT.anchorMax = new Vector2(0.5f, 0.5f);
        pRT.anchoredPosition = Vector2.zero;
        pRT.sizeDelta = new Vector2(750f, 600f);

        Image pBG = _ilacPaneli.AddComponent<Image>();
        pBG.color = new Color(0.08f, 0.08f, 0.12f, 0.98f);
        Sprite bgSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        if (bgSprite != null) { pBG.sprite = bgSprite; pBG.type = Image.Type.Sliced; }

        // === 3. Geliştirilmiş Tıbbi İçerikler ===
        
        // İlaç Resmi (RawImage)
        GameObject resimGO = new GameObject("IlacResmi");
        resimGO.transform.SetParent(_ilacPaneli.transform, false);
        RectTransform resimRT = resimGO.AddComponent<RectTransform>();
        resimRT.anchorMin = new Vector2(0.05f, 0.45f);
        resimRT.anchorMax = new Vector2(0.35f, 0.85f);
        resimRT.offsetMin = Vector2.zero; resimRT.offsetMax = Vector2.zero;
        RawImage resimImg = resimGO.AddComponent<RawImage>();
        Texture2D rTex = Resources.Load<Texture2D>("ursofalkutu");
#if UNITY_EDITOR
        if (rTex == null)
        {
            string[] g = AssetDatabase.FindAssets("ursofalkutu t:Texture2D");
            if (g.Length > 0) rTex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(g[0]));
        }
#endif
        if (rTex != null)
        {
            // Resmin arka planindaki beyazligi sil
            Texture2D transparanTex = BeyazArkaplaniSil(rTex);
            resimImg.texture = transparanTex;
        }

        // Baslik
        TextMeshProUGUI baslikTMP = MetinOlustur(_ilacPaneli.transform, "Baslik",
            "💊 Ursofalk 250mg", 36f, new Color(1f, 0.85f, 0.2f),
            new Vector2(0.40f, 0.75f), new Vector2(0.95f, 0.85f));
        baslikTMP.fontStyle = FontStyles.Bold;
        baslikTMP.alignment = TextAlignmentOptions.TopLeft;

        // Bilgi
        TextMeshProUGUI bilgiTMP = MetinOlustur(_ilacPaneli.transform, "Bilgi",
            "Durum: Karaciğer ameliyatı sonrası safra akışını destekler ve organı korur.\n\nKullanım: Kesinlikle TOK KARNINA, bol su ile içilmelidir.\n\nDoz Aralığı: 12 saatte bir (Günde 2 kez) alınmalıdır.", 22f, Color.white,
            new Vector2(0.40f, 0.40f), new Vector2(0.95f, 0.70f));
        bilgiTMP.alignment = TextAlignmentOptions.TopLeft;

        // SayacText
        _sayacText = MetinOlustur(_ilacPaneli.transform, "SayacText",
            "Sonraki Doz: --:--:--", 42f, new Color(0.3f, 1f, 0.4f),
            new Vector2(0f, 0.28f), new Vector2(1f, 0.40f));
        _sayacText.fontStyle = FontStyles.Bold;

        // SuSayaciText
        _suSayaciText = MetinOlustur(_ilacPaneli.transform, "SuSayaciText",
            "İlaç Öncesi Su Hedefi: 0/2 Bardak", 28f, new Color(0.2f, 0.8f, 1f),
            new Vector2(0f, 0.15f), new Vector2(1f, 0.28f));

        // === 4. Etkileşimli Butonlar ve Mantık ===

        // Su İçtim Butonu
        GameObject suBtnGO = ButonOlustur(_ilacPaneli.transform, "Su İçtim", new Vector2(-220f, -220f), new Color(0.1f, 0.5f, 0.8f));
        suBtnGO.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (_suSayaci < 2)
            {
                _suSayaci++;
                if (_suSayaci >= 2)
                {
                    _ilacIcimeHazir = true;
                    _suSayaciText.text = "İlaç İçime Hazır!";
                    Color readyColor = new Color(0.3f, 1f, 0.4f);
                    _suSayaciText.color = readyColor;
                    Material m = new Material(_suSayaciText.font.material);
                    m.SetColor("_FaceColor", readyColor);
                    _suSayaciText.fontMaterial = m;
                }
                else
                {
                    _suSayaciText.text = $"İlaç Öncesi Su Hedefi: {_suSayaci}/2 Bardak";
                    Color suRenk = new Color(0.2f, 0.8f, 1f);
                    _suSayaciText.color = suRenk;
                    Material m = new Material(_suSayaciText.font.material);
                    m.SetColor("_FaceColor", suRenk);
                    _suSayaciText.fontMaterial = m;
                }
            }
        });

        // İlacı İçtim Butonu
        GameObject icBtnGO = ButonOlustur(_ilacPaneli.transform, "İlacı İçtim", new Vector2(0f, -220f), new Color(0.2f, 0.6f, 0.3f));
        icBtnGO.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (!_ilacIcimeHazir)
            {
                // Suyu tamamlamadan icmeye calisirsa uyar
                _suSayaciText.color = Color.red;
                Material sm = new Material(_suSayaciText.font.material);
                sm.SetColor("_FaceColor", Color.red);
                _suSayaciText.fontMaterial = sm;
                return; 
            }

            // 12 Saat sonrasina hedef zaman belirle ve kaydet
            DateTime hedef = DateTime.Now.AddHours(12);
            PlayerPrefs.SetString(_hedefZamanKey, hedef.ToString("o"));
            PlayerPrefs.Save();

            // Suyu ertesi gun icin sifirla
            _suSayaci = 0;
            _ilacIcimeHazir = false;
            _suSayaciText.text = "İlaç Öncesi Su Hedefi: 0/2 Bardak";
            Color suRenk = new Color(0.2f, 0.8f, 1f);
            _suSayaciText.color = suRenk;
            Material m = new Material(_suSayaciText.font.material);
            m.SetColor("_FaceColor", suRenk);
            _suSayaciText.fontMaterial = m;
        });

        // Kapat Butonu
        GameObject kapatBtnGO = ButonOlustur(_ilacPaneli.transform, "Kapat", new Vector2(220f, -220f), new Color(0.8f, 0.25f, 0.25f));
        kapatBtnGO.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (_ilacPaneli != null) Destroy(_ilacPaneli);
            if (_ilacKutusu != null) Destroy(_ilacKutusu);
            _ilacPaneli = null;
            _ilacKutusu = null;
        });
    }

    private void Update()
    {
        // 3D Obje surekli kameraya baksin (Billboard)
        if (_ilacKutusu != null && Camera.main != null)
        {
            _ilacKutusu.transform.LookAt(Camera.main.transform);
            _ilacKutusu.transform.Rotate(0, 180f, 0);
        }

        if (_sayacText != null)
        {
            string kayitliZaman = PlayerPrefs.GetString(_hedefZamanKey, "");
            if (string.IsNullOrEmpty(kayitliZaman))
            {
                // Hic ilac icilmemis
                _sayacText.text = "İlacı İçme Vakti!";
                Color renk = Color.red;
                _sayacText.color = renk;
                Material mat = new Material(_sayacText.font.material);
                mat.SetColor("_FaceColor", renk);
                _sayacText.fontMaterial = mat;
            }
            else
            {
                DateTime hedef;
                if (DateTime.TryParse(kayitliZaman, out hedef))
                {
                    TimeSpan kalan = hedef - DateTime.Now;
                    if (kalan.TotalSeconds > 0)
                    {
                        _sayacText.text = string.Format("Sonraki Doz: {0:D2}:{1:D2}:{2:D2}", kalan.Hours, kalan.Minutes, kalan.Seconds);
                        Color renk = new Color(0.3f, 1f, 0.4f);
                        _sayacText.color = renk;
                        Material mat = new Material(_sayacText.font.material);
                        mat.SetColor("_FaceColor", renk);
                        _sayacText.fontMaterial = mat;
                    }
                    else
                    {
                        _sayacText.text = "İlacı İçme Vakti!";
                        Color renk = Color.red;
                        _sayacText.color = renk;
                        Material mat = new Material(_sayacText.font.material);
                        mat.SetColor("_FaceColor", renk);
                        _sayacText.fontMaterial = mat;
                    }
                }
            }
        }
    }

    private Texture2D BeyazArkaplaniSil(Texture2D source)
    {
        RenderTexture tmp = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(source, tmp);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmp;
        
        Texture2D result = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        result.Apply();
        
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);

        Color32[] pixels = result.GetPixels32();
        for (int i = 0; i < pixels.Length; i++)
        {
            // Eger piksel beyaza cok yakinsa saydam yap
            if (pixels[i].r > 230 && pixels[i].g > 230 && pixels[i].b > 230)
            {
                pixels[i].a = 0;
            }
        }
        result.SetPixels32(pixels);
        result.Apply();
        
        return result;
    }

    private TextMeshProUGUI MetinOlustur(Transform parent, string ad, string metin, float boyut, Color renk, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = new GameObject(ad);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = metin; tmp.fontSize = boyut;
        tmp.color = renk;
        tmp.alignment = TextAlignmentOptions.Center; 
        tmp.enableWordWrapping = true;
        tmp.raycastTarget = false;

        Material mat = new Material(tmp.font.material);
        mat.SetColor("_FaceColor", renk);
        tmp.fontMaterial = mat;

        return tmp;
    }

    private GameObject ButonOlustur(Transform parent, string yazi, Vector2 pozisyon, Color renk)
    {
        GameObject go = new GameObject("Btn_" + yazi);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pozisyon; rt.sizeDelta = new Vector2(180f, 65f);
        
        Image img = go.AddComponent<Image>(); 
        img.color = renk;
        Sprite bg = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        if (bg != null) { img.sprite = bg; img.type = Image.Type.Sliced; }
        go.AddComponent<Button>();

        GameObject tGO = new GameObject("Text");
        tGO.transform.SetParent(go.transform, false);
        RectTransform tRT = tGO.AddComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = Vector2.zero; tRT.offsetMax = Vector2.zero;
        
        TextMeshProUGUI tmp = tGO.AddComponent<TextMeshProUGUI>();
        tmp.text = yazi; tmp.fontSize = 22f; tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center; 
        tmp.raycastTarget = false;

        Material mat = new Material(tmp.font.material);
        mat.SetColor("_FaceColor", Color.white);
        tmp.fontMaterial = mat;

        return go;
    }
}
