#!/usr/bin/env python3
"""Fails when the actual publish gate in ci.yml would let an ineligible ref publish.

Test-audit G1 (2026-09-25, Critical): the publish job's eligibility is decided by
three independent gates -- the job's own `if:`, the tag-format check inline in the
"Select package version" step, and the "Require a tag reachable from main" ancestry
check -- and nothing exercised any of them. A test that reimplements those conditions
separately can stay green even if the real gate is weakened or removed (the verifier's
construction warning). So this script extracts the named publish conditions, runs the
actual package-version PowerShell body, and runs the actual ancestry command against a
temporary git repository with a detached tag commit. Only GitHub's event context is
simulated; this never calls the GitHub API or a registry.

Exit codes: 0 clean, 1 a scenario disagreed with its expected eligibility, 2 the
checker could not run (ci.yml missing, or its shape does not match what this parses).
"""
import os
import re
import subprocess
import sys
import tempfile
from pathlib import Path

CI_YML = Path(".github/workflows/ci.yml")


class GateShapeError(Exception):
    pass


def extract(pattern, text, what, flags=0):
    match = re.search(pattern, text, flags)
    if not match:
        raise GateShapeError(f"could not find {what} in {CI_YML}; the workflow's shape has changed.")
    return match.group(1)


def eval_if(expr, event_name, ref):
    """Evaluate a `&&`-joined GitHub Actions `if:` expression of `github.event_name`
    and `github.ref` comparisons/startsWith calls -- the only shapes ci.yml uses."""
    context = {"github.event_name": event_name, "github.ref": ref}

    def atom(text):
        text = text.strip()
        call = re.fullmatch(r"startsWith\(\s*([\w.]+)\s*,\s*'([^']*)'\s*\)", text)
        if call:
            return context[call.group(1)].startswith(call.group(2))
        eq = re.fullmatch(r"([\w.]+)\s*==\s*'([^']*)'", text)
        if eq:
            return context[eq.group(1)] == eq.group(2)
        raise ValueError(f"unrecognised if-atom: {text!r}")

    return all(atom(part) for part in expr.split("&&"))


def named_job(text, name):
    match = re.search(
        rf"^  {re.escape(name)}:\s*\n(.*?)(?=^  [A-Za-z0-9_-]+:\s*$|\Z)",
        text,
        re.MULTILINE | re.DOTALL,
    )
    if not match:
        raise GateShapeError(f"could not find the {name} job in {CI_YML}; the workflow's shape has changed.")
    return match.group(1)


def named_step(job, name):
    matches = list(
        re.finditer(
            rf"^      - name: {re.escape(name)}\s*\n(.*?)(?=^      - (?:name:|uses:|run:|id:)|\Z)",
            job,
            re.MULTILINE | re.DOTALL,
        )
    )
    if len(matches) != 1:
        raise GateShapeError(f"expected exactly one '{name}' step in {CI_YML}; found {len(matches)}.")
    return matches[0].group(1)


def powershell_body(step_body, name):
    match = re.search(r"^        run: \|\s*\n((?:^          .*\n|^\s*\n)+)", step_body, re.MULTILINE)
    if not match:
        raise GateShapeError(f"could not extract the {name} PowerShell body from {CI_YML}.")
    return "\n".join(line[10:] if line.startswith("          ") else "" for line in match.group(1).splitlines())


def tag_step_ok(step_body, repo, ref_type, ref_name):
    script = powershell_body(step_body, "Select package version")
    with tempfile.TemporaryDirectory() as tmp:
        env_path = Path(tmp) / "github_env"
        env_path.touch()
        env = {**os.environ, "REF_TYPE": ref_type, "REF_NAME": ref_name, "GITHUB_ENV": str(env_path)}
        try:
            result = subprocess.run(
                ["pwsh", "-NoProfile", "-NonInteractive", "-Command", script],
                cwd=repo,
                env=env,
                capture_output=True,
                text=True,
            )
        except OSError as error:
            raise GateShapeError(f"could not run the extracted PowerShell release step: {error}") from error
        output = env_path.read_text(encoding="utf-8", errors="replace")
    version = re.search(r"^PACKAGE_VERSION=(.+)$", output, re.MULTILINE)
    return result.returncode == 0 and version is not None, version.group(1) if version else None


def ancestor_ok(ancestor_cmd, repo, sha):
    # Run the actual workflow command with the scenario's tag commit and detached HEAD.
    run_git(repo, "checkout", "--detach", sha)
    resolved = ancestor_cmd.replace("${GITHUB_SHA}", sha).replace("$GITHUB_SHA", sha)
    result = subprocess.run(["bash", "-euc", resolved], cwd=repo, env=os.environ)
    return result.returncode == 0


def run_git(repo, *args):
    return subprocess.run(["git", *args], cwd=repo, check=True, capture_output=True, text=True)


def make_repo(tmp):
    repo = Path(tmp) / "repo"
    repo.mkdir()
    run = lambda *args: run_git(repo, *args)
    run("init", "-q", "-b", "main")
    run("config", "user.email", "test@example.invalid")
    run("config", "user.name", "test")
    (repo / "f.txt").write_text("1")
    run("add", "f.txt")
    run("commit", "-q", "-m", "main commit")
    main_sha = run("rev-parse", "HEAD").stdout.strip()
    run("update-ref", "refs/remotes/origin/main", main_sha)

    # A side branch, diverged from main, never merged -- its tip is NOT an ancestor.
    run("checkout", "-q", "-b", "side")
    (repo / "f.txt").write_text("2")
    run("commit", "-q", "-am", "side commit")
    side_sha = run("rev-parse", "HEAD").stdout.strip()
    run("checkout", "-q", "main")
    return repo, main_sha, side_sha


def main():
    if not CI_YML.is_file():
        print(f"::error::{CI_YML} does not exist; nothing to assert.")
        return 2

    text = CI_YML.read_text(encoding="utf-8")

    publish_job = named_job(text, "publish")

    # Job-level keys (name, if, needs, runs-on, ...) sit before `steps:` at 4-space
    # indent; a step's own `if:` is nested under `steps:` at deeper indent with a
    # leading `- `. Slicing to just before `steps:` stops a step-level `if:` from
    # being picked up as the job condition -- moving the job's `if:` onto its first
    # step must fail this check, not silently pass it as equivalent.
    steps_marker = "\n    steps:\n"
    if steps_marker not in publish_job:
        raise GateShapeError(f"could not find the publish job's steps: in {CI_YML}; the workflow's shape has changed.")
    publish_job_header = publish_job[: publish_job.index(steps_marker)]
    publish_if = extract(r"\n\s*if:\s*(.+)", publish_job_header, "the publish job's if:")

    package_step = named_step(named_job(text, "build-and-test"), "Select package version")
    ancestry_step = named_step(publish_job, "Require a tag reachable from main")

    if re.search(r"^        if:", ancestry_step, re.MULTILINE):
        raise GateShapeError("the ancestry check step has an if: condition and may be skipped.")
    continue_on_error = re.search(r"^        continue-on-error:\s*(.+)$", ancestry_step, re.MULTILINE)
    if continue_on_error and continue_on_error.group(1).strip() not in ("false", "${{ false }}"):
        raise GateShapeError("the ancestry check step may continue after failure.")
    ancestor_cmd = extract(r"^        run:\s*(git merge-base[^\n]+)", ancestry_step, "the ancestry check command", re.MULTILINE)
    if "origin/main" not in ancestor_cmd:
        raise GateShapeError("the ancestry check must pin its target to origin/main.")

    with tempfile.TemporaryDirectory() as tmp:
        repo, main_sha, side_sha = make_repo(tmp)

        # (event_name, ref, sha, tag_name, expected eligible)
        scenarios = [
            ("push", "refs/tags/v1.2.3", main_sha, "v1.2.3", True),
            ("push", "refs/tags/v1.2", main_sha, "v1.2", False),  # malformed version
            ("push", "refs/tags/v1.0.0", side_sha, "v1.0.0", False),  # not on main
            ("push", "refs/heads/main", main_sha, "", False),  # plain push, no tag
            ("pull_request", "refs/tags/v1.2.3", main_sha, "v1.2.3", False),  # wrong event
        ]

        failed = False
        for event_name, ref, sha, tag_name, expected in scenarios:
            if_ok = eval_if(publish_if, event_name, ref)
            # The tag-format and ancestry gates only apply once the job's own `if:`
            # would even let this ref reach the publish job; a plain branch push
            # never runs the "Select package version" tag branch either.
            eligible = if_ok
            if eligible and ref.startswith("refs/tags/"):
                run_git(repo, "tag", "-a", tag_name, sha, "-m", tag_name)
                tag_ok, version = tag_step_ok(package_step, repo, "tag", tag_name)
                eligible = tag_ok and version == tag_name[1:] and ancestor_ok(ancestor_cmd, repo, sha)
            label = f"event={event_name} ref={ref} tag={tag_name!r}"
            if eligible != expected:
                print(f"::error::{label}: expected eligible={expected}, got {eligible}")
                failed = True
            else:
                print(f"OK {label}: eligible={eligible}")

    if failed:
        return 1
    print("Release eligibility: all scenarios matched the actual workflow gate.")
    return 0


if __name__ == "__main__":
    try:
        exit_code = main()
    except (GateShapeError, ValueError, KeyError) as error:
        print(f"::error::{error}")
        exit_code = 2
    sys.exit(exit_code)
