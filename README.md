# QR Code Reader (QrCode_Reader)

![.NET](https://img.shields.io/badge/.NET-10.0%2B-purple)
![MAUI](https://img.shields.io/badge/.NET%20MAUI-Cross--Platform-blue)
![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey)
![Language](https://img.shields.io/badge/Language-C%23-green)

**QR Code Reader** is a .NET MAUI application written in C# that allows users to read, scan, and manage QR code data.  
This project serves as a clean and extensible foundation for QR-based workflows such as identification, labeling, and data association.

---

## Features

- .NET MAUI cross-platform architecture
- QR code reading and decoding
- Clean XAML-based UI
- Modular structure (Data, Models, Views, Helpers)
- SQLite-ready data layer

---

## Project Structure

```
QrCode_Reader/
├── Data/
│   └── LocalDatabase.cs
├── Helpers/
│   └── QrHelper.cs
├── Models/
│   └── Cient.cs
├── Platforms/
├── Resources/
├── Views/
│   ├──  ClientDetailsPage.xaml
│   │    └── ClientDetailsPage.xaml.cs
│   ├──  ConfirmDeliveryPage.xaml
│   │    └── ConfirmDeliveryPage.xaml.cs
│   ├──  ListPage.xaml
│   │    └── ListPage.xaml.cs
│   └──  ScannerPage.xaml
│   │    └── ScannerPage.xaml.cs
├── App.xaml
│   └── App.xaml.cs
├── AppShell.xaml
│   └── AppShell.xaml.cs
├── MauiProgram.cs
├── QrCode_Reader.csproj
└── QrCode_Reader.slnx
```

---

## Technology Stack

- **.NET MAUI**
- **C#**
- **SQLite**
- **ZXing.Net.MAUI**
- **CommunityToolkit.MAUI**

---

## Getting Started

### Prerequisites

- Visual Studio
- .NET MAUI workload installed
- .NET 10.0 SDK or newer

### Installation

```bash
git clone https://github.com/rafaferreira31/QrCode_Reader.git
```

Open the solution file:

```bash
QrCode_Reader.slnx
```

Select **Windows Machine** as the startup target and run the project.

---

## Usage

The application starts on the **MainPage**, where QR code scanning and visualization logic is handled.
The current structure allows easy extension for:

- Reading QR codes from image files
- Associating QR codes with CSV data
- Generating printable labels
- Persisting data locally with SQLite

---


