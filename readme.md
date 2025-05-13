# How to Run the Tests

## Prerequisites

- .NET SDK 7.0 or later installed  
- Firefox browser installed  
- GeckoDriver available on your PATH (download: https://github.com/mozilla/geckodriver/releases)  
- A terminal or command prompt

## Steps

1. **Open a terminal** and navigate to the project root (where `telus-qa-test-parth-thakkar.sln` is located):

   ```bash
   cd /path/to/telus-qa-test-parth-thakkar
   ```

2. **Restore NuGet packages**:

   ```bash
   dotnet restore
   ```

3. **Verify GeckoDriver**:

   ```bash
   geckodriver --version
   ```

4. **Run all tests** (UI + API):

   ```bash
   dotnet test TelusQATest/TelusQATest.csproj --logger "html;LogFileName=test_results.html"
   ```

   - This will execute both the `SauceDemoTests` (UI) in Firefox and the `ApiAutomationTests` (API).
   - Test results will be saved in `test_results.trx`.

5. **Optional: Run only UI or API tests**:

   - UI tests only:
     ```bash
     dotnet test TelusQATest/TelusQATest.csproj --filter Category=UI
     ```
   - API tests only:
     ```bash
     dotnet test TelusQATest/TelusQATest.csproj --filter Category=API
     ```

## Viewing Test Results

After running the tests, you can generate and view an HTML report:

### HTML Report

```bash
dotnet test TelusQATest/TelusQATest.csproj \
  --logger "html;LogFileName=TestResults.html"
  ```

### Open TestResults/TestResults.html in your browser.