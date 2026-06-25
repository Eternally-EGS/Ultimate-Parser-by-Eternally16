# Ultimate Parser Engine (v1.0.0-Beta)

A high-performance, modular pipeline engine built on .NET 8, designed for total web automation, bypassing bot protections, and intelligent data extraction.

## 🚀 Key Features

*   **Dual-Engine Core:** Dynamic switching between `Playwright` (for heavy JS rendering) and `AngleSharp` (for lightning-fast static parsing).
*   **100% Declarative:** Total control over system behavior, filters, and selectors via external configuration files—no recompilation required.
*   **Pipeline Processing:** Data flows through isolated, deterministic stages: Extraction -> Filtering -> Validation -> Transformation.

---

## ⚡ Future Improvements & Scaling (Roadmap)

The project is under active evolution. The following architectural and functional upgrades are planned:

### 🧠 Dynamic Logic Module
*   **Roslyn Scripting Integration:** Embedding the Roslyn compiler to execute custom C# code on the fly directly from configs.
*   **Hot-Reload Configurations:** Monitoring config changes and applying them instantly without stopping active parsing threads.

### 🛡️ Anti-Bot & Network Layer
*   **Smart Proxy Rotator:** Advanced proxy pool management with cooldown tracking, ban detection, and automated performance testing.
*   **Anti-Captcha Pipeline:** Seamless integration with solver APIs (CapMonster/2Captcha) as an isolated pipeline stage.
*   **Fingerprint Spoofing:** Mimicking browser fingerprints (Canvas, WebGL, headers) to bypass Cloudflare and advanced anti-bot protection.

### 📊 Interface & Ecosystem
*   **WPF Control Panel:** A full-fledged GUI dashboard for real-time monitoring of threads, logs, and proxy pool status.
*   **CLI Dashboard:** An interactive console UI for running the engine on headless servers (Docker/Linux).
