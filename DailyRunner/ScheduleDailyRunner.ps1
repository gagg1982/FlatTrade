<#
.SYNOPSIS
    Schedules a task to run a specified executable at a given time daily.

.DESCRIPTION
    This script creates a Windows Scheduled Task using the New-ScheduledTask cmdlets.
    The task will execute the specified program and arguments once per day at the
    time you define. It is configured to run in the background.

.PARAMETER TaskName
    A unique name for the scheduled task. Defaults to 'DailyRunnerTask'.

.PARAMETER ExecutablePath
    The full path to the executable to run (e.g., 'C:\Program Files\App\DailyRunner.exe').
    This parameter is mandatory.

.PARAMETER Arguments
    The command-line arguments to pass to the executable.
    For this request, the argument is 'ConfigFile'.

.PARAMETER ExecutionTime
    The time of day to run the task. Defaults to '11:00 AM'.
    The format should be a valid time string, e.g., 'HH:mm'.

PARAMTER Credentials
    The credentials to run the task with. If not provided, the task will run under the current user context.
    
.EXAMPLE
    .\ScheduleDailyRunner.ps1 -ExecutablePath "C:\Tools\DailyRunner.exe" -Arguments "C:\MyConfigs\config.json" -ExecutionTime "02:30 PM" -credentials $credentials
    This will schedule "C:\Tools\DailyRunner.exe C:\MyConfigs\config.json" to run daily at 2:30 PM.

.EXAMPLE
    .\ScheduleDailyRunner.ps1 -ExecutablePath "C:\Tools\DailyRunner.exe"
    This will schedule "C:\Tools\DailyRunner.exe ConfigFile" to run daily at 11:00 AM using default values.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$TaskName = "DailyRunnerTask",

    [Parameter(Mandatory=$true)]
    [string]$ExecutablePath,

    [Parameter(Mandatory=$false)]
    [string]$Arguments = "ConfigFile",

    [Parameter(Mandatory=$false)]
    [string]$ExecutionTime = "11:00 AM",

    [Parameter(Mandatory=$true)]
    [string]$Credentials = 
)

# --- Script Logic ---

Write-Host "Creating a scheduled task named '$TaskName'..." -ForegroundColor Cyan

# 1. Define what the task will do (the Action).
# The -Argument parameter takes the arguments for the executable.
$action = New-ScheduledTaskAction -Execute $ExecutablePath -Argument $Arguments

# 2. Define when the task will run (the Trigger).
# This trigger runs the task daily at the specified time.
$time = [datetime]$ExecutionTime
$trigger = New-ScheduledTaskTrigger -Daily -At $time

# 3. Define the settings for the task.
# - The task should run even if the user is not logged in.
# - The task will run with the highest privileges.
# - It will start the task as soon as possible if the scheduled time is missed.
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries:$true -StartWhenAvailable:$true -WakeToRun:$true -DontStopIfGoingOnBatteries:$true -MultipleInstances:Parallel 
$principal = New-ScheduledTaskPrincipal -UserId "lab1\Administrator" -LogonType "gagg-1234!" -RunLevel Highest

# Check if the task already exists. If so, update it. Otherwise, create a new one.
try {
    Write-Host "Checking for existing task '$TaskName'..."
    $existingTask = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue

    if ($null -ne $existingTask) {
        Write-Host "Task '$TaskName' already exists. Updating it."
        Set-ScheduledTask -Principal $principal -TaskName $TaskName -Action $action -Trigger $trigger -Settings $settings
        Write-Host "Task '$TaskName' updated successfully." -ForegroundColor Green
    } else {
        Write-Host "Task '$TaskName' not found. Creating a new task."
        # 4. Register the new scheduled task.
        Register-ScheduledTask -TaskName -Principal $principal $TaskName -Action $action -Trigger $trigger -Settings $settings
        Write-Host "Task '$TaskName' created successfully." -ForegroundColor Green
    }
}
catch {
    Write-Host "An error occurred while creating/updating the task: $_" -ForegroundColor Red
}

# Optional: Display the details of the newly created/updated task.
Write-Host ""
Write-Host "Task Details:"
Get-ScheduledTask -TaskName $TaskName | Select-Object TaskName, State, @{N='Actions'; E={$_.Actions.Execute}}, @{N='Triggers'; E={$_.Triggers.NextExecutionTime}}
