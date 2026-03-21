param(
    [string]$ProjectRoot = "."
)

$targetRoots = @(
    "Assets/Art/Characters",
    "Assets/Art/Environment",
    "Assets/Art/Sprites",
    "Assets/Art/UI"
)

$skipPatterns = @(
    "\\Examples\\",
    "\\Preview",
    "Assets List",
    "preview_",
    "preview.",
    "tiny_animalsanctuary",
    "tiny_fishing",
    "tiny_rpg",
    "tiny_whowouldwin"
)

function Should-SkipMeta([string]$path) {
    foreach ($pattern in $skipPatterns) {
        if ($path -match $pattern) {
            return $true
        }
    }
    return $false
}

function Get-SpriteCount([string]$content) {
    $match = [regex]::Match($content, 'nameFileIdTable:\s*(?<body>[\s\S]*?)\n\s*mipmapLimitGroupName:', [System.Text.RegularExpressions.RegexOptions]::Singleline)
    if (-not $match.Success) {
        return 0
    }

    $body = $match.Groups['body'].Value
    return ([regex]::Matches($body, '^\s{6,}[^:\r\n]+:', [System.Text.RegularExpressions.RegexOptions]::Multiline)).Count
}

function Set-ScalarValue([string]$content, [string]$name, [string]$value) {
    $escapedName = [regex]::Escape($name)
    return [regex]::Replace(
        $content,
        "(?m)^(\s*${escapedName}:\s*).*$",
        {
            param($match)
            return $match.Groups[1].Value + $value
        }
    )
}

function Repair-CorruptedScalarLines([string]$content) {
    $content = $content.Replace(
@'
    serializedVersion: 2
$10
    aniso: 1
'@,
@'
    serializedVersion: 2
    filterMode: 0
    aniso: 1
'@
    )

    $content = $content.Replace(
@'
  lightmap: 0
$10
$11
  spriteExtrude: 1
'@,
@'
  lightmap: 0
  compressionQuality: 0
  spriteMode: 1
  spriteExtrude: 1
'@
    )

    $content = $content.Replace(
@'
  lightmap: 0
$10
$12
  spriteExtrude: 1
'@,
@'
  lightmap: 0
  compressionQuality: 0
  spriteMode: 2
  spriteExtrude: 1
'@
    )

    $content = $content.Replace(
@'
  spriteTessellationDetail: -1
$18
  textureShape: 1
'@,
@'
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
'@
    )

    $content = $content.Replace(
@'
    textureFormat: -1
$10
$10
    crunchedCompression: 0
'@,
@'
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 0
    crunchedCompression: 0
'@
    )

    return $content
}

$resolvedRoot = Resolve-Path $ProjectRoot
$updated = New-Object System.Collections.Generic.List[string]

foreach ($root in $targetRoots) {
    $fullRoot = Join-Path $resolvedRoot $root
    if (-not (Test-Path $fullRoot)) {
        continue
    }

    Get-ChildItem -Path $fullRoot -Recurse -Filter "*.png.meta" -File | ForEach-Object {
        $metaPath = $_.FullName
        $normalizedMetaPath = $metaPath.Replace('/', '\')
        if (Should-SkipMeta $normalizedMetaPath) {
            return
        }

        $content = Get-Content $metaPath -Raw
        $original = $content
        $content = Repair-CorruptedScalarLines $content

        $spriteCount = Get-SpriteCount $content
        $spriteMode = if ($spriteCount -gt 1) { "2" } else { "1" }

        $content = Set-ScalarValue $content "filterMode" "0"
        $content = Set-ScalarValue $content "spriteMode" $spriteMode
        $content = Set-ScalarValue $content "textureCompression" "0"
        $content = Set-ScalarValue $content "compressionQuality" "0"
        $content = Set-ScalarValue $content "textureType" "8"

        if ($content -ne $original) {
            [System.IO.File]::WriteAllText($metaPath, $content, [System.Text.UTF8Encoding]::new($false))
            $updated.Add($metaPath.Replace($resolvedRoot.Path + "\", ""))
        }
    }
}

Write-Host ("Updated {0} image meta files." -f $updated.Count)
$updated | Sort-Object | ForEach-Object { Write-Host $_ }
