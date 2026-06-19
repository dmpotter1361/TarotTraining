# Tarot Training — notes for Claude Code

C# / .NET 10 WinForms tarot **flash-card quiz** app (ported from VB.NET). Distinct
from Devine Clairvoyance (which is a reading app — this one quizzes you). Windows-only.
Repo `dmpotter1361/TarotTraining` (latest v1.0.1 MSI; "Slate & Gold" theme).

## Run / build

```powershell
dotnet run                 # launch the app
dotnet build -c Release    # release build
./build.ps1                # packaging / release (MSI) script
```

No tests. Plain `dotnet` is enough.

## Layout

- **`cards.json`** — **single source of truth** for card data. Edit this to change
  card text — no code change required. Copied next to the exe at build.
- **`TarotDeck.cs`** — loads `cards.json`; exposes deck/lookups.
- **`MainForm.cs`** — quiz UI ("Mystic Veil" reveal).
- **`DetailForm.cs`** — per-card detail popup.

## Conventions

- Keep `cards.json` as the source of truth — don't hard-code card text in C#.
- Forms hand-written in code; **UI must survive font/display-scaling changes**
  (TableLayoutPanel/FlowLayoutPanel + AutoSize + font fallback).
