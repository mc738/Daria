// Learn more about F# at http://fsharp.org

open System
open System.IO
open FDOM.Core.Common
open FDOM.Rendering

let defaultStyle = DOM.Style.Default

let testDoc =
    
    let header = DOM.createH1 true defaultStyle [ DOM.createText "Hello, World!" ]
    
    let paragraph = DOM.createParagraph defaultStyle [
        DOM.createSpans [
            DOM.createSpan defaultStyle "Hello, this is span 1..."
            DOM.createSpan defaultStyle "...and this is span 2!"
        ]
    ]
    
    
    let section = DOM.createSection defaultStyle None "section-1" [
        header
        paragraph
    ]
    
    
    DOM.createDocument defaultStyle None "test-doc" [
        section
    ]
    
    


[<EntryPoint>]
let main argv =
    
    let map = [
        "", ""
    ] 
    
    let style1 = DOM.Style.Custom (Map.ofList [
        "font-size", "16px"
        "color", "blue"
        "background-color", "green"
    ])

    let style2 = DOM.Style.Ref [ "block"; "article" ]
    
    let style3 = DOM.Style.Default
   
    let doc = testDoc
    
    let stylesheets = [
        "css/main.css"
        "css/style.css"
    ]
    
    let scripts = [
        "js/index.js"
        "js/main.js"
    ]
    
    let doc = Html.render stylesheets scripts doc
    
    File.WriteAllText("/home/max/Projects/Daria/test.html", doc)
    
    printfn "The doc: %A" doc
    0 // return an integer exit code
