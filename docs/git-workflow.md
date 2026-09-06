# Git Workflow

Kebijakan branch diperbarui atas permintaan pengguna pada 6 September 2026: gunakan tiga branch anggota permanen dan satu branch integrasi `main`.

## Branch

| Branch | Pemilik / fungsi |
|---|---|
| `main` | Hasil integrasi yang sudah direview |
| `534714` | Rasyadwa Arsya Irnantyanto |
| `539913` | Raditya Azhar Ananta |
| `540091` | Ghaisan Rifqi Kamiel |

Total empat branch, termasuk `main`. Branch anggota dipertahankan setelah PR selesai. Branch sementara `docs/*` yang sudah seluruhnya digabung dibersihkan.

## Alur Utama

Sinkronkan branch NIU dengan `main` → kerjakan satu tugas → commit → push branch NIU → Pull Request ke `main` → review anggota lain → Squash and Merge → sinkronkan kembali.

- Jangan mengerjakan atau push langsung ke `main`.
- Setiap anggota bekerja dan push pada branch NIU miliknya.
- Satu PR berisi satu tugas yang jelas. Perubahan diagram dan pembaruan workflow pada PR transisi ini dipisahkan dalam commit masing-masing.
- Selesaikan PR aktif sebelum memulai tugas lain pada branch yang sama; push berikutnya akan ikut masuk ke PR yang masih terbuka.
- Jangan force push atau menulis ulang history branch anggota.
- Setelah PR ditutup atau di-merge, push berikutnya tidak membuka PR secara otomatis. Buat PR baru untuk tugas berikutnya.

## Mulai atau Lanjut Bekerja

Contoh untuk pemilik NIU `534714`; anggota lain mengganti nomor dengan NIU sendiri. Pastikan `git status` bersih terlebih dahulu. Jika ada perubahan yang belum disimpan, selesaikan commit atau simpan dengan sengaja sebelum sinkronisasi.

```powershell
git status
git fetch origin
git switch 534714
git merge origin/534714
git merge origin/main
```

Jika branch NIU belum tersedia secara lokal, gunakan `git switch --track origin/534714` sekali untuk membuat branch lokal dari remote.

Setelah mengedit:

```powershell
git status
git diff
git add <file-yang-diperiksa>
git diff --staged
git commit -m "docs(diagram): add class diagram"
git push -u origin 534714
```

Jalankan build dan test yang relevan jika mengubah aplikasi. Untuk perubahan dokumentasi saja, periksa isi, gambar, dan tautan; jangan mengklaim build/test sudah dijalankan.

## Commit dan Pull Request

Gunakan Conventional Commits berbahasa Inggris: `<type>(<scope>): <description>`. Scope opsional. Type ditentukan oleh perubahan, bukan oleh nama branch NIU.

- `docs(diagram): add class diagram`
- `feat(simulation): add simulation input form`
- `fix(validation): reject invalid latitude`

Deskripsi menggunakan kata kerja perintah, diawali huruf kecil, tidak diakhiri titik, dan idealnya maksimal 72 karakter. Satu commit memuat satu perubahan logis.

PR menargetkan `main`, menggunakan judul Conventional Commits, serta menjelaskan perubahan dan verifikasi. Minimal satu anggota lain harus memberikan approval. Build/test yang relevan harus berhasil. Gunakan **Squash and Merge**; jangan hapus branch NIU setelah merge.

## Setelah PR Di-merge

Squash menghasilkan commit baru di `main`; history branch anggota tidak otomatis sama dengan `main`. Sebelum tugas berikutnya, jalankan kembali sinkronisasi lalu push:

```powershell
git fetch origin
git switch 534714
git merge origin/534714
git merge origin/main
git push origin 534714
```

Sinkronisasi dapat menghasilkan merge commit. Pertahankan history tersebut; jangan menggunakan force push untuk merapikannya. Jika conflict terjadi, periksa kedua perubahan, selesaikan dengan sengaja, lalu verifikasi hasil sebelum commit/push.

## Keamanan dan Perlindungan

Jangan commit API key, token, password, connection string rahasia, `.env`, `bin/`, `obj/`, atau `.vs/`. Gunakan `.gitignore` yang sesuai dan periksa file sebelum staging.

`main` tetap memerlukan PR dan minimal satu approval, serta memblokir force push dan penghapusan. Aktifkan status checks ketika CI tersedia. Jangan aktifkan penghapusan branch otomatis jika itu akan menghapus branch NIU permanen.
