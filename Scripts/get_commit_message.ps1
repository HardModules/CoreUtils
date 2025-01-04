$SCRIPT_VERSION = "1.0.0"
$API_BASE_URL = "https://generativelanguage.googleapis.com/v1beta/models"
$MODEL = "gemini-2.0-flash-exp"

$MAX_RETRIES = 3
$TIMEOUT_SECONDS = 30
$MAX_DIFF_LENGTH = 30000

$LOG_FILE = "commit_message.log"

$logBuffer = New-Object System.Text.StringBuilder

function Write-Log {
    param (
        [string]$message,
        [switch]$Console
    )
    $timestamp = Get-Date -Format "HH:mm:ss"
    $logMessage = "${timestamp}: ${message}"
    $logBuffer.AppendLine($logMessage) | Out-Null
    if ($Console) {
        Write-Host $message
    }
}

function Invoke-ApiRequestWithRetries {
    param (
        [System.Net.Http.HttpContent]$content,
        [int]$maxRetries = $MAX_RETRIES,
        [int]$timeoutSeconds = $TIMEOUT_SECONDS
    )

    $apiKey = [System.Environment]::GetEnvironmentVariable("COMMIT_API_KEY")
    $apiUrl = "${API_BASE_URL}/${MODEL}:generateContent?key=$apiKey"

    $httpClient = New-Object System.Net.Http.HttpClient
    $httpClient.Timeout = [TimeSpan]::FromSeconds($timeoutSeconds)

    for ($attempt = 1; $attempt -le $maxRetries; $attempt++) {
        Write-Log "Attempt $attempt of $maxRetries"
        Write-Log "Sending request..." -Console
        try {
            $task = $httpClient.PostAsync($apiUrl, $content)
            $response = $task.GetAwaiter().GetResult()

            if ($response.IsSuccessStatusCode) {
                Write-Log "Successful response: $($response.StatusCode)"
                return $response
            }
            Write-Log "Request error: $($response.StatusCode)"
            $errorContent = $response.Content.ReadAsStringAsync().Result
            Write-Log "Error content: $errorContent"
        }
        catch {
            Write-Log "An error occurred during the request: $_"
            if ($_.Exception.InnerException) {
                Write-Log "Inner exception: $($_.Exception.InnerException)"
            }
            Write-Log "Stack trace: $($_.ScriptStackTrace)"
        }
        if ($attempt -lt $maxRetries) {
            Write-Log "Waiting before the next attempt..."
            Start-Sleep -Seconds 1
        }
    }
    throw "All $maxRetries API request attempts failed"
}

function Get-GitDiff {
    Write-Log "Getting git diff HEAD"
    Write-Log "Retrieving changes..." -Console
    $diff = git diff HEAD
    if ($diff.Length -eq 0) {
        throw "git diff HEAD returned no changes"
    }
    if ($diff.Length -gt $MAX_DIFF_LENGTH) {
        Write-Log "Limiting diff to $MAX_DIFF_LENGTH characters"
        $diff = $diff.Substring(0, $MAX_DIFF_LENGTH)
    }
    return $diff
}

function Get-UserPrompt {
    Write-Host "Enter additional information for the commit (optional, press Enter to skip):"
    $userPrompt = Read-Host
    return $userPrompt
}

function Get-CommitMessage {
    $apiKey = [System.Environment]::GetEnvironmentVariable("COMMIT_API_KEY")
    if (-not $apiKey) {
        throw "Environment variable COMMIT_API_KEY is not set. Example: AIzaSyD..."
    }

    $diff = Get-GitDiff
    $userPrompt = Get-UserPrompt

    $systemPrompt = @"
Create a concise and informative commit message following the Conventional Commits specification, but in English.

Input: You will receive the output of 'git diff HEAD' and optional user input.
Task: Analyze the diff and create a commit message that neutrally describes the changes.

Requirements:
- Use the format: <type>[optional scope]: <description>
- Each line should be no longer than 75 characters.
- Provide a brief description in the first line.
- Use passive voice or neutral statements (e.g., "file added", "function updated").
- Be specific but concise.
- Do not use codeblocks (```) or other formatting.
- If user input is provided, it should be the main focus of the commit message.
- Emphasize the user's reason for changes and describe how the code changes support this reason.

Example format:
feature(authentication): password reset functionality implemented
- Added API endpoint for password reset according to security requirements
- Created email template with reset instructions to improve UX
- Updated user model with reset token field for secure storage

User input (if provided): $userPrompt

Important: If user input is provided, make it the central theme of the commit message,
explaining how the code changes relate to and support the user's stated reason or goal.
"@

    $requestBody = @{
        contents = @(
            @{
                parts = @(
                    @{
                        text = "$systemPrompt`n`n$diff"
                    }
                )
            }
        )
    }

    $jsonBody = $requestBody | ConvertTo-Json -Depth 10
    $content = New-Object System.Net.Http.StringContent($jsonBody, [System.Text.Encoding]::UTF8, "application/json")

    Write-Log "Sending request to API"
    $response = Invoke-ApiRequestWithRetries -content $content

    Write-Log "Reading stream response"
    $responseStream = $response.Content.ReadAsStreamAsync().Result
    $reader = [System.IO.StreamReader]::new($responseStream)
    $responseBody = $reader.ReadToEnd()

    Write-Log "Full response:"
    Write-Log $responseBody

    $responseJson = $responseBody | ConvertFrom-Json
    if (-not $responseJson.candidates) {
        throw "Failed to obtain commit message"
    }

    $commitMessage = $responseJson.candidates[0].content.parts[0].text
    return $commitMessage
}

function Main {
    try {
        Add-Type -AssemblyName System.Net.Http

        $diagnosticInfo = Get-DiagnosticInfo
        Write-Log $diagnosticInfo -Console
        Write-Log $diagnosticInfo

        $commitMessage = Get-CommitMessage
        Set-Clipboard -Value $commitMessage
        Write-Log "Commit message copied to clipboard"
        Write-Log "Commit message:" -Console
        Write-Log $commitMessage -Console
    }
    catch {
        Write-Log "An error occurred: $_"
        $logBuffer.ToString() | Out-File -FilePath $LOG_FILE
        Write-Log "An error occurred. Details in file: $LOG_FILE" -Console
    }
    finally {
        Write-Host "Press any key to exit..."
        [System.Console]::ReadKey() | Out-Null
    }
}

function Get-DiagnosticInfo {
    $psVersion = $PSVersionTable.PSVersion
    $osInfo = Get-CimInstance Win32_OperatingSystem

    try {
        $gitVersion = (git --version 2>$null) -replace 'git version '
        if ([string]::IsNullOrEmpty($gitVersion)) {
            $gitInfo = "Git is not installed or not found in PATH"
        }
        else {
            $gitInfo = "Git version: $gitVersion"
        }
    }
    catch {
        $gitInfo = "Failed to get Git information: $_"
    }

    $info = @"
Diagnostic information:
- Script version: $SCRIPT_VERSION
- PowerShell version: $psVersion
- OS: $($osInfo.Caption) $($osInfo.Version)
- $gitInfo
- API Model: $MODEL
"@
    return $info
}

Main
