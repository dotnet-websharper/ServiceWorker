# WebSharper Service Worker API Binding

This repository provides an F# [WebSharper](https://websharper.com/) binding for the [Service Worker API](https://developer.mozilla.org/en-US/docs/Web/API/Service_Worker_API), enabling seamless integration of background tasks and offline capabilities in WebSharper applications.

## Repository Structure

The repository consists of two main projects:

1. **Binding Project**:

   - Contains the F# WebSharper binding for the Service Worker API.

2. **Sample Project**:
   - Demonstrates how to use the Service Worker API with WebSharper syntax.
   - Includes a GitHub Pages demo: [View Demo](https://dotnet-websharper.github.io/ServiceWorker/).

## Features

- WebSharper bindings for the Service Worker API.
- Background task handling and caching for offline applications.
- Example usage for registering, updating, and communicating with service workers.
- Hosted demo to explore API functionality.

## Installation and Building

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) installed on your machine.
- Node.js and npm (for building web assets).
- WebSharper tools.

### Steps

1. Clone the repository:

   ```bash
   git clone https://github.com/dotnet-websharper/ServiceWorker.git
   cd ServiceWorker
   ```

2. Build the Binding Project:

   ```bash
   dotnet build WebSharper.ServiceWorker/WebSharper.ServiceWorker.fsproj
   ```

3. Build and Run the Sample Project:

   ```bash
   cd WebSharper.ServiceWorker.Sample
   dotnet build
   dotnet run
   ```

4. Open the hosted demo to see the Sample project in action:
   [https://dotnet-websharper.github.io/ServiceWorker/](https://dotnet-websharper.github.io/ServiceWorker/)

## Why Use the Service Worker API

The Service Worker API allows web applications to run background tasks and support offline functionality. Key benefits include:

1. **Offline Support**: Cache assets and serve them when there is no network connectivity.
2. **Background Sync**: Perform tasks like syncing data even when the app is closed.
3. **Push Notifications**: Receive real-time updates without keeping the app open.
4. **Performance Optimization**: Reduce network requests by intercepting and caching them.

**Note:** Ensure your web server supports HTTPS, as service workers require a secure context to function.

Integrating the Service Worker API with WebSharper allows developers to build robust, high-performance web applications in F#.

## How to Use the Service Worker API

### Example Usage

Below is an example of how to use the Service Worker API in a WebSharper project:

```fsharp
namespace WebSharper.ServiceWorker.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Notation
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.ServiceWorker

[<JavaScript>]
module Client =
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    // Variable to store the service worker registration status
    let status = Var.Create "Checking service worker..."

    // Function to register the service worker
    let registerServiceWorker() =
        try
            // Access the Service Worker API
            let serviceWorker = As<Navigator>(JS.Window.Navigator).ServiceWorker

            // Register the service worker script
            serviceWorker.Register("service-worker.js").Then(fun _ ->
                status := "Service Worker registered!"
            ).Catch(fun error ->
                status := $"Service Worker failed: {error}"
            )
        with error -> promise {
            // Handle errors that may occur during service worker registration
            status := $"Service Workers error: {error.Message}."
        }

    [<SPAEntryPoint>]
    let Main () =
        // Initialize the UI template and bind variables to UI elements
        IndexTemplate.Main()
            // Call the service worker registration function on page load
            .pageInit(fun () ->
                async {
                    do! registerServiceWorker().AsAsync()
                }
                |> Async.Start
            )
            // Bind the status variable to display the registration status
            .status(status.View)
            .Doc()
        |> Doc.RunById "main"
```

This example demonstrates how to register a service worker and handle registration status dynamically.

For a complete implementation, refer to the [Sample Project](https://dotnet-websharper.github.io/ServiceWorker/).
