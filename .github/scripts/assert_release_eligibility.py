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


SUBPROCESS_TIMEOUT = 60


def safe_env(extra=None):
    """os.environ minus GIT_* variables, plus any overrides.

    A caller-exported GIT_DIR/GIT_WORK_TREE would point child git processes --
    including any git run inside the extracted workflow body -- at the real
    repository instead of the temporary one this checker builds."""
    env = {key: value for key, value in os.environ.items() if not key.startswith("GIT_")}
    if extra:
        env.update(extra)
    return env


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
            rf"^      - name: {re.escape(name)}\s*\n(.*?)(?=^      - |\Z)",
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


def version_step_env(step_body, ref_type, ref_name):
    """Resolve the 'Select package version' step's env: block against the simulated event.

    REF_TYPE and REF_NAME reach the step body only through this block in ci.yml. A
    checker that names the variables itself stays green when the block is deleted or
    rewired -- which is exactly the weakening this script exists to catch, because the
    body then skips its semver throw and keeps the 1.0.0 default for every tag. The
    simulated event values are therefore fed THROUGH the parsed expressions, and any
    other wiring fails closed."""
    match = re.search(r"^        env:\s*\n((?:^ {10}\S[^\n]*\n)+)", step_body, re.MULTILINE)
    if not match:
        raise GateShapeError(
            f"could not find an env: block on the 'Select package version' step in {CI_YML}; "
            "the workflow's shape has changed."
        )
    entries = {}
    for line in match.group(1).splitlines():
        key, _, raw = line.strip().partition(":")
        entries[key.strip()] = raw.strip()
    for variable, source in (("REF_TYPE", "github.ref_type"), ("REF_NAME", "github.ref_name")):
        if re.fullmatch(r"\$\{\{\s*" + re.escape(source) + r"\s*\}\}", entries.get(variable, "")) is None:
            raise GateShapeError(f"the version step must set {variable} from {source}; found {entries.get(variable)!r}.")
    context = {"github.ref_type": ref_type, "github.ref_name": ref_name}
    resolved = {}
    for key, value in entries.items():
        expression = re.fullmatch(r"\$\{\{\s*([\w.]+)\s*\}\}", value)
        if expression is None:
            resolved[key] = value
        elif expression.group(1) in context:
            resolved[key] = context[expression.group(1)]
        else:
            raise GateShapeError(
                f"the version step's {key} comes from {expression.group(1)}, which this checker cannot simulate."
            )
    return resolved


def run_version_step(step_body, repo, ref_type, ref_name):
    script = powershell_body(step_body, "Select package version")
    step_env = version_step_env(step_body, ref_type, ref_name)
    with tempfile.TemporaryDirectory() as tmp:
        env_path = Path(tmp) / "github_env"
        env_path.touch()
        env = safe_env({**step_env, "GITHUB_ENV": str(env_path)})
        try:
            result = subprocess.run(
                ["pwsh", "-NoProfile", "-NonInteractive", "-Command", script],
                cwd=repo,
                env=env,
                capture_output=True,
                text=True,
                timeout=SUBPROCESS_TIMEOUT,
            )
        except OSError as error:
            raise GateShapeError(f"could not run the extracted PowerShell release step: {error}") from error
        output = env_path.read_text(encoding="utf-8", errors="replace")
    version = re.search(r"^PACKAGE_VERSION=(.+)$", output, re.MULTILINE)
    return result.returncode, version.group(1) if version else None


def ancestor_ok(ancestor_cmd, repo, sha):
    # Run the actual workflow command with the scenario's tag commit and detached HEAD.
    run_git(repo, "checkout", "--detach", sha)
    resolved = ancestor_cmd.replace("${GITHUB_SHA}", sha).replace("$GITHUB_SHA", sha)
    result = subprocess.run(["bash", "-euc", resolved], cwd=repo, env=safe_env(), timeout=SUBPROCESS_TIMEOUT)
    return result.returncode == 0


def run_git(repo, *args):
    return subprocess.run(
        ["git", *args], cwd=repo, env=safe_env(), check=True, capture_output=True, text=True, timeout=SUBPROCESS_TIMEOUT
    )


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
    # ci-gate is what stops a tag publishing over a red build-and-test or gate-coverage;
    # nothing else checks publish's needs:, so a needs: [build-and-test] edit would pass
    # every gate. The match is anchored to the job's four-space top level: an unanchored
    # search is satisfied by a `needs:` key nested in the job's env: block, which GitHub
    # reads as an environment variable while the job itself needs nothing. Flow style
    # (`needs: [a, b]`) and a bare scalar both parse; a block-style list is a shape change
    # and fails closed above.
    publish_needs = extract(r"\n    needs:[ \t]*(.+)", publish_job_header, "the publish job's needs:")
    needed = [job.strip() for job in publish_needs.strip().strip("[]").split(",") if job.strip()]
    if "ci-gate" not in needed:
        raise GateShapeError(f"the publish job must need ci-gate; found needs: {publish_needs.strip()}.")

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

        # (event_name, ref, sha, tag_name, version-step expectation, expected eligible)
        # The version-step expectation is True where the step must ACCEPT the tag (exit 0,
        # emitting PACKAGE_VERSION == tag without the 'v'), False where it must REJECT it
        # (a NON-ZERO exit), and None where the job's own if: keeps the ref away from the
        # publish steps entirely. Rejection is asserted from the exit code, not inferred
        # from version inequality: a step whose semver throw was weakened still exits 0 and
        # still emits SOME version, and inequality alone counted that as a rejection.
        scenarios = [
            ("push", "refs/tags/v1.2.3", main_sha, "v1.2.3", True, True),
            ("push", "refs/tags/v1.2", main_sha, "v1.2", False, False),  # malformed version
            ("push", "refs/tags/v1.0.0", side_sha, "v1.0.0", True, False),  # not on main
            ("push", "refs/heads/main", main_sha, "", None, False),  # plain push, no tag
            ("pull_request", "refs/tags/v1.2.3", main_sha, "v1.2.3", None, False),  # wrong event
        ]

        failed = False
        for event_name, ref, sha, tag_name, step_expected, expected in scenarios:
            if_ok = eval_if(publish_if, event_name, ref)
            # The tag-format and ancestry gates only apply once the job's own `if:`
            # would even let this ref reach the publish job; a plain branch push
            # never runs the "Select package version" tag branch either.
            eligible = if_ok
            label = f"event={event_name} ref={ref} tag={tag_name!r}"
            if eligible and ref.startswith("refs/tags/"):
                run_git(repo, "tag", "-a", tag_name, sha, "-m", tag_name)
                exit_code, version = run_version_step(package_step, repo, "tag", tag_name)
                accepted = exit_code == 0
                if accepted != step_expected:
                    print(
                        f"::error::{label}: the version step exited {exit_code}, "
                        f"expected {'accept' if step_expected else 'reject'}"
                    )
                    failed = True
                if accepted and version != tag_name[1:]:
                    print(f"::error::{label}: PACKAGE_VERSION={version!r}, expected {tag_name[1:]!r}")
                    failed = True
                eligible = accepted and version == tag_name[1:] and ancestor_ok(ancestor_cmd, repo, sha)
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
    except (
        GateShapeError,
        ValueError,
        KeyError,
        subprocess.TimeoutExpired,
        subprocess.CalledProcessError,
        OSError,
    ) as error:
        print(f"::error::{error}")
        exit_code = 2
    sys.exit(exit_code)
