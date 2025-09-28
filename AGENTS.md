# Agents Instructions

The CI workflow uses static checks that do not require Resonite assemblies.

- CI enforces whitespace-only formatting: `dotnet format whitespace --verify-no-changes`.
- Locally, prefer full checks: `dotnet format --verify-no-changes` and `dotnet format` to fix.
