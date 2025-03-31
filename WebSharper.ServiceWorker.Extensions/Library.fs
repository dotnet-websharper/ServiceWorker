namespace WebSharper.ServiceWorker

open WebSharper
open WebSharper.JavaScript

[<JavaScript; AutoOpen>]
module Extensions =

    type Navigator with
        [<Inline "$this.serviceWorker">]
        member this.ServiceWorker with get(): ServiceWorkerContainer = X<ServiceWorkerContainer>

    type WorkerNavigator with
        [<Inline "$this.serviceWorker">]
        member this.ServiceWorker with get(): ServiceWorkerContainer = X<ServiceWorkerContainer>

    type Window with
        [<Inline "$this.caches">]
        member this.Caches with get(): CacheStorage = X<CacheStorage>

    type WorkerGlobalScope with
        [<Inline "$this.caches">]
        member this.Caches with get(): CacheStorage = X<CacheStorage>
