module FDOM.Core.TestDoc

open FDOM.Core.Common
open FDOM.Core.DSL
open FDOM.Core.DSL.Article

let lorem =
    "Lorem ipsum dolor sit amet consectetur, adipisicing elit. Aperiam quaerat est nobis rerum, id explicabo ullam soluta iste vitae distinctio? Deleniti assumenda quis enim harum laborum magnam ratione cupiditate voluptate."

type Test =
    static member Get =
        document
            "test-doc"
            None
            DOM.Style.Default
            [ section
                "section-1"
                  None
                  DOM.Style.Default
                  [ h1 [ text "Hello, World!" ]
                    p [ spans [ span Style.none "Hello, this is span 1.."
                                span Style.none "...Hello, this is span 2" ] ]
                    p [ text lorem ]
                    ol [ li [ text lorem ]
                         li [ text lorem ]
                         li [ text lorem ] ]
                    ul [ li [ text lorem ]
                         li [ text lorem ]
                         li [ text lorem ] ] ] ]
