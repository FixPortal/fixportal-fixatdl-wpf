"""Offline contracts for the 2026-10-13 review, exercising the shipped tooling."""
import importlib.util
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[2]
SMELLS = ROOT / '.github/ai-smells'


def command(*args, cwd=ROOT):
    result = subprocess.run(args, cwd=cwd, capture_output=True, text=True, timeout=60)
    if result.returncode:
        raise AssertionError(result.stdout + result.stderr)
    return result.stdout


def load(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


release = load('release', ROOT / '.github/scripts/assert_release_eligibility.py')


class ReviewRegressions(unittest.TestCase):
    def detector(self, name, files):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            for rel, text in files.items():
                (root / rel).write_text(text, encoding='utf-8')
            listing = root / 'files.txt'
            listing.write_text('\n'.join(files), encoding='utf-8')
            return json.loads(command(sys.executable, str(SMELLS / 'detectors/scripts' / name),
                                      '--root', folder, '--files', str(listing), '--smell', 'test'))

    def test_F1_new_oversized_function_is_not_hidden_by_baseline(self):
        pairs = [
            ('public void Handle1() {', 'public void Handle2() {'),
            ('public void Handle(\n    int value) {', 'public void Handle(\n    string value) {'),
            ('public void ' + 'Long' * 22 + '1() {', 'public void ' + 'Long' * 22 + '2() {'),
        ]
        for first, second in pairs:
            with self.subTest(second=second), tempfile.TemporaryDirectory() as folder:
                root = Path(folder)
                command('git', 'init', '-q', folder)
                command('git', 'config', 'user.name', 'Review test', cwd=root)
                command('git', 'config', 'user.email', 'test@example.invalid', cwd=root)
                def body(signature, length):
                    return signature + '\n' + '    Work();\n' * length + '}\n'
                file = root / 'Example.cs'
                file.write_text(body(first, 130) + body(second, 110), encoding='utf-8')
                command('git', 'add', '.', cwd=root)
                command('git', 'commit', '-qm', 'base', cwd=root)
                base = command('git', 'rev-parse', 'HEAD', cwd=root).strip()
                file.write_text(body(first, 130) + body(second, 130), encoding='utf-8')
                command('git', 'add', '.', cwd=root)
                command('git', 'commit', '-qm', 'head', cwd=root)
                catalogue = root / 'smells.json'
                catalogue.write_text(json.dumps({'smells': [{'id': 'S30', 'name': 'size',
                    'languages': ['any'], 'detector': {'kind': 'script', 'rule': 'detectors/scripts/size_outliers.py'}}]}))
                output = root / 'hits.json'
                command('pwsh', '-NoProfile', '-File', str(SMELLS / 'pr-detect.ps1'),
                        '-RepoPath', folder, '-Base', base, '-Head', 'HEAD', '-OutFile', str(output),
                        '-SmellsPath', str(catalogue))
                hits = json.loads(output.read_text())['hits']
                self.assertEqual(len(hits), 1, 'F1: only the newly oversized second function should remain')
                self.assertGreater(hits[0]['line'], 130)

    def test_F7_functions_named_record_and_struct_are_measured(self):
        for signature in ('function record(value) {', 'function struct(value) {', 'public void record() {'):
            with self.subTest(signature=signature):
                result = self.detector('size_outliers.py', {'large.ts': signature + '\n' + '    work();\n' * 130 + '}\n'})
                self.assertEqual(len(result['hits']), 1, 'F7: keyword-like function names must be measured')
        result = self.detector('size_outliers.py', {'type.cs': 'public record R(int X) {\n' + '    int x;\n' * 130 + '}\n'})
        self.assertEqual(result['hits'], [])

    def test_CX1_comment_density_measures_other_source_extensions(self):
        for extension in ('.ps1', '.psm1', '.mjs', '.cjs', '.go'):
            with self.subTest(extension=extension):
                prefix = '#' if extension in ('.ps1', '.psm1') else '//'
                result = self.detector('comment_density.py', {'probe' + extension: (prefix + ' comment\n') * 20})
                self.assertEqual(len(result['hits']), 1, 'CX1: source files must be measured')
        self.assertEqual(self.detector('comment_density.py', {'readme.md': '# Heading\n' * 20})['hits'], [])

    def test_CX1_size_reports_unsupported_languages(self):
        result = self.detector('size_outliers.py', {'probe.ps1': 'Write-Host test\n'})
        self.assertEqual(result['status'], 'not assessed', 'CX1: skipped source must not look assessed')
        self.assertIn('probe.ps1', result['reason'])

    def test_CX6_comments_and_step_names_are_not_environment_writes(self):
        job = '''
      # GITHUB_ENV is set below
      - name: Select GITHUB_ENV version
        run: |
          # mention GITHUB_ENV in a comment
          "PACKAGE_VERSION=$v" >> $env:GITHUB_ENV # GITHUB_ENV
'''
        release.check_env_writes(job)

    def test_CX6_second_writer_and_zero_writers_are_rejected(self):
        for writer in ('"X=1" >> "$GITHUB_ENV"', 'Add-Content -LiteralPath $env:GITHUB_ENV -Value "X=1"',
                       'Set-Content -Path $env:GITHUB_ENV -Value "X=1"'):
            with self.subTest(writer=writer), self.assertRaises(release.GateShapeError):
                release.check_env_writes('''
      - run: |
          "PACKAGE_VERSION=$v" >> $env:GITHUB_ENV
      - run: |\n          ''' + writer + '\n')
        with self.assertRaises(release.GateShapeError):
            release.check_env_writes('      - run: echo GITHUB_ENV\n')

    def test_CX6_literal_writer_examples_are_not_executed_writes(self):
        release.check_env_writes('''
      - run: |
          echo 'Example: "X=1" >> $env:GITHUB_ENV'
          "PACKAGE_VERSION=$v" >> "$env:GITHUB_ENV"
''')

    def test_CX6_braced_powershell_paths_are_writers(self):
        for writer in ('"X=1" >> ${env:GITHUB_ENV}', 'Add-Content -LiteralPath "${env:GITHUB_ENV}" -Value "X=1"'):
            with self.subTest(writer=writer), self.assertRaises(release.GateShapeError):
                release.check_env_writes('      - run: |\n          "PACKAGE_VERSION=$v" >> $env:GITHUB_ENV\n          ' + writer + '\n')


if __name__ == '__main__':
    unittest.main()
