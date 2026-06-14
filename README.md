# 1. Proje Başlığı
AR Tabanlı Post-Op Karaciğer Takip Sistemi

# 2. Proje Özeti
Bu proje, karaciğer ameliyatı geçiren hastaların taburcu olduktan sonraki nekahet dönemlerini yönetmelerine yardımcı olan, Unity ve Vuforia tabanlı bir Artırılmış Gerçeklik (AR) asistanıdır. Hastalara verilen fiziksel bir iyileşme afişini dijital bir bilgi merkezine dönüştürerek; diyet, egzersiz ve doktor uyarılarını interaktif bir arayüzle sunmayı hedefler.

# 3. Problem Tanımı
Ameliyat sonrası evde bakım sürecinde hastalara verilen geleneksel basılı afiş ve broşürler tek yönlü ve karmaşıktır. Hastaların bu statik listeleri takip etmekte zorlanması, yanlış beslenme veya hatalı fiziksel hareket yapma riskini artırırken, iyileşme sürecindeki motivasyonlarını da düşürmektedir.

# 4. Amaçlar
* Hastaların iyileşme sürecini oyunlaştırılmış ve interaktif bir yapıya kavuşturmak.
* Görsel ve işitsel rehberlik sayesinde hastanın tedaviye olan uyumunu artırmak.
* Karmaşık tıbbi listeleri 2D AR panelleri aracılığıyla sadeleştirerek okunabilirliği yükseltmek.
* Donanım maliyetini minimize ederek sadece bir akıllı telefon kamerasıyla sistemi erişilebilir kılmak.

# 5. Temel Kavramlar
* **Artırılmış Gerçeklik (AR):** Fiziksel dünya ile dijital nesnelerin eş zamanlı olarak etkileşime girdiği ortam.
* **Image Target:** AR kamerasının dijital içerikleri göstermek için referans aldığı tetikleyici görsel (fiziksel afiş).
* **Post-Op:** Ameliyat sonrası iyileşme ve bakım dönemi.
* **UI (User Interface):** Kullanıcının sistemle etkileşime girdiği görsel paneller ve butonlar.

# 6. Kullanılan Teknolojiler
* **Oyun Motoru:** Unity 3D
* **AR Kütüphanesi:** Vuforia Engine
* **Programlama Dili:** C#
* **Versiyon Kontrol:** Git & GitHub
* **Geliştirme Ortamı:** macOS (Apple Silicon), Xcode (iOS Derleme için)

# 7. Proje Kapsamı
Proje, karaciğer ameliyatı sonrası evde dinlenen hastaların diyet listelerini, 30 saniyelik temel nefes/hareket videolarını ve kritik uyarı bildirimlerini kapsar. Sistem, internet bağlantısı gerektirmeyen (offline) lokal bir mobil uygulama olarak çalışacak şekilde sınırlandırılmıştır; bu aşamada doktorların uzaktan veri girebileceği bir bulut/veritabanı mimarisi kapsam dışı bırakılmıştır.

# 8. Beklenen Çıktılar
* Duvara asılmaya hazır, Vuforia tarafından tanınabilecek zengin desenli fiziksel bir "İyileşme Afişi" tasarımı.
* Akıllı telefonlara (iOS/Android) yüklenebilir, Image Target üzerinden çalışan bir mobil AR uygulaması (prototip).
* Sistemin kararlılığını gösteren RAMS (Güvenilirlik, Kullanılabilirlik, Bakım Yapılabilirlik, Güvenlik) analiz raporu.

# 9. Katkıda Bulunanlar
* **Hivda Korkmaz** - Yazılım Geliştirici ve Proje Yürütücüsü

# 10. Kaynaklar
* Unity Documentation (UI Management & Video Player)
* Vuforia Engine Developer Portal (Image Target Configuration)

# 11. Anahtar Kelimeler
Artırılmış Gerçeklik, AR, Unity 3D, Vuforia, Sağlık Teknolojileri, Karaciğer, Post-Op, Image Target, C#.
