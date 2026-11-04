# UE 5.6 에디터 내장 브라우저(CEF = Chromium 90)용 blazor.web.js 를 만드는 스크립트
#
# 왜 필요한가:
#   .NET 10/11 의 blazor.web.js 는 Chromium 94 이상에서만 읽을 수 있는 문법(class static block)을 쓴다.
#   Chromium 90 은 이 파일을 파싱하다 SyntaxError 를 내고 통째로 버리므로, Blazor 화면이 동작하지 않는다.
#   esbuild 로 문법만 Chrome 90 수준으로 낮춘 사본을 wwwroot/compat 에 만든다.
#
# 언제 다시 돌려야 하나:
#   UnrealAgent.Frontend.csproj 의 Microsoft.AspNetCore.App.Internal.Assets 버전을 바꿨을 때 (반드시!)
#
# 실행: 저장소 루트에서  powershell -ExecutionPolicy Bypass -File tools\make-blazor-chrome90.ps1

$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
$CsprojPath = Join-Path $RepoRoot "UnrealAgent.Frontend\UnrealAgent.Frontend.csproj"

# 1. csproj 에서 blazor.web.js 를 제공하는 패키지 버전을 읽는다 (서버와 스크립트 버전이 반드시 같아야 한다)
[xml]$Csproj = Get-Content $CsprojPath -Encoding UTF8
$AssetsVersion = ($Csproj.Project.ItemGroup.PackageReference |
    Where-Object { $_.Include -eq "Microsoft.AspNetCore.App.Internal.Assets" }).Version
if (-not $AssetsVersion) { throw "csproj 에서 Microsoft.AspNetCore.App.Internal.Assets 버전을 찾지 못했습니다." }

$SourcePath = Join-Path $env:USERPROFILE ".nuget\packages\microsoft.aspnetcore.app.internal.assets\$AssetsVersion\_framework\blazor.web.js"
if (-not (Test-Path $SourcePath)) { throw "원본이 없습니다. 먼저 dotnet restore 를 실행하세요: $SourcePath" }

$OutputDir = Join-Path $RepoRoot "UnrealAgent.Frontend\wwwroot\compat"
$OutputPath = Join-Path $OutputDir "blazor.web.chrome90.js"
New-Item -ItemType Directory -Force $OutputDir | Out-Null

Write-Host "원본 패키지 버전 : $AssetsVersion"
Write-Host "원본 파일        : $SourcePath"

# 2. esbuild 로 문법을 Chrome 90 수준으로 낮춘다 (함수 추가는 하지 않는다 - 그건 App.razor 의 폴리필 담당)
npx --yes esbuild@0.25.0 $SourcePath --target=chrome90 --minify --log-level=warning --outfile=$OutputPath
if ($LASTEXITCODE -ne 0) { throw "esbuild 변환 실패" }

# 3. 결과 검사: class static block 이 하나라도 남아 있으면 실패로 본다
$Before = ([regex]::Matches((Get-Content $SourcePath -Raw), "static\{")).Count
$After = ([regex]::Matches((Get-Content $OutputPath -Raw), "static\{")).Count
Write-Host "class static block : 변환 전 $Before 개 -> 변환 후 $After 개"
if ($After -ne 0) { throw "변환 후에도 class static block 이 남아 있습니다." }

Write-Host "완료: $OutputPath"
