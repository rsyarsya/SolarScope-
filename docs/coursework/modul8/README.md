# Homework Modul 8

Bukti pengerjaan Worksheet Homework 8.

1. **Method dengan Inheritance, Encapsulation, dan Polymorphism** — perhitungan estimasi energi di `src/SolarScope.Core/Services/`:
   - `EnergyCalculator` (abstract): method publik `Calculate()` berisi validasi, formula, dan ringkasan 12 bulan; detail formula bersifat `private`.
   - `ClimatologyEnergyCalculator` dan `YearlyEnergyCalculator` mewarisi `EnergyCalculator` dan meng-override `GetDaysInMonth()` (365 hari vs mengikuti tahun kabisat).
2. **Koneksi ke database PostgreSQL** — Supabase (PostgreSQL 17) melalui `Npgsql`:
   - `PostgresSimulationRepository`: simpan, daftar, detail, dan hapus riwayat simulasi.
   - Connection string dibaca dari environment variable `SOLARSCOPE_DB_CONNECTION` dan tidak disimpan di repository.

## Screenshot

- `screenshots/01-verifikasi-calculator-dan-database.png`: `dotnet run --project tests/SolarScope.Core.Verification`, 18 pemeriksaan lulus (12 kalkulator, 6 round trip database).
- `screenshots/02-koneksi-supabase.png`: `dotnet run --project tools/SolarScope.ConnectionCheck`, koneksi berhasil; tabel kosong karena data uji dihapus kembali.

Antarmuka `SolarScope.Desktop` belum memakai kalkulator dan repository ini; integrasi UI merupakan pekerjaan selanjutnya.
