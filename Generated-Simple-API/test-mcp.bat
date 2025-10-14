@echo off
echo Testing SimpleApiMcpServer MCP Server
echo.

echo Testing tools/list...
echo {"jsonrpc": "2.0", "id": 1, "method": "tools/list"} | dotnet run

echo.
echo Testing tools/call (first tool)...
echo {"jsonrpc": "2.0", "id": 2, "method": "tools/call", "params": {"name": "GetUsers", "arguments": {}}} | dotnet run

echo.
echo Test completed.
pause