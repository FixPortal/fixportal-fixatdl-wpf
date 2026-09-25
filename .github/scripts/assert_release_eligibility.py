#!/usr/bin/env python3
"""Fails when the actual publish gate in ci.yml would let an ineligible ref publish.

Test-audit G1 (2026-09-25, Critical): the publish job's eligibility is decided by
three independent gates -- the job's own `if:`, the tag-format check inline in the
"Select package version" step, and the "Require a tag reachable from main" ancestry
check -- and nothing exercised any of them. A test that reimplements those conditions
separately can stay green even if the real gate is weakened or removed (the verifier's
construction warning). So this script does not reimplement them: it EXTRACTS the exact
`if:` string, tag regex, and ancestry command from ci.yml's own text and evaluates
those, against a real git repository it builds and a tiny real expression evaluator.
Only the GitHub event/API boundary and package publishing are stubbed -- there is
nothing to stub for those since this never calls the GitHub API or a registry.

Exit codes: 0 clean, 1 a scenario disagreed with its expected eligibility, 2 the
checker could not run (ci.yml missing, or its shape does not match what this parses).
"""
import re
import subprocess
import sys
import tempfile
from pathlib import Path

CI_YML = Path(".github/workflows/ci.yml")


def extract(pattern, text, what, flags=0):
    match = re.search(pattern, text, flags)
    if not match:
        sys.exit(f"could not find {what} in {CI_YML}; the workflow's shape has changed.")
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


def tag_format_ok(regex, tag_name):
    return re.fullmatch(regex, tag_name) is not None


def ancestor_ok(ancestor_cmd, repo, sha, target_ref):
    # The extracted command is the literal bash line from the workflow, e.g.
    # `git merge-base --is-ancestor "${GITHUB_SHA}^{commit}" origin/main`.
    resolved = ancestor_cmd.replace("${GITHUB_SHA}", sha).replace("$GITHUB_SHA", sha)
    result = subprocess.run(["bash", "-c", resolved], cwd=repo)
    return result.returncode == 0


def make_repo(tmp):
    repo = Path(tmp) / "repo"
    repo.mkdir()
    run = lambda *args: subprocess.run(
        ["git", *args], cwd=repo, check=True, capture_output=True, text=True
    )
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

    marker = "\n  publish:\n"
    if marker not in text:
        sys.exit(f"could not find the publish job in {CI_YML}; the workflow's shape has changed.")
    publish_job = text[text.index(marker) + len(marker) :]  # last job in the file

    # Job-level keys (name, if, needs, runs-on, ...) sit before `steps:` at 4-space
    # indent; a step's own `if:` is nested under `steps:` at deeper indent with a
    # leading `- `. Slicing to just before `steps:` stops a step-level `if:` from
    # being picked up as the job condition -- moving the job's `if:` onto its first
    # step must fail this check, not silently pass it as equivalent.
    steps_marker = "\n    steps:\n"
    if steps_marker not in publish_job:
        sys.exit(f"could not find the publish job's steps: in {CI_YML}; the workflow's shape has changed.")
    publish_job_header = publish_job[: publish_job.index(steps_marker)]
    publish_if = extract(r"\n\s*if:\s*(.+)", publish_job_header, "the publish job's if:")

    tag_regex = extract(r"-notmatch\s+'(\^v[^']+\$)'", text, "the tag-format regex")

    ancestor_step_match = re.search(
        r"Require a tag reachable from main\s*\n(.*?)(?=\n\s*-\s*(?:name|uses|run):|\Z)",
        text,
        re.DOTALL,
    )
    if not ancestor_step_match:
        sys.exit(f"could not find the ancestry check step in {CI_YML}; the workflow's shape has changed.")
    ancestor_step_body = ancestor_step_match.group(1)
    if re.search(r"continue-on-error:\s*true", ancestor_step_body):
        sys.exit(
            "the ancestry check step sets continue-on-error: true; a failed ancestry "
            "check would no longer block publishing."
        )
    ancestor_cmd = extract(
        r"run:\s*(git merge-base[^\n]+)", ancestor_step_body, "the ancestry check command"
    )

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
                eligible = tag_format_ok(tag_regex, tag_name) and ancestor_ok(
                    ancestor_cmd, repo, sha, "origin/main"
                )
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
    sys.exit(main())
