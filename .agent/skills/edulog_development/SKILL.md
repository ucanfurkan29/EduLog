---
name: EduLog Development Rules
description: EduLog projesi için mimari kurallar, kodlama standartları ve iş mantığı kurallarını içerir.
---

# EduLog — Agent Skills & Development Rules

Bu belge, EduLog eğitim yönetim platformunun geliştirilmesi sırasında uyulması gereken mimari kuralları, kodlama standartlarını ve temel iş mantıklarını (business logic) içermektedir.

## Proje Kimliği ve Mimari Yapı
EduLog, ASP.NET Core 8 MVC, Entity Framework Core 8 ve SQL Server kullanılarak geliştirilen çok katmanlı (layered architecture) bir projedir.

- **Katmanlı mimari kullanımı zorunludur.** Tüm kodları Web projesine yığmaktan kaçının.
  - `EduLog.Core`: Entity sınıfları, interfaceler ve DTO'lar burada yer alır.
  - `EduLog.Data`: DbContext (`AppDbContext`), migrationlar ve Repository implementasyonları burada bulunur.
  - `EduLog.Services`: İş mantığı (Business logic), AI servisleri (`AnthropicService`), dosya servisleri burada olmalıdır.
  - `EduLog.Web`: Controller, View ve `wwwroot` dizini.
- **Controller Sınıfları İnce Olmalıdır**: Controller'lar sadece request alır ve response döner; tüm iş mantığı `EduLog.Services` katmanında yürütülmelidir.
- **Repository Katmanı**: Her entity için `IRepository<T>` arayüzü ve EF Core implementasyonu kullanılacaktır.
- **Asenkron Çalışma**: Her yerde `async/await` kullanılmalıdır. (async Controller action'lar, async service metodları).

## Kodlama Standartları
- **İsimlendirme (Naming)**: C# kurallarına uygun olarak sınıflar `PascalCase`, metot içindeki değişkenler `camelCase`, private field'lar `_camelCase` olmalıdır.
- **ViewModel Kullanımı**: Entity'leri View üzerine doğrudan göndermeyin. Her view için mutlaka ViewModel (örn: `LoginViewModel`, `CreateCourseViewModel`) oluşturun.
- **Doğrulama (Validation)**: DataAnnotations kullanın ve controller içerisinde mutlaka `ModelState.IsValid` kontrolü yapın.
- **Hata ve Mesaj Gösterimi**: Hatalar `TempData["Error"]` veya flash message pattern kullanılarak, frontend'de Bootstrap Alert'leri ile anlamlı bir dilde gösterilmelidir.

## Veritabanı ve EF Core Kuralları
- **Migration'lar**: Sadece Code-First migration yaklaşımı kullanılmaktadır. Her yeni tablo/entity sonrasında migration çıkarılmalı.
- **Silme İşlemi**: Soft delete *kullanılmamaktadır*. Doğrudan kalıcı silme işlemi (hard delete) uygulayın.
- **İlişkiler (Foreign Keys)**: Navigation property'leri ve foreign key'ler entity'ler üzerinde açıkça tanımlanmalıdır.
- **Seed Data**: Projenin varsayılan verileri (Instructor yetkisine sahip kullanıcı ve örnek C#, Python, Unity vs. dersleri) başlangıçta oluşturulmalıdır.

## Kimlik Doğrulama ve Rol Sistemi (Auth)
- ASP.NET Core Identity altyapısı mevcuttur. Sadece iki ana yetki (rol) vardır: `Instructor` ve `Student`.
- Eğitmen sayfaları `[Authorize(Roles = "Instructor")]`, öğrenci sayfaları `[Authorize(Roles = "Student")]` niteliğiyle korunmalıdır.
- **Öğrenci Kaydı**: Öğrenciler sisteme dışarıdan kayıt olabilir ancak *JoinCode* (Sınıfa Katılım Kodu) girmek zorundadır. JoinCode yoksa veya geçersizse kayıt işlemi sunucu tarafından reddedilmelidir.

## Frontend ve Görsel Tasarım
- Sistem genelinde özel CSS asgari düzeyde tutulmalı, tasarımı oluşturmak için **Bootstrap 5** class'larından yararlanılmalıdır.
- **Razor Views**: İki farklı ana tasarım mevcuttur: `_LayoutInstructor.cshtml` (sidebar mevcuttur) ve `_LayoutStudent.cshtml` (üst navbar mevcuttur). Tekrarlanan parçalar için Razor Partial View'lar oluşturun.
- **JavaScript Minimum Seviyede**: Sadece form validasyonu ve basit etkileşimler için JavaScript kullanılmaktadır.

## Dosya Yükleme (File Upload)
- Sistemdeki PDF vb. kaynak dosyaları `wwwroot/uploads/{classGroupId}/{weekNumber}/` klasör şablonu altında saklanmalıdır.
- Maksimum dosya kısıtı **20 MB**'tır.
- Yüklemelerde şu uzantılara yalnızca izin verilir: `.pdf`, `.zip`, `.cs`, `.py`, `.txt`, `.md`.

## Yapay Zeka (AI) Entegrasyonu
- **API Key Yönetimi**: Anthropic (Claude) API anahtarı kesinlikle `appsettings.json`'dan okunmalı (`AnthropicApi:ApiKey`), kodun içine hardcode gömülmemelidir. Model olarak `claude-sonnet-4-6` tercih edilmelidir.
- AI çağrısı yapan `AnthropicService` sınıfı `EduLog.Services` içerisinde bulunmalıdır.
- Ödev üretimi (MultipleChoice) için AI'ye gönderilen "prompt" şablonları `string constant` olarak tanımlanmalıdır.
- AI'den dönen JSON verilerinin parse edilmesi işlemi mutlaka `try/catch` blokları içinde yapılmalıdır.
- Yanıtlar doğrudan veritabanına yazılmaz; ödev üretildikten sonra eğitmenin önüne bir "önizleme" arayüzü çıkarılmalı, eğitmen onaylarsa veritabanına kayıt atılmalıdır.

## İş Mantığı (Business Logic) Detayları

### JoinCode Düzeni ve Üretimi
- Öğrenci kayıt kodları şu kurallara uymalıdır: 6 Haneli, Alfanümerik, Yalnızca Büyük Harf. (Örn: `PY2024`).
- Kodlar sistemde eşsiz (unique) olmalıdır. Üretim esnasında çakışma olup olmadığı veritabanından sorgulanmalıdır.
- Üretim örneği: `Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()` 

### Hafta Kilidi ve Açılma Mantığı (Week Unlock)
- `ClassGroups` tablosundaki `CurrentWeek` sütunu son açık haftayı temsil eder. Sınıf ilk açıldığında `0` kabul edilir.
- Eğitmen **"Sonraki Haftayı Aç"** butonuna tıkladığında `CurrentWeek` değeri `1` artırılır. Gelecek haftalara atlanamaz, haftalar sırayla açılır.
- Öğrenciler için view tarafında ve API tarafında: Yalnızca `SyllabusWeek.WeekNumber <= ClassGroup.CurrentWeek` şartını sağlayan haftalar, materyaller ve ödevler görüntülenebilir.

### Puanlama ve Sıralama (Leaderboard)
- **CodeTask (Kodlama) Ödevleri**: Öğrencinin teslim ettiği çalışmaya puan eğitmen tarafından *manuel* olarak girilir.
- **MultipleChoice (Çoktan Seçmeli) Ödevleri**: Sistem otomatik hesaplar. Formül: `(Doğru Cevap Sayısı / Toplam Soru Sayısı) * MaxScore`.
- **Leaderboard Görünümü**: Sınıf içindeki öğrenciler güncel **Toplam Puanlarına** göre sıralanır. Eşit puana sahip öğrenciler arasında **alfabetik** isme göre sıralama algoritması işletilmelidir.
