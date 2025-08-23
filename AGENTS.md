# Agents Instructions

The CI workflow uses static checks that do not require Resonite assemblies.

- Formatting is enforced with `dotnet format`.
- Before committing, run `dotnet format --verify-no-changes` to verify formatting.
- Use `dotnet format` to apply formatting fixes.
