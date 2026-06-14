using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Ilac3DAsistan : MonoBehaviour
{
    private GameObject _ilacKutusu;
    private GameObject _ilacPaneli;
    private Canvas _anaCanvas;
    
    private float _kalanSure = 43200f; // 12 saat
    private bool _zamanlayiciAktif = true;
    private TextMeshProUGUI _sayacText;

    private void Start()
    {
        _anaCanvas = FindFirstObjectByType<Canvas>();

        // === 1. 3D İLAÇ GÖRÜNÜMÜ (Billboard Quad) ===
        _ilacKutusu = GameObject.CreatePrimitive(PrimitiveType.Quad);
        _ilacKutusu.name = "KlinikIlacKutusu";
        // Resmin en-boy oranina uygun (456x330)
        _ilacKutusu.transform.localScale = new Vector3(0.456f, 0.330f, 1f);
        
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            _ilacKutusu.transform.position = mainCam.transform.position + mainCam.transform.forward * 1.5f;
        }

        // === 2. DİNAMİK RESİM KAPLAMA VE TRANSPARANLIK ===
        Renderer rend = _ilacKutusu.GetComponent<Renderer>();
        if (rend != null)
        {
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
                // Beyaz arkaplani kodla sil (Transparan yap)
                Texture2D transparanTex = BeyazArkaplaniSil(kutuTex);
                
                // Transparan destekleyen Unlit shader
                Material mat = new Material(Shader.Find("Unlit/Transparent"));
                mat.mainTexture = transparanTex;
                rend.material = mat;
            }
            else
            {
                Debug.LogWarning("[Ilac3DAsistan] ursofalkutu gorseli bulunamadi!");
            }
        }
    }

    // Gorseli okunabilir yapip beyaz (veya beyaza yakin) pikselleri transparan yapar
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
            // Eger piksel cok acik renkliyse (beyaza yakinsa) alpha degerini 0 yap
            if (pixels[i].r > 230 && pixels[i].g > 230 && pixels[i].b > 230)
            {
                pixels[i].a = 0;
            }
        }
        result.SetPixels32(pixels);
        result.Apply();
        
        return result;
    }

    private void Update()
    {
        // === KAMERAYA BAKMA (BILLBOARD) ===
        // Objenin her zaman kameraya donuk kalmasini saglayarak 3D illuzyonu verir
        if (_ilacKutusu != null && Camera.main != null)
        {
            _ilacKutusu.transform.LookAt(Camera.main.transform);
            _ilacKutusu.transform.Rotate(0, 180f, 0); // Quad ters baktigi icin ceviriyoruz
        }

        // === 6. ZAMANLAYICI MANTIĞI ===
        if (_zamanlayiciAktif && _ilacPaneli != null && _sayacText != null)
        {
            if (_kalanSure > 0)
            {
                _kalanSure -= Time.deltaTime;
                TimeSpan ts = TimeSpan.FromSeconds(_kalanSure);
                _sayacText.text = string.Format("Sonraki Doz: {0:D2}:{1:D2}:{2:D2}", ts.Hours, ts.Minutes, ts.Seconds);
                
                Color acikYesil = new Color(0.3f, 1f, 0.4f);
                _sayacText.color = acikYesil;
                Material mat = new Material(_sayacText.font.material);
                mat.SetColor("_FaceColor", acikYesil);
                _sayacText.fontMaterial = mat;
            }
            else
            {
                _sayacText.text = "Ilaci Icme Vakti!";
                _sayacText.color = Color.red;
                Material mat = new Material(_sayacText.font.material);
                mat.SetColor("_FaceColor", Color.red);
                _sayacText.fontMaterial = mat;
            }
        }

        // === 3. TIKLAMA (RAYCAST) ETKİLEŞİMİ ===
        if (Input.GetMouseButtonDown(0))
        {
            bool isPointerOverUI = false;
            if (EventSystem.current != null)
            {
                #if UNITY_EDITOR || UNITY_STANDALONE
                isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
                #else
                if (Input.touchCount > 0)
                    isPointerOverUI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
                #endif
            }

            if (!isPointerOverUI && Camera.main != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject.name == "KlinikIlacKutusu")
                    {
                        PaneliAc();
                    }
                }
            }
        }
    }

    // === 4. PROCEDURAL UI ===
    private void PaneliAc()
    {
        if (_anaCanvas == null) _anaCanvas = FindFirstObjectByType<Canvas>();
        if (_anaCanvas == null) { Debug.LogWarning("Sahne icinde Canvas bulunamadi!"); return; }

        if (_ilacPaneli != null) Destroy(_ilacPaneli);

        _ilacPaneli = new GameObject("IlacBilgiPaneli");
        _ilacPaneli.transform.SetParent(_anaCanvas.transform, false);
        RectTransform pRT = _ilacPaneli.AddComponent<RectTransform>();
        pRT.anchorMin = new Vector2(0.5f, 0.5f);
        pRT.anchorMax = new Vector2(0.5f, 0.5f);
        pRT.anchoredPosition = Vector2.zero;
        pRT.sizeDelta = new Vector2(650f, 500f);

        Image pBG = _ilacPaneli.AddComponent<Image>();
        pBG.color = new Color(0.1f, 0.1f, 0.15f, 0.95f); // Yari saydam, koyu renk
        Sprite bgSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        if (bgSprite != null) { pBG.sprite = bgSprite; pBG.type = Image.Type.Sliced; }

        // === 5. TIBBİ İÇERİKLER ===
        // BASLIK
        TextMeshProUGUI baslikTMP = MetinOlustur(_ilacPaneli.transform, "Baslik",
            "💊 Ursofalk 250mg", 36f, new Color(1f, 0.85f, 0.2f), // Sari renk
            new Vector2(0.05f, 0.75f), new Vector2(0.95f, 0.95f));
        baslikTMP.fontStyle = FontStyles.Bold;

        // BILGI
        TextMeshProUGUI bilgiTMP = MetinOlustur(_ilacPaneli.transform, "Bilgi",
            "Durum: Karaciger ameliyati sonrasi safra akisini destekler ve organi korur.\n\nTok karnina alinmalidir.", 24f, Color.white,
            new Vector2(0.05f, 0.45f), new Vector2(0.95f, 0.70f));
        bilgiTMP.alignment = TextAlignmentOptions.TopLeft;

        // SAYAC
        _sayacText = MetinOlustur(_ilacPaneli.transform, "SayacText",
            "Sonraki Doz: 12:00:00", 36f, new Color(0.3f, 1f, 0.4f),
            new Vector2(0f, 0.25f), new Vector2(1f, 0.4f));
        _sayacText.fontStyle = FontStyles.Bold;

        // === 6. İŞLEVSEL BUTONLAR ===
        // ILACI ICTIM BUTONU
        GameObject icBtnGO = ButonOlustur(_ilacPaneli.transform, "Ilaci Ictim", new Vector2(-150f, -180f), new Color(0.2f, 0.6f, 0.3f));
        icBtnGO.GetComponent<Button>().onClick.AddListener(() =>
        {
            _kalanSure = 43200f; // 12 saat
            // Textin rengi Update icinde siradaki frame'de otomatik yesile donecektir
        });

        // KAPAT BUTONU
        GameObject kapatBtnGO = ButonOlustur(_ilacPaneli.transform, "Kapat", new Vector2(150f, -180f), new Color(0.8f, 0.25f, 0.25f));
        kapatBtnGO.GetComponent<Button>().onClick.AddListener(() =>
        {
            Destroy(_ilacPaneli);
            _ilacPaneli = null;
        });
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
        tmp.enableWordWrapping = true; // Metni sigdirmak icin Wrap ayari
        tmp.raycastTarget = false;

        // Font FaceColor cozumu
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
        rt.anchoredPosition = pozisyon; rt.sizeDelta = new Vector2(200f, 65f);
        
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
        tmp.text = yazi; tmp.fontSize = 24f; tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center; 
        tmp.raycastTarget = false;

        Material mat = new Material(tmp.font.material);
        mat.SetColor("_FaceColor", Color.white);
        tmp.fontMaterial = mat;

        return go;
    }
}
