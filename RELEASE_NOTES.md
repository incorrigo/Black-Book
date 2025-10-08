# Black Book — Release Notes

## v1.0.0 (2025-10-09)

First official release of Black Book — a portable, profile-based personal correspondence manager built as a native WPF application for .NET 8.

### Highlights
- Privacy-first architecture designed around strong local security and zero plaintext persistence.
- Password protection hardened with Argon2id key derivation to resist dictionary and brute-force attacks.
- AEAD encryption using ChaCha20-Poly1305 to provide confidentiality and integrity (tamper detection).
- Two-file profile lock: your encrypted profile must be accompanied by its companion file (`file.file`) to be usable.
- Forensic-grade profile deletion workflow designed to make recovery impractical.
- Native Windows UI (WPF, .NET 8) with responsive lists and context menus for quick editing.

### Core features
- Profiles are composed of "things": persons, companies, interactions, situations, and objectives.
- All edits occur in-memory; the full profile is re-encrypted and then saved — nothing is written in plaintext, even temporarily.
- Automatic in-memory wipe on exit to remove keys and profile contents.
- Interaction history:
  - Per person, company, and situation, with newest-first ordering.
  - Quick edit/delete via context menus; double-click to open editors.
- Situation management:
  - Status categories: New, Ongoing, Ad Hoc, Done With.
  - Custom status ordering in lists: New → Ongoing → Ad Hoc → Done With.
  - Archived situations are excluded from the main list by default, with a dedicated access button.
  - Visual status cues and interaction count badges.
- Navigation helpers for quick access to related people, companies, and interactions throughout the app.

### Security model (at a glance)
- Argon2id-based key derivation; parameters selected to increase cost for offline guessing attacks.
- AEAD (ChaCha20-Poly1305) encryption with authenticity tags; suspected tampering halts decryption pathways.
- Second-file requirement: without the companion `file.file`, a profile is unusable by design.
- Forensic delete option intended to render on-disk recovery non-viable.

### System requirements
- Windows 10/11 (x64)
- .NET 8 Desktop Runtime

### Installation & usage
- Download and extract the release package.
- Run Black Book. Profiles are portable; keep the companion `file.file` safe and separate when necessary.
- Use Alt+F4 to immediately exit and clear sensitive material from memory.

### Known limitations
- Argon2id intensity/parameters are not yet user-configurable per profile.
- There is no password recovery. Losing your password or the companion `file.file` will render a profile inaccessible.
- No cloud sync or auto-update in this release.

### Licensing and acknowledgements
- Free for non-commercial use; attribution required. Commercial use requires a license. See LICENCE.md for details and contact operations@incorrigo.io.
- Third-party: Konscious.Security.Cryptography (see 3PL/L01.txt) is used for Argon2id.

### Notes for developers
- Target framework: net8.0-windows.
- UI: WPF (XAML); MVVM-friendly models with computed navigation properties and JSON serialization guards where appropriate.

Thanks for trying Black Book. Feedback and commercial inquiries: https://incorrigo.io/