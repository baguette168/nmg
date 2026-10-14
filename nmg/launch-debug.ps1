$env:WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS = "--remote-debugging-port=9222"
$exe = Join-Path $PSScriptRoot "src\Client\bin\Debug\net8.0-windows\NMG.Client.exe"
Start-Process $exe
