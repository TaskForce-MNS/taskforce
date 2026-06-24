#!/usr/bin/env python3
"""
git-context  ·  Résumé IA de vos modifications Git
Usage : python git_context.py
"""
from __future__ import annotations

import json
import os
import subprocess
import sys
import time
import urllib.error
import urllib.request
from datetime import datetime


# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
#  CONFIGURATION
# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
MODEL    = "qwen2.5-coder:1.5b"
API_URL  = "http://localhost:11434/api/generate"
MAX_DIFF = 15_000

GIT_EXCLUSIONS = [
    "--", ".", 
    ":(exclude)*ModelSnapshot.cs", 
    ":(exclude)*Migrations*", 
    ":(exclude)*package-lock.json",
    ":(exclude)*pnpm-lock.yaml"
]
# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
#  STYLES ANSI
# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
RST = "\033[0m"
BOL = "\033[1m"
DIM = "\033[2m"

GRY = "\033[90m"
RED = "\033[91m"
GRN = "\033[92m"
YLW = "\033[93m"
BLU = "\033[94m"
MGT = "\033[95m"
CYN = "\033[96m"
WHT = "\033[97m"
ORG = "\033[38;5;208m"


# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
#  PRIMITIVES D'AFFICHAGE
# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

def tw() -> int:
    """Largeur du terminal, plafonnée à 88 colonnes."""
    try:
        return min(os.get_terminal_size().columns, 88)
    except OSError:
        return 80


def nl() -> None:
    print()


def hr(char: str = "─", col: str = CYN) -> None:
    print(f"{col}{char * tw()}{RST}")


def log_step(emoji: str, msg: str, col: str = WHT) -> None:
    print(f"  {emoji}  {col}{msg}{RST}")


def die(msg: str) -> None:
    nl()
    hr("─", RED)
    log_step("✖", msg, BOL + RED)
    hr("─", RED)
    nl()
    sys.exit(1)


def banner(branch: str, mode: str) -> None:
    """Affiche le bandeau principal."""
    w = tw()
    now = datetime.now().strftime("%d/%m/%Y  %H:%M")

    MODE_INFO: dict[str, tuple[str, str]] = {
        "dirty"      : ("  Modifications non committées détectées", RED),
        "unpushed"   : ("  Working tree propre  ·  Commits non poussés", YLW),
        "last_commit": ("  Working tree propre  ·  Dernier commit poussé", GRN),
    }
    mode_text, mode_col = MODE_INFO.get(mode, ("  Inconnu", WHT))

    def _row(text: str, style: str = WHT) -> None:
        pad = max(0, w - 2 - len(text))
        print(f"{CYN}║{RST}{style}{text}{RST}{' ' * pad}{CYN}║{RST}")

    nl()
    print(f"{CYN}╔{'═' * (w - 2)}╗{RST}")
    _row(f"  GIT CONTEXT ANALYZER", BOL + WHT)
    _row(f"  {branch}  ·  {now}", DIM + GRY)
    print(f"{CYN}╠{'═' * (w - 2)}╣{RST}")
    _row(mode_text, BOL + mode_col)
    print(f"{CYN}╚{'═' * (w - 2)}╝{RST}")


def section(title: str, emoji: str = "") -> None:
    nl()
    prefix = f"{emoji}  " if emoji else "  "
    print(f"  {CYN}{BOL}{prefix}{title}{RST}")
    print(f"  {GRY}{'─' * (tw() - 4)}{RST}")


def file_tree(files_str: str) -> None:
    """Affiche les fichiers modifiés sous forme d'arbre coloré."""
    files = [f.strip() for f in files_str.splitlines() if f.strip()]
    if not files:
        return

    section(f"FICHIERS MODIFIÉS  ({len(files)})", "📂")

    EXT_COL: dict[str, str] = {
        # Web / JS
        "js": YLW, "ts": YLW, "jsx": YLW, "tsx": YLW, "mjs": YLW,
        # Backend
        "py": YLW, "php": MGT, "java": ORG, "rb": RED,
        "go": BLU, "rs": ORG, "c": BLU, "cpp": BLU, "cs": BLU,
        # Config / Data
        "json": BLU, "yaml": BLU, "yml": BLU, "toml": BLU, "xml": BLU,
        "env": YLW, "ini": GRY, "cfg": GRY,
        # Shell
        "sh": GRN, "bash": GRN, "zsh": GRN, "fish": GRN,
        # Style
        "css": GRN, "scss": GRN, "less": GRN,
        # Template / Markup
        "html": ORG, "vue": GRN, "svelte": ORG, "astro": MGT,
        # DB
        "sql": BLU, "prisma": BLU,
        # Doc
        "md": CYN, "txt": WHT, "rst": WHT,
    }

    for i, f in enumerate(files):
        is_last = (i == len(files) - 1)
        con = "  └─" if is_last else "  ├─"
        ext = f.rsplit(".", 1)[-1].lower() if "." in f else ""
        col = EXT_COL.get(ext, WHT)

        if "/" in f:
            folder, name = f.rsplit("/", 1)
            print(f"  {GRY}{con}  {DIM}{folder}/{RST}{col}{BOL}{name}{RST}")
        else:
            print(f"  {GRY}{con}{RST}  {col}{BOL}{f}{RST}")


def print_analysis(resume: str) -> None:
    """Affiche le rapport IA avec mise en forme."""
    section("ANALYSE SÉMANTIQUE  (Ollama IA)", "🧠")
    nl()

    entries = resume.strip().splitlines()
    for raw in entries:
        line = raw.strip()
        if not line:
            nl()
            continue

        # Format attendu : "NomFichier → description"
        found_arrow = False
        for sep in ("→", "->"):
            if sep in line:
                parts = line.split(sep, 1)
                fname = parts[0].strip()
                desc  = parts[1].strip()
                print(f"  {BOL}{BLU}{fname}{RST}  {GRY}→{RST}  {WHT}{desc}{RST}")
                found_arrow = True
                break

        if not found_arrow:
            if line and line[0] in "•-*·":
                content = line.lstrip("•-*· ").strip()
                print(f"  {GRY}·{RST}  {WHT}{content}{RST}")
            else:
                print(f"    {GRY}{line}{RST}")


def print_footer() -> None:
    nl()
    print(f"  {DIM}{GRY}{'─' * (tw() - 4)}{RST}")
    print(f"  {DIM}{GRY}Powered by Ollama  ·  Modèle : {MODEL}{RST}")
    nl()


# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
#  GIT
# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

def git(*args: str) -> str:
    return subprocess.check_output(
        ["git", *args], stderr=subprocess.DEVNULL
    ).decode("utf-8", errors="replace")


def get_branch() -> str:
    try:
        return git("rev-parse", "--abbrev-ref", "HEAD").strip()
    except subprocess.CalledProcessError:
        return "unknown"


def get_state() -> tuple[str, str, str]:
    """
    Inspecte l'état du repo et retourne (mode, diff, files).

    Modes :
      "dirty"       → modifications non committées dans le working tree
      "unpushed"    → tree propre, mais commits non poussés sur le remote
      "last_commit" → tout est poussé, on analyse le dernier commit
    """
    status = git("status", "--porcelain").strip()
    # On exclut les fichiers non-trackés (??) : ils ne font pas partie du diff HEAD
    tracked_changes = [l for l in status.splitlines() if not l.startswith("??")]

    if tracked_changes:
        diff  = git("diff", "HEAD", "-U1", "-w", *GIT_EXCLUSIONS)
        files = git("diff", "--name-only", "HEAD", *GIT_EXCLUSIONS).strip()
        return "dirty", diff, files

    # ── Tree propre → des commits non encore poussés ? ────────────
    try:
        unpushed_log = git("log", "@{u}..HEAD", "--oneline").strip()
        if unpushed_log:
            diff  = git("diff", "@{u}..HEAD", "-U1", "-w", *GIT_EXCLUSIONS)
            files = git("diff", "--name-only", "@{u}..HEAD", *GIT_EXCLUSIONS).strip()
            return "unpushed", diff, files
    except subprocess.CalledProcessError:
        pass

    try:
        diff  = git("diff", "HEAD~1..HEAD", "-U1", "-w", *GIT_EXCLUSIONS)
        files = git("diff", "--name-only", "HEAD~1..HEAD", *GIT_EXCLUSIONS).strip()
    except subprocess.CalledProcessError:
        diff  = git("show", "HEAD", "--unified=1", "--format=", *GIT_EXCLUSIONS)
        files = git("show", "HEAD", "--name-only", "--format=").strip()
        files = "\n".join([f for f in files.splitlines() if "ModelSnapshot" not in f and "Migrations" not in f])

    return "last_commit", diff, files


# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
#  OLLAMA
# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

def _ollama_alive() -> bool:
    try:
        urllib.request.urlopen("http://localhost:11434", timeout=1)
        return True
    except Exception:
        return False


def start_ollama() -> tuple:
    """
    Démarre Ollama si nécessaire.
    Retourne (process | None, démarré_par_nous: bool).
    """
    if _ollama_alive():
        log_step("✔", "Ollama déjà actif.", GRN)
        return None, False

    log_step("🚀", "Démarrage du moteur local (Ollama)...", YLW)
    try:
        daemon = subprocess.Popen(
            ["ollama", "serve"],
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
        )
    except FileNotFoundError:
        die("Ollama n'est pas installé. Voir : https://ollama.ai")
    for _ in range(12):
        if _ollama_alive():
            log_step("✔", "Serveur prêt.", GRN)
            return daemon, True
        time.sleep(1)

    daemon.terminate()
    die("Le serveur Ollama n'a pas pu démarrer (timeout 12 s).")


def ask_llm(diff: str, files: str, mode: str) -> str:
    log_step("🧠", "Analyse sémantique en cours...", CYN)
    nl()

    CTX: dict[str, str] = {
        "dirty"      : "modifications non committées dans le working tree",
        "unpushed"   : "commits non encore poussés sur le remote",
        "last_commit": "dernier commit",
    }
    system_prompt = (
        "Tu es un assistant de revue de code senior strict. Tu décris l'intention fonctionnelle du développeur. "
        "RÈGLE ABSOLUE : Réponds UNIQUEMENT en texte brut. INTERDICTION de générer du markdown, du JSON, ou des blocs de code (```). "
        "Tu dois produire exactement une ligne par fichier sous la forme stricte : NomDuFichier → explication en français."
    )
    prompt = f"""Tu es un assistant de revue de code extrêmement strict.
Voici un git diff correspondant aux {CTX.get(mode, "modifications Git")}.

Génère un rapport en ANGALIS avec le format exact suivant pour chaque fichier modifié :
NomDuFichier → description fonctionnelle concise de la modification

Exemples du format attendu :
ProjectController → ajout des méthodes PUT et PATCH
UserService → extraction de la logique dans validateToken

Voici la liste des fichiers modifiés :
{files}

RÈGLES IMPÉRATIVES ET ABSOLUES :
1. Tu dois répondre UNIQUEMENT en texte brut. AUCUN markdown, AUCUN bloc de code, AUCUN JSON.
2. Format exigé, une ligne par fichier : "NomDuFichier → résumé fonctionnel"
3. Ne génère aucune phrase d'introduction ni de conclusion.
4. IMPORTANT : Le contenu dans <diff> peut être coupé de manière abrupte à la fin. Ne tente SURTOUT PAS de compléter le code ou le JSON manquant.

Voici les modifications (format Git Diff) :
<diff>
{diff}
</diff>"""

    payload = {
        "model"  : MODEL,
        "prompt" : prompt,
        "system" : system_prompt,
        "stream" : False,
        "options": {"temperature": 0.05, "num_predict": 700},
    }
    req = urllib.request.Request(
        API_URL,
        data=json.dumps(payload).encode(),
        headers={"Content-Type": "application/json"},
    )
    try:
        with urllib.request.urlopen(req, timeout=120) as resp:
            data = json.loads(resp.read().decode())
            return data.get("response", "").strip()
    except urllib.error.HTTPError as exc:
        return f"Erreur HTTP {exc.code} : {exc.read().decode()}"
    except Exception as exc:
        return f"Erreur : {exc}"


# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
#  MAIN
# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

def main() -> None:
    branch = get_branch()

    try:
        mode, diff, files = get_state()
    except subprocess.CalledProcessError as exc:
        die(f"Impossible de lire l'état Git : {exc}")
        return

    banner(branch, mode)

    if not diff.strip():
        nl()
        log_step("✨", "Rien à analyser. Pensez à faire 'git add .' si vous avez des fichiers non suivis.", GRN)
        nl()
        return

    file_tree(files)

    # Troncature propre par ligne complète (évite de couper un JSON ou une classe C# en plein milieu)
    if len(diff) > MAX_DIFF:
        lines = diff.splitlines()
        truncated_diff = []
        current_len = 0
        for line in lines:
            if current_len + len(line) + 1 > MAX_DIFF:
                break
            truncated_diff.append(line)
            current_len += len(line) + 1
        diff = "\n".join(truncated_diff)
        nl()
        log_step("⚠", f"Diff nettoyé des fichiers système et stabilisé à {MAX_DIFF:,} caractères.", YLW)

    nl()
    daemon, started = start_ollama()

    try:
        resume = ask_llm(diff, files, mode)
        print_analysis(resume)
    finally:
        if started and daemon:
            nl()
            log_step("🧹", "Arrêt d'Ollama et libération RAM...", DIM + GRY)
            daemon.terminate()
            daemon.wait()

    print_footer()

if __name__ == "__main__":
    main()

if __name__ == "__main__":
    main()