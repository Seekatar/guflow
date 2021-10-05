param( 
    [string] $checkoutPath = $Env:APPVEYOR_BUILD_FOLDER,
    [string] $version = $Env:APPVEYOR_BUILD_VERSION
)

$nuspecFilePath = Join-Path $checkoutPath -ChildPath "Guflow\Guflow.nuspec"
$packageFilePath = Join-Path $checkoutPath -ChildPath "Guflow\packages.config"

$nuspecContent = [xml](Get-Content $nuspecFilePath )
$packageContent = [xml](Get-Content $packageFilePath)

$nuspecNamespace = $nuspecContent.package.xmlns
#update version
$nuspecContent.package.metadata.version = $version

#update copyright
$nuspecContent.package.metadata.copyright = "Copyright $([char]0x00A9) $((Get-Date).year) - Gurmit Teotia"

#Update dependency in nuspec file
foreach($package in $packageContent.packages.package)
{
    $dependency = $nuspecContent.CreateElement("dependency", $nuspecNamespace)
    $dependency.SetAttribute("id",$package.id)
    $dependency.SetAttribute("version", $package.version)
    $nuspecContent.package.metadata.dependencies.AppendChild($dependency)
}

$nuspecContent.Save($nuspecFilePath)