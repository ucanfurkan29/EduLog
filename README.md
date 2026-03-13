<div align="center">

# 🎓 EduLog

### Eğitim Yönetim Platformu

*Yazılım eğitmenlerine müfredattan leaderboard'a kadar eksiksiz yönetim araçları*

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![EF Core](https://img.shields.io/badge/EF_Core-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://learn.microsoft.com/en-us/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-LocalDB-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)](https://getbootstrap.com/)
[![Claude AI](https://img.shields.io/badge/Claude_AI-Anthropic-FF6B35?style=for-the-badge)](https://www.anthropic.com/)

</div>

---

## 📖 Proje Hakkında

**EduLog**, yazılım eğitmenlerinin ders müfredatlarını, sınıflarını, ödevlerini ve öğrenci performansını tek bir yerden yönetmesini sağlayan modern bir web uygulamasıdır.

Eğitmen müfredatı önceden hazırlar, sınıf açtığında o müfredatı bağlar ve **hafta hafta** öğrencilere içerik açar. Öğrenciler sınıfa katılmak için eğitmenin paylaştığı JoinCode'u kullanır. AI destekli ödev üretimi sayesinde Claude API üzerinden çoktan seçmeli sorular otomatik oluşturulabilir.

---

## ✨ Özellikler

### 👨‍🏫 Eğitmen Paneli
- 📚 **Ders Yönetimi** — Ders oluştur, düzenle, sil
- 📋 **Müfredat Yönetimi** — Haftalık konu, not, örnek kod ve PDF kaynakları ekle
- 🏫 **Sınıf Yönetimi** — Sınıf oluştur, JoinCode paylaş, hafta hafta içerik aç
- 📝 **Ödev Sistemi** — Manuel ödev oluştur veya **AI ile çoktan seçmeli sorular üret**
- 🏆 **Leaderboard** — Sınıf içi puan sıralaması, öğrenci submission'larını görüntüle ve puan gir

### 🎓 Öğrenci Paneli
- 🔑 **JoinCode ile Kayıt** — Sisteme öğrenci olarak katıl
- 📅 **Açık Haftalara Göz At** — Konu, notlar, örnek kodlar, PDF kaynaklar
- 📤 **Ödev Teslimi** — Kod ödevleri ve çoktan seçmeli quizler
- 🥇 **Sıralamam** — Sınıf içindeki kendi pozisyonunu takip et

---

## 🏗️ Mimari

```
EduLog/
├── EduLog.Core/        → Entities, Interfaces, DTOs
├── EduLog.Data/        → DbContext, EF Migrations, Repository Pattern
├── EduLog.Services/    → Business Logic, AI Service (Anthropic), File Service
└── EduLog.Web/         → ASP.NET Core MVC, Controllers, Razor Views, wwwroot
```

**Katmanlı mimari** ile controller'lar ince tutulmuş, tüm iş mantığı `EduLog.Services` katmanında yürütülmektedir.

---

## 🗄️ Veritabanı Şeması

```
Courses ──── Syllabi ──── SyllabusWeeks ──── WeekResources
                              │
                              ├──── Assignments ──── AssignmentQuestions
                              │
ClassGroups ─────────────────┘
    │
    ├──── ClassEnrollments ──── ApplicationUser
    │
    └──── Submissions
```

---

## 🤖 AI Ödev Üretici Akışı

```
Eğitmen "AI ile Ödev Üret" butonuna tıklar
        ↓
Haftanın konusu + notlar + örnekler Claude API'ye gönderilir
        ↓
5 adet çoktan seçmeli soru JSON formatında döner
        ↓
Önizleme ekranında gösterilir → Eğitmen onaylar
        ↓
Veritabanına kaydedilir ✓
```

---

## 🚀 Kurulum

### Gereksinimler
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server LocalDB](https://www.microsoft.com/sql-server) (Visual Studio ile birlikte gelir)
- [Anthropic API Key](https://www.anthropic.com/) (AI ödev üretici için)

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
| Yapay Zeka | Anthropic Claude API |
| Frontend | Razor Views + Bootstrap 5 |
| Dosya Depolama | Local FileSystem (`wwwroot/uploads`) |

---

## 📏 Kurallar ve Standartlar

- ✅ Repository pattern (`IRepository<T>`) ile veri erişimi
- ✅ Her view için ayrı **ViewModel** (entity doğrudan view'e geçilmez)
- ✅ Her yerde `async/await`
- ✅ DataAnnotations + `ModelState.IsValid` doğrulaması
- ✅ API key'ler `appsettings.json`'dan okunur, hardcode yok
- ✅ Soft delete yok, hard delete uygulanır
- ✅ JoinCode: 6 haneli, alfanümerik, büyük harf, unique (örn: `PY2024`)

---

## 🗺️ Geliştirme Yol Haritası

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
- [x] Leaderboard ve manuel puanlama

---

<div align="center">

Made with ❤️ for software education

</div>
