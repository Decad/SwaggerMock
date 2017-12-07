# Taken from MediatR https://github.com/jbogard/MediatR

<#
.SYNOPSIS
  This is a helper function that runs a scriptblock and checks the PS variable $lastexitcode
  to see if an error occcured. If an error is detected then an exception is thrown.
  This function allows you to run command-line programs without having to
  explicitly check the $lastexitcode variable.
.EXAMPLE
  exec { svn info $repository_trunk } "Error executing SVN. Please verify SVN command-line client is installed"
#>
function Exec
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
        [Parameter(Position=1,Mandatory=0)][string]$errorMessage = ($msgs.error_bad_command -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        throw ("Exec: " + $errorMessage)
    }
}

if(Test-Path .\src\SwaggerMock\artifacts) { Remove-Item .\src\SwaggerMock\artifacts -Force -Recurse }

exec { & dotnet restore }

exec { & dotnet build SwaggerMock.sln -c Release -v q /nologo }

Push-Location -Path .\test\SwaggerMock.Tests

exec { & dotnet test -c Release }

Pop-Location

exec { & dotnet pack .\src\SwaggerMock\SwaggerMock.csproj -c Release -o .\artifacts --include-symbols --no-build }
