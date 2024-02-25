# Revit to Navisworks Conversion Service

## Overview

This project provides a solution for automatically converting Revit files (.rvt) into Navisworks (.nwd) format, leveraging a background ASP.NET service. Designed to seamlessly integrate into existing infrastructure, the service operates in the background, ensuring minimal disruption and maintaining clear separation of architecture responsibilities. This solution is ideal for organizations looking to automate the conversion process within their BIM (Building Information Modeling) workflows, enhancing productivity and facilitating easier model integration and review processes.

## Features

- **Automated Conversion:** Automates the process of converting Revit files to Navisworks format, eliminating manual tasks and streamlining workflows.
- **Background Processing:** Runs entirely in the background, allowing users to continue with other tasks without interruption.
- **Split Architecture:** Adheres to a split architecture model, ensuring that the service is decoupled and responsibilities are clearly separated.
- **Logging and Monitoring:** Includes comprehensive logging and monitoring capabilities to track conversions and identify any issues promptly.
- **Easy Integration:** Designed to easily integrate with existing systems and workflows, providing a flexible and scalable solution.

## Getting Started

### Prerequisites

- .NET Core 3.1 or later
- Autodesk Revit 202x (for file generation)
- Autodesk Navisworks Manage 202x (for conversion capabilities)

### Installation

1. Clone the repository to your local machine:
git clone

2. Navigate to the cloned directory:
cd 

3. Build the solution:
dotnet build

### Configuration

Before running the service, configure the necessary settings in the `appsettings.json` file, including:

- Revit file source directory
- Navisworks output directory
- Logging preferences

### Running the Service

To start the conversion service, execute:
dotnet run

The service will now monitor the specified source directory for new Revit files and automatically convert them to Navisworks format, placing the converted files in the output directory.

## Usage

Simply place Revit files (.rvt) in the configured source directory. The service automatically detects new files, converts them to Navisworks (.nwd) format, and saves them in the output directory. No further user interaction is required.

## Contributing

We welcome contributions to improve the Revit to Navisworks Conversion Service! Please feel free to submit pull requests or open issues to discuss proposed changes or report bugs.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Autodesk Revit and Navisworks teams for providing the software and SDKs necessary for file conversion.
