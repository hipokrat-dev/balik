# Derinliklerin Hazinesi

Telefon ve tablet için dikey Unity balık yakalama oyunu. Unity **6000.6.8f1**.

## Aç ve oyna

Unity Hub → Add project from disk → bu klasörü seç. `Assets/Scenes/Fishing.unity` sahnesini aç. Game görünümünde **1080 × 1920** seç, Play'e bas, **Maceraya Başla** düğmesine dokun.

Sahne `FishingGame` tarafından çalışma anında oluşturulur; Play dışında sahnede yalnızca başlangıç nesnesi görünür. Oyun başladığında Canvas, HUD, canlılar ve düğmeler Hierarchy'de incelenebilir.

## Oynanış

- 120 saniye ile başlar. Tek düğme: yukarıdayken **SAL**, su altındayken **ÇEK**.
- Rüzgâr atışta hafif sola / düz / sağa sapma verir. Oyuncu yönlendirmez.
- Kanca yalnızca aşağı inerken ilk değdiği hedefi yakalar. Boş yukarı çekişte hiçbir hedef yakalanmaz. Misina olta ucundaki halkayı ve kancanın gözünü takip eder.
- Tür, boyut, zaman, yön, hız ve sürü sayısı ağırlıklı rastgeledir. 12–22 sn sakin dönemlerde 5–11 sn aralıklarla en fazla 3; 7–12 sn yoğun dönemlerde 1,8–3,5 sn aralıklarla en fazla 8 hedef bulunur. Başlangıçta 2–3 hedef vardır; boş atışlar normaldir.
- Büyük avlarda sık dokunmak gerekir. Çekiş göstergesi tükenirse av kaçar; boş kanca geri çekilir.
- Balık, sandık ve dört çöp türü tekneye teslim edildiğinde değer/45 saniye kazandırır (en az 1 sn). Sayaç 180 saniye ile sınırlıdır.
- Her aşamada aynı harita parçası 18. ve 48. saniyelerde en fazla iki kez, 14 saniyeliğine görünür. Yakalanırsa veya aşama para hedefi tamamlanırsa bir parça kazanılır ve aşama geçilir. Beş parça oyunu kazandırır.
- Para aşama geçişinde sıfırlanır; skor birikir. En yüksek skor cihazda PlayerPrefs ile kaydedilir.
- Duraklatma ve uygulama arka plana geçişi sayacı durdurur. Ses aç/kapat tercihi kaydedilir.

## Görsel ve ses kaynakları

Kullanıcının sağladığı 6 nesne sayfası `Assets/Resources/Art` içindedir. Orijinaller değiştirilmez; nesneler Sprite dikdörtgenleriyle ayrılır. Deniz arka planı kullanıcının ekran referansından Imagegen ile temizlenmiştir. UI çerçeveleri ve kanca Unity'de üretilir.

15 Türkçe uyarı kaydı ElevenLabs, **Adam – Deep, Professional and Serious**, Eleven v3 ile üretildi. Kaynak kayıt ve kesim zamanları `Documentation/Audio` içindedir. Sesler çevrimdışı çalışır; API anahtarı gerekmez. SAL/ÇEK konuşmaları devre dışıdır. Yakalama için kısa su efekti ve kesintisiz dalga ambiyansı yerel olarak sentezlenir; kaynak üretim betiği `Tools/make_water_audio.py` içindedir. Diğer olay konuşmaları korunur. Gerçekçi parşömen harita görseli Imagegen ile üretilmiştir.

## Ayarlar ve kod

- `Assets/Scripts/FishingRules.cs`: tür değerleri, ağırlık, derinlik, zorluk, süre, aşama hedefi, harita takvimi.
- `Assets/Scripts/FishingGame.cs`: ekranlar, tek düğmeli oyun döngüsü, animasyonlar, kayıt ve ses olayları.
- `Assets/Scripts/SeaArt.cs`: sprite dilimleri ve hacimli UI yüzeyleri.
- `Assets/Editor/BuildGame.cs`: proje hazırlama, kural testleri, Mac ve Android derlemeleri.

## Derleme ve doğrulama

Unity menüsü **Balık → Kural Testleri** saf kuralları doğrular. **Mac Test Sürümü Oluştur** ve **Android APK Oluştur** çıktıları `Builds/` altına yazar. Android için kurulu SDK/NDK ve ARM64 IL2CPP kullanılır. `Projeyi Hazırla` başlangıç sahnesini yeniden oluşturur; elle sahne düzenlediysen önce ayrı kaydet.

Development Mac çalıştırıcısına `--balik-smoke -batchmode -logFile <dosya>` verilirse atış, geri çekiş, ağır av, kaçış ve beş parça zafer kontrolü yürütülür. `--balik-preview` oyun ekranı görüntüsünü `/tmp/balik-phone.png` yoluna alır.

Bu sürüm oynanabilir ilk prototiptir. Gerçek cihaz performans testi ve oyun ekonomisi ayarı gerekir. iOS derlemesi için iOS Build Support ve Apple imzalama kurulmalıdır; bu makinede iOS modülü bulunmuyor.
