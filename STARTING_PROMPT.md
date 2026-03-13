# EduLog — Başlangıç Promptu (Antigravity Agent'a Ver)

Aşağıdaki metni Antigravity'de yeni bir agent görevi olarak ver:

---

## GÖREV

EduLog adlı bir eğitim yönetim platformu geliştiriyoruz. Aşağıdaki teknik spesifikasyona göre projenin temelini kur.

### Tech Stack
- ASP.NET Core 8 MVC
- Entity Framework Core 8
- SQL Server (LocalDB)
- ASP.NET Core Identity
- Bootstrap 5

### Solution Yapısı

Aşağıdaki 4 projeli bir .NET solution oluştur:

```
EduLog.sln
├── EduLog.Web       (ASP.NET Core MVC Web Application)
├── EduLog.Core      (Class Library — Entities, Interfaces)
├── EduLog.Data      (Class Library — DbContext, Migrations, Repositories)
└── EduLog.Services  (Class Library — Business logic, AI service)
```

Referanslar:
- EduLog.Web → EduLog.Services, EduLog.Data
- EduLog.Services → EduLog.Core, EduLog.Data
- EduLog.Data → EduLog.Core

### EduLog.Core — Entities

Aşağıdaki entity sınıflarını oluştur:

```csharp
// Courses.cs
public class Course { int Id; string Name; string Description; ICollection<Syllabus> Syllabi; ICollection<ClassGroup> ClassGroups; }

// Syllabus.cs
public class Syllabus { int Id; int CourseId; string Title; DateTime CreatedAt; Course Course; ICollection<SyllabusWeek> Weeks; }

// SyllabusWeek.cs
public class SyllabusWeek { int Id; int SyllabusId; int WeekNumber; string Topic; string? Examples; string? Notes; Syllabus Syllabus; ICollection<WeekResource> Resources; ICollection<Assignment> Assignments; }

// WeekResource.cs
public class WeekResource { int Id; int SyllabusWeekId; string FileName; string FilePath; string ResourceType; SyllabusWeek Week; }

// Assignment.cs
public class Assignment { int Id; int SyllabusWeekId; string Title; string? Description; string Type; // "CodeTask" | "MultipleChoice" bool IsAIGenerated; int MaxScore; DateTime? DueDate; SyllabusWeek Week; ICollection<AssignmentQuestion> Questions; ICollection<Submission> Submissions; }

// AssignmentQuestion.cs
public class AssignmentQuestion { int Id; int AssignmentId; string QuestionText; string OptionA; string OptionB; string OptionC; string OptionD; string CorrectAnswer; int OrderIndex; Assignment Assignment; }

// ClassGroup.cs
public class ClassGroup { int Id; int CourseId; int SyllabusId; string Name; string JoinCode; int CurrentWeek; DateTime CreatedAt; Course Course; Syllabus Syllabus; ICollection<ClassEnrollment> Enrollments; ICollection<Submission> Submissions; }

// ClassEnrollment.cs
public class ClassEnrollment { int Id; int ClassGroupId; int UserId; DateTime JoinedAt; ClassGroup ClassGroup; ApplicationUser User; }

// Submission.cs
public class Submission { int Id; int AssignmentId; int ClassGroupId; int UserId; string Content; int? Score; string? InstructorNote; DateTime SubmittedAt; Assignment Assignment; ClassGroup ClassGroup; ApplicationUser User; }
```

ApplicationUser, IdentityUser'ı extend etmeli:
```csharp
public class ApplicationUser : IdentityUser<int> { public string FullName { get; set; } }
```

### EduLog.Data — DbContext

`AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>` olarak tanımla.

Tüm entity'ler için DbSet ekle. HasIndex ile JoinCode unique constraint ekle.

### EduLog.Data — Repository Pattern

`IRepository<T>` interface'i: GetAll, GetById, Add, Update, Delete, SaveChanges metodları.
`Repository<T>` generic implementasyonu EF Core üzerine.

### ASP.NET Core Identity Kurulumu

- Rolleri: `Instructor` ve `Student`
- Seed data olarak şu bilgileri kullan:
  - Email: `furkan@edulog.com`
  - Password: `Admin123!`
  - Role: `Instructor`
  - FullName: `Furkan`
- Örnek dersler seed: Python, C#, SQL Server, Unity, Web Yazılım

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EduLogDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "AnthropicApi": {
    "ApiKey": "BURAYA_API_KEY_GEL",
    "Model": "claude-sonnet-4-6"
  }
}
```

### Kayıt Akışı

Kayıt formunda standart Identity alanlarına ek olarak:
- `FullName` (zorunlu)
- `JoinCode` (zorunlu)

Register işlemi sırasında:
1. JoinCode ile eşleşen aktif ClassGroup bulunmalı
2. Bulunamazsa "Geçersiz sınıf kodu" hatası döndür
3. Kullanıcı oluşturulunca `Student` rolü ata ve ClassEnrollment kaydı oluştur

### Layout ve Ana Sayfalar

Bootstrap 5 ile temiz bir layout yap. İki ayrı layout:
- `_LayoutInstructor.cshtml` — sidebar navigasyonlu
- `_LayoutStudent.cshtml` — üst navbar'lı

### İlk Migration ve Veritabanı

Tüm entity'leri oluşturduktan sonra:
1. `Add-Migration InitialCreate`
2. `Update-Database`
3. Seed data çalıştır

---

Tüm bu adımları tamamladıktan sonra projenin çalıştığını doğrula: uygulamayı başlat, `/` adresinin açıldığını, login sayfasının çalıştığını ve seed instructor ile giriş yapılabildiğini kontrol et.
