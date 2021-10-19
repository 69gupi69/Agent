[CmdletBinding()]
Param
(
    [parameter(Mandatory=$true,Position = 0,HelpMessage="Enter path to project file")]
	[string] $ProjectPath
)
Write-Host "Running NuGet deploy script"

$ProjectDir = (Split-Path -Path $ProjectPath -Parent) + "\"
$ProjectName = Split-Path -Path $ProjectPath -Leaf

$indexOfExtension = $ProjectName.LastIndexOf(".csproj")

If ($indexOfExtension -eq -1)
{
    Write-Host "Not finded C# Project File"
    return
}

Get-ChildItem -Path $ProjectDir -Include "*.nupkg" -Recurse | ForEach-Object {Remove-Item $_ | out-null}

$ProjectName = $ProjectName.Remove($indexOfExtension)

#path to .nuspec file
$nuspec = $ProjectDir + $ProjectName + ".nuspec"

#path to options xml
$SettingsFile = $ProjectDir + "PackageOptions.xml"

#get specification
cd $ProjectDir
$nugetPath = $ProjectDir + "nuget.exe"
& $nugetPath @('spec') @('-Force')

#transform specification
$nuspecXml = [xml](Get-Content "$nuspec")

$xmlRoot = $nuspecXml.DocumentElement

#добавление секции загрузки контента
$ContentPath = $ProjectDir + "Content"

If (Test-Path -Path $ContentPath)
{
    $filesNode = $nuspecXml.CreateElement('files')
    $xmlRoot.AppendChild($filesNode)
    $filesNode = $xmlRoot.SelectSingleNode("//files")
    $fileNode = $nuspecXml.CreateElement('file')
    $fileNode.SetAttribute('src','Content\**\*.*')
    $fileNode.SetAttribute('target','Content')
    $filesNode.AppendChild($fileNode)
}

$node = $xmlRoot.SelectSingleNode("//metadata/authors")
$node.InnerText = "ORIS"

$node = $xmlRoot.SelectSingleNode("//metadata/projectUrl")
$node.InnerText = "http://tfs.ctd.tn.corp:8080/tfs/DevCollection/DiCore"

$node = $xmlRoot.SelectSingleNode("//metadata/tags")
$node.InnerText = $ProjectName

$parentNode = $xmlRoot.SelectSingleNode("//metadata")

$node = $xmlRoot.SelectSingleNode("//metadata/releaseNotes")
$parentNode.RemoveChild($node)

#Exist options file
If (Test-Path -Path $SettingsFile)
{
    $setXmlFile = [xml](Get-Content "$SettingsFile")
    
    $setXmlFileRoot = $setXmlFile.DocumentElement

    
    $setNode = $setXmlFileRoot.SelectSingleNode("//authors")
    IF ($setNode -ne $null)
    {
        $node = $xmlRoot.SelectSingleNode("//metadata/authors")
        $node.InnerText = $setNode.InnerText
    }

    $setNode = $setXmlFileRoot.SelectSingleNode("//projectUrl")
    IF ($setNode -ne $null)
    {
        $node = $xmlRoot.SelectSingleNode("//metadata/projectUrl")
        $node.InnerText = $setNode.InnerText
    }

    $setNode = $setXmlFileRoot.SelectSingleNode("//releaseNotes")
    IF ($setNode -ne $null)
    {
        $node = $xmlRoot.SelectSingleNode("//metadata/releaseNotes")
        $node.InnerText = $setNode.InnerText
    }

    
    $setNode = $setXmlFileRoot.SelectSingleNode("//tags")
    IF ($setNode -ne $null)
    {
        $node = $xmlRoot.SelectSingleNode("//metadata/tags")
        $node.InnerText = $setNode.InnerText
    }

    #добавление dll
    
    $setNode = $setXmlFileRoot.SelectSingleNode("//dllReference")

    If ($setNode -ne $null)
    {
        If($setNode.InnerText.ToUpper().StartsWith("t".ToUpper()))
        {
            $filesNode = $xmlRoot.SelectSingleNode("//files")
            If($filesNode -eq $null)
            {
                $filesNode = $nuspecXml.CreateElement('files')
                $xmlRoot.AppendChild($filesNode)
            }
       
            $fileNode = $nuspecXml.CreateElement('file')
            $fileNode.SetAttribute('src','bin\NugetPublish\*.dll')
            $fileNode.SetAttribute('target','lib')
            $filesNode.AppendChild($fileNode)
        }
    } 

    #добавить pdb

    $setNode = $setXmlFileRoot.SelectSingleNode("//pdb")

    If ($setNode -ne $null)
    {
        If($setNode.InnerText.ToUpper().StartsWith("t".ToUpper()))
        {
            $filesNode = $xmlRoot.SelectSingleNode("//files")
            If($filesNode -eq $null)
            {
                $filesNode = $nuspecXml.CreateElement('files')
                $xmlRoot.AppendChild($filesNode)
            }
       
            $fileNode = $nuspecXml.CreateElement('file')
            $fileNode.SetAttribute('src','bin\NugetPublish\*.pdb')
            $fileNode.SetAttribute('target','lib')
            $filesNode.AppendChild($fileNode)
        }
    } 

    #добавление scripts

    $setNode = $setXmlFileRoot.SelectSingleNode("//scripts")

    If ($setNode -ne $null)
    {
        If($setNode.InnerText.ToUpper().StartsWith("t".ToUpper()))
        {
                
            $filesNode = $xmlRoot.SelectSingleNode("//files")
            If($filesNode -eq $null)
            {
                $filesNode = $nuspecXml.CreateElement('files')
                $xmlRoot.AppendChild($filesNode)
            }

            $fileNode = $nuspecXml.CreateElement('file')
            $fileNode.SetAttribute('src','Scripts\dicore\**\*.*')
            $fileNode.SetAttribute('target','Scripts\dicore')
            $filesNode.AppendChild($fileNode)
        }
    }

    #добавление css
    $setNode = $setXmlFileRoot.SelectSingleNode("//css")

    If ($setNode -ne $null)
    {
        If($setNode.InnerText.ToUpper().StartsWith("t".ToUpper()))
        {
                
            $filesNode = $xmlRoot.SelectSingleNode("//files")
            If($filesNode -eq $null)
            {
                $filesNode = $nuspecXml.CreateElement('files')
                $xmlRoot.AppendChild($filesNode)
            }

            $fileNode = $nuspecXml.CreateElement('file')
            $fileNode.SetAttribute('src','css\dicore\**\*.*')
            $fileNode.SetAttribute('target','css\dicore')
            $filesNode.AppendChild($fileNode)
        }
    }

}

$node = $xmlRoot.SelectSingleNode("//metadata/owners")
$node.InnerText = "Transneft-Diascan"

#delete attributes
$parentNode = $xmlRoot.SelectSingleNode("//metadata")
$node = $xmlRoot.SelectSingleNode("//metadata/licenseUrl")
$parentNode.RemoveChild($node)
$node = $xmlRoot.SelectSingleNode("//metadata/iconUrl")
$parentNode.RemoveChild($node)

$nuspecXml.Save($nuspec)

#compile package
& $nugetPath @('pack') @('-IncludeReferencedProjects') @('-Prop') @('Configuration=NugetPublish')

#publish package

$SearchPath = (Split-Path -Path $ProjectDir -Parent) + "\"

Get-ChildItem -Path $SearchPath -Include "*.nupkg" -Recurse | ForEach-Object {& $nugetPath @('push') @($_.FullName) @('d7c0933a-0606-46f9-bb69-53e65d2fbd0f') @('-source') @('http://nuget.ctd.tn.corp/') @('-Verbosity') @('detailed')}

Write-Host "NuGet deploy script Finished"

