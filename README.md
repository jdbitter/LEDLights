# LEDLights

A C#/.NET application for controlling Keepsmile KS03 Bluetooth Low Energy (BLE) LED light controllers.

## Overview

LEDLights provides a Windows application for communicating with and controlling Keepsmile KS03 LED light controllers over Bluetooth Low Energy.

The application is primarily intended as an alternative control interface for the KS03, while also providing a foundation for interacting with the device programmatically.

## Features

LEDLights currently provides control over:

* **Color** — Set the color of the LED lights.
* **Brightness** — Adjust the brightness of the lights.
* **Presets** — Select from the KS03's available color presets.
* **Preset Speed** — Adjust the speed of supported presets.
* **Power** — Turn the lights on and off.
* **Device Persistence** — Remember a previously connected KS03 device for subsequent connections.

<!--
## Demonstration

TODO: add a GIF demonstrating functionality

-->
## Project Structure

```text id="a59s0p"
LEDLights
├── Libraries
│   ├── Bluetooth
│   ├── CriticalData
│   └── FileSystem
│
├── Lights
│   ├── Lights.Cli
│   └── Lights.Core
│
└── LEDLights.slnx
```

### Libraries

* **Bluetooth** — Reusable Bluetooth Low Energy communication functionality.
* **CriticalData** — Management of persistent application-critical data.
* **FileSystem** — File system and serialization utilities.

### Lights

* **Lights.Core** — Core LED controller functionality, including support for the Keepsmile KS03.
* **Lights.Cli** — Command-line interface for interacting with the application.

## Technologies

* C#
* .NET 10
* Windows Bluetooth LE APIs
