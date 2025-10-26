# GitHub Actions Setup Guide

This document explains how to configure GitHub Actions workflows for building and publishing the ProcessUtils NuGet package.

## Prerequisites

1. A GitHub repository with the code pushed
2. A NuGet.org account
3. (Optional) A Codecov account for code coverage reports

## Required GitHub Secrets

To enable the workflows, you need to configure the following secrets in your GitHub repository:

### Setting up GitHub Secrets

Go to your repository on GitHub:
1. Navigate to **Settings** → **Secrets and variables** → **Actions**
2. Click **New repository secret**

### Required Secrets

#### 1. NUGET_API_KEY
- **Purpose**: Authenticates with NuGet.org to publish packages
- **How to get it**:
  1. Log in to [nuget.org](https://www.nuget.org)
  2. Go to your account settings
  3. Navigate to **API Keys**
  4. Click **Create**
  5. Set a name (e.g., "GitHub Actions ProcessUtils")
  6. Set expiration (recommended: 365 days)
  7. Select scope: **Push** and **Push new packages and package versions**
  8. Select packages: Choose the pattern or specific package
  9. Copy the generated API key
- **Add to GitHub**: Create a secret named `NUGET_API_KEY` with the copied value

#### 2. CODECOV_TOKEN (Optional)
- **Purpose**: Uploads code coverage reports to Codecov
- **How to get it**:
  1. Log in to [codecov.io](https://codecov.io)
  2. Add your GitHub repository
  3. Copy the upload token
- **Add to GitHub**: Create a secret named `CODECOV_TOKEN` with the copied value
- **Note**: This is optional. The workflow will continue if this secret is not set.

## Workflows Overview

### 1. CI Build (`ci-build.yml`)

**Triggers:**
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop` branches
- Manual trigger via workflow dispatch

**What it does:**
- Builds the project on Ubuntu, Windows, and macOS
- Runs all unit tests
- Collects code coverage (Ubuntu only)
- Uploads coverage to Codecov (if configured)
- Creates NuGet package artifacts
- Runs code quality checks (formatting, warnings)

**Artifacts:** 
- NuGet packages (`.nupkg` files) available for 7 days

### 2. Publish to NuGet (`publish-nuget.yml`)

**Triggers:**
- When a GitHub release is published
- Manual trigger with version input

**What it does:**
- Builds the project in Release configuration
- Runs all tests
- Creates versioned NuGet packages
- Publishes to NuGet.org
- Uploads package artifacts (retained for 90 days)

**Important Notes:**
- For manual triggers, specify the version (e.g., `1.0.0`)
- For release triggers, version is extracted from the Git tag (e.g., `v1.0.0` → `1.0.0`)
- The workflow uses a production environment for additional protection

## Creating a Release

### Option 1: GitHub Release (Recommended)

1. Go to your repository on GitHub
2. Click on **Releases** → **Draft a new release**
3. Click **Choose a tag**
4. Type a new tag name following semantic versioning with 'v' prefix (e.g., `v1.0.0`)
5. Click **Create new tag on publish**
6. Fill in release title and description
7. Check **Set as a pre-release** if appropriate
8. Click **Publish release**

The `publish-nuget.yml` workflow will automatically trigger and publish the package.

### Option 2: Manual Workflow Dispatch

1. Go to **Actions** → **Publish to NuGet**
2. Click **Run workflow**
3. Select the branch
4. Enter the version number (e.g., `1.0.1`)
5. Check prerelease if needed
6. Click **Run workflow**

## Version Management

### Updating Package Version

To change the package version, update the `<Version>` property in `processutils/processutils.csproj`:

```xml
<Version>1.0.1</Version>
```

For releases, the workflow overrides this with the version from the Git tag or manual input.

### Versioning Strategy

Follow [Semantic Versioning](https://semver.org/):
- **Major** (1.0.0): Breaking changes
- **Minor** (1.1.0): New features, backward compatible
- **Patch** (1.0.1): Bug fixes, backward compatible

### Pre-release Versions

For pre-release packages, use suffixes:
- `1.0.0-alpha.1`
- `1.0.0-beta.1`
- `1.0.0-rc.1`

## Environment Protection (Optional)

For additional security, configure a production environment:

1. Go to **Settings** → **Environments**
2. Click **New environment**
3. Name it `production`
4. Add protection rules:
   - Required reviewers (recommended for production releases)
   - Wait timer
   - Deployment branches (e.g., only `main`)

The `publish-nuget.yml` workflow will wait for approval if configured.

## Testing the Package Locally

Before publishing, test the package locally:

```bash
# Build and create package
dotnet pack ./processutils/processutils.csproj --configuration Release --output ./artifacts

# Install locally for testing
dotnet add package ProcessUtils --source ./artifacts --version 1.0.0
```

## Troubleshooting

### Workflow fails with "secrets.NUGET_API_KEY"
- Ensure the secret is correctly named `NUGET_API_KEY`
- Verify the API key hasn't expired
- Check the API key has the correct permissions

### Package push fails with "409 Conflict"
- This version already exists on NuGet.org
- Increment the version number
- NuGet.org doesn't allow overwriting published packages

### Tests fail in workflow but pass locally
- Check for platform-specific issues (the CI runs on Linux, Windows, and macOS)
- Verify all dependencies are properly declared
- Check for hardcoded paths or environment-specific code

## Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [NuGet.org Package Publishing](https://docs.microsoft.com/en-us/nuget/nuget-org/publish-a-package)
- [Semantic Versioning](https://semver.org/)
- [.NET Pack Command](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-pack)
