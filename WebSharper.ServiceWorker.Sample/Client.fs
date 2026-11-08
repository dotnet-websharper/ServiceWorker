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
    // The template is loaded from the DOM, so editing index.html and refreshing
    // the browser is enough; a rebuild is only needed when holes change.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let status = Var.Create "Registering the service worker..."

    let registerServiceWorker () =
        try
            let serviceWorker = JS.Window.Navigator.ServiceWorker
            serviceWorker.Register("service-worker.js").Then(fun _ ->
                status := "Service worker registered — this page is cached for offline use."
            ).Catch(fun error ->
                status := $"Service worker registration failed: {error}"
            )
        with error ->
            promise { status := $"Service worker error: {error.Message}." }

    [<SPAEntryPoint>]
    let Main () =
        async {
            do! registerServiceWorker().AsAsync()
        }
        |> Async.Start

        IndexTemplate()
            .status(status.View)
            .Bind()
