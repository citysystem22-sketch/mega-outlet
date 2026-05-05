# Mega Outlet Product Check

A Windows desktop application for checking WooCommerce product availability, price, and description. Designed for in-store customers in physical retail locations.

## Features

- **Live Search** - Real-time product search as you type (300ms debounce)
- **Stock Status** - Color-coded display (green=available, red=out of stock, orange=low stock)
- **Touch Optimized** - Large, touch-friendly UI elements for tablet/kiosk use
- **Polish Language** - Full Polish localization
- **Offline Mode** - Caches products for offline fallback
- **WooCommerce Integration** - Direct REST API v3 connection

## Quick Start

### Download & Run

**Requires Windows 10/11 (64-bit)**

1. **Download these two files:**
   - [MegaOutletCheck.exe](./MegaOutletCheck/publish/MegaOutletCheck.exe) (~158 MB)
   - [settings.json](./MegaOutletCheck/publish/settings.json)

2. **Place both files in the same folder**

3. **Run `MegaOutletCheck.exe`**

That's it! The app is pre-configured with mega-outlet.pl API keys.

## Building from Source (Optional)

If you want to build yourself:

```bash
# Requires .NET 8.0 SDK
dotnet publish MegaOutletCheck -c Release -r win-x64 --self-contained true -o publish
```

## Configuration

The `settings.json` file contains your store configuration:

```json
{
  "StoreUrl": "https://mega-outlet.pl",
  "ApiKey": "ck_...",
  "ApiSecret": "cs_...",
  "EnableCache": true
}
```

To change stores, generate new API keys in WooCommerce admin:
- WooCommerce → Settings → Advanced → REST API
- Create key with **Read** permissions

## Usage

1. Launch the app
2. Type product name, SKU, or keywords
3. Results appear automatically
4. Click product to view details

### Keyboard
- **Escape** - Clear search
- **F11** - Fullscreen mode

## System Requirements

- Windows 10/11 (64-bit)
- No .NET runtime needed (self-contained)
- ~200 MB disk space

## Troubleshooting

- **"Invalid API keys"** → Verify keys in WooCommerce
- **"Connection failed"** → Check internet
- **No products** → Try different search terms

## License

MIT License