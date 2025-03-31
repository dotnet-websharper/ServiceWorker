namespace WebSharper.ServiceWorker

open WebSharper
open WebSharper.JavaScript
open WebSharper.InterfaceGenerator

module Definition =

    module Enum =
        let ServiceWorkerState =
            Pattern.EnumStrings "ServiceWorkerState" [
                "parsed"
                "installing"
                "installed"
                "activating"
                "activated"
                "redundant"
            ]

        let ServiceWorkerType =
            Pattern.EnumStrings "ServiceWorkerType" [
                "classic"
                "module"
            ]

        let ClientFrameType =
            Pattern.EnumStrings "ClientFrameType" [
                "auxiliary"
                "top-level"
                "nested"
                "none"
            ]

        let ClientType =
            Pattern.EnumStrings "ClientType" [
                "window"
                "worker"
                "sharedworker"
            ]

        let UpdateViaCache =
            Pattern.EnumStrings "UpdateViaCache" [
                "imports"
                "all"
                "none"
            ]

        let RunningStatus = 
            Pattern.EnumStrings "RunningStatus" [
                "running" 
                "not-running"
            ]

        let SourceEnum =
            Pattern.EnumStrings "SourceType" [
                "cache"
                "fetch-event"
                "network"
                "race-network-and-fetch-handler"
            ]

        let WorkerType =
            Pattern.EnumStrings "WorkerType" [
                "classic"
                "module"
            ]

        let ClientMatchAllType =
            Pattern.EnumStrings "ClientMatchAllType" [
                "window"
                "worker"
                "sharedworker"
                "all"
            ]

    let ExtendableEvent =
        Class "ExtendableEvent"
        |=> Inherits T<Dom.Event>
        |+> Static [
            Constructor (T<string>?``type`` * !?T<obj>?options)
        ]
        |+> Instance [
            "waitUntil" => T<Promise<unit>>?promise ^-> T<unit>
        ]

    let Client =
        Class "Client"
        |+> Instance [
            "frameType" =? Enum.ClientFrameType.Type
            "id" =? T<string>
            "type" =? Enum.ClientType.Type
            "url" =? T<string>

            "postMessage" => T<obj>?message * !?T<obj>?transfer ^-> T<unit>
        ]

    let ClientsMatchAllOptions = 
        Pattern.Config "ClientsMatchAllOptions" {
            Required = []
            Optional = [
                "includeUncontrolled", T<bool>
                "type", Enum.ClientMatchAllType.Type
            ]
        }

    let WindowClient =
        Class "WindowClient"
        |=> Inherits Client
        |+> Instance [
            "ancestorOrigins" =? !| T<string>  
            "focused" =? T<bool>  
            "visibilityState" =? T<string> 

            "focus" => T<unit> ^-> T<Promise<_>>[TSelf]  
            "navigate" => T<string>?url ^-> T<Promise<_>>[TSelf]  
        ]


    let Clients =
        Class "Clients"
        |+> Instance [
            "claim" => T<unit> ^-> T<Promise<unit>>  
            "get" => T<string>?id ^-> T<Promise<_>>[Client]  
            "matchAll" => !?ClientsMatchAllOptions?options ^-> T<Promise<_>>[!? Client]  // Returns all controlled clients
            "openWindow" => T<string>?url ^-> T<Promise<_>>[!?WindowClient]  // Opens a new window or tab
        ]

    let ServiceWorker =
        Class "ServiceWorker"
        |=> Inherits T<Dom.EventTarget>
        |+> Instance [
            "scriptURL" =? T<string>
            "state" =? Enum.ServiceWorkerState.Type

            "postMessage" => T<obj>?message * !?T<obj>?transfer ^-> T<unit>

            "onerror" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnError instead"
            "onerror" =@ T<Dom.Event> ^-> T<unit>
            |> WithSourceName "OnError"
            "onstatechange" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnStateChange instead"
            "onstatechange" =@ T<Dom.Event> ^-> T<unit>
            |> WithSourceName "OnStateChange"
        ]

    let SourceType = Client.Type + ServiceWorker.Type + T<MessagePort>

    let ExtendableMessageEventOptions = 
        Pattern.Config "ExtendableMessageEventOptions" {
            Required = []
            Optional = [
                "data", T<obj>
                "origin", T<string>
                "lastEventId", T<string>
                "source", SourceType
                "port", !|T<MessagePort>
            ]
        }

    let ExtendableMessageEvent =
        Class "ExtendableMessageEvent"
        |=> Inherits ExtendableEvent
        |+> Static [
            Constructor (T<string>?``type`` * !?ExtendableMessageEventOptions?options)
        ]
        |+> Instance [
            "data" =? T<obj>
            "lastEventId" =? T<string>
            "origin" =? T<string>
            "ports" =? !| T<MessagePort>
            "source" =? SourceType
        ]

    let FetchEventOptions = 
        Pattern.Config "FetchEventOptions" {
            Required = []
            Optional = [
                "request", T<Request>
                "preloadResponse", T<Promise<obj>>
                "clientId", T<string> + Client
                "isReload", T<bool>
                "replacesClientId", T<string>
                "resultingClientId", T<string>
                "handled", T<Promise<bool>>
            ]
        }

    let FetchEvent =
        Class "FetchEvent"
        |=> Inherits ExtendableEvent
        |+> Static [
            Constructor (T<string>?``type`` * FetchEventOptions?options)
        ]
        |+> Instance [
            "clientId" =? T<string>
            "handled" =? T<Promise<bool>>
            "isReload" =? T<bool>  
            "preloadResponse" =? T<Promise<obj>>
            "replacesClientId" =? T<string>
            "request" =? T<Request>
            "resultingClientId" =? T<string>

            "respondWith" => (T<Promise<Response>> + T<Response>)?response ^-> T<unit>
        ]

    let Condition =
        Pattern.Config "Condition" {
            Required = []
            Optional = [
                "not", TSelf
                "or", !| TSelf
                "requestMethod", T<string>
                "requestMode", T<string>
                "requestDestination", T<string>
                "runningStatus", Enum.RunningStatus.Type 
                "urlPattern", T<obj> // Represents a URLPattern instance
            ]
        }

    let Source =
        Pattern.Config "Source" {
            Required = []
            Optional = [
                "cacheName", T<string>
            ]
        } |=> Inherits Enum.SourceEnum.Type

    let RouterRules =
        Pattern.Config "RouterRules" {
            Required = []
            Optional = [
                "condition", Condition.Type
                "source", Source.Type
            ]
        }

    let InstallEvent =
        Class "InstallEvent"
        |=> Inherits ExtendableEvent
        |+> Static [
            Constructor (T<string>?``type`` * !?T<obj>?options)
        ]
        |+> Instance [
            "addRoutes" => RouterRules?routerRules ^-> T<Promise<unit>> 
        ]

    let GetNotificationsOptions =
        Pattern.Config "GetNotificationsOptions" {
            Required = []
            Optional = [
                "tag", T<string>
            ]
        }

    let NavigationPreloadState =
        Pattern.Config "NavigationPreloadState" {
            Required = [
                "enabled", T<bool>
                "headerValue", T<string>
            ]
            Optional = []
        }

    let NavigationPreloadManager =
        Class "NavigationPreloadManager"
        |+> Instance [
            "disable" => T<unit> ^-> T<Promise<_>>[T<unit>]
            "enable" => T<unit> ^-> T<Promise<_>>[T<unit>]
            "getState" => T<unit> ^-> T<Promise<_>>[NavigationPreloadState]
            "setHeaderValue" => T<string>?value ^-> T<Promise<_>>[T<unit>]
        ]

    let NotificationAction =
        Pattern.Config "NotificationAction" {
            Required = [
                "action", T<string>
                "title", T<string>
            ]
            Optional = [
                "icon", T<string>
            ]
        }

    let NotificationOptions =
        Pattern.Config "NotificationOptions" {
            Required = []
            Optional = [
                "actions", !| NotificationAction.Type
                "badge", T<string>
                "body", T<string>
                "data", T<obj>
                "dir", T<string>
                "icon", T<string>
                "image", T<string>
                "lang", T<string>
                "renotify", T<bool>
                "requireInteraction", T<bool>
                "silent", T<bool>
                "tag", T<string>
                "timestamp", T<int>
                "vibrate", !| T<int>
            ]
        }

    let ServiceWorkerRegistration =
        Class "ServiceWorkerRegistration"
        |=> Inherits T<Dom.EventTarget>
        |+> Instance [
            "active" =? !? ServiceWorker.Type
            "backgroundFetch" =? T<obj> // BackgroundFetchManager object
            "cookies" =? T<obj> // CookieStoreManager object
            "index" =? T<obj> // ContentIndex object
            "installing" =? !? ServiceWorker.Type
            "navigationPreload" =? NavigationPreloadManager.Type
            "paymentManager" =? T<obj> // A PaymentManager object
            "periodicSync" =? T<obj> // A PeriodicSyncManager object
            "pushManger" =? T<obj> // A PushManager object
            "scope" =? T<string>
            "sync" =? T<obj> // A SyncManager object
            "updateViaCache" =? Enum.UpdateViaCache
            "waiting" =? !? ServiceWorker.Type
            
            "getNotifications" => !?GetNotificationsOptions?options ^-> T<Promise<obj>> // A Promise of Notification objects.
            "showNotification" => T<string>?title * !?NotificationOptions?options ^-> T<Promise<unit>>
            "update" => T<unit> ^-> T<Promise<_>>[TSelf]
            "unregister" => T<unit> ^-> T<Promise<bool>>

            "onupdatefound" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnUpdateFound instead"
            "onupdatefound" =@ T<Dom.Event> ^-> T<unit>
            |> WithSourceName "OnUpdateFound"
        ]

    let ServiceWorkerRegistrationOptions =
        Pattern.Config "ServiceWorkerRegistrationOptions" {
            Required = []
            Optional = [
                "scope", T<string>
                "type", Enum.WorkerType.Type
                "updateViaCache", Enum.UpdateViaCache.Type
            ]
        }

    let ServiceWorkerContainer =
        Class "ServiceWorkerContainer"
        |=> Inherits T<Dom.EventTarget>
        |+> Instance [
            "controller" =? !? ServiceWorker.Type
            "ready" =? T<Promise<_>>[ServiceWorkerRegistration]

            "register" => T<string>?scriptURL * !?ServiceWorkerRegistrationOptions?options ^-> T<Promise<_>>[ServiceWorkerRegistration.Type]
            "getRegistration" => !?T<string>?clientURL ^-> T<Promise<_>>[!?ServiceWorkerRegistration.Type]
            "getRegistrations" => T<unit> ^-> T<Promise<_>>[!| ServiceWorkerRegistration.Type]
            "startMessages" => T<unit> ^-> T<unit>

            "oncontrollerchange" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnControllerChange instead"
            "oncontrollerchange" =@ T<Dom.Event> ^-> T<unit>      
            |> WithSourceName "OnControllerChange"
            "onerror" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnError instead"
            "onerror" =@ T<Dom.Event> ^-> T<unit>
            |> WithSourceName "OnError"
            "onmessage" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnMessage instead"
            "onmessage" =@ T<MessageEvent> ^-> T<unit>
            |> WithSourceName "OnMessage"
            "onmessageerror" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnMessageError instead"
            "onmessageerror" =@ T<MessageEvent> ^-> T<unit>
            |> WithSourceName "OnMessageError"
        ]

    let ServiceWorkerGlobalScope =
        Class "ServiceWorkerGlobalScope"
        |=> Inherits T<WorkerGlobalScope> 
        |+> Instance [
            "clients" =? Client
            "cookieStore" =? T<obj>  // A CookieStore object instance.
            "registration" =? ServiceWorkerRegistration 
            "serviceWorker" =? ServiceWorker

            "skipWaiting" => T<unit> ^-> T<Promise<unit>>  

            "onactivate" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnActivate instead"
            "onactivate" =@ ExtendableEvent ^-> T<unit>
            |> WithSourceName "OnActivate"
            "onbackgroundfetchabort" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnBackgroundFetchAbort instead"
            "onbackgroundfetchabort" =@ ExtendableEvent ^-> T<unit> // BackgroundFetchEvent
            |> WithSourceName "OnBackgroundFetchAbort"
            "onbackgroundfetchclick" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnBackgroundFetchClick instead"
            "onbackgroundfetchclick" =@ ExtendableEvent ^-> T<unit> // BackgroundFetchEvent
            |> WithSourceName "OnBackgroundFetchClick"
            "onbackgroundfetchfail" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnBackgroundFetchFail instead"
            "onbackgroundfetchfail" =@ ExtendableEvent ^-> T<unit> // BackgroundFetchUpdateUIEvent
            |> WithSourceName "OnBackgroundFetchFail"
            "onbackgroundfetchsuccess" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnBackgroundFetchSuccess instead"
            "onbackgroundfetchsuccess" =@ ExtendableEvent ^-> T<unit> // BackgroundFetchUpdateUIEvent
            |> WithSourceName "OnBackgroundFetchSuccess"
            "oncanmakepayment" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnCanMakePayment instead"
            "oncanmakepayment" =@ ExtendableEvent ^-> T<unit> // CanMakePaymentEvent
            |> WithSourceName "OnCanMakePayment"
            "oncontentdelete" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnContentDelete instead"
            "oncontentdelete" =@ ExtendableEvent ^-> T<unit> // ContentIndexEvent
            |> WithSourceName "OnContentDelete"
            "oncookiechange" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnCookieChange instead"
            "oncookiechange" =@ ExtendableEvent ^-> T<unit> // ExtendableCookieChangeEvent
            |> WithSourceName "OnCookieChange"
            "oninstall" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnInstall instead"
            "oninstall" =@ ExtendableEvent ^-> T<unit>
            |> WithSourceName "OnInstall"
            "onmessage" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnMessage instead"
            "onmessage" =@ ExtendableMessageEvent ^-> T<unit>
            |> WithSourceName "OnMessage"
            "onmessageerror" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnMessageError instead"
            "onmessageerror" =@ ExtendableMessageEvent ^-> T<unit>
            |> WithSourceName "OnMessageError"
            "onnotificationclick" =@ T<unit> ^-> T<unit> 
            |> ObsoleteWithMessage "Use OnNotificatiOnClick instead"
            "onnotificationclick" =@ ExtendableEvent ^-> T<unit> // NotificationEvent
            |> WithSourceName "OnNotificatiOnClick"
            "onnotificationclose" =@ T<unit> ^-> T<unit> 
            |> ObsoleteWithMessage "Use OnNotificationClose instead"
            "onnotificationclose" =@ ExtendableEvent ^-> T<unit> // NotificationEvent
            |> WithSourceName "OnNotificationClose"
            "onpaymentrequest" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnPaymentRequest instead"
            "onpaymentrequest" =@ ExtendableEvent ^-> T<unit> // PaymentRequestEvent
            |> WithSourceName "OnPaymentRequest"
            "onperiodicsync" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnPeriodicSync instead"
            "onperiodicsync" =@ ExtendableEvent ^-> T<unit> // PeriodicSyncEvent
            |> WithSourceName "OnPeriodicSync"
            "onpush" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnPush instead"
            "onpush" =@ ExtendableEvent ^-> T<unit> // PushEvent
            |> WithSourceName "OnPush"
            "onpushsubscriptionchange" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnPushSubscriptionChange instead"
            "onpushsubscriptionchange" =@ T<Dom.Event> ^-> T<unit> 
            |> WithSourceName "OnPushSubscriptionChange"
            "onfetch" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnFetch instead"
            "onfetch" =@ FetchEvent ^-> T<unit>
            |> WithSourceName "OnFetch"
            "onsync" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnSync instead"
            "onsync" =@ ExtendableEvent ^-> T<unit> // SyncEvent
            |> WithSourceName "OnSync"
        ]

    let CacheQueryOptions =
        Pattern.Config "CacheQueryOptions" {
            Required = []
            Optional = [
                "ignoreSearch", T<bool>
                "ignoreMethod", T<bool>
                "ignoreVary", T<bool>
                "cacheName", T<string>
            ]
        }

    let cacheRequest = T<Request> + T<string>

    let Cache =
        Class "Cache"
        |+> Instance [
            "add" => cacheRequest?request ^-> T<Promise<unit>>
            "addAll" => (!|cacheRequest)?requests ^-> T<Promise<unit>>
            "delete" => cacheRequest?request * !?CacheQueryOptions?options ^-> T<Promise<bool>>
            "keys" => !?cacheRequest?request * !?CacheQueryOptions?options ^-> T<Promise<_>>[!| T<Request>]
            "match" => cacheRequest?request * !?CacheQueryOptions?options ^-> T<Promise<_>>[T<Response>]
            "matchAll" => !?cacheRequest?request * !?CacheQueryOptions?options ^-> T<Promise<_>>[!| T<Response>]
            "put" => cacheRequest?request * T<Response>?response ^-> T<Promise<unit>>
        ]

    let CacheStorage =
        Class "CacheStorage"
        |+> Instance [
            "open" => T<string>?cacheName ^-> T<Promise<_>>[Cache.Type]
            "has" => T<string>?cacheName ^-> T<Promise<bool>>
            "delete" => T<string>?cacheName ^-> T<Promise<bool>>
            "keys" => T<unit> ^-> T<Promise<_>>[!| T<string>]
            "match" => cacheRequest?request * !?CacheQueryOptions?options ^-> T<Promise<_>>[T<Response>]
        ]

    let Assembly =
        Assembly [
            Namespace "WebSharper.ServiceWorker" [
                Enum.ServiceWorkerState
                Enum.ServiceWorkerType
                Enum.ClientMatchAllType
                Enum.WorkerType
                Enum.SourceEnum
                Enum.RunningStatus
                Enum.UpdateViaCache
                Enum.ClientType
                Enum.ClientFrameType

                ServiceWorker
                ServiceWorkerRegistration
                ServiceWorkerContainer
                ServiceWorkerGlobalScope
                CacheStorage
                Cache
                CacheQueryOptions
                ServiceWorkerRegistrationOptions
                NotificationOptions
                NotificationAction
                NavigationPreloadManager
                NavigationPreloadState
                GetNotificationsOptions
                InstallEvent
                RouterRules
                Source
                Condition
                FetchEvent
                FetchEventOptions
                ExtendableMessageEvent
                ExtendableMessageEventOptions
                Clients
                ClientsMatchAllOptions
                Client
                ExtendableEvent
                WindowClient
            ]
        ]

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly =
            Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()
