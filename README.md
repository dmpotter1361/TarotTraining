# Tarot Training

<p align="center">
  <img src="appicon-preview.png" width="96" alt="Tarot Training icon"><br>
  <a href="https://github.com/dmpotter1361/TarotTraining/releases/latest"><img src="https://img.shields.io/github/v/release/dmpotter1361/TarotTraining?label=download&sort=semver&cacheSeconds=300" alt="Latest release"></a>
</p>

A small Windows app for learning the tarot deck. It shows you a card with its name
hidden and four keyword sets to choose from — pick the meaning that matches the card.
It keeps score as you go, lets you drill a single suit or the whole deck, and has a
full reference for every card when you want to read up rather than guess.

> Personal project, for learning the card meanings — not fortune-telling advice.

### ⬇ Download

**[Get the latest version](https://github.com/dmpotter1361/TarotTraining/releases/latest)** —
download `TarotTraining-x.y.z-x64.msi` from the latest release and run it. It installs
like any normal program (shows in **Add/Remove Programs**, and a newer version replaces
the old one automatically). No setup or accounts. (It's a personal build and not
code-signed, so Windows SmartScreen may warn — choose **More info → Run anyway**.)

## Features

- **Flash-card quiz** — a random card is shown with its printed name covered; pick the
  matching keywords from four choices and click **Check**. The right answer turns green
  and the name is revealed.
- **Score tracking** — running **Played / Correct / Incorrect** tally, with
  **Change Suit/Reset** to start over.
- **Drill any suit** — quiz on the **Major Arcana**, **Cups**, **Pentacles**, **Swords**,
  **Wands**, or the **All** 78-card deck.
- **Quick cheat sheets** — **right-click** any suit button to pop up that suit's full
  card-and-keyword list for a fast refresher.
- **Card details** — click **Card Details** for a full write-up of the current card's
  meaning and symbolism.
- **Background music** — an optional ambient loop you can toggle on or off.

## Requirements

- **Windows 10/11 (x64).** The .NET runtime is bundled in the installer.

## Build from source

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/) and
WiX v5 (`dotnet tool install --global wix --version 5.0.2`).

```powershell
# Run the app
dotnet run --project TarotTraining.csproj

# Build the MSI installer (publishes self-contained, then builds the MSI)
pwsh ./build.ps1 -Version 1.0.0
```

## How it works

Every card — its suit, keywords, a brief meaning, and the full description — lives in a
single data file, [`cards.json`](cards.json). The app loads it once at startup, so adding
or editing a card is just a JSON edit; no code changes. The card images, marble texture,
and music sit in [`Resources/`](Resources/) and are loaded by name at runtime.

### Helpful map for a new contributor (human or AI)

- **`cards.json`** — the single source of truth: all 78 cards (suit, keywords, brief, and
  full description) plus the per-suit overviews. Edit this to change a card's meaning.
- **`TarotDeck.cs`** — strongly-typed model + loader for `cards.json`.
- **`MainForm.cs`** — the main window: the card panel, the four choices, scoring, suit
  selection, cheat sheets, and music.
- **`DetailForm.cs`** — the read-only popup used for card details and the suit cheat sheets.
- **`installer/Package.wxs` + `build.ps1`** — the WiX MSI installer.

## Continuing development with Claude Code

This project was cleaned up and converted with AI assistance and is set up so you can
keep going the same way. To pick up where it left off on your own machine:

1. **Get the code onto your PC**

   ```bash
   git clone https://github.com/dmpotter1361/TarotTraining.git
   cd TarotTraining
   ```

2. **Install [Claude Code](https://claude.com/claude-code)** (Anthropic's coding CLI)
   and start it in the project folder:

   ```bash
   npm install -g @anthropic-ai/claude-code
   claude
   ```

   (You can also use the Claude Code extension for VS Code / JetBrains, or
   [claude.ai/code](https://claude.ai/code).)

3. **Point Claude at the project and ask for what you want.** A good first prompt:

   > Read the README, `cards.json`, and `MainForm.cs`, then run the app so you understand
   > it. I'd like to add &lt;your feature&gt;.

## Acknowledgments

Tarot Training was **originally written by the author in Visual Basic**. It was later
**cleaned up and rewritten in C# / .NET 10 collaboratively with Claude** (Anthropic's AI) —
consolidating all the card data into a single source of truth, untangling the duplicated
logic, and adding the installer and this README. The direction, decisions, and real-world
testing are human; a lot of the cleanup and conversion was AI-assisted — and we're happy
to say so. 🤖🤝

## License

[GPL-3.0](LICENSE)
