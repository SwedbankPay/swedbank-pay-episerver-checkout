Foundation.UiTests
===========
This project is a set of UI tests which aims to ensure that the Payex plugin developed for EPiServer Commerce is functioning properly.

Nuget packages
-------------
The UI Tests are written based on the framework Atata in order to achieve good separation of concern between the presentation and the test logic. Atata is written on top of Selenium as it is the most used UI test framework today.

* Selenium.WebDriver
* Atata
* Atata.WebDriverExtras

The framework used to run the test uses NUnit for its simplicity of use and its good integration with the DevOps Azure platform.

* NUnit
* NUnit3TestAdapter

The web drivers required to run the tests are Chrome web driver (default) and Firefox (has configuration needed to run with)

* Selenium.WebDriver.ChromeDriver
* Selenium.Firefox.WebDriver

As the tests are checking the results both against the UI and the Payex Api, it includes also the latest version of Swedbank.Pay.Sdk

* SwedbankPay.Sdk

Installation
------------

1.  Configure Visual Studio to add this package sources:
* https://pkgs.dev.azure.com/SwedbankPay/swedbank-pay-episerver-checkout/_packaging/SwedbankPayCheckout/nuget/v3/index.json
* https://api.nuget.org/v3/index.json

This allows missing packages to be downloaded, when the solution is built.

2.  Open the project and build to download nuget package dependencies.
3.  If the two previous steps are respected, the project should build without any issues.

Running the tests
------------

### --- With Visual studio ---

Running the tests can be done either directly in Visual Studio via the Test Explorer :
```
(Test > Test Explorer or Ctrl+E,T)
```
Select the tests to run or click on "Run all"

### --- With CLI ---

1. Install latest msi package (official github repository):
```
https://github.com/nunit/nunit-console/releases
```
2. Add to system PATH environment variable:
```
"C:\Program Files (x86)\NUnit.org\nunit-console"
```
3. Run :
```
nunit3-console <Foundation.UiTests.dll> (located in/debug or bin/release directory after building the project)
```

NB : Possibility to filter for running fewer test cases (Card, Swish, Authorization, Sale, Reversal...)

Example :
```
nunit3-console <path_to_EqualityAnalyzer.UiTests.dll> --where "cat == Pro"
```

Possible issues
------------

- Make sure that the correct version of Chrome is installed on the machine

At the time of writing this document, the version of Chrome web driver is v80, which means that the version of Chrome installed on the computer running the test must match. Running a lower version (or possibly higher version) could result in the following error :
```
This version of ChromeDriver only supports Chrome version 80
```
- Make sure that .NET framework 4.7.2 is installed on the machine
