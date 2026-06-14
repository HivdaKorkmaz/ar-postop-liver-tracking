using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// KaracigerAR — Sahne Kurulum Sihirbazı
/// Geliştiriciye adım adım Unity sahne kurulumunu rehberlik eden Editor Window.
/// </summary>
public class KaracigerARSetup : EditorWindow
{
    private Vector2 _scrollPos;
    private int _currentStep = 0;

    private static readonly string[] Steps = new string[]
    {
        "1️⃣  AR Camera Kurulumu",
        "2️⃣  Image Target Ayarı",
        "3️⃣  Interaction Zones",
        "4️⃣  Vuforia License Key",
        "5️⃣  LiverAssistantManager",
        "6️⃣  UI Canvas & InfoPanel",
        "7️⃣  Final Doğrulama"
    };

    [MenuItem("KaracigerAR/Kurulum Rehberini Aç")]
    public static void ShowSetupWindow()
    {
        var window = GetWindow<KaracigerARSetup>("KaracigerAR Kurulum");
        window.minSize = new Vector2(480, 600);
        window.Show();
    }

    private void OnGUI()
    {
        // Başlık
        GUILayout.Space(10);
        var titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.2f, 0.8f, 0.6f) }
        };
        GUILayout.Label("🪄 Karaciğer AR — Kurulum Sihirbazı", titleStyle);
        GUILayout.Label("Unity 6 + Vuforia 11.4.4", EditorStyles.centeredGreyMiniLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Bu sihirbaz, AR sahnenizi adım adım kurmanıza yardımcı olur.\n" +
            "Her adımı tamamladıktan sonra 'Sonraki Adım' butonuna basın.",
            MessageType.Info);

        GUILayout.Space(10);

        // Adım sekmeleri
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        for (int i = 0; i < Steps.Length; i++)
        {
            bool isActive = i == _currentStep;
            var style = isActive
                ? new GUIStyle(EditorStyles.helpBox) { normal = { background = MakeTex(2, 2, new Color(0.15f, 0.35f, 0.5f, 0.3f)) } }
                : new GUIStyle(EditorStyles.helpBox);

            EditorGUILayout.BeginVertical(style);

            var headerStyle = new GUIStyle(EditorStyles.foldoutHeader)
            {
                fontStyle = isActive ? FontStyle.Bold : FontStyle.Normal,
                normal = { textColor = isActive ? Color.cyan : Color.white }
            };
            GUILayout.Label(Steps[i], headerStyle);

            if (isActive)
            {
                GUILayout.Space(5);
                DrawStepContent(i);
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(4);
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();

        if (_currentStep > 0 && GUILayout.Button("← Önceki", GUILayout.Height(32)))
            _currentStep--;
        GUILayout.FlexibleSpace();
        if (_currentStep < Steps.Length - 1 && GUILayout.Button("Sonraki Adım →", GUILayout.Height(32)))
            _currentStep++;
        if (_currentStep == Steps.Length - 1 && GUILayout.Button("✅ Kurulum Tamamlandı!", GUILayout.Height(32)))
            Close();

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
    }

    private void DrawStepContent(int step)
    {
        switch (step)
        {
            case 0: // AR Camera
                EditorGUILayout.HelpBox(
                    "1. Hierarchy'de 'Main Camera'yı silin (sağ tık → Delete)\n" +
                    "2. Hierarchy'de boş alana sağ tık → 'Vuforia Engine → AR Camera'\n" +
                    "3. AR Camera otomatik olarak eklenir.", MessageType.None);
                if (GUILayout.Button("Yeni Sahne Oluştur"))
                {
                    var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    EditorSceneManager.SaveScene(scene, "Assets/Scenes/KaracigerAR_Main.unity");
                    AssetDatabase.Refresh();
                    Debug.Log("[KaracigerAR] ✅ Boş sahne oluşturuldu: Assets/Scenes/KaracigerAR_Main.unity");
                }
                break;

            case 1: // Image Target
                EditorGUILayout.HelpBox(
                    "1. Hierarchy'de: sağ tık → 'Vuforia Engine → Image Target'\n" +
                    "2. Inspector'da 'Image Target Behaviour' bileşenini bulun\n" +
                    "3. Database: 'KaracigerAR_DB' seçin\n" +
                    "4. Image Target: listeden afişi seçin\n\n" +
                    "⚠️ Eğer 'KaracigerAR_DB' listede görünmüyorsa:\n" +
                    "   Window → Vuforia Configuration → Add Database", MessageType.None);
                break;

            case 2: // Interaction Zones
                EditorGUILayout.HelpBox(
                    "ImageTarget nesnesinin altına 3 Quad ekleyin:\n\n" +
                    "🍽️  Zone_Beslenme:\n" +
                    "   Position: (0, 0.28, 0.01) | Scale: (0.85, 0.22, 1)\n\n" +
                    "🏃  Zone_Egzersiz:\n" +
                    "   Position: (0, 0, 0.01)    | Scale: (0.85, 0.22, 1)\n\n" +
                    "⚠️  Zone_Uyarilar:\n" +
                    "   Position: (0, -0.28, 0.01) | Scale: (0.85, 0.22, 1)\n\n" +
                    "Her birine:\n" +
                    "• Box Collider ekleyin\n" +
                    "• Mesh Renderer'ı KAPATUN (invisible)\n" +
                    "• InteractionZone.cs scriptini ekleyin\n" +
                    "• Tag alanına Zone adını girin (Beslenme / Egzersiz / Uyarilar)", MessageType.None);
                if (GUILayout.Button("Interaction Zone'ları Otomatik Oluştur"))
                    CreateInteractionZones();
                break;

            case 3: // License Key
                EditorGUILayout.HelpBox(
                    "⚡ DURAKLATMA NOKTASI — Vuforia License Key Gerekiyor!\n\n" +
                    "1. developer.vuforia.com adresine gidin\n" +
                    "2. 'Log In' veya ücretsiz hesap oluşturun\n" +
                    "3. 'Develop → License Manager → Get Basic'\n" +
                    "4. Lisans adı girin → Confirm\n" +
                    "5. Oluşturulan lisans anahtarını kopyalayın\n\n" +
                    "Ardından Unity'de:\n" +
                    "Window → Vuforia Configuration → App License Key alanına yapıştırın",
                    MessageType.Warning);
                if (GUILayout.Button("Vuforia Developer Portal'ı Aç"))
                    Application.OpenURL("https://developer.vuforia.com/vui/develop/licenses");
                if (GUILayout.Button("Vuforia Configuration'ı Aç"))
                    EditorApplication.ExecuteMenuItem("Window/Vuforia Configuration");
                break;

            case 4: // LiverAssistantManager
                EditorGUILayout.HelpBox(
                    "1. Hierarchy'de boş bir 'GameObject' oluşturun\n" +
                    "   Adını 'LiverAssistantManager' yapın\n" +
                    "2. Inspector → 'Add Component'\n" +
                    "3. 'LiverAssistantManager' scriptini arayıp ekleyin\n" +
                    "4. Script Inspector'da görünen alanları doldurun:\n" +
                    "   • Info Panel: Canvas altındaki paneli sürükleyin\n" +
                    "   • Video Player: VideoPlayer bileşenini ekleyin\n" +
                    "   • Zone Zones: 3 zone'u listeleyin", MessageType.None);
                break;

            case 5: // UI Canvas
                EditorGUILayout.HelpBox(
                    "1. Hierarchy → sağ tık → 'UI → Canvas'\n" +
                    "   Render Mode: Screen Space - Overlay\n\n" +
                    "2. Canvas altına 'Panel' ekleyin → adı 'InfoPanel'\n" +
                    "3. InfoPanel altına ekleyin:\n" +
                    "   • Image (arka plan)\n" +
                    "   • TextMeshPro - Text (başlık)\n" +
                    "   • TextMeshPro - Text (içerik)\n" +
                    "   • RawImage (video ekranı)\n" +
                    "   • Button (kapat butonu)\n\n" +
                    "4. InfoPanel'e 'InfoPanelController.cs' scriptini ekleyin\n" +
                    "5. InfoPanel'i başlangıçta KAPALI bırakın (Inspector → uncheck)", MessageType.None);
                break;

            case 6: // Final Doğrulama
                EditorGUILayout.HelpBox(
                    "✅ Kontrol Listesi:\n\n" +
                    "☐ Hierarchy'de 'AR Camera' var mı?\n" +
                    "☐ 'ImageTarget' → Database: KaracigerAR_DB\n" +
                    "☐ 3 Interaction Zone ImageTarget'ın altında mı?\n" +
                    "☐ Her Zone'da BoxCollider + InteractionZone script var mı?\n" +
                    "☐ Vuforia License Key girildi mi?\n" +
                    "☐ LiverAssistantManager sahnede var mı?\n" +
                    "☐ InfoPanel Inspector'da kapalı (inactive) mı?\n" +
                    "☐ Console'da kırmızı hata var mı? (Window → General → Console)\n\n" +
                    "Tüm bunlar tamamsa → ▶ Play tuşuna basın!\n" +
                    "Webcam veya arka kamera açılacak, afişe tutun.",
                    MessageType.Info);
                if (GUILayout.Button("Console'u Aç"))
                    EditorApplication.ExecuteMenuItem("Window/General/Console");
                break;
        }
    }

    /// <summary>
    /// Editörden Interaction Zone'ları otomatik oluşturur.
    /// ImageTarget sahnede olmalı.
    /// </summary>
    private void CreateInteractionZones()
    {
        var imageTarget = GameObject.Find("ImageTarget");
        if (imageTarget == null)
        {
            EditorUtility.DisplayDialog("Hata",
                "Sahnede 'ImageTarget' bulunamadı!\nÖnce 2. adımı tamamlayın.", "Tamam");
            return;
        }

        var zones = new (string name, Vector3 pos, Vector3 scale)[]
        {
            ("Zone_Beslenme", new Vector3(0f, 0.28f, 0.01f), new Vector3(0.85f, 0.22f, 1f)),
            ("Zone_Egzersiz", new Vector3(0f, 0f,    0.01f), new Vector3(0.85f, 0.22f, 1f)),
            ("Zone_Uyarilar", new Vector3(0f, -0.28f, 0.01f), new Vector3(0.85f, 0.22f, 1f)),
        };

        foreach (var (zoneName, pos, sc) in zones)
        {
            // Mevcut zone varsa atla
            if (GameObject.Find(zoneName) != null)
            {
                Debug.Log($"[KaracigerAR] {zoneName} zaten mevcut, atlanıyor.");
                continue;
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = zoneName;
            go.transform.SetParent(imageTarget.transform);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = sc;

            // Mesh Renderer'ı kapat (görünmez)
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false;

            // BoxCollider ekle
            var col = go.GetComponent<Collider>() ?? go.AddComponent<BoxCollider>();
            
            // InteractionZone scripti ekle
            var script = go.AddComponent<InteractionZone>();
            script.ZoneName = zoneName.Replace("Zone_", "");

            Debug.Log($"[KaracigerAR] ✅ {zoneName} oluşturuldu");
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[KaracigerAR] ✅ Tüm Interaction Zone'lar oluşturuldu. Sahneyi kaydedin (Ctrl+S).");
    }

    private static Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        var result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
