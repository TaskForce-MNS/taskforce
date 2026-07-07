#!/usr/bin/env python3
"""git-context · AI Summary of Your Git Changes"""
from __future__ import annotations

import json
import os
import re
import subprocess
import sys
import time
import urllib.error
import urllib.request
from datetime import datetime


# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
#  CONFIGURATION
# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
MODEL    = "gemma2:2b"
API_URL  = "http://localhost:11434/api/generate"
MAX_DIFF = 40_000

# Files with more total changed lines than this bypass the LLM
# (new files, deleted files, full rewrites — raw_summary handles them better)
LLM_LINE_THRESHOLD = 80

GIT_EXCLUSIONS = [
    "--", ".",
    ":(exclude)*ModelSnapshot.cs",
    ":(exclude)*Migrations*",
    ":(exclude)*package-lock.json",
    ":(exclude)*pnpm-lock.yaml",
]


# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
#  ANSI STYLES
# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
RST = "\033[0m"; BOL = "\033[1m"; DIM = "\033[2m"
GRY = "\033[90m"; RED = "\033[91m"; GRN = "\033[92m"
YLW = "\033[93m"; BLU = "\033[94m"; MGT = "\033[95m"
CYN = "\033[96m"; WHT = "\033[97m"; ORG = "\033[38;5;208m"

EXT_COL: dict[str, str] = {
    "js": YLW, "ts": YLW, "jsx": YLW, "tsx": YLW, "mjs": YLW,
    "py": YLW, "php": MGT, "java": ORG, "rb": RED,
    "go": BLU, "rs": ORG, "c": BLU, "cpp": BLU, "cs": BLU,
    "json": BLU, "yaml": BLU, "yml": BLU, "toml": BLU, "xml": BLU,
    "env": YLW, "ini": GRY, "cfg": GRY,
    "sh": GRN, "bash": GRN, "zsh": GRN, "fish": GRN,
    "css": GRN, "scss": GRN, "less": GRN,
    "html": ORG, "vue": GRN, "svelte": ORG, "astro": MGT,
    "sql": BLU, "prisma": BLU,
    "md": CYN, "txt": WHT, "rst": WHT,
}

_ANSI_RE = re.compile(r"\033\[[^m]*m")


def get_file_color(filename: str) -> str:
    ext = filename.rsplit(".", 1)[-1].lower() if "." in filename else ""
    return EXT_COL.get(ext, WHT)


def get_emoji(filename: str) -> str:
    name = filename.lower()
    ext  = filename.rsplit(".", 1)[-1].lower() if "." in filename else ""
    if "makefile" in name: return "⚙ "
    return {
        "cs": "✦ ", "py": "🐍", "java": "☕",
        "ts": "🚀", "tsx": "🚀", "js": "🚀", "jsx": "🚀",
        "yaml": "⚙ ", "yml": "⚙ ", "json": "⚙ ", "toml": "⚙ ",
        "md": "📝", "sql": "🗄 ", "css": "🎨", "scss": "🎨",
        "html": "🌐", "vue": "💚", "php": "🐘",
        "go": "🐹", "rs": "🦀",
    }.get(ext, "▸ ")


def vlen(s: str) -> int:
    """Visual length without ANSI codes."""
    return len(_ANSI_RE.sub("", s))


# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
#  DISPLAY PRIMITIVES
# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

def tw() -> int:
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
    nl(); hr("─", RED); log_step("✖", msg, BOL + RED); hr("─", RED); nl()
    sys.exit(1)


def banner(branch: str, mode: str) -> None:
    w = tw()
    now = datetime.now().strftime("%d/%m/%Y  %H:%M")
    MODE_INFO: dict[str, tuple[str, str]] = {
        "dirty"      : ("  Uncommitted changes detected", RED),
        "unpushed"   : ("  Working tree clean  ·  Unpushed commits", YLW),
        "last_commit": ("  Working tree clean  ·  Latest commit", GRN),
    }
    mode_text, mode_col = MODE_INFO.get(mode, ("  Unknown", WHT))

    def _row(text: str, style: str = WHT) -> None:
        pad = max(0, w - 2 - vlen(text))
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
    print(f"  {DIM}{CYN}{'╌' * (tw() - 4)}{RST}")


def file_tree(files_str: str) -> None:
    files = [f.strip() for f in files_str.splitlines() if f.strip()]
    if not files:
        return
    section(f"MODIFIED FILES  ({len(files)})", "📂")
    for i, f in enumerate(files):
        con = "  └─" if i == len(files) - 1 else "  ├─"
        col = get_file_color(f)
        if "/" in f:
            folder, name = f.rsplit("/", 1)
            print(f"  {GRY}{con}  {DIM}{folder}/{RST}{col}{BOL}{name}{RST}")
        else:
            print(f"  {GRY}{con}{RST}  {col}{BOL}{f}{RST}")


# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
#  DIFF PARSING
# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

FileInfo = dict  # {added, removed, added_count, removed_count}


def parse_diff(diff: str) -> dict[str, FileInfo]:
    """Parses a git diff into per-file change data. Stats are always accurate."""
    result: dict[str, FileInfo] = {}
    current: FileInfo | None = None

    for line in diff.splitlines():
        if line.startswith("diff --git "):
            parts = line.split(" b/")
            if len(parts) >= 2:
                key = parts[-1].strip()
                current = {"added": [], "removed": [], "added_count": 0, "removed_count": 0}
                result[key] = current
        elif current is not None:
            if line.startswith("+") and not line.startswith("+++"):
                content = line[1:]
                if content.strip():
                    current["added"].append(content.strip())
                current["added_count"] += 1
            elif line.startswith("-") and not line.startswith("---"):
                content = line[1:]
                if content.strip():
                    current["removed"].append(content.strip())
                current["removed_count"] += 1

    return result


def _find_in(file_data: dict[str, FileInfo], filepath: str) -> FileInfo | None:
    for key, val in file_data.items():
        if key == filepath or filepath.endswith(key) or key.endswith(filepath):
            return val
    return None


def is_large_change(fi: FileInfo) -> bool:
    """True for new files, deleted files, or major rewrites (>LLM_LINE_THRESHOLD lines)."""
    return (fi["added_count"] + fi["removed_count"]) > LLM_LINE_THRESHOLD


def build_llm_context(file_data: dict[str, FileInfo], files: str) -> str:
    """
    Builds focused per-file context for the LLM.
    KEY: skips files with >LLM_LINE_THRESHOLD changed lines — those are new/deleted
    files or full rewrites that raw_summary() handles better than the LLM.
    Only sends +/- lines (no context), max 30 per file.
    """
    file_list = [f.strip() for f in files.splitlines() if f.strip()]
    parts: list[str] = []

    for filepath in file_list:
        fi = _find_in(file_data, filepath)

        # Large changes: new file, deleted file, full rewrite → bypass LLM
        if fi and is_large_change(fi):
            continue

        lines: list[str] = []
        if fi:
            for l in fi["added"][:30]:
                lines.append(f"+ {l}")
            for l in fi["removed"][:30]:
                lines.append(f"- {l}")

        content = "\n".join(lines) if lines else "(no visible code change)"
        parts.append(f"=== {filepath} ===\n{content}")

    return "\n\n".join(parts)


def raw_summary(fi: FileInfo) -> str:
    """
    Fallback summary computed directly from the diff — always accurate.
    Handles large-change cases (new file, deleted, rewrite) with clear labels.
    """
    added   = fi.get("added", [])
    removed = fi.get("removed", [])
    ac = fi.get("added_count", 0)
    rc = fi.get("removed_count", 0)

    # Large changes: auto-detect without LLM
    # if ac > 100 and rc == 0:
    #     return f"Nouveau fichier  —  {ac} lignes"
    if rc > 100 and ac == 0:
        return f"Deleted file  —  {rc} lines"
    if ac > 50 and rc > 50:
        return f"Major refactor  —  +{ac}  −{rc} lines"

    # # Small / medium changes
    # if ac == 1 and rc == 0 and added:
    #     return f"Addition : {added[0]}"
    if ac == 0 and rc == 1 and removed:
        return f"Deletion : {removed[0]}"
    if ac > 0 and rc == 0:
        return f"Addition of {ac} line{'s' if ac > 1 else ''}"
    if rc > 0 and ac == 0:
        return f"Deletion of {rc} line{'s' if rc > 1 else ''}"
    return f"Modifications : +{ac}  −{rc} lines"


def parse_llm_json(raw: str) -> dict[str, str]:
    """
    Robust LLM JSON parser — handles several output formats small models tend to use.
    Returns {filepath: summary}.
    """
    cleaned = raw.strip()
    for fence in ("```json", "```"):
        if cleaned.startswith(fence):
            cleaned = cleaned[len(fence):]
    cleaned = cleaned.rstrip("```").strip()

    summaries: dict[str, str] = {}
    try:
        data = json.loads(cleaned)

        # Format A: {"changes": [{"file": ..., "summary": ...}]}  ← expected
        if isinstance(data.get("changes"), list):
            for item in data["changes"]:
                f = item.get("file") or item.get("path") or item.get("filename", "")
                s = item.get("summary") or item.get("description") or item.get("change", "")
                if f:
                    summaries[f] = s

        # Format B: {"changes": {"file": "summary"}}
        elif isinstance(data.get("changes"), dict):
            summaries = {k: v for k, v in data["changes"].items() if isinstance(v, str)}

        # Format C: {"files": [...]}
        elif isinstance(data.get("files"), list):
            for item in data["files"]:
                f = item.get("file") or item.get("path", "")
                s = item.get("summary") or item.get("description", "")
                if f:
                    summaries[f] = s

        # Format D: {"filename": "summary", ...}  (flat dict)
        elif all(isinstance(v, str) for v in data.values()):
            summaries = data

    except (json.JSONDecodeError, AttributeError):
        pass

    return summaries


# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
#  ANALYSIS DISPLAY
# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

def print_analysis(
    resume_json: str,
    file_data: dict[str, FileInfo],
    file_list: list[str],
) -> None:
    section("SYNTHÈSE DES CHANGEMENTS", "✨")
    nl()

    summaries = parse_llm_json(resume_json)

    def find_summary(filepath: str) -> str:
        """LLM summary with fuzzy matching, falls back to diff-based summary."""
        if filepath in summaries:
            return summaries[filepath]
        base = filepath.split("/")[-1]
        for key, val in summaries.items():
            kb = key.split("/")[-1]
            if kb == base or filepath.endswith(key) or key.endswith(filepath):
                return val
        fi = _find_in(file_data, filepath)
        return raw_summary(fi) if fi else "Modification"

    for filepath in file_list:
        fi        = _find_in(file_data, filepath)
        added_n   = fi["added_count"]   if fi else 0
        removed_n = fi["removed_count"] if fi else 0
        summary   = find_summary(filepath)

        basename  = filepath.split("/")[-1] if "/" in filepath else filepath
        folder    = filepath.rsplit("/", 1)[0] if "/" in filepath else ""
        col       = get_file_color(basename)
        emoji     = get_emoji(basename)

        stat_col = GRN if (added_n > 0 and removed_n == 0) \
              else RED if (removed_n > 0 and added_n == 0) \
              else YLW
        stat_str = f"{DIM}{stat_col}+{added_n}  -{removed_n}{RST}"

        # Card:  ╭─ emoji  Filename
        #        │  folder/   +N -N
        #        ╰─  Summary
        print(f"  {col}╭─ {emoji} {BOL}{basename}{RST}")
        if folder:
            print(f"  {col}│  {DIM}{GRY}{folder}/{RST}   {stat_str}")
        else:
            print(f"  {col}│   {stat_str}")
        print(f"  {col}╰─{RST}  {WHT}{summary}{RST}")
        nl()


def print_footer() -> None:
    nl()
    print(f"  {DIM}{GRY}{'─' * (tw() - 4)}{RST}")
    print(f"  {DIM}{GRY}⚙  Powered by Ollama  ·  Model: {MODEL}{RST}")
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
    status = git("status", "--porcelain").strip()
    tracked_changes = [l for l in status.splitlines() if not l.startswith("??")]

    if tracked_changes:
        diff  = git("diff", "HEAD", "-U1", "-w", *GIT_EXCLUSIONS)
        files = git("diff", "--name-only", "HEAD", *GIT_EXCLUSIONS).strip()
        return "dirty", diff, files

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
        files = "\n".join(
            f for f in files.splitlines()
            if "ModelSnapshot" not in f and "Migrations" not in f
        )

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
    if _ollama_alive():
        log_step("✔", "Ollama already active.", GRN)
        return None, False

    log_step("🚀", "Starting local engine (Ollama)...", YLW)
    try:
        daemon = subprocess.Popen(
            ["ollama", "serve"],
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
        )
    except FileNotFoundError:
        die("Ollama is not installed. See: https://ollama.ai")

    for _ in range(12):
        if _ollama_alive():
            log_step("✔", "Server ready.", GRN)
            return daemon, True
        time.sleep(1)

    daemon.terminate()
    die("Ollama server could not start (timeout 12s).")


def ask_llm(file_data: dict[str, FileInfo], files: str, mode: str) -> str:
    log_step("🧠", "AI semantic analysis in progress...", CYN)
    nl()

    llm_context = build_llm_context(file_data, files)

    # If every file bypassed the LLM (all large), skip the API call entirely
    if not llm_context.strip():
        return "{}"

    CTX = {
        "dirty"      : "uncommitted changes",
        "unpushed"   : "unpushed commits",
        "last_commit": "latest commit",
    }

    # One-shot example: small models follow examples far better than instructions
    EXAMPLE_IN = (
        "=== apps/Api/ProjectController.cs ===\n"
        "+ //test\n\n"
        "=== Makefile ===\n"
        "+ sum:\n"
        "+ \tpython3 apps/Scripts/summerize_context.py"
    )
    EXAMPLE_OUT = (
        '{"changes": ['
        '{"file": "apps/Api/ProjectController.cs", "summary": "Added comment //test"}, '
        '{"file": "Makefile", "summary": "Added make target \'sum\'"}'
        '{"file": "summerize_context.py", "summary": "Added 2 new functions for analyzing and summarizing context"}'
        "]}"
    )

    prompt = f"""Summarize these git {CTX.get(mode, "changes")} in English.
Each === FILE === section shows ONLY changed lines: + added, - removed.

EXAMPLE INPUT:
{EXAMPLE_IN}

EXAMPLE OUTPUT:
{EXAMPLE_OUT}

NOW SUMMARIZE (same JSON format, one entry per === section):
{llm_context}"""

    payload = {
        "model"  : MODEL,
        "prompt" : prompt,
        "format" : "json",
        "stream" : False,
        "options": {
            "temperature": 0.0,
            "num_predict": 800,
            "num_ctx"    : 4096,
        },
    }

    req = urllib.request.Request(
        API_URL,
        data=json.dumps(payload).encode(),
        headers={"Content-Type": "application/json"},
    )
    try:
        with urllib.request.urlopen(req, timeout=120) as resp:
            data = json.loads(resp.read().decode())
            return data.get("response", "{}").strip()
    except urllib.error.HTTPError as exc:
        return f'{{"error": "HTTP {exc.code}"}}'
    except Exception as exc:
        return f'{{"error": "{exc}"}}'


# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
#  MAIN
# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

def main() -> None:
    branch = get_branch()

    try:
        mode, diff, files = get_state()
    except subprocess.CalledProcessError as exc:
        die(f"Could not read Git state: {exc}")
        return

    banner(branch, mode)

    if not diff.strip():
        nl()
        log_step("✨", "Nothing to analyze. Run 'git add .' if you have untracked files.", GRN)
        nl()
        return

    file_tree(files)

    # Parse diff ONCE — reused for stats + LLM context
    file_data = parse_diff(diff)
    file_list = [f.strip() for f in files.splitlines() if f.strip()]

    char_count = len(diff)
    if char_count > MAX_DIFF:
        lines, truncated, current_len = diff.splitlines(), [], 0
        for line in lines:
            if current_len + len(line) + 1 > MAX_DIFF:
                break
            truncated.append(line)
            current_len += len(line) + 1
        diff = "\n".join(truncated)
        nl()
        log_step("⚠", f"Diff truncated to {MAX_DIFF:,} / {char_count:,} chars.", YLW)
    else:
        llm_count = sum(
            1 for f in file_list
            if not (lambda fi: fi and is_large_change(fi))(_find_in(file_data, f))
        )
        large_count = len(file_list) - llm_count
        note = f"  ·  {large_count} auto-detected" if large_count else ""
        nl()
        log_step("📏", f"Diff: {char_count:,} chars  ·  {llm_count} file(s) sent to AI{note}", DIM + GRY)

    nl()
    daemon, started = start_ollama()

    try:
        resume = ask_llm(file_data, files, mode)
        print_analysis(resume, file_data, file_list)
    finally:
        if started and daemon:
            nl()
            log_step("🧹", "Stopping Ollama and freeing RAM...", DIM + GRY)
            daemon.terminate()
            daemon.wait()

    print_footer()


if __name__ == "__main__":
    main()