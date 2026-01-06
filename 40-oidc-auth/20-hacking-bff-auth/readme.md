# Auth Security Sample

**⚠️ THIS SAMPLE CONTAINS HORRIBLE CODE ⚠️ DO NOT WRITE CODE LIKE THIS ⚠️**

The purpose of this sample is to demonstrate lots of security flaws. It generally works, but it's not secure. It is used as a basis for a hacking exercise.

## What this app does

* Login with Microsoft Entra ID
* Uses OpenID Connect in the Backend-for-Frontend (BFF)
* Uses Cookie Authentication to the frontend to not expose tokens to the frontend

## How to install

* Adjust path in all _appsettings.json_ files according to your local setup.
* `npm install` or `bun install` in the _Frontend_ folder.
* `npm install` or `bun install` in the _VanillaWeb_ folder.
* Execute `dotnet user-secrets set "MicrosoftEntraID:ClientSecret" ...` in the _WebApi_ folder. You will get the client secret from your teacher (or you have to create your own app registration in your Entra ID tenant)

## How to run

* `dotnet run` to start web api.
* `npm start` or `bun start` to start frontend.
* `npm start` or `bun start` to start vanilla web.

## What to do

* Start app (see above)
* Open http://localhost:4200
* Login with a school user
* Create a secret
* Create a customer
* Try entering HTML as a customer name (e.g. `This is an <b>important</b> customer`), should work; end users wanted to have this feature

## Exercises

* **Hack this app in every way you can think of!**
* Examples:
    * Use CSRF and XSS to steal the login cookie
    * Use the stolen cookie to retrieve all user claims
    * Add a link to vanilla web with which you can update secrets without the user noticing
    * Login after logout without needing to know the previous user's credentials (e.g. if user works on a public computer in a library)
* Use as much AI as you want
    * However, you must be able to explain and demonstrate your hack to the class **without** using AI
* Document your hack and demonstrate it to the class
    * Describe the underlying problem
    * Propose enhancements to make the app more secure
