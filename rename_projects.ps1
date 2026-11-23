# Renomme les projets du puissance 4.
# A lancer depuis la racine Godot_AI_Project, Rider et Godot FERMES :
#   powershell -ExecutionPolicy Bypass -File .\rename_projects.ps1

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

function Write-Utf8($path, $content) {
    [System.IO.File]::WriteAllText((Join-Path $PSScriptRoot $path), $content, (New-Object System.Text.UTF8Encoding $false))
}
function Remove-BuildDirs($project) {
    foreach ($d in 'bin', 'obj') {
        if (Test-Path "$project\$d") { Remove-Item -Recurse -Force "$project\$d" }
    }
}

# ---------------- Solution_Core : la librairie de logique ----------------
$old = 'Solution_Core\Godot_Project'
Move-Item "$old\Godot_Project_Library" 'Solution_Core\Connect4.Core'
Move-Item "$old\global.json"           'Solution_Core\global.json'
Remove-Item -Recurse -Force $old   # ancien exe vide (Program.cs), .idea, ancien .slnx

Remove-Item 'Solution_Core\Connect4.Core\Godot_Project_Library.csproj', 'Solution_Core\Connect4.Core\Class1.cs'
Remove-BuildDirs 'Solution_Core\Connect4.Core'

Write-Utf8 'Solution_Core\Connect4.Core\Connect4.Core.csproj' @'
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <RootNamespace>Connect4.Core</RootNamespace>
    </PropertyGroup>

</Project>
'@

Write-Utf8 'Solution_Core\Connect4.Core.slnx' @'
<Solution>
  <Project Path="Connect4.Core/Connect4.Core.csproj" />
</Solution>
'@

# ---------------- Console_Runner : l'appli console ----------------
$old = 'Console_Runner\Godot_Console_Runner'
Move-Item "$old\Godot_Console_Runner" 'Console_Runner\Connect4.Console'
Move-Item "$old\global.json"          'Console_Runner\global.json'
Remove-Item -Recurse -Force $old   # .idea, ancien .slnx

Remove-Item 'Console_Runner\Connect4.Console\Godot_Console_Runner.csproj', 'Console_Runner\Connect4.Console\Class1.cs'
Remove-BuildDirs 'Console_Runner\Connect4.Console'

Write-Utf8 'Console_Runner\Connect4.Console\Connect4.Console.csproj' @'
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net8.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <RootNamespace>Connect4.ConsoleApp</RootNamespace>
    </PropertyGroup>

    <ItemGroup>
        <ProjectReference Include="..\..\Solution_Core\Connect4.Core\Connect4.Core.csproj" />
    </ItemGroup>

</Project>
'@

Write-Utf8 'Console_Runner\Connect4.Console\Program.cs' @'
Console.WriteLine("Puissance 4 - console");
'@

Write-Utf8 'Console_Runner\Connect4.Console.slnx' @'
<Solution>
  <Project Path="Connect4.Console/Connect4.Console.csproj" />
  <Project Path="../Solution_Core/Connect4.Core/Connect4.Core.csproj" />
</Solution>
'@

Write-Host "OK. Test : dotnet run --project Console_Runner\Connect4.Console" -ForegroundColor Green
