# EduLog — Agent Skills

## Project Identity

Bu proje EduLog adlı bir eğitim yönetim platformudur. ASP.NET Core 8 MVC + Entity Framework Core + SQL Server kullanır. Layered architecture: Web, Core, Data, Services katmanları.

## Architecture Rules

- **Katmanlı mimari zorunludur.** Her şeyi Web projesine koyma.
  - Entities ve interface'ler → `EduLog.Core`
  - DbContext, migrations, repository implementasyonları → `EduLog.Data`
  - İş mantığı, servisler → `EduLog.Services`
  - Controller, View, wwwroot → `EduLog.Web`
- Controller'lar ince olmalı. İş mantığı service katmanında.
- Her entity için `IRepository<T>` interface'i ve EF implementasyonu yaz.
- Async/await her yerde kullanılmalı (async Controller actions, async service methods).

## Coding Standards

- C# naming: PascalCase sınıflar, camelCase değişkenler, _camelCase private fields.
- View model'lar her zaman ayrı sınıf olarak tanımlanmalı, entity'leri doğrudan View'a geçirme.
- Validation için DataAnnotations kullan, controller'da `ModelState.IsValid` kontrol et.
- Her hata kullanıcıya anlamlı mesajla gösterilmeli (TempData["Error"] pattern).

## Database & EF Core

- Code-first migrations kullan. Her yeni entity'den sonra migration oluştur.
- Soft delete yok, gerçek delete yeterli bu projede.
- Tüm foreign key'ler açıkça tanımlanmalı.
- Seed data: Instructor kullanıcısı ve örnek dersler (C#, Python, SQL, Unity, Web).

## Authentication & Authorization

- ASP.NET Core Identity üzerine kurulu, iki rol: `Instructor`, `Student`.
- Instructor sayfaları `[Authorize(Roles = "Instructor")]` ile korunmalı.
- Student sayfaları `[Authorize(Roles = "Student")]` ile korunmalı.
- Kayıt formunda JoinCode alanı zorunlu. JoinCode geçersizse kayıt reddedilmeli.

## Frontend Rules

- Bootstrap 5 kullan, özel CSS minimum düzeyde tut.
- Razor partial view'ları tekrarlayan bileşenler için kullan.
- JavaScript minimum: yalnızca form validasyonu ve basit UI etkileşimleri.
- Flash mesajları için TempData + Bootstrap alert komponenti.

## File Upload

- PDF ve dosyalar `wwwroot/uploads/{classGroupId}/{weekNumber}/` altına kaydedilmeli.
- Maksimum dosya boyutu: 20MB.
- İzin verilen uzantılar: .pdf, .zip, .cs, .py, .txt, .md

## AI Integration (Claude API)

- API key `appsettings.json`'dan okunmalı, asla hardcode edilmemeli.
- `AnthropicService` sınıfı `EduLog.Services` içinde olmalı.
- AI ödev üretici için prompt şablonu sabit bir string constant olmalı.
- AI'dan gelen JSON parse edilmeden önce try/catch ile sarılmalı.
- Üretilen sorular önizleme ekranında gösterilmeli, kaydedilmeden önce eğitmen onayı alınmalı.

## JoinCode Generation

- 6 haneli alfanumerik, büyük harf: örn. `PY2024`
- Unique olmalı, üretimde çakışma kontrolü yapılmalı.
- `Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()` kullanılabilir.

## Week Unlock Logic

- `ClassGroups.CurrentWeek` alanı açık olan son haftayı tutar.
- Eğitmen "Sonraki Haftayı Aç" butonuna tıkladığında `CurrentWeek++`.
- Öğrenci yalnızca `SyllabusWeek.WeekNumber <= ClassGroup.CurrentWeek` olan haftalara erişebilir.
- Aynı şekilde ödevler de yalnızca açık haftalardakiler görünür.

## Leaderboard & Scoring

- CodeTask ödevleri için puan eğitmen tarafından manuel girilir.
- MultipleChoice (quiz) ödevleri otomatik puanlanır: doğru sayısı / toplam soru × MaxScore.
- Leaderboard: bir sınıf içindeki öğrenciler toplam puana göre sıralanır.
- Eşit puanda alfabetik sıralama yapılır.
