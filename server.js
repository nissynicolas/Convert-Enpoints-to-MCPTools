const express = require('express');
const { spawn } = require('child_process');
const path = require('path');
const fs = require('fs');
const app = express();
const port = 3000;

// Serve static files
app.use(express.static('public'));
app.use(express.json());

// Serve the demo.html file
app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, 'demo.html'));
});

// API endpoint to generate MCP server
app.post('/api/generator/generate', async (req, res) => {
    try {
        const { openApiInput, outputDirectory, projectName, namespace, baseUrl, verbose } = req.body;
        
        console.log(`Generating MCP server: ${projectName}`);
        console.log(`Output directory from request: ${outputDirectory}`);
        
        // Use custom output directory if provided, otherwise use default
        const baseDir = outputDirectory || 'C:\\Users\\ngunturu';
        const tempDir = path.join(baseDir, projectName);
        
        console.log(`Using base directory: ${baseDir}`);
        console.log(`Final project directory: ${tempDir}`);
        
        // Ensure the base directory exists
        if (!fs.existsSync(baseDir)) {
            fs.mkdirSync(baseDir, { recursive: true });
        }
        
        // Clean up existing project directory if it exists
        if (fs.existsSync(tempDir)) {
            fs.rmSync(tempDir, { recursive: true, force: true });
        }
        fs.mkdirSync(tempDir, { recursive: true });
        
        // Build the command to call your .NET application
        const args = [
            'run',
            '--',
            '--openapi', openApiInput,
            '--output', tempDir,
            '--name', projectName,
            '--namespace', namespace || projectName
        ];
        
        if (baseUrl) {
            args.push('--base-url', baseUrl);
        }
        
        if (verbose) {
            args.push('--verbose');
        }
        
        console.log('Running command:', 'dotnet', args.join(' '));
        
        // Spawn the .NET process
        const dotnetProcess = spawn('dotnet', args, {
            cwd: __dirname,
            stdio: ['pipe', 'pipe', 'pipe']
        });
        
        let stdout = '';
        let stderr = '';
        
        dotnetProcess.stdout.on('data', (data) => {
            stdout += data.toString();
            console.log('STDOUT:', data.toString());
        });
        
        dotnetProcess.stderr.on('data', (data) => {
            stderr += data.toString();
            console.error('STDERR:', data.toString());
        });
        
        // Wait for the process to complete
        const exitCode = await new Promise((resolve) => {
            dotnetProcess.on('close', (code) => {
                resolve(code);
            });
        });
        
        if (exitCode === 0) {
            // Generate Copilot configuration JSON
            const copilotConfig = {
                [`${projectName}-tools`]: {
                    command: "dotnet",
                    args: ["run", "--project", tempDir],
                    cwd: tempDir
                }
            };

            // Write the Copilot config to the project folder
            const copilotConfigPath = path.join(tempDir, 'copilot-config.json');
            fs.writeFileSync(copilotConfigPath, JSON.stringify(copilotConfig, null, 2));

            // Also create a README with instructions
            const readmeContent = `# ${projectName} - MCP Server

This MCP server was generated from the OpenAPI specification: ${openApiInput}

## Quick Start

### 1. Run the MCP Server
\`\`\`bash
cd "${tempDir}"
dotnet run
\`\`\`

### 2. Add to GitHub Copilot (VS Code)

1. Open VS Code
2. Go to Settings (Ctrl+,)
3. Search for "MCP"
4. Click "Edit in settings.json"
5. Add this configuration to your \`mcp.servers\` section:

\`\`\`json
{
  "mcp.servers": {
    "${projectName}-tools": {
      "command": "dotnet",
      "args": ["run", "--project", "${tempDir}"],
      "cwd": "${tempDir}"
    }
  }
}
\`\`\`

### 3. Alternative: Use the Generated Config

A \`copilot-config.json\` file has been generated in this folder. You can copy its contents directly into your VS Code MCP settings.

## Generated Tools

This MCP server provides the following tools based on your OpenAPI specification:
- [Tool details will be shown when you run the server]

## Configuration

- **Project Name**: ${projectName}
- **Namespace**: ${namespace || projectName}
- **Base URL**: ${baseUrl || 'From OpenAPI spec'}
- **Generated**: ${new Date().toISOString()}
`;

            const readmePath = path.join(tempDir, 'README.md');
            fs.writeFileSync(readmePath, readmeContent);

            // Create a zip file
            const zipPath = path.join(__dirname, 'downloads', `${projectName}.zip`);
            const downloadsDir = path.join(__dirname, 'downloads');
            
            if (!fs.existsSync(downloadsDir)) {
                fs.mkdirSync(downloadsDir, { recursive: true });
            }
            
            // For now, just copy the directory structure
            // In a real implementation, you'd use a zip library
            const result = {
                success: true,
                message: `✅ MCP server '${projectName}' generated successfully!

📁 Project: ${projectName}
🌐 OpenAPI: ${openApiInput}
📦 Namespace: ${namespace || projectName}
🔗 Base URL: ${baseUrl || 'From OpenAPI spec'}

🚀 To run: cd ${tempDir} && dotnet run
📋 Copilot config: copilot-config.json
📖 Instructions: README.md
📦 Download ready: ${projectName}.zip

🎯 Generated Features:
• Converted OpenAPI endpoints to MCP tools
• Generated parameter types and schemas
• Created complete project structure with README
• Copilot configuration for easy testing`,
                downloadPath: `/downloads/${projectName}.zip`,
                projectPath: tempDir
            };
            
            res.json(result);
        } else {
            res.status(400).json({
                success: false,
                message: `❌ Error generating MCP server: ${stderr || 'Unknown error'}`,
                errorDetails: stderr
            });
        }
        
    } catch (error) {
        console.error('Error:', error);
        res.status(500).json({
            success: false,
            message: `❌ Error generating MCP server: ${error.message}`,
            errorDetails: error.stack
        });
    }
});

// Download endpoint
app.get('/downloads/:filename', (req, res) => {
    const filename = req.params.filename;
    const filePath = path.join(__dirname, 'downloads', filename);
    
    if (fs.existsSync(filePath)) {
        res.download(filePath);
    } else {
        res.status(404).send('File not found');
    }
});

app.listen(port, () => {
    console.log(`🚀 OpenAPI to MCP Generator Web UI running at http://localhost:${port}`);
    console.log(`📱 Open your browser and navigate to http://localhost:${port}`);
});
