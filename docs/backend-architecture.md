# SolarScope — Backend Architecture & Mental Model Blueprint

Dokumen arsitektur backend SolarScope untuk panduan implementasi, integrasi antarmuka, dan kesiapan asesmen teknis (PBO).

---

## 1. Mental Model & Roadmap Pengerjaan
Pembangunan backend dibagi menjadi 5 tahapan berurutan:

```
[Step 1: Domain Models (Class Diagram Alignment)] 
         ↓
[Step 2: External Client (NASA API)] 
         ↓
[Step 3: Calculation Engine (Formula Solar)] 
         ↓
[Step 4: Persistence Layer (SQLite Database)] 
         ↓
[Step 5: Facade / Service Result (Titik Temu UI)]
```

| Tahapan | Komponen | Tanggung Jawab Utama | Konsep PBO yang Ditonjolkan |
| :--- | :--- | :--- | :--- |
| **Step 1** | **Domain Models** | Objek domain (`Simulation`, `Location`, dll.) | Encapsulation & Composition |
| **Step 2** | **NasaPowerClient** | Mengambil data radiasi matahari via HTTP | Single Responsibility & Abstraksi Eksternal |
| **Step 3** | **SolarCalculator** | Menghitung estimasi energi bulanan & tahunan | Pure Business Logic (Deterministic) |
| **Step 4** | **SqliteSimulationRepository** | Operasi CRUD riwayat ke database lokal SQLite | Data Persistence & Repository Pattern |
| **Step 5** | **SimulationManager** | Orkestrator alur end-to-end + Result Pattern | Facade Pattern & Graceful Error Handling |

---

## 2. Struktur Proyek (Solution Structure)
Arsitektur decoupled 2-project:

```text
SolarScope/
├── docs/
│   ├── SolarScope_Project_Context.md
│   ├── backend-architecture.md
│   └── ...
├── src/
│   ├── SolarScope.Core/             <-- Backend (Class Library .NET 8)
│   │   ├── Models/
│   │   │   ├── Location.cs
│   │   │   ├── SystemParameter.cs
│   │   │   ├── MonthlyProduction.cs
│   │   │   ├── SimulationSummary.cs
│   │   │   ├── Simulation.cs
│   │   │   └── ServiceResult.cs
│   │   ├── Services/
│   │   │   ├── NasaPowerClient.cs
│   │   │   ├── SolarCalculator.cs
│   │   │   └── SqliteSimulationRepository.cs
│   │   ├── SimulationManager.cs     <-- Facade untuk UI
│   │   └── SolarScope.Core.csproj
│   │
│   └── SolarScope.WinForms/         <-- Frontend Desktop (WinForms .NET 8)
│       └── (Dikelola oleh Frontend Developer)
├── AGENTS.md
└── README.md
```

### Mengapa struktur ini dipilih? (Alasan untuk Kuis/Ujian Dosen):
1. **Separation of Concerns (SoC):** Logika backend tidak bercampur dengan kode tombol atau form UI.
2. **Loose Coupling:** Jika UI berganti dari Windows Forms ke WPF, Web, atau CLI, seluruh kode `SolarScope.Core` tidak perlu diubah satu baris pun.
3. **Testability:** Komponen backend dapat diuji secara terisolasi tanpa perlu menjalankan antarmuka grafis.

---

## 3. Spesifikasi Teknis Komponen

### A. Formula Kalkulasi Energi
```text
E_bulan (kWh) = P_peak (kWp) × G_harian (kWh/m²/hari) × Hari_dalam_bulan × PR
E_tahun (kWh) = Total dari 12 bulan E_bulan
```
- `P_peak`: Kapasitas sistem panel surya yang diinput pengguna (kWp).
- `G_harian`: Radiasi matahari harian rata-rata dari parameter NASA POWER `ALLSKY_SFC_SW_DWN`.
- `Hari_dalam_bulan`: Jumlah hari sesuai bulan (Januari: 31, Februari: 28, dst.).
- `PR` (Performance Ratio): Faktor efisiensi sistem (default = `0.75`).

### B. Sumber Data Eksternal (NASA POWER Climatology)
- **Endpoint:**
  `https://power.larc.nasa.gov/api/temporal/climatology/point?parameters=ALLSKY_SFC_SW_DWN&community=RE&longitude={lon}&latitude={lat}&format=JSON`
- **Autentikasi:** Tanpa API Key (bebas akses, publik, reliabel).
- **Output:** Nilai radiasi bulanan (`JAN` s/d `DEC`) dan tahunan (`ANN`).

### C. Database Lokal (SQLite)
- **Library:** `Microsoft.Data.Sqlite` (ringan, tanpa server/daemon terpisah).
- **File Database:** `solarscope.db` (disimpan lokal di direktori aplikasi).
- **Tabel:** `simulations`
- **Operasi:**
  - `InsertSimulation(Simulation sim)`: Menyimpan riwayat simulasi baru beserta rincian 12 bulan.
  - `GetAllSimulations()`: Mengambil seluruh daftar ringkasan riwayat untuk ditampilkan di tabel UI.
  - `GetSimulationById(int id)`: Membuka detail hasil simulasi tertentu.
  - `DeleteSimulation(int id)`: Menghapus satu riwayat simulasi.

### D. Skema Tabel Database (SQLite)
Sesuai atribut pada `class-diagram-simple.png`:
```sql
CREATE TABLE IF NOT EXISTS simulations (
    simulation_id INTEGER PRIMARY KEY AUTOINCREMENT,
    scenario_name TEXT NOT NULL,
    created_at TEXT NOT NULL,
    latitude REAL NOT NULL,
    longitude REAL NOT NULL,
    location_name TEXT,
    system_capacity_kwp REAL NOT NULL,
    performance_ratio REAL NOT NULL DEFAULT 0.75,
    total_annual_energy_kwh REAL NOT NULL,
    peak_production_month TEXT NOT NULL,
    lowest_production_month TEXT NOT NULL,
    monthly_productions_json TEXT NOT NULL
);
```

### E. Error Handling (Result Pattern)
Backend tidak membiarkan exception lolos langsung ke antarmuka pengguna agar aplikasi tidak pernah crash di hadapan dosen penguji. Semua proses dibungkus oleh `ServiceResult<T>`:
```csharp
public class ServiceResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }

    public static ServiceResult<T> Ok(T data) => new() { IsSuccess = true, Data = data };
    public static ServiceResult<T> Fail(string error) => new() { IsSuccess = false, ErrorMessage = error };
}
```
