using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// KaracigerAR — Otomatik Proje Kurulum Sihirbazı
/// Unity Editor açıldığında çalışır ve .unitypackage dosyalarını sırayla import eder.
/// Sonrasında sahne kurulum rehberini gösterir.
/// </summary>
[InitializeOnLoad]
public static class AutoImporter
{
    // Proje kök dizinini al (Assets'in bir üstü)
    private static readonly string ProjectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
    
    // Import edilecek paketlerin sırası önemli: önce Vuforia, sonra veritabanı
    private static readonly string[] PackageOrder = new string[]
    {
        "add-vuforia-package-11-4-4.unitypackage",
        "KaracigerAR_DB.unitypackage"
    };

    private const string PrefsKey_VuforiaImported = "KaracigerAR_VuforiaImported";
    private const string PrefsKey_DBImported      = "KaracigerAR_DBImported";

    static AutoImporter()
    {
        // Domain reload sonrası tekrar çalışmayı önle
        EditorApplication.delayCall += TryImportPackages;
    }

    private static void TryImportPackages()
    {
        bool vuforiaImported = EditorPrefs.GetBool(PrefsKey_VuforiaImported, false);
        bool dbImported      = EditorPrefs.GetBool(PrefsKey_DBImported, false);

        if (!vuforiaImported)
        {
            string vuforiaPath = Path.Combine(ProjectRoot, PackageOrder[0]);
            if (File.Exists(vuforiaPath))
            {
                Debug.Log("[KaracigerAR] Vuforia Engine paketi import ediliyor...");
                AssetDatabase.ImportPackage(vuforiaPath, false); // false = interactive dialog gösterme
                EditorPrefs.SetBool(PrefsKey_VuforiaImported, true);
                Debug.Log("[KaracigerAR] ✅ Vuforia Engine başarıyla import edildi.");
            }
            else
            {
                Debug.LogWarning($"[KaracigerAR] ⚠️ Vuforia paketi bulunamadı: {vuforiaPath}");
            }
            return; // Vuforia import sonrası domain reload olacak, DB sonraki çalışmada import edilecek
        }

        if (!dbImported)
        {
            string dbPath = Path.Combine(ProjectRoot, PackageOrder[1]);
            if (File.Exists(dbPath))
            {
                Debug.Log("[KaracigerAR] KaracigerAR_DB veritabanı import ediliyor...");
                AssetDatabase.ImportPackage(dbPath, false);
                EditorPrefs.SetBool(PrefsKey_DBImported, true);
                Debug.Log("[KaracigerAR] ✅ KaracigerAR_DB başarıyla import edildi.");
                
                // Her iki paket de import edildi, kurulum rehberini göster
                EditorApplication.delayCall += KaracigerARSetup.ShowSetupWindow;
            }
            else
            {
                Debug.LogWarning($"[KaracigerAR] ⚠️ DB paketi bulunamadı: {dbPath}");
            }
        }
    }

    /// <summary>
    /// Import durumunu sıfırlar (geliştirme sırasında kullanışlı)
    /// </summary>
    [MenuItem("KaracigerAR/Import Durumunu Sıfırla")]
    public static void ResetImportFlags()
    {
        EditorPrefs.DeleteKey(PrefsKey_VuforiaImported);
        EditorPrefs.DeleteKey(PrefsKey_DBImported);
        Debug.Log("[KaracigerAR] Import durumu sıfırlandı. Editörü yeniden başlatın.");
    }

    [MenuItem("KaracigerAR/Paketleri Manuel Import Et")]
    public static void ManualImport()
    {
        EditorPrefs.DeleteKey(PrefsKey_VuforiaImported);
        EditorPrefs.DeleteKey(PrefsKey_DBImported);
        TryImportPackages();
    }
}
