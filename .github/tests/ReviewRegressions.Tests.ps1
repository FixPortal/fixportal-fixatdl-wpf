#Requires -Version 7
BeforeAll {
    $script:repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
    $script:skill = Join-Path $repoRoot '.github/ai-smells'
    . (Join-Path $skill 'detect.ps1')
    . (Join-Path $skill 'pr-scope.ps1')
    function semgrep { }
    $script:catalogue = [pscustomobject]@{ Smells = @(
        [pscustomobject]@{ id = 'S13'; languages = @('csharp', 'python'); detector = [pscustomobject]@{ kind = 'semgrep'; rule = 'detectors/rules/S13.yml' } }
    ) }
    function Detect {
        Invoke-SmellDetectors -TreePath $TestDrive -Catalogue $catalogue -SkillRoot $skill -OutDir (Join-Path $TestDrive 'out') -SemgrepCommand semgrep -JscpdCommand @()
    }
}

Describe 'Review detector contracts' {
    BeforeEach {
        Set-Content (Join-Path $TestDrive 'A.cs') 'class A {}'
        Set-Content (Join-Path $TestDrive 'B.py') 'x = 1'
        Mock semgrep {
            $global:LASTEXITCODE = 0
            if ($args -contains '--version') { return '1.163.0' }
            '{"results":[],"errors":[],"paths":{"skipped":[]}}'
        }
    }

    It 'F2 asks semgrep for skipped paths with verbose output' {
        $null = Detect
        Should -Invoke semgrep -Times 1 -ParameterFilter { $args -contains 'scan' -and $args -contains '--verbose' -and $args -notcontains '--quiet' }
    }

    It 'F2 reports permission skips as coverage gaps' {
        Mock semgrep {
            $global:LASTEXITCODE = 0
            if ($args -contains '--version') { return '1.163.0' }
            @{ results = @(); errors = @(); paths = @{ skipped = @(@{ path = (Join-Path $TestDrive 'A.cs'); reason = 'insufficient_permissions' }) } } | ConvertTo-Json -Depth 6
        }
        (Detect).Coverage | Where-Object reason -Match 'csharp' | Select-Object -ExpandProperty status | Should -Be 'not assessed'
    }

    It 'F3 a file-specific warning preserves coverage for the other language' {
        Mock semgrep {
            $global:LASTEXITCODE = 0
            if ($args -contains '--version') { return '1.163.0' }
            @{ results = @(); errors = @(@{ path = (Join-Path $TestDrive 'A.cs'); level = 'warn'; type = 'PartialParsing'; message = 'partial parse' }); paths = @{ skipped = @() } } | ConvertTo-Json -Depth 6
        }
        $coverage = (Detect).Coverage
        $coverage | Where-Object reason -EQ 'python' | Select-Object -ExpandProperty status | Should -Be 'assessed'
        $coverage | Where-Object reason -Match 'csharp' | Select-Object -ExpandProperty status | Should -Be 'not assessed'
    }

    It 'F3 a pathless error remains a run-wide gap' {
        Mock semgrep {
            $global:LASTEXITCODE = 0
            if ($args -contains '--version') { return '1.163.0' }
            '{"results":[],"errors":[{"message":"engine error"}],"paths":{"skipped":[]}}'
        }
        @((Detect).Coverage | Where-Object status -NE 'not assessed').Count | Should -Be 0
    }

    It 'CX3 comments inside a catch do not move its start or include finally' {
        Set-Content (Join-Path $TestDrive 'A.cs') "try { Work(); }`ncatch (Exception ex)`n{`n    // catch intentionally ignored`n}`nfinally {`n    Work();`n}"
        Mock semgrep {
            $global:LASTEXITCODE = 0
            if ($args -contains '--version') { return '1.163.0' }
            @{ results = @(@{ check_id = 'S13'; path = (Join-Path $TestDrive 'A.cs'); start = @{ line = 1 }; end = @{ line = 8 }; extra = @{ lines = "try { Work(); }`ncatch (Exception ex)`n{`n    // catch intentionally ignored`n}`nfinally {`n    Work();`n}" } }); errors = @(); paths = @{ skipped = @() } } | ConvertTo-Json -Depth 6
        }
        $hit = (Detect).Hits[0]
        $hit.scopeStart | Should -Be 2
        $hit.endLine | Should -Be 5
        Test-HitOnAddedLines $hit @{ 'A.cs' = [Collections.Generic.HashSet[int]]::new([int[]]@(7)) } | Should -BeFalse
        Test-HitOnAddedLines $hit @{} @{ 'A.cs' = [Collections.Generic.HashSet[int]]::new([int[]]@(3)) } | Should -BeTrue
    }

    It 'CX3 nested try matches emit each distinct empty catch once' -TestCases @(
        @{ Text = 'try { try { Work(); } catch {} } catch {}'; InnerStart = 1; InnerEnd = 1; OuterEnd = 1 },
        @{ Text = "try {`n    try { Work(); }`n    catch {}`n}`ncatch {}"; InnerStart = 2; InnerEnd = 3; OuterEnd = 5 }
    ) {
        param($Text, $InnerStart, $InnerEnd, $OuterEnd)
        Set-Content (Join-Path $TestDrive 'A.cs') $Text
        $script:nestedResults = @(@{ check_id = 'S13'; path = (Join-Path $TestDrive 'A.cs'); start = @{ line = 1 }; end = @{ line = $OuterEnd }; extra = @{ lines = $Text } },
            @{ check_id = 'S13'; path = (Join-Path $TestDrive 'A.cs'); start = @{ line = $InnerStart }; end = @{ line = $InnerEnd }; extra = @{ lines = $Text } })
        Mock semgrep {
            $global:LASTEXITCODE = 0
            if ($args -contains '--version') { return '1.163.0' }
            @{ results = $nestedResults; errors = @(); paths = @{ skipped = @() } } | ConvertTo-Json -Depth 6
        }
        @((Detect).Hits).Count | Should -Be 2
    }

    It 'CX3 an unscopable producer result reports a coverage gap' {
        Mock semgrep {
            $global:LASTEXITCODE = 0
            if ($args -contains '--version') { return '1.163.0' }
            @{ results = @(@{ check_id = 'S13'; path = (Join-Path $TestDrive 'A.cs'); start = @{ line = 1 }; end = @{ line = 1 }; extra = @{ lines = 'class A {}' } }); errors = @(); paths = @{ skipped = @() } } | ConvertTo-Json -Depth 6
        }
        $result = Detect
        @($result.Hits).Count | Should -Be 0
        $result.Coverage | Where-Object reason -Match 'could not scope' | Select-Object -ExpandProperty status | Should -Be 'not assessed'
    }

    It 'CX3 strings and populated sibling clauses are excluded from catch spans' {
        $text = "try { var text = `"catch { }`"; }`ncatch (First ex) { Work(); }`ncatch (Second ex) { /* catch { } */ }`nfinally { Work(); }"
        $spans = @(Get-CatchSpans $text 10 'csharp')
        $spans.Count | Should -Be 1
        $spans[0].Start | Should -Be 12
        $spans[0].End | Should -Be 12
    }

    It 'C# preprocessor directives are not Python comments' {
        @(Get-CatchSpans "try {}`ncatch {`n#if DEBUG`n#endif`n}" 1 'csharp').Count | Should -Be 0
    }

    It 'CX3 Python except spans end before finally and ignore comments' {
        $text = "try:`n    work()`nexcept ValueError:`n    # except ignored`n    pass`nfinally:`n    work()"
        $spans = @(Get-CatchSpans $text 1 'python')
        $spans.Count | Should -Be 1
        $spans[0].Start | Should -Be 3
        $spans[0].End | Should -Be 5
    }

    It 'CX3 Python inline and wrapped exception headers retain their handlers' -TestCases @(
        @{ Text = "try:`n    work()`nexcept ValueError: pass"; End = 3 },
        @{ Text = "try:`n    work()`nexcept (`n    ValueError,`n    TypeError,`n):`n    pass"; End = 7 }
    ) {
        param($Text, $End)
        $spans = @(Get-CatchSpans $Text 1 'python')
        $spans.Count | Should -Be 1
        $spans[0].Start | Should -Be 3
        $spans[0].End | Should -Be $End
    }

    It 'F4 inserting an identical anchor above preserves old markers and posts the new hit' {
        $old = @(
            [pscustomobject]@{ smell = 'S13'; file = 'A.cs'; line = 4; lineText = 'catch {}'; context = 'void First()' },
            [pscustomobject]@{ smell = 'S13'; file = 'A.cs'; line = 8; lineText = 'catch {}'; context = 'void Second()' }
        )
        $existing = @(Add-HitOccurrence $old | ForEach-Object { Get-SmellMarker $_ })
        $new = [pscustomobject]@{ smell = 'S13'; file = 'A.cs'; line = 1; lineText = 'catch {}'; context = 'void New()' }
        $old[0].line = 7; $old[1].line = 11
        $selected = Select-NewSmellHits -Hits (@($new) + $old) -Existing $existing
        $selected.Post.Count | Should -Be 1
        $selected.Post[0].context | Should -Be 'void New()'
        @(Add-HitOccurrence $old | ForEach-Object { Get-SmellMarker $_ }) | Should -Be $existing
    }

    It 'F4 derives context from the nearest different source line, ignoring braces' {
        Get-HitContext @('void First()', '{', 'catch {}') 3 | Should -Be 'void First()'
        Get-HitContext @('void New()', '{', 'catch {}', 'void First()', '{', 'catch {}') 6 | Should -Be 'void First()'
    }

    It 'CX4 deletion-only density hits are retained and anchored above the deletion' {
        $hit = [pscustomobject]@{ smell = 'S41'; file = 'A.cs'; line = 1; endLine = 25; scopeStart = 1 }
        $deleted = @{ 'A.cs' = [Collections.Generic.HashSet[int]]::new([int[]]@(12)) }
        Test-HitOnAddedLines $hit @{} $deleted | Should -BeTrue
        (Get-AnchoredHit $hit @{} $deleted).line | Should -Be 12
    }

    It 'CX4 whole-file density includes boundary deletions with a valid anchor' -TestCases @(
        @{ Point = 0; Anchor = 1 },
        @{ Point = 20; Anchor = 20 }
    ) {
        param($Point, $Anchor)
        $hit = [pscustomobject]@{ smell = 'S41'; file = 'A.cs'; line = 1; endLine = 20; scopeStart = 1 }
        $deleted = @{ 'A.cs' = [Collections.Generic.HashSet[int]]::new([int[]]@($Point)) }
        Test-HitOnAddedLines $hit @{} $deleted | Should -BeTrue
        (Get-AnchoredHit $hit @{} $deleted).line | Should -Be $Anchor
    }
}
