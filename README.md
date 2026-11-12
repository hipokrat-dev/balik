# Derinliklerin Hazinesi

Telefon ve tablet için dikey Unity balık yakalama oyunu. Unity **6000.6.8f1**.

## Aç ve oyna

Unity Hub → Add project from disk → bu klasörü seç. `Assets/Scenes/Fishing.unity` sahnesini aç. Game görünümünde **1080 × 1920** seç, Play'e bas, **Maceraya Başla** düğmesine dokun.

Sahne `FishingGame` tarafından çalışma anında oluşturulur; Play dışında sahnede yalnızca başlangıç nesnesi görünür. Oyun başladığında Canvas, HUD, canlılar ve düğmeler Hierarchy'de incelenebilir.

## Oynanış

- 120 saniye ile başlar. Tek düğme: yukarıdayken **SAL**, su altındayken **ÇEK**.
- Rüzgâr atışta hafif sola / düz / sağa sapma verir. Oyuncu yönlendirmez.
- Kanca yalnızca aşağı inerken ilk değdiği hedefi yakalar. Boş yukarı çekişte hiçbir hedef yakalanmaz. Misina olta ucundaki halkayı ve kancanın gözünü takip eder.
- Tür, boyut, zaman, yön, hız ve sürü sayısı ağırlıklı rastgeledir. 8–14 sn sakin dönemlerde 3–5 sn aralıklarla en fazla 5; 8–13 sn yoğun dönemlerde 1,3–2,6 sn aralıklarla en fazla 10 hedef bulunur. Başlangıçta 5 hedef vardır; boş atışlar normaldir.
- Büyük avlarda sık dokunmak gerekir. Çekiş göstergesi tükenirse av kaçar; boş kanca geri çekilir.
- Balık, harita bonusu ve sandık tekneye teslim edildiğinde değer/45 saniye kazandırır (en az 1 sn). Sayaç 180 saniye ile sınırlıdır.
- Her turda aynı bonus harita parçası 18. ve 48. saniyelerde en fazla iki kez ekranın dışından akıntı yönünde gelir. Ekranı 18–22 saniyede geçer ve diğer kenardan çıkar; ortada belirmez veya kaybolmaz. Yakalanırsa bonus puan ve süre verir; aşama geçişi veya beş parça hedefi yoktur.
- Tek turda skor birikir; amaç süre dolmadan en yüksek skoru yapmaktır. En yüksek skor cihazda PlayerPrefs ile kaydedilir.
- Duraklatma ve uygulama arka plana geçişi sayacı durdurur. Ses aç/kapat tercihi kaydedilir.

## Görsel ve ses kaynakları

Kullanıcının sağladığı 6 nesne sayfası `Assets/Resources/Art` içindedir. Orijinaller değiştirilmez; nesneler Sprite dikdörtgenleriyle ayrılır. Deniz arka planı kullanıcının ekran referansından Imagegen ile temizlenmiştir. UI çerçeveleri ve kanca Unity'de üretilir.

15 Türkçe uyarı kaydı ElevenLabs, **Adam – Deep, Professional and Serious**, Eleven v3 ile üretildi. Kaynak kayıt ve kesim zamanları `Documentation/Audio` içindedir. Sesler çevrimdışı çalışır; API anahtarı gerekmez. SAL/ÇEK konuşmaları devre dışıdır. Yakalama için kısa su efekti ve kesintisiz dalga ambiyansı yerel olarak sentezlenir; kaynak üretim betiği `Tools/make_water_audio.py` içindedir. Diğer olay konuşmaları korunur. Gerçekçi parşömen harita görseli Imagegen ile üretilmiştir.

## Ayarlar ve kod

- `Assets/Scripts/FishingRules.cs`: tür değerleri, ağırlık, derinlik, zorluk, süre ve bonus harita takvimi.
- `Assets/Scripts/FishingGame.cs`: ekranlar, tek düğmeli oyun döngüsü, animasyonlar, kayıt ve ses olayları.
- `Assets/Scripts/SeaArt.cs`: sprite dilimleri ve hacimli UI yüzeyleri.
- `Assets/Editor/BuildGame.cs`: proje hazırlama, kural testleri, Mac ve Android derlemeleri.

## Derleme ve doğrulama

Unity menüsü **Balık → Kural Testleri** saf kuralları doğrular. **Mac Test Sürümü Oluştur** ve **Android APK Oluştur** çıktıları `Builds/` altına yazar. Android için kurulu SDK/NDK ve ARMv7 + ARM64 IL2CPP kullanılır. `Projeyi Hazırla` başlangıç sahnesini yeniden oluşturur; elle sahne düzenlediysen önce ayrı kaydet.

Development Mac çalıştırıcısına `--balik-smoke -batchmode -logFile <dosya>` verilirse atış, geri çekiş, ağır av, kaçış ve süre sonunda sonuç, çıkıştan vazgeçme ve sınırsız skor kontrolü yürütülür. `--balik-preview` oyun ekranı görüntüsünü `/tmp/balik-phone.png` yoluna alır.

Bu sürüm oynanabilir ilk prototiptir. Gerçek cihaz performans testi ve oyun ekonomisi ayarı gerekir. iOS derlemesi için iOS Build Support ve Apple imzalama kurulmalıdır; bu makinede iOS modülü bulunmuyor.

Arka plan müziği: kullanıcının sağladığı `Untitled.wav`, `Assets/Resources/Audio/background-music.wav` olarak eklendi. Stereo müzik %30 seviyesinde döngüde çalar; ses tercihini ve uygulamanın arka plana alınmasını takip eder. Mobilde bellek kullanımını azaltmak için sıkıştırılmış akış (streaming) olarak içe aktarılır.

Seyrek dönemlerde de en az iki serbest normal balık görünür kalır. Çıkışa yaklaşan veya yakalanan balıklar için kenardan yeni balık girer; çöp ve harita bu alt sınıra sayılmaz.

Çöpler (konserve, çorap, ayakkabı, poşet) 0 puan ve 0 saniye verir. Canlılar yalnızca baş/ağız çevresinden yakalanır; iğne ağız noktasına sabitlenir ve gövde bu noktanın etrafında çırpınır.

Uygulama ikonu balık–olta–hazine temalı 3D görseldir. Açılışta “Yener Game Studio” ve “Ayşe Senanın Oyun Serisi” yazıları Lora serif tipografi, altın ışık taraması, su altı huzmeleri ve kademeli geçişlerle belirir; 6.8 saniye sonra ana menü açılır veya dokunarak geçilir. Android 1.0.4, hem ARMv7 hem ARM64 içerir.

Üstteki × çıkış simgesi ve mola menüsü uygulamadan çıkışa erişim sağlar. Onay ekranı turu duraklatır; vazgeçince devam eder. Ağız yakalama alanı genişletildi; gövde/kuyruk teması hâlâ yeterli değildir.

Stüdyo açılışındaki Lora yazı tipi SIL Open Font License ile dağıtılır; lisans `Assets/Resources/Fonts/Lora-LICENSE.txt` içindedir.
