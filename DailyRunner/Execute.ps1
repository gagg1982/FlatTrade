
 Push-Location

    # Change the current directory to the script's directory
    Set-Location -Path $PSScriptRoot

    $credential = Get-Credential 

#Evening Schedule....
.\ScheduleDailyRunner.ps1 -ExecutablePath "D:\TradingApplication\DailyRunner\bin\Release\net8.0\DailyRunner.exe" -Arguments "D:\TradingApplication\DailyRunner\Configs\01-Exchanges.json"  -TaskName "01-Exchanges_Evening"  -ExecutionTime "07:00 PM" -Credentials $credential
.\ScheduleDailyRunner.ps1 -ExecutablePath "D:\TradingApplication\DailyRunner\bin\Release\net8.0\DailyRunner.exe" -Arguments "D:\TradingApplication\DailyRunner\Configs\02-StockInstruments.json" -TaskName "02-StockInstruments_Evening" -ExecutionTime "07:20 PM"  -Credentials $credential
.\ScheduleDailyRunner.ps1 -ExecutablePath "D:\TradingApplication\DailyRunner\bin\Release\net8.0\DailyRunner.exe" -Arguments "D:\TradingApplication\DailyRunner\Configs\03a-BSE_OHLCV_1.json" -TaskName "03a-BSE_OHLCV_1" -ExecutionTime "09:45 PM" -Credentials $credential
.\ScheduleDailyRunner.ps1 -ExecutablePath "D:\TradingApplication\DailyRunner\bin\Release\net8.0\DailyRunner.exe" -Arguments "D:\TradingApplication\DailyRunner\Configs\03b-NSE_OHLCV_1.json" -TaskName "03b-NSE_OHLCV_1" -ExecutionTime "09:48 PM" -Credentials $credential

#Morning Schedule....
.\ScheduleDailyRunner.ps1 -ExecutablePath "D:\TradingApplication\DailyRunner\bin\Release\net8.0\DailyRunner.exe" -Arguments "D:\TradingApplication\DailyRunner\Configs\01-Exchanges.json" -TaskName "01-Exchanges_Morning" -ExecutionTime "02:00 AM" -Credentials $credential
.\ScheduleDailyRunner.ps1 -ExecutablePath "D:\TradingApplication\DailyRunner\bin\Release\net8.0\DailyRunner.exe" -Arguments "D:\TradingApplication\DailyRunner\Configs\02-StockInstruments.json" -TaskName "02-StockInstruments_Morning" -ExecutionTime "02:05 AM" -Credentials $credential
.\ScheduleDailyRunner.ps1 -ExecutablePath "D:\TradingApplication\DailyRunner\bin\Release\net8.0\DailyRunner.exe" -Arguments "D:\TradingApplication\DailyRunner\Configs\03a-BSE_OHLCV_1440.json" -TaskName "03a-BSE_OHLCV_1440" -ExecutionTime "03:30 AM" -Credentials $credential
.\ScheduleDailyRunner.ps1 -ExecutablePath "D:\TradingApplication\DailyRunner\bin\Release\net8.0\DailyRunner.exe" -Arguments "D:\TradingApplication\DailyRunner\Configs\03b-NSE_OHLCV_1440.json" -TaskName "03b-NSE_OHLCV_1440" -ExecutionTime "04:00 AM" -Credentials $credential

Pop-Location