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
  YearsCoded : int
  Indentation: string
  Salary : float
}

type SalarySummary = {
  YearsCoded: string;
  SalaryTabs: float;
  SalarySpaces: float;
  SalaryBoth: float
}

type SalarySummary2 = {
  YearsCoded: string;
  SalaryTabs: float;
  SalarySpaces: float;
  SalaryBoth: float  
  SalaryTabsOpenSource: float;
  SalarySpacesOpenSource: float;
  SalaryBothOpenSource: float
  SalaryTabsNoOpenSource: float;
  SalarySpacesNoOpenSource: float;
  SalaryBothNoOpenSource: float
}


let run() =
  let allData =
    surveyData.Rows
    |> Seq.filter (fun row -> 
        row.Professional = "Professional developer"
        && not (Double.IsNaN(row.Salary))
        && row.YearsCodedJob <> "NA"
        && row.Country = "United States")
    |> Seq.map (fun row -> row.YearsCodedJob, row.TabsSpaces, row.Salary, row.ProgramHobby)
    |> Array.ofSeq
    |> Array.map (fun (yearsCoded, tabsSpaces, salary, hobby) ->
        let numberMatches = Regex.Matches(yearsCoded, @"\d+")
        let n = 
          if numberMatches.Count = 0 then 
            0
          else
            numberMatches.[numberMatches.Count-1].Value |> int
        let openSource = hobby.ToLower().Contains("open source") || hobby.ToLower().Contains("both")
        n, tabsSpaces, salary, openSource)
  
  let openSourceData = 
    allData
    |> Array.groupBy (fun (n, tabsSpaces, salary, openSource) ->
        if n <= 5 then "<= 5 years" else
        if n <= 10 then "6-10 years" else
        if n <= 15 then "11-15 years" else
        "15+"
        )
    |> Array.sortBy (fun (experience, values) ->
       match experience with
       | "<= 5 years" -> 5
       | "6-10 years" -> 10
       | "11-15 years" -> 15
       | "15+" -> 20)
    |> Array.map (fun (experience, values) ->
        { YearsCoded = experience
          SalaryTabs = values |> Array.filter (fun (_, ts, salary,_) -> ts = "Tabs") |> Array.averageBy (fun (_, _, x, _) -> x)
          SalarySpaces = values |> Array.filter (fun (_, ts, salary,_) -> ts = "Spaces") |> Array.averageBy (fun (_, _, x,_) -> x)
          SalaryBoth = values |> Array.filter (fun (_, ts, salary,_) -> ts = "Both") |> Array.averageBy (fun (_, _, x,_) -> x)
          SalaryTabsOpenSource = 
            values |> Array.filter (fun (_, ts, salary, os) -> os && ts = "Tabs") |> Array.averageBy (fun (_, _, x, _) -> x)
          SalarySpacesOpenSource = 
            values |> Array.filter (fun (_, ts, salary, os) -> os && ts = "Spaces") |> Array.averageBy (fun (_, _, x, _) -> x)
          SalaryBothOpenSource = 
            values |> Array.filter (fun (_, ts, salary, os) -> os && ts = "Both") |> Array.averageBy (fun (_, _, x, _) -> x)
          SalaryTabsNoOpenSource = 
            values |> Array.filter (fun (_, ts, salary, os) -> not os && ts = "Tabs") |> Array.averageBy (fun (_, _, x, _) -> x)
          SalarySpacesNoOpenSource = 
            values |> Array.filter (fun (_, ts, salary, os) -> not os && ts = "Spaces") |> Array.averageBy (fun (_, _, x, _) -> x)
          SalaryBothNoOpenSource = 
            values |> Array.filter (fun (_, ts, salary, os) -> not os && ts = "Both") |> Array.averageBy (fun (_, _, x, _) -> x)
    })
  openSourceData     
      
// http://fable.io/repl2/#?code=LYewJgrgNgpgBAEQIYBckDUCWBnCSqYBeqmIAdgFAUgAOMZcAygJ7YozDV0MBiSARrAB0ABRAB3GACcRSAMYBrLvTh9BMIQGEQUmMt4Dh23UIBS2AJJl2U2vtWGNF4DR0p7AUSjAcAC0-efkIASjDy7rQqasKhcr5IUijY9tEasfGJ2KK2NMmgkLBwwXAAvA7qQgASMFB0Ulmh4RT50PAipUXZtMmRvNDASCn9SEI85CgAgpLYIMB6FAD0C3AAKr7w9GCumNZw27s0tgBumGA7AOZwKOtwYKhIcEhkYHC+EnAgAGbsDGAgcJIAU8UFd-jRoFA4Jh3LAQQBtAA8ABlodJ8AA+AC6cAAqsEkR0AES+FAoGgALiWAAYhDS6eSABxUhkAVkJFFhcERKJsGOxCAmKwmAH0eB4VppKsLGB5NMKLAA5FYeYLoCYEsoAZioSzgAFkAPIIDxIigoZh0fXgGqlChwe1wADeiHu5I+-AAVgBuO0O+0ADTdbCkFw+NBQpEofvtAE0gygQ2RLrQI+RfX7GG9xAbuIwQBApHJ4G7+CAQJCU5GAL5Uc2WvXYS4ldNwAA+qhgKDiyDQLfbPE7cVC2FcZGw8C+RRguCgKARIE9ABo4DAAB5kdF9phZnP0PMFosfT5wUvlsOpyiLZY4kQC5UcztwT6D3w9pCaYAvAAUAEpbX7OXOTs3zgX8OkOWYcHgR0WwAzsAEI4F0EdyHHDoBy7XwhGfTDcXxLlMVgh1YUQj0ZgYMpkNHcchDI8hfyI+1dBQAsGDoqMHRrP0PzAIQvhEWwfDQoCUBAhjowdL8DQUOB0XRDtMOHaiYB-RjQI8KRbCkWT5Iwodp2U1SHxBHZoVAv9mz9Z0QLKUtVy5VtW0xL04H9DoFXIGAXJjdzPJczMJF3Mh90LeAyg8sh4CrZccO7e4eIY4y4AgGg7nYUDgEbckG3OP8v3yGpsutKALJbAZcMyy5xGhfw-X7F8QIAWk3CT7QKqAYoa+LP0SuqFP0lCx3gZC4GatTyriJDpwBGq1PbaTbnuUb5LUqy4HambrhdNAOjSh5orgHihDITy5rgDStPWxtltWh0ACFbHEccpCEORUPLDRpEuyrbra4rlyOk7IqoPqAuzXN81Cj5sGWuA1OdDbqq2sGgpCw8yhAbADsB06rzgdALA8AB1PG9R0eAdk+f4BHzEF0gSJJHhBEkyUpBZdDiBmsh0c4FiSgYpHOHYriQk84EhSyHU0DIUCEPUEiFhhnRQWgOhQFzS1JWYOn4FyQ3OEkOikFzYG+DpIRrKhOV0Z5pEYfAEmYFFIulhn1uKxadrOEdUEmyXORoBIkDmGwYbKMrfd8d2wBqIQUYhg94CRqOtzzOYrikCB4Gark4EJbAHakZgVgEbBUchotCRc-PC+YRhA6LMuE9Cqu84LqBHbukBrnLxPCUI0HZngT58DQ5q4Tb2uS-4bAPN7lvq-bx36-kac5+byvF9rrvrnXvcK5gfutwi7P5InmuO6L6fsFbi-l4b6db6Xoud98I-fWCIQziOAiuRbAgXYyz-q1daCthYsiEHAAATDSOAEC4A0jUq7RIQgianC2gANipIgkByDZbVEwAbEELJsFCCQTLIQIE9otmxBPNSq4kCrhwFyTQDNpyYCeJQ+4ABpGAzA87MDCPUbQMcwD9wImpZgjDmFwloQPVqKtywRhoL-WRalYBAWeKo+REk5BsOwBwsgABxEMLw4QiDjgmEACgYDIGwBkKQSB+GEk1HATU4i1HqJ2PAOEv1Dr6MMUIFYFp4BkxOirYGIDuIBM4W+Xh-DA6OJDtILIcIqQ6KiRYxgVibF5wAMQMkKQAFjAAydkUSHRZJyTANBYAtpQLIRU+0rDEjsM4QqYO8BCTX3Ka1ORXjIrAKaS09gBjOHBPrOQbup0mn+NaWMsgXC0DxL2EHZJ9QhBwgAIwZJAVU2wuTCQFKgXogAnGIvx+zrE1PQVHBpfiRltMWR09OhIV6N16RJfpICAE+IeTExZEzQlTIiXoWZjyFlLKQCsxJnTQ6bKgbs1qVzDl5M+CPEhVJPmZMsQcm5dS7mNOGQCoQLyumv2xX6OR9oaESRbC2T+39c4WM0B3bAs9Ol50ECARQhI4C0L8X6O6EAtaLP4CKiJudBWtWFaKoQBoyCsswIoUCnwIAMGFLDb2gdcJfnjvvROcAT4-lUrMkBsqIlaHLDoUCmBjztTjjuDeYUjWeSuOsBgFhsACUwALfhNQ0JepMXwv82JpUSQnp-YMecDRHDRFASE3sEyYHFRePlSKKkWvIEIVNkq6FmplRK7NCqlUqq-GqjVWqcA6smnqp1BqoZ6qHhnLOJrTUFujFmxZ2goA2q-Ha6Osd9XBQPh0NO8AExZ3dSoL1Pq-UrigIG7AwbmChrhh26MkahDRsJNoawIZU3TRVmGFQMw+78vDXALtOai0MHzRu+6t75WKoIGWitcBNU521ZHUCw60bwCbenEei6VImsvUKp9Pa+0DodX+0dZRx1PlHhOj1cBZ0hnnQG+AQbdCrv5euh99ot07oQCAaa4S4BvX3SmkVE7-i9DgGeluF6N0ZoI4RK2j4TgwHEBlYqboyYxxKrcatP7Jb2kZZgH+sihkSSWDbGOUg7oJDwfbFwhR4LwQdSBB1bkHUxjUgpu2tdnYwDwYOyEmntNLW-ZhGhVA8YTBoDQCgAkQDnCSUIYACg3MeeDlCMgZkUppXgNx8QFBWzyV8555OjAIAzzkCGcMkZVXqvWjdOCIJModDvCKMUEopQyjlIqZUqp1RwAAFRwC2dgqkLY4Tvts5NHOphGDbs7FYGwRx8CpYYGBL9oncJ6VfPcP82XItQnOCdXQmJVITei8HIQydGhyBBISGoQlfCNSQM59k83bB+eAEITOZAgA&html=DwCwLgtgNgfAUAAlAUwIYBN4O0iyyoLhgAOAtMgI4CuAlgG4C8A5AMID2AdmMt2QCoBPEsmYIAxlx7cWPAB5gA9OGgBuCSFQAnAM75G1MADMyADmZYcwHeK20SYCVvY6d7OwHNanBDq3jGACJiEh0ALkVFak4SAGsPADpJCEUtNHEwAAEARgA2KIh0VPSwBJJndGoM2i4EiG8EgCsdQJhgRRs7B0tsa1t7R1sXN09vX38gkPDI6LjE5OLUDLJ0dggc-OpCxeXViDKKqrAazjqG5tb2zoH4Pq7B51d3Wi8fPwDgsFJpqJj4pLWinK7HIYGEyB0OQArAl8sDQeCdGdThc2h1+t1EEhrg4nMNnq9xh8phFfnMASk0uJNFowJDsgkAMwJbIFIoAJWQ1O0dORTRaaJxYBgAEgscAoN5Ygg0lAgjowVAISBkPhAkQ0kZJl9QqTxOhOM0klB2NR0EYoNpkBTFKhGqg5IpJQAjHSKZ3UKAQVCKAAMCQA7CzFOJXO7Pd6+aGWghFD0JVKZcg5YEFYIlToVWqNcgtZ9vnqDUbxCazRarTa7Q6nbRXYojFIyKgAO4QtbIRQAFkDCV9IbDDb4LbbeCjrnVcbg7RVGCwwGd7HQggkltcQVQJHIkm4qG8yC0rSxvXQDAQtHQQWT9UzTc36pLqDXgSvtBvG5Il0UJ-ot3di8Ec7KJAsBAA&css=Q