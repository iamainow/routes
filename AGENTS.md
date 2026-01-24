# AGENTS.md

Purpose
- Provide quick-start commands and style rules for agentic tools.
- This repo is a .NET 10 AOT-friendly IPv4/routing library and tools.

Repository Notes
- Primary language: C# with xUnit tests.
- Target framework: net10.0.
- AOT: PublishAot=true; avoid reflection/dynamic codegen.
- Build is strict: TreatWarningsAsErrors=true, AnalysisLevel=latest-all.
- Implicit usings enabled; nullable reference types enabled.
- No Cursor or Copilot rule files found in this repo.

Core Commands (run from repo root)
- Build all: `dotnet build`
- Test all: `dotnet test`
- Test project: `dotnet test Tests/routes.Test/routes.Test.csproj`
- Test project (parsers): `dotnet test Tests/Ip4Parsers.Test/Ip4Parsers.Test.csproj`
- Test project (extensions): `dotnet test Tests/routes.Extensions/routes.Extensions.csproj`
- Build app: `dotnet build Apps/nifroute/nifroute.csproj`
- Build app: `dotnet build Apps/nipset/nipset.csproj`
- Publish AOT: `dotnet publish Apps/nifroute/nifroute.csproj -c Release`
- Publish AOT: `dotnet publish Apps/nipset/nipset.csproj -c Release`
- Run benchmarks: `dotnet run --project Benchmarks/routes.Benchmarks/routes.Benchmarks.csproj -c Release`
- Run benchmarks: `dotnet run --project Benchmarks/Ip4Parsers.Benchmarks/Ip4Parsers.Benchmarks.csproj -c Release`

Single Test (xUnit)
- By fully-qualified name:
  `dotnet test Tests/routes.Test/routes.Test.csproj --filter "FullyQualifiedName~Namespace.TypeName.MethodName"`
- By class name:
  `dotnet test Tests/routes.Test/routes.Test.csproj --filter "FullyQualifiedName~Namespace.TypeName"`
- By trait/category (if present):
  `dotnet test Tests/routes.Test/routes.Test.csproj --filter "Category=Fast"`

Code Style (from .editorconfig)
- Indentation: 4 spaces, no tabs.
- Line endings: CRLF.
- File-scoped namespaces preferred (warning if not).
- Braces required; opening brace on new line.
- Keep single-line blocks/statements when already single-line.
- Use predefined types (`int`, `string`) instead of `Int32`, `String`.
- Prefer explicit types except when type is apparent:
  - `var` ok when RHS makes type obvious.
  - Avoid `var` for built-in types and elsewhere by default.
- Object/collection initializers preferred where applicable.
- Use `?.` and `??` where they simplify code.
- Use pattern matching (`is`, `switch`) instead of casts or `as` checks.
- Prefer switch expressions when readable.
- Prefer expression-bodied accessors/properties/indexers; avoid for methods/constructors.
- Use `this.` qualification for fields, methods, and properties.
- Operator placement when wrapping: beginning of line.
- Parentheses: always for clarity in arithmetic/relational/other binary ops.
- `using` directives placed outside namespace.
- Modifier order: public, private, protected, internal, file, static, extern,
  new, virtual, abstract, sealed, override, readonly, unsafe, required,
  volatile, async.

Naming Conventions
- Types (class/struct/enum): PascalCase.
- Interfaces: PascalCase prefixed with `I`.
- Methods/properties/events: PascalCase.
- Avoid underscores in identifiers (generally); tests may relax CA1707.

Formatting and Layout
- One blank line between logical blocks; multiple blank lines allowed.
- Preserve compact formatting for small single-line constructs.
- New lines before `catch`, `else`, `finally`.
- New lines before members in object/anonymous initializers.
- Space after keywords in control flow (`if`, `for`, `while`).
- Space around binary operators.
- No extra spaces inside parentheses/brackets.

Nullability and Types
- Nullable enabled: annotate reference types and handle null paths.
- Prefer null-propagation and explicit null checks over reference equality.
- Prefer readonly fields and readonly structs when possible.
- Prefer `scoped`/ref-safe patterns for stack-only types (e.g., `Ip4RangeArray`).

Error Handling
- Treat warnings as errors; keep code analyzers clean.
- Favor explicit `InvalidOperationException` for routing table operations.
- Keep exception messages concise and actionable.
- Avoid dynamic features; stay AOT-safe.

Performance and AOT Constraints
- SIMD-friendly patterns are encouraged in `SpanHelper`/`Ip4Address` paths.
- Avoid allocations in hot paths; prefer spans and stack allocation
  (benchmarks may relax CA2014).
- No reflection-based serialization or dynamic code generation.
- Avoid async with `ref struct` types.

Testing Guidance
- Tests mirror source names (e.g., `Ip4Address.cs` → `Ip4AddressTest.cs`).
- Include boundary/edge cases (min/max, adjacency, overlaps).
- Keep test data minimal but explicit.

Project Layout
- Core library: `routes/`.
- Parsers: `Ip4Parsers/`.
- Route diffing: `RoutesCalculator/`.
- Windows native interop: `NativeMethods.Windows/`.
- Apps: `Apps/nifroute/`, `Apps/nipset/`.
- Tests: `Tests/`.
- Benchmarks: `Benchmarks/`.

When Adding New Features
- Add algorithm in `SpanHelper.cs` if performance-critical.
- Add wrapper in `Ip4RangeArray` for span-based operations.
- Maintain normalized range invariants (sorted, non-overlapping, non-adjacent).
- Update/add tests and benchmarks as needed.

Agent Workflow Tips
- Prefer repo-wide conventions over personal preferences.
- Keep changes minimal and focused; avoid unrelated refactors.
- Do not change analyzer settings unless requested.
