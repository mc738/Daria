module FDOM.Core.DSL

open FDOM.Core.Common

[<RequireQualifiedAccess>]
module Style =
    
    let custom items =  DOM.Style.Custom (Map.ofList items)
    
    let references items = DOM.Style.Ref items
    
    let none = DOM.Style.Default
    
let h1 indexed style content = DOM.createH1 indexed style content

let h2 indexed style content = DOM.createH2 indexed style content

let h3 indexed style content = DOM.createH3 indexed style content

let h4 indexed style content = DOM.createH4 indexed style content

let h5 indexed style content = DOM.createH5 indexed style content

let h6 indexed style content = DOM.createH6 indexed style content

let p style content = DOM.createParagraph style content

let ol style items = DOM.createUnorderedList style items

let ul style items = DOM.createOrderedList style items

let li style content = DOM.createListItem style content

let section name title style content =
    DOM.createSection style title name content

let text content = DOM.createText content

let span style content = DOM.createSpan style content

let spans items = DOM.createSpans items

let document name title style sections =
    DOM.createDocument style title name sections