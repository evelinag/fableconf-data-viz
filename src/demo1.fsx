#I "../packages/FSharp.Data/lib/net45"
#I "../packages/MathNet.Numerics/lib/net461"
#I "../packages/MathNet.Numerics.FSharp/lib/net45"
#I "../packages/System.Text.RegularExpressions/lib/net45"

#r "FSharp.Data.dll"
#r "MathNet.Numerics.dll"
#r "MathNet.Numerics.FSharp.dll"
#r "System.Text.RegularExpressions.dll"

open System
open System.IO
open FSharp.Data
open System.Text.RegularExpressions

let [<Literal>] datafile = "../data/survey_results_public.csv"
type SurveyData = CsvProvider<datafile>
let surveyData = SurveyData.GetSample()

type Data = {
  Salary : float
  Indentation : string
}

let run() =
    surveyData.Rows
    |> Seq.filter (fun row -> not (Double.IsNaN(row.Salary) ))
    |> Seq.filter (fun row -> row.TabsSpaces <> "NA")
    |> Seq.groupBy (fun row -> row.TabsSpaces)
    |> Seq.map (fun (indentation, rows) -> 
      { Salary = rows |> Seq.averageBy (fun row -> row.Salary); 
        Indentation = indentation })

// http://fable.io/repl2/#?code=LYewJgrgNgpgBAEQIYBckDUCWBnCSqYBeqmIAdgFAUgAOMZcAygJ7YozDV0MBiSARrAB0ABRAB3GACcRSAMYBrLvTh9BMIQGEQUmMt4Dh23UIBS2AJJl2U2vtWGNF4DR0p7AUSjAcAC0-efkIASjDy7rQqasKhcr5IUijY9tEasfGJ2KK2NMmRvNDASBSgkLBwwXAAvA7qQgASMFB0Ulmh4SXg0PAi1RXZtMkUAPTDcAAqvvD0YK6Y1nBzCzS2AG6YYPMA5nAoU3BgqEhwSGRgcL4ScCAAZuwMYCBwks+nKLtPNNBQcJjusO8ANoAHgAMn9pPgAHwAXTgAFVgqC+gAiXwoFA0ABcowADEJ8YSsQAOXHEgCsKIoALgIPBNmhcIQAEFxsyAPo8DzjTT1dmMDyadkWABy4w8wXQzORNQAzFRRnAALIAeQQHlBFBQzDoyvATWqFDgxrgAG9EEcsdd+AArOAAXyo2t1SuwOyqRpNAB9VDAUHFkGhPcafTw-XFQthXGRsPBbhUYLgoChgSBbQAaOAwAAeZChCrG8JELPF1L9cBu4d8gaQmmA5wAFABKQ0muA0rZ+mtwZt9FYgHyxs3Btvtv0AQjguij5CHNTD-t8Qkri4RSNpMJHbYBk5t2HIfWn0djQj35GbW5NuhQECkDDPlDbjrbdbAQluIlsg-gnZQ3Yvo4mg2KoKHAUJQr6i6RseMBNpexoNh4Ui2FIYEQQuEaJjBcFlu88x-D2LYem25rdjU-AgNmtJel6cL2pmK4Bkcr4XrhcAQDQhzsD2wBulirpbC2DalE0-H6lAREjkUq68Ts4h-P4bahlW3YALT5oBxoiVADEqcx9asUpkGYTOMbwNOcDqfBcDSXEU6Js8CnWT6IEHEclkQea2mOXsFpoH0XHHPRcCvkIZDkHomlwD6SEoTZboedZbYAEK2OIsZSEIciziAwjSHFslJSa2mZqF4VkHobG6Gc0jJQkmgZO8gWtia-D1Y1tLWQ1CQoEIADqGy+aS+JdY1DQwJgWzonAAAsuIjVF3WJEI3aBSOcKAtZ2ZINmOC0poPWJpgpwrUcADSMDMHAKLzGA9BoCgpBkCiG2blFzA7XtgIwt91mwJ2Zwbr9UUoCAuWPTQQNvZpciHdgx1kAA4lIGy0nAIhCIwKC2AoMDINgGRSEgV0orKcCUnAP3Q4BbWoYCB2JEdJ01hdJPYPgCTMCiADc6NCDwmBQD8KIAMTEgATLDACcYAvRu61UDS6wwOIPHiVaSriS2mxRqgdnESawRCJsqwbmj1mNEgmxkFsQi+AAjGbgJG2wqEoowHNSFdpznGg-DYMMutyImL1UFF1V3VIdVSEt7zjuO2mnUGbabiMYzMjQNAUJ+IBbETwBCMACg53nSDAL8ZAERxXHwMr4gUF6EEl-nQjyXsjAQP7cgozQj0Hg2NwQAw5dWdu5a8X0JYclyPJ8gKQqiuKkrSnAABUcD2-NuIjoCg8PDgNB674HlwKYjBCLGKBWDYqz4D2e+ESfOuH6uGHVkcLYT43vxbOFugwnBb+zcy6twUu0OQ7wURNEHL4VSSBM5UiAbYUuBcpBDyAA&html=DwCwLgtgNgfAUAAlAUwIYBN4O0iyyoLhgAOAtMgI4CuAlgG4C8A5AMID2AdmMt2QCoBPEsmYIAxlx7cWPAB5gA9OGgBuCSFQAnAM75G1MADMyADmZYcwHeK20SYCVvY6d7OwHNanBDq3jGACJiEh0ALkVFak4SAGsPADpJCEUtNHEwAAEARgA2KIh0VPSwBJJndGoM2i4EiG8EgCsdQJhgRRs7B0tsa1t7R1sXN09vX38gkPDI6LjE5OLUDLJ0dggc-OpCxeXViDKKqrAazjqG5tb2zoH4Pq7B51d3Wi8fPwDgsFJpqJj4pLWinK7HIYGEyB0OQArAl8sDQeCdGdThc2h1+t1EEhrg4nMNnq9xh8phFfnMASk0uJNFowJDsgkAMwJbIFIoAJWQ1O0dORTRaaJxYBgAEgscAoN5Ygg0lAgjowVAISBkPhAkQ0kZJl9QqTxOhOM0klB2NR0EYoNpkBTFKhGqg5IpJQAjHSKZ3UKAQVCKAAMCQA7CzFOJXO7Pd6+aGWghFD0JVKZcg5YEFYIlToVWqNcgtZ9vnqDUbxCazRarTa7Q6nbRXYojFIyKgAO4QtbIRQAFkDCV9IbDDb4LbbeCjrnVcbg7RVGCwwGd7HQggkltcQVQJHIkm4qG8yC0rSxvXQDAQtHQQWT9UzTc36pLqDXgSvtBvG5Il0UJ-ot3di8Ec7KJAsBAA&css=Q