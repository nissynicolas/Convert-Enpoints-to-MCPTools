@echo off
echo 🚀 Testing OpenAPI to MCP Generator Web UI
echo.

echo 📋 Checking prerequisites...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ .NET SDK not found. Please install .NET 9.0 SDK or later.
    pause
    exit /b 1
)

echo ✅ .NET SDK found
echo.

echo 🔧 Building the solution...
dotnet build
if %errorlevel% neq 0 (
    echo ❌ Build failed. Please check the error messages above.
    pause
    exit /b 1
)

echo ✅ Build successful
echo.

echo 🌐 Starting the web application...
echo.
echo 📱 The application will be available at:
echo    https://localhost:5001 (or next available port)
echo.
echo 🎯 Demo features to test:
echo    • Load Petstore API example
echo    • Generate MCP server
echo    • Download generated project
echo.
echo Press Ctrl+C to stop the server
echo.

cd WebUI
dotnet run

