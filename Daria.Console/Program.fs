// Learn more about F# at http://fsharp.org

open System
open System.IO
open FDOM.Core.Common
open FDOM.Rendering
open FDOM.Core.DSL

let defaultStyle = DOM.Style.Default

let testDoc =
    document "test-doc" None defaultStyle [
        section "section-1" None defaultStyle [
            h1 true (Style.references [ "main-header"; "header" ]) [
                text "Hello, World!"
            ]
            p Style.none [
                spans [
                    span Style.none "Hello, this is span 1"
                    span Style.none "Hello, this is span 2"
                ]
            ]
        ]
    ]

[<EntryPoint>]
let main argv =
    
    let doc = testDoc
    
    let stylesheets = [
        "css/main.css"
        "css/style.css"
    ]
    
    let scripts = [
        "js/index.js"
        "js/main.js"
    ]
    
    let layout = {
        Head = "<section id=\"sidebar\"><small>Main</small><section><main><small>Main</small>"
        Foot = "</main>" } : Html.Layout
    
    let doc = Html.render layout stylesheets scripts doc
    
    File.WriteAllText("/home/max/Projects/Daria/test.html", doc)
    
    printfn "The doc: %A" doc
    0 // return an integer exit code
