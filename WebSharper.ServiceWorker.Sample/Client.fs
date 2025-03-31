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

    let status = Var.Create "Checking service worker..."
    
    let registerServiceWorker() =
        try
            let serviceWorker = JS.Window.Navigator.ServiceWorker
            serviceWorker.Register("service-worker.js").Then(fun _ ->
                status := "Service Worker registered!"
            ).Catch(fun error ->
                status := $"Service Worker failed: {error}"
            )
        with error -> promise {
            status := $"Service Workers error: {error.Message}."
        }

    [<SPAEntryPoint>]
    let Main () =
        async {
            do! registerServiceWorker().AsAsync()
        }
        |> Async.Start
        IndexTemplate.Main()
            .status(status.View)
            .Doc()
        |> Doc.RunById "main"
