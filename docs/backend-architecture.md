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
[Step 4: Persistence Layer (PostgreSQL / Supabase)] 
         ↓
[Step 5: Facade / Service Result (Titik Temu UI)]
```

| Tahapan | Komponen | Tanggung Jawab Utama | Konsep PBO yang Ditonjolkan |
| :--- | :--- | :--- | :--- |
| **Step 1** | **Domain Models** | Objek domain (`Simulation`, `Location`, dll.) | Encapsulation & Composition |
| **Step 2** | **NasaPowerClient** | Mengambil data radiasi matahari via HTTP | Single Responsibility & Abstraksi Eksternal |
| **Step 3** | **EnergyCalculator** | Menghitung estimasi energi bulanan & tahunan | Encapsulation, Inheritance, Polymorphism |
| **Step 4** | **PostgresSimulationRepository** | Operasi CRUD riwayat ke PostgreSQL (Supabase) via Npgsql | Data Persistence & Repository Pattern |
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
│   │   │   ├── EnergyCalculator.cs             <-- abstract, formula + alur 12 bulan
│   │   │   ├── ClimatologyEnergyCalculator.cs  <-- tahun tipikal (365 hari)
│   │   │   ├── YearlyEnergyCalculator.cs       <-- tahun tertentu (ikut kabisat)
│   │   │   └── PostgresSimulationRepository.cs
│   │   ├── SimulationManager.cs     <-- Facade untuk UI
│   │   └── SolarScope.Core.csproj
│   │
│   └── SolarScope.Desktop/          <-- Frontend Desktop (WinForms .NET 9)
│       └── (Dikelola oleh Frontend Developer)
├── tests/
│   ├── SolarScope.Core.Verification/     <-- Verifikasi kalkulator energi
│   └── SolarScope.Desktop.Verification/  <-- Verifikasi alur UI
├── tools/
│   └── SolarScope.ConnectionCheck/       <-- Cek koneksi database Supabase
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

### C. Database PostgreSQL (Supabase)
- **Server:** PostgreSQL 17 yang dikelola Supabase (Free plan), diakses melalui Supavisor pooler (`aws-0-ap-northeast-1.pooler.supabase.com:5432`, SSL `VerifyFull`).
- **Library:** `Npgsql` (driver ADO.NET resmi PostgreSQL untuk .NET), tanpa ORM.
- **Connection string:** dibaca dari environment variable `SOLARSCOPE_DB_CONNECTION`. Tidak pernah ditulis di kode, `appsettings`, atau repository.
- **Tabel:** `simulations` (induk) dan `monthly_productions` (12 baris per simulasi). Detail kolom ada di `docs/diagrams/erd/README.md`.
- **Row Level Security:** aktif pada kedua tabel. Aplikasi terhubung sebagai role database melalui connection string, bukan melalui API publik Supabase.
- **Cek koneksi:** `dotnet run --project tools/SolarScope.ConnectionCheck` menampilkan jumlah baris kedua tabel.
- **Catatan operasional:** project Free plan otomatis di-pause setelah sekitar 7 hari tidak aktif. Gejalanya error `XX000 ... tenant/user ... not found`. Pulihkan melalui **Restore project** di dashboard Supabase.
- **Operasi repository (`PostgresSimulationRepository`, semua mengembalikan `ServiceResult<T>`):**
  - `InsertSimulationAsync(Simulation sim)`: Menyimpan riwayat simulasi baru beserta rincian 12 bulan dalam satu transaksi.
  - `GetAllSimulationsAsync()`: Mengambil daftar ringkasan riwayat (tanpa rincian bulanan) untuk ditampilkan di tabel UI.
  - `GetSimulationByIdAsync(int id)`: Membuka detail hasil simulasi tertentu beserta 12 bulan.
  - `DeleteSimulationAsync(int id)`: Menghapus satu riwayat simulasi (rincian bulanan ikut terhapus melalui `ON DELETE CASCADE`).
- **Verifikasi:** `dotnet run --project tests/SolarScope.Core.Verification` menjalankan simpan, baca, dan hapus terhadap Supabase. Data uji dihapus kembali, dan bagian ini dilewati jika `SOLARSCOPE_DB_CONNECTION` belum diisi.

### D. Skema Tabel Database (PostgreSQL)
Sesuai skema yang aktif di Supabase:
```sql
CREATE TABLE simulations (
    simulation_id integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    scenario_name text NOT NULL CHECK (btrim(scenario_name) <> ''),
    created_at timestamptz NOT NULL DEFAULT now(),
    latitude double precision NOT NULL CHECK (latitude BETWEEN -90 AND 90),
    longitude double precision NOT NULL CHECK (longitude BETWEEN -180 AND 180),
    location_name text,
    system_capacity_kwp double precision NOT NULL CHECK (system_capacity_kwp > 0),
    performance_ratio double precision NOT NULL DEFAULT 0.75 CHECK (performance_ratio BETWEEN 0.01 AND 1),
    data_source text NOT NULL DEFAULT 'NASA POWER Climatology',
    annual_average_radiation double precision NOT NULL CHECK (annual_average_radiation >= 0),
    total_annual_energy_kwh double precision NOT NULL CHECK (total_annual_energy_kwh >= 0),
    peak_production_month text NOT NULL,
    lowest_production_month text NOT NULL
);

CREATE TABLE monthly_productions (
    simulation_id integer NOT NULL REFERENCES simulations(simulation_id) ON DELETE CASCADE,
    month_index integer NOT NULL CHECK (month_index BETWEEN 1 AND 12),
    month_name text NOT NULL,
    solar_radiation_daily_average double precision NOT NULL CHECK (solar_radiation_daily_average >= 0),
    estimated_energy_kwh double precision NOT NULL CHECK (estimated_energy_kwh >= 0),
    PRIMARY KEY (simulation_id, month_index)
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
