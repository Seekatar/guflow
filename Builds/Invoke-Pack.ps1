param(
    [Parameter(Mandatory)]
    [ValidateScript({Test-Path $_ -PathType Container})]
    [string] $OutputFolder
)

Push-Location (Join-Path $PSScriptRoot "../Guflow")
try {
    dotnet pack -c Release -o $outputFolder --include-source
} finally {
    Pop-Location
}
