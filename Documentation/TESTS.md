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
