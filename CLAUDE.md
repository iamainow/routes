# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a high-performance .NET 10.0 library for managing IPv4 addresses, ranges, and network routing tables. The project is optimized for AOT (Ahead-of-Time) compilation and uses SIMD operations for performance-critical code paths.

**Key technologies:**
- Target Framework: .NET 10.0
- AOT Compilation: Enabled (`PublishAot=true`)
- Code Quality: `TreatWarningsAsErrors=true`, `AnalysisLevel=latest-all`
- Testing: xUnit
- Benchmarking: BenchmarkDotNet

## Build and Test Commands

### Building the solution
```bash
dotnet build
```

### Running all tests
```bash
dotnet test
```

### Running tests for a specific project
```bash
dotnet test Tests/routes.Test/routes.Test.csproj
dotnet test Tests/Ip4Parsers.Test/Ip4Parsers.Test.csproj
```

### Running benchmarks
```bash
dotnet run --project Benchmarks/routes.Benchmarks/routes.Benchmarks.csproj -c Release
dotnet run --project Benchmarks/Ip4Parsers.Benchmarks/Ip4Parsers.Benchmarks.csproj -c Release
```

### Building applications
```bash
dotnet build Apps/nifroute/nifroute.csproj
dotnet build Apps/nipset/nipset.csproj
```

### Publishing with AOT
```bash
dotnet publish Apps/nifroute/nifroute.csproj -c Release
dotnet publish Apps/nipset/nipset.csproj -c Release
```

## Architecture

### Core Libraries

**routes** (routes/): Core library containing IPv4 address/range manipulation
- `Ip4Address`: Low-level IPv4 address struct with SIMD optimizations
- `Ip4Range`: Represents a continuous range of IP addresses
- `Ip4RangeArray`: ref struct for efficient range set operations (union, except, intersection)
- `Ip4Subnet`: CIDR subnet representation
- `Ip4Mask`: Subnet mask utilities
- `SpanHelper`: Core algorithms for range normalization, union, and set operations on sorted/normalized spans
- Generic/ subdirectory: Generic implementations for range operations that can be specialized

**Ip4Parsers** (Ip4Parsers/): Parsing library for IP address formats
- `Ip4SubnetParser`: Parses IP addresses and subnets from string representations

**RoutesCalculator** (RoutesCalculator/): Calculates differences between route sets
- `RoutesDifferenceCalculator`: Determines which routes to add, remove, or change when updating routing tables

**NativeMethods.Windows** (NativeMethods.Windows/): Windows-specific P/Invoke for routing table manipulation
- `Ip4RouteTable`: Read/write Windows routing table via native API
- Platform: Windows only (uses Windows API for routing)

**AnsiColoredWriter** (AnsiColoredWriter/): Console output with ANSI color support

### Applications

**nifroute** (Apps/nifroute/): Network interface routing management tool
- Manages Windows routing tables for specific network interfaces
- Commands: set routes, print interfaces, print routes
- Uses all core libraries to read stdin IP ranges and apply them to a network interface

**nipset** (Apps/nipset/): IP range set manipulation utility
- Performs set operations (union, except) on IP ranges from stdin

### Important Implementation Details

**Normalization**: The `Ip4RangeArray` maintains ranges in "normalized" form:
- Sorted by `FirstAddress`
- Non-overlapping
- Non-adjacent (consecutive ranges are merged)

This invariant is maintained by `SpanHelper.MakeNormalizedFromUnsorted()` and enables efficient set operations.

**SIMD Optimization**: The current branch (SIMD) likely contains vectorization improvements. When modifying `SpanHelper.cs` or `Ip4Address.cs`, consider SIMD-friendly implementations.

**ref struct Usage**: `Ip4RangeArray` is a `ref struct` to enable stack-only allocation for performance. This means:
- Cannot be used as a field in non-ref structs or classes
- Cannot be boxed
- Cannot be used in async methods
- Uses `scoped` parameters to control lifetime

**AOT Compatibility**: All code must be AOT-compatible:
- No reflection-based serialization
- No dynamic code generation
- Careful with generic instantiations

## Testing Strategy

- Unit tests use xUnit
- Test files mirror source structure: `Ip4Address.cs` → `Ip4AddressTest.cs`
- Tests cover edge cases (min/max values, boundary conditions)
- Performance-critical code has corresponding benchmarks in Benchmarks/

## Common Development Patterns

**Adding a new set operation to Ip4RangeArray:**
1. Implement the algorithm in `SpanHelper.cs` as a static method
2. Add a wrapper method in `Ip4RangeArray` that allocates buffers and calls SpanHelper
3. Ensure the result maintains normalization invariants
4. Add unit tests in `Tests/routes.Test/`
5. Consider adding benchmarks if performance-critical

**Working with the route table:**
- Read operations: `Ip4RouteTable.GetRouteTable()`
- Modifications: Use `CreateRoute()`, `DeleteRoute()`, `ChangeMetric()`
- Windows Admin required for route modifications
- All route table operations throw `InvalidOperationException` on failure

**Parsing IP addresses:**
- Use `Ip4SubnetParser.GetRanges()` which returns `Span<Ip4Range>`
- Parser handles CIDR notation, single IPs, and range formats
- Results may be unsorted/overlapping; normalize with `Ip4RangeArray`
