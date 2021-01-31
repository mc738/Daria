module FDOM.Core.TestDoc

open FDOM.Core.Common
open FDOM.Core.DSL
open FDOM.Core.DSL.General

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
                  [ h1
                      true
                        (Style.references [ "main-header"
                                            "header" ])
                        [ text "Hello, World!" ]
                    p
                        Style.none
                        [ spans [ span Style.none "Hello, this is span 1"
                                  span Style.none "Hello, this is span 2" ] ] ] ]
