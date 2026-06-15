# THS (Teknoloji Hazırlık Seviyesi) Değerlendirme Raporu

**Proje Adı:** AR Post-op Liver Tracking (Artırılmış Gerçeklik Destekli Karaciğer Takip Asistanı)
**Değerlendirilen Sistem:** iOS ARKit tabanlı Mobil Uygulama Prototipe
**Güncel THS Seviyesi:** THS 7 (Çalışma Ortamında Sistem Prototipi Gösterimi)

---

## 1. Projenin Güncel Durumu ve THS 7 Gerekçesi

Projemiz an itibarıyla temel araştırma ve teknoloji konsepti aşamalarını başarıyla geçmiş olup, **Teknoloji Hazırlık Seviyesi 7 (THS 7)** aşamasına ulaşmıştır. Bu seviyeye ulaşıldığının temel kanıtları şunlardır:

* **Gerçek Cihazda Çalışabilirlik:** Uygulama sadece bilgisayar ortamında (Unity Editor) değil, doğrudan hedeflenen son kullanıcı cihazında (iOS işletim sistemli iPhone) donanımsal olarak başarıyla derlenmiş (Build) ve test edilmiştir.
* **AR Entegrasyonu:** ARKit altyapısı kullanılarak cihaz kamerası üzerinden fiziksel dünyada 3 boyutlu karaciğer modellemesi yapılmış ve hedeflenen çalışma ortamında (operasyonel ortam) testleri tamamlanmıştır.
* **Etkileşimli Arayüz:** Kullanıcının 3D model üzerindeki belirli noktalara (örn. beslenme/diyet yönlendirmeleri) dokunarak bilgi alabildiği etkileşimli kullanıcı arayüzü (UI/UX) aktif olarak çalışmaktadır.

---

## 2. Tamamlanan Önceki THS Aşamaları

Aşağıdaki aşamalar proje geliştirme süreci boyunca başarıyla tamamlanmıştır:

* **THS 1-3 (Temel Araştırma ve Konsept):** Ameliyat sonrası (post-op) iyileşme sürecindeki hastaların bilgilendirme eksiklikleri tespit edilmiş, AR teknolojisinin bu sorunu çözmek için uygun bir konsept olduğu kanıtlanmıştır.
* **THS 4-5 (Laboratuvar ve Bileşen Doğrulaması):** Unity 3D ortamında karaciğer modellemesi yapılmış, C# dilinde gerekli etkileşim scriptleri yazılmış ve bilgisayar ortamında bileşenlerin birbiriyle uyumlu çalıştığı doğrulanmıştır.
* **THS 6 (İlgili Ortamda Prototip Gösterimi):** Xcode üzerinden Apple geliştirici sertifikaları (Provisioning) ve "Linker" ayarları yapılandırılarak, yazılımın iOS mobil platformuyla tam entegrasyonu sağlanmıştır.

---

## 3. Bir Sonraki Aşama Hedefleri (THS 8 ve THS 9)

Projenin tam bir ticari ürüne veya aktif hastane kullanımına dönüşebilmesi için planlanan sonraki adımlar:

* **THS 8 (Gerçek Sistem Tamamlanması):** Hata ayıklama (Bug-fixing) süreçlerinin tamamlanması, farklı iOS cihazlarında (eski ve yeni nesil iPhone'lar) stabilite testlerinin yapılması.
* **THS 9 (Başarılı Görev İşleyişi):** Uygulamanın App Store sağlık koşullarına uygun hale getirilerek mağazada yayınlanması ve pilot hastalar üzerinde gerçek dünya testlerine başlanması.
