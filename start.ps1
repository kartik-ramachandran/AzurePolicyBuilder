# APIM Policy Studio - Quick Start Script
# This script helps you get started quickly

function Find-FreePort([int]$StartPort) {
    $port = $StartPort
    while ($port -lt 65535) {
        try {
            $l = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, $port)
            $l.Start()
            $l.Stop()
            return $port
        } catch {
            $port++
        }
    }
    throw "No free port found starting from $StartPort"
}

$backendPort = Find-FreePort 5000
$frontendPort = Find-FreePort 5173

Write-Host "🚀 APIM Policy Studio - Quick Start" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan
Write-Host ""

# Check prerequisites
Write-Host "📋 Checking prerequisites..." -ForegroundColor Yellow

# Check Node.js
try {
    $nodeVersion = node --version
    Write-Host "✅ Node.js installed: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Node.js not found. Please install from https://nodejs.org/" -ForegroundColor Red
    exit 1
}

# Check .NET
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK installed: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET SDK not found. Please install from https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Ask what to do
Write-Host "What would you like to do?" -ForegroundColor Yellow
Write-Host "1. Full Setup (Install dependencies and run both frontend and backend)"
Write-Host "2. Install Dependencies Only"
Write-Host "3. Run Frontend Only"
Write-Host "4. Run Backend Only"
Write-Host "5. Build for Production"
Write-Host "6. Exit"
Write-Host ""

$choice = Read-Host "Enter your choice (1-6)"

switch ($choice) {
    "1" {
        Write-Host ""
        Write-Host "🔧 Installing frontend dependencies..." -ForegroundColor Yellow
        Set-Location frontend
        npm install
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Frontend installation failed" -ForegroundColor Red
            exit 1
        }
        Write-Host "✅ Frontend dependencies installed" -ForegroundColor Green
        
        Set-Location ..
        
        Write-Host ""
        Write-Host "🔧 Restoring backend dependencies..." -ForegroundColor Yellow
        Set-Location backend
        dotnet restore
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Backend restore failed" -ForegroundColor Red
            exit 1
        }
        Write-Host "✅ Backend dependencies restored" -ForegroundColor Green
        
        Set-Location ..
        
        Write-Host ""
        Write-Host "🎉 Setup complete!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Starting applications..." -ForegroundColor Yellow
        Write-Host ""
        
        # Start backend in new window
        Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD\backend'; Write-Host '🔵 Starting Backend API on http://localhost:$backendPort' -ForegroundColor Blue; dotnet run --project ApimPolicyStudio.Api --urls 'http://localhost:$backendPort'"

        Start-Sleep -Seconds 2

        # Start frontend in new window
        Start-Process powershell -ArgumentList "-NoExit", "-Command", "`$env:BACKEND_URL='http://localhost:$backendPort'; cd '$PWD\frontend'; Write-Host '🟢 Starting Frontend on http://localhost:$frontendPort' -ForegroundColor Green; npm run dev -- --port $frontendPort"

        Write-Host ""
        Write-Host "✨ Applications starting in separate windows..." -ForegroundColor Cyan
        Write-Host ""
        Write-Host "📱 Frontend: http://localhost:$frontendPort" -ForegroundColor Green
        Write-Host "🔌 Backend API: http://localhost:$backendPort" -ForegroundColor Blue
        Write-Host "📚 Swagger UI: http://localhost:$backendPort/swagger" -ForegroundColor Magenta
        Write-Host ""
        Write-Host "Press any key to exit this window (apps will keep running)..." -ForegroundColor Yellow
        $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    }
    
    "2" {
        Write-Host ""
        Write-Host "🔧 Installing dependencies..." -ForegroundColor Yellow
        
        Write-Host "Installing frontend dependencies..." -ForegroundColor Yellow
        Set-Location frontend
        npm install
        Write-Host "✅ Frontend dependencies installed" -ForegroundColor Green
        
        Set-Location ..
        
        Write-Host "Restoring backend dependencies..." -ForegroundColor Yellow
        Set-Location backend
        dotnet restore
        Write-Host "✅ Backend dependencies restored" -ForegroundColor Green
        
        Set-Location ..
        
        Write-Host ""
        Write-Host "🎉 All dependencies installed!" -ForegroundColor Green
        Write-Host "Run './start.ps1' again and select option 1 to start the applications." -ForegroundColor Cyan
    }
    
    "3" {
        Write-Host ""
        Write-Host "🟢 Starting Frontend on http://localhost:$frontendPort..." -ForegroundColor Green
        Set-Location frontend
        npm run dev -- --port $frontendPort
    }

    "4" {
        Write-Host ""
        Write-Host "🔵 Starting Backend API on http://localhost:$backendPort..." -ForegroundColor Blue
        Set-Location backend
        dotnet run --project ApimPolicyStudio.Api --urls "http://localhost:$backendPort"
    }
    
    "5" {
        Write-Host ""
        Write-Host "🏗️ Building for production..." -ForegroundColor Yellow
        
        Write-Host "Building frontend..." -ForegroundColor Yellow
        Set-Location frontend
        npm run build
        Write-Host "✅ Frontend built to: frontend/dist/" -ForegroundColor Green
        
        Set-Location ..
        
        Write-Host "Building backend..." -ForegroundColor Yellow
        Set-Location backend
        dotnet publish -c Release -o publish
        Write-Host "✅ Backend published to: backend/publish/" -ForegroundColor Green
        
        Set-Location ..
        
        Write-Host ""
        Write-Host "🎉 Production build complete!" -ForegroundColor Green
    }
    
    "6" {
        Write-Host "👋 Goodbye!" -ForegroundColor Cyan
        exit 0
    }
    
    default {
        Write-Host "❌ Invalid choice. Please run the script again." -ForegroundColor Red
        exit 1
    }
}
