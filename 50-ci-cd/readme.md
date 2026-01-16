# CD/CD Exercise

## Introduction

Welcome to SaasWizard's, a company that provides cutting-edge SaaS software development solutions. In this exercise, you must create a production-ready runtime environment in the Azure cloud and demonstrate a fully automated CI/CD pipeline using GitHub Actions and Azure.

## Your Task

* Implement an ASP.NET Core Minimal API.
* The Web API must contain any kind of business logic (calculation, check of input data, etc.). You must write at least one xUnit unit test for this logic.
* The API must use Entity Framework Core with SQL Server to store data.
* Create an Angular frontend that consumes the API.
* For now, it is not important what kind of data you are maintaining. Choose books, movies, products, Labubus - whatever you like.

The goal is that the application runs in a production-ready environment in Azure. It must be accessible via HTTPS over the public internet.

* The SQL DB Server has already been created and you will get your own database on it.
* An Azure Container Registry has already been created that you can use to build and store your container images.
* You must decide which Azure services you want to use to host the API and the Angular frontend.
  * Recommendation: Azure App Service Web App (with Containers) or Azure Container Apps

The main objective is to create a fully automated CI/CD pipeline that performs the following tasks:

* Whenever code is pushed to the `main` branch, the CI/CD pipeline should be triggered automatically.
* The pipeline must build the code and make sure that no compilation errors occur.
* Next, the pipline must run the unit tests and ensure that all tests pass successfully.
* If the build and tests are successful, the pipeline should deploy the application to Azure.

## Azure Access

Here are some important details for accessing the Azure resources:

* You work in the following Azure tenant:
  * `rainertimecockpit.onmicrosoft.com`
  * Tenant ID: `022e4faf-c745-475a-be06-06b1e1c9e39d`
  * Subscription ID: `b33f0285-db27-4896-ac5c-df22004b0aba`
* Four Microsoft Entra ID users for four teams have been created:
  * `5ahif-group1@rainertimecockpit.onmicrosoft.com`
  * `5ahif-group2@rainertimecockpit.onmicrosoft.com`
  * `5ahif-group3@rainertimecockpit.onmicrosoft.com`
  * `5ahif-group4@rainertimecockpit.onmicrosoft.com`
  * The initial password will be provided by your teacher during class. You will be required to change the password upon first login.
  * Use the Entra ID users to sign in to the [Azure Portal](https://portal.azure.com).
* Each group has its own resource group:
  * `htl-5ahif-group-1`
  * `htl-5ahif-group-2`
  * `htl-5ahif-group-3`
  * `htl-5ahif-group-4`
  * The Entra ID users have `Contributor` role assignments on their respective resource groups.
  * There is a shared resource group `htl-5ahif-shared` that contains shared resources (e.g., the SQL Server and the ACR). The Entra ID users do have read access to this resource group.
* Existing Azure SQL Server
  * Server name: `5ahif.database.windows.net`
  * Database names:
    * `5ahif-group1` with user `htl5ahif1`
    * `5ahif-group2` with user `htl5ahif2`
    * `5ahif-group3` with user `htl5ahif3`
    * `5ahif-group4` with user `htl5ahif4`
    * The passwords for the SQL users will be provided by your teacher during class.
* Existing Azure Container Registry
  * There is only one ACR for all teams: `5ahif.azurecr.io`
  * Your container image names must contain your group name, e.g., `5ahif.azurecr.io/group1backend:latest`

**Do you need more permissions?** Your teacher will act as your Azure administrator during the exercise. If you need additional permissions, ask him. However, you cannot just say "something does not work". **You must explain what you want to do and why you need additional permissions in an email to your teacher.** This let's you practice interacting with an administrative team in a professional manner.

## Sourcecode Management

* You must use GitHub to host your source code.
* One team member must create a GitHub repository and add the other team members as collaborators.
* You must use GitHub Actions to implement the CI/CD pipeline.

## Complexity Levels

1. Deploy a web API without DB access and demonstrate that the CI/CD pipeline works and the API is reachable via e.g. REST Client.
2. Add a simple Angular frontend.
  * Step 1: Without API access
  * Step 2: With API access
3. Add DB access to the API
4. Secure DB connection management (e.g. Managed Identity, Key Vault, etc.)
5. Automate the creation of the Azure infrastructure (e.g. using Bicep, Open Tofu, etc.)
