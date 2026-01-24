![CodeRabbit Pull Request Reviews](https://img.shields.io/coderabbit/prs/github/iamainow/routes?utm_source=oss&utm_medium=github&utm_campaign=iamainow%2Froutes&labelColor=171717&color=FF570A&link=https%3A%2F%2Fcoderabbit.ai&label=CodeRabbit+Reviews)

# routes

A high-performance .NET library for managing IPv4 addresses, ranges, and network routing tables. Optimized for AOT compilation with SIMD-accelerated operations for lightning-fast IP range manipulation.

## Features

- **Fast IP Range Operations**: Union and difference operations on IP ranges with SIMD optimizations
- **Normalized Range Sets**: Automatic normalization ensures ranges are sorted, non-overlapping, and non-adjacent
- **Windows Routing Table Management**: Direct Windows API integration for reading and modifying routing tables
- **AOT-Compiled Tools**: Native executables with minimal startup time and small binary sizes
- **Zero-Allocation Hot Paths**: `ref struct` and `Span<T>` for memory-efficient operations
- **Comprehensive Parsing**: Support for CIDR notation, single IPs, and range formats

## Quick Start

### Prerequisites

- .NET 10.0 SDK or later
- Windows OS (for routing table manipulation features)

### Building

```bash
# Build entire solution
dotnet build

# Run tests
dotnet test

# Run benchmarks
dotnet run --project Benchmarks/routes.Benchmarks/routes.Benchmarks.csproj -c Release
```

### Installation

Add a project reference (recommended for the source layout):

```bash
dotnet add reference routes/routes.csproj
```

## Core Libraries

### routes

Core library for IPv4 address and range manipulation.

**Key Types:**
- `Ip4Address`: Low-level IPv4 address struct with SIMD optimizations
- `Ip4Range`: Represents a continuous range of IP addresses
- `Ip4RangeArray`: High-performance ref struct for range set operations (union, except)
- `Ip4Subnet`: CIDR subnet representation
- `Ip4Mask`: Subnet mask utilities

**Example:**
```csharp
using routes;

// Create IP ranges
var range1 = new Ip4Range(new Ip4Address(192, 168, 1, 0), new Ip4Address(192, 168, 1, 255));
var range2 = new Ip4Range(new Ip4Address(192, 168, 2, 0), new Ip4Address(192, 168, 2, 255));

// Perform set operations
var rangeArray = Ip4RangeArray.Create([range1, range2]);
var otherArray = new Ip4RangeArray(new Ip4Range(new Ip4Address(10, 0, 0, 0), new Ip4Address(10, 0, 0, 255)));
var union = rangeArray.Union(otherArray);
```

### Ip4Parsers

Parsing library for IP address formats.

**Example:**
```csharp
using Ip4Parsers;

// Parse CIDR notation
var ranges = Ip4SubnetParser.GetRanges("192.168.1.0/24");
foreach (var range in ranges)
{
    // Use range
}
```

### RoutesCalculator

Calculates differences between route sets for efficient routing table updates.

```csharp
using RoutesCalculator;

RoutesDifferenceCalculator.CalculateDifference(
    oldRoutes,
    newRoutes,
    toAdd: route => Console.WriteLine($"Add {route}"),
    toRemove: route => Console.WriteLine($"Remove {route}"),
    toChangeMetric: change => Console.WriteLine($"Metric {change}"));
```

### NativeMethods.Windows

Windows-specific P/Invoke for routing table manipulation.

```csharp
using NativeMethods.Windows;

// Read current routing table
var routes = Ip4RouteTable.GetRouteTable();

// Create a new route (requires admin privileges)
Ip4RouteTable.CreateRoute(destination, mask, gateway, interfaceIndex, metric);
```

## Command-Line Tools

### nifroute

Network interface routing management tool. Manages Windows routing tables for specific network interfaces.

**Usage:**
```bash
# Publish as native executable
dotnet publish Apps/nifroute/nifroute.csproj -c Release

# Set routes for an interface
cat routes.txt | nifroute set-routes <interface-name>

# Print available interfaces
nifroute print-interfaces

# Print current routes
nifroute print-routes
```

### nipset

IP range set manipulation utility. Performs set operations on IP ranges from stdin.

**Usage:**
```bash
# Publish as native executable
dotnet publish Apps/nipset/nipset.csproj -c Release

# Perform union operation
cat ranges1.txt | nipset union ranges2.txt

# Perform difference operation
cat ranges1.txt | nipset except ranges2.txt
```

## Architecture

The library is designed with performance and AOT compatibility in mind:

- **Normalization**: `Ip4RangeArray` maintains ranges in normalized form (sorted, non-overlapping, non-adjacent), enabling efficient O(n) set operations
- **SIMD Optimization**: Critical code paths in `SpanHelper` and `Ip4Address` use vectorized operations
- **ref struct**: Stack-only allocation via `Ip4RangeArray` eliminates heap pressure in hot paths
- **AOT-Safe**: No reflection, no dynamic code generation, fully compatible with Native AOT

## Development

### Project Structure

```
routes/
├── routes/                      # Core library
├── Ip4Parsers/                  # IP parsing library
├── RoutesCalculator/            # Route difference calculator
├── NativeMethods.Windows/       # Windows routing table API
├── AnsiColoredWriter/           # Console output utilities
├── Apps/
│   ├── nifroute/               # Network interface routing tool
│   └── nipset/                 # IP set manipulation tool
├── Tests/
│   ├── routes.Test/
│   ├── Ip4Parsers.Test/
│   └── routes.Extensions/
└── Benchmarks/
    ├── routes.Benchmarks/
    └── Ip4Parsers.Benchmarks/
```

### Building and Testing

```bash
# Build everything
dotnet build

# Run all tests
dotnet test

# Run specific test project
dotnet test Tests/routes.Test/routes.Test.csproj

# Run benchmarks
dotnet run --project Benchmarks/routes.Benchmarks/routes.Benchmarks.csproj -c Release

# Publish AOT applications
dotnet publish Apps/nifroute/nifroute.csproj -c Release
dotnet publish Apps/nipset/nipset.csproj -c Release
```

### Code Quality

- **Strict Compilation**: `TreatWarningsAsErrors=true` with `AnalysisLevel=latest-all`
- **Modern C#**: File-scoped namespaces, nullable reference types, pattern matching
- **Performance-First**: SIMD operations, zero-allocation hot paths, benchmarking culture
- **Testing**: Comprehensive xUnit tests with edge case coverage

### Adding New Features

1. Implement the algorithm in `SpanHelper.cs` as a static method
2. Add a wrapper method in `Ip4RangeArray` that allocates buffers and calls SpanHelper
3. Ensure the result maintains normalization invariants
4. Add unit tests in `Tests/routes.Test/`
5. Add benchmarks if performance-critical

## Performance

This library is designed for performance-critical applications:

- SIMD-accelerated IP address operations
- Zero-allocation range set operations using stack allocation
- Normalized ranges enable O(n) union/difference
- AOT compilation eliminates JIT overhead
- Benchmark-driven development ensures regression-free optimization

Run the benchmarks to see the performance characteristics:

```bash
dotnet run --project Benchmarks/routes.Benchmarks/routes.Benchmarks.csproj -c Release
```

## Platform Support

- **Core Libraries**: Cross-platform (.NET 10.0+)
- **NativeMethods.Windows**: Windows only (uses Windows Routing API)
- **Applications**: Best on Windows for full routing table management

## License

See [LICENSE](LICENSE) for details.

## Contributing

Contributions are welcome! Please ensure:

- All tests pass: `dotnet test`
- Code follows .editorconfig conventions
- New features include tests and benchmarks
- Changes maintain AOT compatibility

For detailed development guidelines, see [CLAUDE.md](CLAUDE.md) and [AGENTS.md](AGENTS.md).
