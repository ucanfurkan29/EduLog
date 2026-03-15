<div align="center">

# 🎓 EduLog

### Eğitim Yönetim Platformu

*Yazılım eğitmenlerine müfredattan leaderboard'a kadar eksiksiz yönetim araçları*

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![EF Core](https://img.shields.io/badge/EF_Core-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://learn.microsoft.com/en-us/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-LocalDB-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)](https://getbootstrap.com/)
[![Claude AI](https://img.shields.io/badge/Claude_AI-Anthropic-FF6B35?style=for-the-badge)](https://www.anthropic.com/)
[![Gemini AI](https://img.shields.io/badge/Gemini_AI-Google-4285F4?style=for-the-badge&logo=google&logoColor=white)](https://ai.google.dev/)

</div>

---

## 📖 Proje Hakkında

**EduLog**, yazılım eğitmenlerinin ders müfredatlarını, sınıflarını, ödevlerini ve öğrenci performansını tek bir yerden yönetmesini sağlayan modern bir web uygulamasıdır.

Eğitmen müfredatı önceden hazırlar, sınıf açtığında o müfredatı bağlar ve **hafta hafta** öğrencilere içerik açar. Öğrenciler sınıfa katılmak için eğitmenin paylaştığı JoinCode'u kullanır. AI destekli ödev üretimi sayesinde **Claude** veya **Gemini** API üzerinden çoktan seçmeli sorular ve kodlama ödevleri otomatik oluşturulabilir.

> 📋 **Proje Yönetimi:** Geliştirme süreci [Linear](https://linear.app/edulogplatform/project/edulog-platform-4a1191f7e725) üzerinden takip edilmektedir.

---

## ✨ Özellikler

### 👨‍🏫 Eğitmen Paneli
- 📚 **Ders Yönetimi** — Ders oluştur, düzenle, sil (programlama dili seçimi)
- 📋 **Müfredat Yönetimi** — Haftalık konu, not, örnek kod ve PDF kaynakları ekle
- 🏫 **Sınıf Yönetimi** — Sınıf oluştur, JoinCode paylaş, hafta hafta içerik aç
- 📝 **Ödev Sistemi** — Manuel ödev oluştur veya **AI ile çoktan seçmeli sorular / kodlama ödevleri üret**
- 🤖 **AI Kod İnceleme** — Öğrenci kod ödevlerini AI ile otomatik inceletme ve geri bildirim (toplu inceleme desteği)
- 🏆 **Leaderboard** — Sınıf içi puan sıralaması, öğrenci submission'larını görüntüle ve puan gir

### 🎓 Öğrenci Paneli
- 🔑 **JoinCode ile Kayıt** — Sisteme öğrenci olarak katıl
- 📅 **Açık Haftalara Göz At** — Konu, notlar, örnek kodlar, PDF kaynaklar
- 📤 **Ödev Teslimi** — Kod ödevleri (StarterCode ile, syntax highlighting + autocomplete) ve çoktan seçmeli quizler
- 🥇 **Sıralama Sayfası** — Sınıf içindeki kendi pozisyonunu takip et
- 👤 **Profil Sayfası** — Toplam puan, tamamlanan ödev sayısı, performans istatistikleri
- 📋 **Ödevlerim** — Tüm ödevleri ve durumlarını merkezi bir sayfadan takip et

---

## 🏗️ Mimari

```
EduLog/
├── EduLog.Core/        → Entities, Interfaces, DTOs
├── EduLog.Data/        → DbContext, EF Migrations, Repository Pattern
├── EduLog.Services/    → Business Logic, AI Services (Anthropic + Gemini), File Service
└── EduLog.Web/         → ASP.NET Core MVC, Controllers, Razor Views, wwwroot
```

**Katmanlı mimari** ile controller'lar ince tutulmuş, tüm iş mantığı `EduLog.Services` katmanında yürütülmektedir.

---

## 🗄️ Veritabanı Şeması

```
Courses ──── Syllabi ──── SyllabusWeeks ──── WeekResources
                              │
                              ├──── Assignments ──── AssignmentQuestions
                              │          │
                              │          ├──── AIGeneratedCodeTask
                              │          └──── AICodeReview
ClassGroups ─────────────────┘
    │
    ├──── ClassEnrollments ──── ApplicationUser
    │
    └──── Submissions
```

---

## 🤖 AI Entegrasyonu

EduLog, **iki farklı AI provider** destekler. Eğitmen ödev üretirken istediği provider'ı seçebilir:

| Provider | Kullanım Alanı | Model |
|----------|---------------|-------|
| **Anthropic Claude** | Çoktan seçmeli soru + Kod ödevi üretimi + Kod inceleme | `claude-sonnet-4-6` |
| **Google Gemini** | Çoktan seçmeli soru + Kod ödevi üretimi + Kod inceleme | `gemini-3.1-flash-lite-preview` |

### Çoktan Seçmeli Ödev Üretimi

```
Eğitmen "AI ile Ödev Üret" butonuna tıklar → Provider seçer (Claude/Gemini)
        ↓
Haftanın konusu + notlar + örnekler API'ye gönderilir
        ↓
5 adet çoktan seçmeli soru JSON formatında döner
        ↓
Önizleme ekranında gösterilir → Eğitmen onaylar
        ↓
Veritabanına kaydedilir ✓
```

### Kod Ödevi Üretimi & İncelemesi

```
Eğitmen "AI ile Kod Ödevi Üret" seçer → Provider seçer
        ↓
Haftanın konusuna uygun kodlama ödevi + StarterCode üretilir
        ↓
Öğrenci kodu yazar ve gönderir
        ↓
Eğitmen "AI ile İncele" tıklar → Kod otomatik incelenir
        ↓
Doğruluk, Kod Kalitesi, Verimlilik, Öneriler raporlanır ✓
```

---

## 🚀 Kurulum

### Gereksinimler
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server LocalDB](https://www.microsoft.com/sql-server) (Visual Studio ile birlikte gelir)
- [Anthropic API Key](https://www.anthropic.com/) ve/veya [Google Gemini API Key](https://ai.google.dev/) (AI ödev üretici için)

### Adımlar

```bash
# 1. Repository'yi klonla
git clone https://github.com/ucanfurkan29/EduLog.git
cd EduLog

# 2. appsettings.json'u yapılandır
# EduLog.Web/appsettings.json dosyasını düzenle
```

**`appsettings.json`** içeriği:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EduLogDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "AnthropicApi": {
    "ApiKey": "YOUR_ANTHROPIC_API_KEY",
    "Model": "claude-sonnet-4-6"
  },
  "GeminiApi": {
    "ApiKey": "YOUR_GEMINI_API_KEY",
    "Model": "gemini-3.1-flash-lite-preview"
  }
}
```

```bash
# 3. Veritabanını oluştur (EduLog.Web dizininde)
cd EduLog.Web
dotnet ef database update

# 4. Uygulamayı başlat
dotnet run
```

### 🔐 Varsayılan Eğitmen Hesabı (Seed Data)

| Alan | Değer |
|------|-------|
| E-posta | `furkan@edulog.com` |
| Şifre | `Admin123!` |
| Rol | `Instructor` |

> **Not:** İlk başlatmada seed data otomatik oluşturulur. Python, C#, SQL Server, Unity ve Web Yazılım dersleri hazır gelir.

---

## 🔒 Kimlik Doğrulama

| Rol | Erişim | Kayıt Yöntemi |
|-----|--------|---------------|
| `Instructor` | Tüm yönetim paneli | Seed data (manuel) |
| `Student` | Yalnızca kayıtlı sınıflar | Kayıt formu + **JoinCode** zorunlu |

---

## 📁 Teknoloji Yığını

| Katman | Teknoloji |
|--------|-----------|
| Backend | ASP.NET Core 8 MVC |
| ORM | Entity Framework Core 8 |
| Veritabanı | SQL Server (LocalDB) |
| Kimlik Doğrulama | ASP.NET Core Identity |
| Yapay Zeka | Anthropic Claude + Google Gemini |
| Frontend | Razor Views + Bootstrap 5 |
| Dosya Depolama | Local FileSystem (`wwwroot/uploads`) |
| Proje Yönetimi | Linear |

---

## 📏 Kurallar ve Standartlar

- ✅ Repository pattern (`IRepository<T>`) ile veri erişimi
- ✅ Her view için ayrı **ViewModel** (entity doğrudan view'e geçilmez)
- ✅ Her yerde `async/await`
- ✅ DataAnnotations + `ModelState.IsValid` doğrulaması
- ✅ API key'ler `appsettings.json`'dan okunur, hardcode yok
- ✅ Soft delete yok, hard delete uygulanır
- ✅ JoinCode: 6 haneli, alfanümerik, büyük harf, unique (örn: `PY2024`)
- ✅ AI provider seçimi: Claude ve Gemini arasında geçiş yapılabilir

---

## 🗺️ Geliştirme Yol Haritası

### ✅ v1.0 — Çekirdek Platform (Tamamlandı)
- [x] Solution ve proje yapısı
- [x] EF Core + SQL Server bağlantısı + DbContext
- [x] ASP.NET Core Identity + Seed Data
- [x] Courses CRUD
- [x] Syllabi + SyllabusWeeks + WeekResources yönetimi
- [x] ClassGroups oluşturma + JoinCode üretimi
- [x] Öğrenci kayıt akışı (JoinCode doğrulama)
- [x] Hafta açma/kilitleme sistemi
- [x] Öğrenci içerik görünümü
- [x] Assignments CRUD (manuel)
- [x] Submission sistemi (CodeTask + MultipleChoice)
- [x] Claude API entegrasyonu (AI ödev üretici)
- [x] Gemini API entegrasyonu
- [x] AI Kod Ödevi üretimi ve incelemesi
- [x] Leaderboard ve manuel puanlama

### ✅ v1.1 — Bug Fix & UX İyileştirmeleri (Tamamlandı)
- [x] Eğitmen dashboard ve sidebar navigasyon düzeltmeleri
- [x] Öğrenci navbar menü link düzeltmeleri
- [x] AI JSON parse hata mesajlarının iyileştirilmesi
- [x] AI provider varsayılan seçimi (Gemini default)
- [x] Frontend dosya yükleme boyutu validasyonu (20MB)
- [x] Toplu AI kod incelemesi (Tümünü İncele ve Kaydet)
- [x] CodeMirror 5 ile syntax highlighting (dil bazlı renklendirme)
- [x] CodeMirror autocomplete desteği
- [x] Öğrenci profil sayfası ve istatistikler
- [x] Öğrenci sıralama (leaderboard) sayfası
- [x] Öğrenci ödevlerim sayfası

### 📊 v1.2 — Dashboard & İstatistikler
- [ ] Eğitmen dashboard istatistikleri
- [ ] Bildirim sistemi
- [ ] Ödev deadline sistemi
- [ ] Sınıf seçici (Ödevlerim ve Sıralama sayfalarında)
- [ ] Sınıfa Katıl butonu (öğrenci paneli)

### 🚀 v2.0 — Gelişmiş Özellikler
- [ ] PDF dışa aktarma (öğrenci karnesi)
- [ ] Dark mode desteği
- [ ] Responsive mobil iyileştirme
- [ ] Real-time leaderboard (SignalR)
- [ ] JoinCode çakışma optimizasyonu

---

<div align="center">

Made with ❤️ for software education

</div>
