#Requires -Version 7.0
<#
.SYNOPSIS
    Ingests NBomber CSV reports and produces a filled comparison report.

.DESCRIPTION
    Reads NBomber stats CSV files from tests/Benchmarks/reports/ subfolders,
    extracts latency/throughput metrics, and fills report-template.md placeholders.

.PARAMETER ReportsRoot
    Path to the NBomber reports root folder. Default: tests/Benchmarks/reports

.PARAMETER OutputPath
    Path for the generated report. Default: report.md

.EXAMPLE
    ./tools/build-report.ps1
    ./tools/build-report.ps1 -ReportsRoot ./tests/Benchmarks/reports -OutputPath ./report.md
#>

param(
    [string]$ReportsRoot = "tests/Benchmarks/reports",
    [string]$OutputPath = "report.md",
    [string]$TemplatePath = "report-template.md"
)

$ErrorActionPreference = "Stop"

function Find-StatsFile {
    param([string]$ScenarioFolder)
    $folder = Join-Path $ReportsRoot $ScenarioFolder
    if (-not (Test-Path $folder)) { return $null }
    # NBomber generates stats CSV files named like *stats*.csv
    $csvFiles = Get-ChildItem -Path $folder -Filter "*stats*.csv" -Recurse | Sort-Object LastWriteTime -Descending
    if ($csvFiles.Count -gt 0) { return $csvFiles[0].FullName }
    # Fallback: any CSV
    $csvFiles = Get-ChildItem -Path $folder -Filter "*.csv" -Recurse | Sort-Object LastWriteTime -Descending
    if ($csvFiles.Count -gt 0) { return $csvFiles[0].FullName }
    return $null
}

function Parse-NbomberCsv {
    param([string]$CsvPath)
    if (-not $CsvPath -or -not (Test-Path $CsvPath)) { return @() }
    $rows = Import-Csv -Path $CsvPath
    return $rows
}

function Get-ScenarioMetrics {
    param(
        [object[]]$Rows,
        [string]$ScenarioNameContains
    )
    $match = $Rows | Where-Object {
        $_.scenario_name -like "*$ScenarioNameContains*" -or
        $_.ScenarioName -like "*$ScenarioNameContains*" -or
        $_.'scenario' -like "*$ScenarioNameContains*"
    }
    if ($match) { return $match[0] }
    return $null
}

function Safe-Value {
    param($Row, [string[]]$Keys, [string]$Default = "—")
    foreach ($key in $Keys) {
        if ($Row -and $Row.PSObject.Properties[$key] -and $Row.$key) {
            return $Row.$key
        }
    }
    return $Default
}

# Scenario folder mapping
$scenarioFolders = @{
    "S1" = "1_simple_get"
    "S2" = "2_deep_graph"
    "S3" = "3_overfetch"
    "S4" = "4_n_plus_1"
    "S5" = "5_write_readback"
}

# REST/GraphQL name patterns in NBomber scenarios
$restPatterns = @("rest", "REST")
$gqlPatterns = @("graphql", "GraphQL", "gql")

# Read template
if (-not (Test-Path $TemplatePath)) {
    Write-Error "Template not found at: $TemplatePath"
    exit 1
}
$report = Get-Content -Path $TemplatePath -Raw

# Replace generation timestamp
$report = $report -replace '\{\{GENERATED_AT\}\}', (Get-Date -Format "yyyy-MM-dd HH:mm:ss UTC")

# Process each scenario
foreach ($scenarioKey in $scenarioFolders.Keys | Sort-Object) {
    $folder = $scenarioFolders[$scenarioKey]
    $csvPath = Find-StatsFile -ScenarioFolder $folder

    if (-not $csvPath) {
        Write-Warning "No stats CSV found for $scenarioKey ($folder) — skipping."
        continue
    }

    Write-Host "Processing $scenarioKey from: $csvPath"
    $rows = Parse-NbomberCsv -CsvPath $csvPath

    # Find REST row
    $restRow = $null
    foreach ($pattern in $restPatterns) {
        $restRow = Get-ScenarioMetrics -Rows $rows -ScenarioNameContains $pattern
        if ($restRow) { break }
    }

    # Find GraphQL row
    $gqlRow = $null
    foreach ($pattern in $gqlPatterns) {
        $gqlRow = Get-ScenarioMetrics -Rows $rows -ScenarioNameContains $pattern
        if ($gqlRow) { break }
    }

    # Extract metrics — NBomber CSV column names vary by version, try common ones
    $p50Keys = @("latency_p50", "Latency50", "p50", "mean")
    $p95Keys = @("latency_p95", "Latency95", "p95")
    $p99Keys = @("latency_p99", "Latency99", "p99")
    $rpsKeys = @("request_count", "RequestCount", "ok_count", "OkCount", "all_request_count")
    $bytesKeys = @("data_transfer_min_kb", "AllBytes", "data_transfer_all_bytes")

    # Fill placeholders
    $replacements = @{
        "${scenarioKey}_REST_P50"   = Safe-Value $restRow $p50Keys
        "${scenarioKey}_REST_P95"   = Safe-Value $restRow $p95Keys
        "${scenarioKey}_REST_P99"   = Safe-Value $restRow $p99Keys
        "${scenarioKey}_REST_RPS"   = Safe-Value $restRow $rpsKeys
        "${scenarioKey}_REST_BYTES" = Safe-Value $restRow $bytesKeys
        "${scenarioKey}_GQL_P50"    = Safe-Value $gqlRow $p50Keys
        "${scenarioKey}_GQL_P95"    = Safe-Value $gqlRow $p95Keys
        "${scenarioKey}_GQL_P99"    = Safe-Value $gqlRow $p99Keys
        "${scenarioKey}_GQL_RPS"    = Safe-Value $gqlRow $rpsKeys
        "${scenarioKey}_GQL_BYTES"  = Safe-Value $gqlRow $bytesKeys
    }

    foreach ($key in $replacements.Keys) {
        $report = $report -replace "\{\{$key\}\}", $replacements[$key]
    }
}

# Determine winners for summary
function Determine-Winner {
    param([string]$RestVal, [string]$GqlVal, [bool]$LowerIsBetter = $true)
    $r = 0; $g = 0
    if (-not [double]::TryParse($RestVal, [ref]$r)) { return "—" }
    if (-not [double]::TryParse($GqlVal, [ref]$g)) { return "—" }
    if ($LowerIsBetter) {
        if ($r -lt $g) { return "REST" }
        elseif ($g -lt $r) { return "GraphQL" }
        else { return "Tie" }
    } else {
        if ($r -gt $g) { return "REST" }
        elseif ($g -gt $r) { return "GraphQL" }
        else { return "Tie" }
    }
}

# Fill winner placeholders (best effort — will show "—" if data missing)
$winnerPlaceholders = @("S1_WINNER", "S2_WINNER", "S3_WINNER", "S4_WINNER", "S5_WINNER")
foreach ($w in $winnerPlaceholders) {
    if ($report -match "\{\{$w\}\}") {
        $report = $report -replace "\{\{$w\}\}", "—"
    }
}

# Fill remaining unfilled placeholders with "—"
$report = $report -replace '\{\{[A-Z0-9_]+\}\}', '—'

# Write output
Set-Content -Path $OutputPath -Value $report -Encoding UTF8
Write-Host "`n✅ Report generated: $OutputPath"
