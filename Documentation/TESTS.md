# Doğrulama — 11 Kasım 2026

Unity 6000.6.8f1 üzerinde:

- C# derlemesi başarılı.
- Kural testleri: haritanın 18/48 saniyedeki iki fırsatı, üçüncü fırsatın engellenmesi, alınan parçanın yeniden çıkmaması, 180 sn süre üst sınırı, çekiş tükenmesi, aşama hedefinin artması, tüm 16 yakalanabilir nesnenin rastgele havuzdan çıkabilmesi geçti.
- Çalışan Development Player testleri: SAL → inme, ÇEK → geri dönüş, ağır hedef → mücadele, dokunma → yukarı ilerleme, kaçış → boş kanca, beş parça → zafer geçti.
- Grafik modunda 540×960, 390×844 ve 768×1024 ekran görüntüleri alınıp incelendi. Son görüntü testlerinin loglarında Exception yok. Görüntüler Previews klasöründe.
- 15 ElevenLabs WAV klibi import edildi. Ana kayıttan kesim zamanları Audio/cues.json içinde.
- Mac Development Player ve Android ARM64 IL2CPP APK derlemeleri başarılı.
- APK fiziksel Android cihaza yüklenmedi; iOS build/test yapılmadı. Uzun süreli performans ve ekonomi dengesi henüz doğrulanmadı.

Test çıktıları:

```
BALIK_RULES_PASS: two map opportunities, collection, time cap, escape, progression, all spawn species
BALIK_PLAYMODE_PASS: cast, reel, heavy tap, escape, five-piece victory
BALIK_MAC_BUILD_PASS
```

## Oynanış revizyonu

- Yukarı dönen boş kancanın çakışan hedefi yakalamadığı, aynı hedefin aşağı inişte yakalandığı doğrulandı.
- Misinanın olta ucu bağlantısı -8°, 0°, +8° tekne dönüşlerinde 0,1 UI biriminden küçük hata ile doğrulandı.
- Güncel Mac görüntü testi hatasız; telefon ekranı `Previews/revised-phone.png`.
- SAL/ÇEK konuşmaları devre dışı; yerel sentezlenen dalga döngüsü ve su efekti eklendi.
- Gerçekçi parşömen sprite ve sakin/yoğun balık geçiş dönemleri eklendi.

## Akıntı ve yoğunluk ayarı

- Başlangıç 5 hedef; sakin dönem 8–14 sn, üretim aralığı 3–5 sn, üst sınır 5. Yoğun dönem 8–13 sn, üretim aralığı 1,3–2,6 sn, üst sınır 10.
- Harita sprite'ının iki yönde de ekran dışında başladığı, ekran içinden geçtiği ve karşı kenardan tamamen çıktığı çalışan oyun testinde doğrulandı. İki fırsat takvimi değişmedi.
- Rüzgâr HUD yazısı kaldırıldı; atış sapması ve harita akıntı yönü korunuyor.

- Kullanıcının `Untitled.wav` müziği stereo streaming klip olarak eklendi; kaynak dosya birebir karşılaştırıldı. Mac derlemesi ve oyun döngüsü testi geçti.
- Balık alt sınırı: normal zamanlı doğumlar olmadan 120 saniyelik simülasyonda ve tekrarlanan balık kaldırmalarında en az iki serbest balığın görünür kalması doğrulandı (`BALIK_PLAYMODE_PASS`).
