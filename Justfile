# Pollux Polyglot .NET task runner.
# Standard Sxnnyside quality and build recipes.

# Bootstrap dependencies.
install:
    dotnet restore

# Fast dev build in debug mode.
dev:
    dotnet build

# Compile release distribution.
build:
    dotnet build -c Release

# Run test suite.
test:
    dotnet test

# Verify type checking and compilation without testing.
typecheck:
    dotnet build

# Check code formatting without applying modifications.
lint:
    dotnet format --verify-no-changes

# Format source files deterministically.
format:
    dotnet format

# Generate NuGet packages.
pack:
    dotnet pack -c Release -o ./artifacts

# Full non-mutating quality gate; invoked by CI.
check: lint typecheck test

# Clean build artifacts.
clean:
    dotnet clean
    rm -rf src/Pollux/bin src/Pollux/obj tests/Pollux.Tests/bin tests/Pollux.Tests/obj artifacts
