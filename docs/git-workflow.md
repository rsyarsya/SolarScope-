# Git Workflow

## Alur Utama

main → task branch → Pull Request → review → Squash and Merge

## Aturan Branch

Format:

`<type>/<short-description>`

Contoh:

- `feat/simulation-form`
- `fix/latitude-validation`
- `docs/project-foundation`

## Aturan Commit

Gunakan Conventional Commits:

`<type>(<scope>): <description>`

Contoh:

- `docs: add project foundation documents`
- `feat(simulation): add simulation input form`
- `fix(validation): reject invalid latitude`

## Aturan Pull Request

- Jangan push langsung ke `main`
- Satu branch untuk satu tugas
- Minimal satu approval
- Build dan test harus berhasil
- Gunakan Squash and Merge
- Hapus branch setelah selesai

## Keamanan

Jangan commit API key, token, password, connection string, `.env`, `bin/`, `obj/`, atau `.vs/`.