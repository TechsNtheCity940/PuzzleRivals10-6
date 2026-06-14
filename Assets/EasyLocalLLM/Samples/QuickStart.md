# EasyLocalLLM QuickStart Scene Setup Guide

This is a scene setup guide for running QuickStart in the Unity Editor.

## Overview

This document explains how to create a scene using **QuickStart**.

---

## QuickStart Scene

This is a test that communicates with a real Ollama server and runs an LLM.

### Prerequisites

- ✅ Ollama server is running (`ollama serve`)
- ✅ Model is installed (`ollama pull mistral`)

### Setup Steps

1. **Create a new scene**
   - Right-click in `Assets/EasyLocalLLM/Samples/Scenes/` → Create → Scene → Scene
   - Name: `QuickStartScene.unity`
   - Double-click the created scene to open it

2. **Create a GameObject**
   - Right-click in Hierarchy
   - Create Empty
   - Name: `QuickStartManager`

3. **Attach the script**
   - In Inspector, click Add Component
   - Search for Script → `QuickStart`
   - Attach it

4. **Run**
   - Click the Play button
   - Check logs in the Console window

### Test Details

QuickStart runs the following five steps:

| Step | Description | Expected Output |
|------|-------------|-----------------|
| 1 | Initialize OllamaClient | `✓ Client initialized` |
| 2 | Send a simple message | `✓ Response received: ...` |
| 3 | Follow-up with session history | `✓ Follow-up response: ...` |
| 4 | Streaming feature test | `✓ Streaming completed!` |
| 5 | Tool usage feature test | `✓ Tool call passed!` |


## Troubleshooting

### ❌ "Failed to connect to server" Error

**Cause**: Ollama server is not running

**Solution**:
```bash
ollama serve
```

### ❌ "Model not found" Error

**Cause**: The specified model has not been downloaded

**Solution**:
```bash
ollama pull mistral
```

### ❌ Timeout Error

**Cause**: Server response is slow

**Solution**:
1. Increase `HttpTimeoutSeconds`
   ```csharp
   config.HttpTimeoutSeconds = 60; // Default: 30
   ```
2. Check server resource usage
3. Check network connectivity

### ❌ Console Logs Are Not Displayed

**Cause**: Incorrect Scripting Backend configuration

**Solution**:
1. Open Window → General → Console
2. Make sure `Debug.Log` is enabled
3. Confirm Scripting Backend is set to Mono

---

## Next Steps After Testing

1. **Learn practical usage**
   - Refer to `SimpleChat` / `LateralThinkingQuiz` to learn how to use it in an actual game
   - The scene is already built for Unity 6.2 (6000.2.6f2)

2. **Use the library in a custom scene**
   - Use this guide as a reference to import and use EasyLocalLLM in your custom scene

---

## Reference Link

- [Documentation/API_Reference.md](../Documentation/API_Reference.md) - API documentation