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
        && row.YearsCodedJob <> "NA")
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

  let summaryData =
    allData
    |> Array.groupBy (fun (n, tabsSpaces, salary, _) ->
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
      })
  
  summaryData
      
// http://fable.io/repl2/#?code=LYewJgrgNgpgBAEQIYBckDUCWBnCSqYBeqmIAdgFAUgAOMZcAygJ7YozDV0MBiSARrAB0ABRAB3GACcRSAMYBrLvTh9BMIQGEQUmMt4Dh23UIBS2AJJl2U2vtWGNF4DR0p7AUSjAcAC0-efkIASjDy7rQqasKhcr5IUijY9tEasfGJ2KK2NMmgkLBwwXAAvA7qQgASMFB0Ulmh4RT50PAipUXZtMmRvNDASCn9SEI85CgAgpLYIMB6FAD0C3AAKr7w9GCumNZw27s0tgBumGA7AOZwKOtwYKhIcEhkYHC+EnAgAGbsDGAgcJIAU8UFd-jRoFA4Jh3LAQQBtAA8ABlodJ8AA+AC6cAAqsEkR0AES+FAoGgALiWAAYhDS6eSABxUhkAVkJFFhcERKJsGOxCAmKwmAH0eB4VppKsLGB5NMKLAA5FYeYLoCYEsoAZioSzgAFkAPIIDxIigoZh0fXgGqlChwe1wADeiHu5I+-AAVgBuO0O+0ADTdbCkFw+NBQpEofvtAE0gygQ2RLrQI+QAL5Uc2WvXYS4lX32gA+qhgKDiyDQBbgxZ4pbioWwrjI2HgXyKMFwUBQCJAnoANHAYAAPMjonXLHEiAXKjmluCfOu+CtITTAF4ACgAlLa-ZzzqXl3Atx1DrMcPBHVXd6WAIRwXSN8gtjq1su+IQLt+4-FczFXh2wneHozAwZQPk2LZCMB5Bbv+9q6CgEBSAw0FRg6GZ+quYBCF8Ii2D4z77igh6wdGDrrgaChwOi6Ilm+DYQTAm5wUeHhSLYUjUbRr71h2jHMbOII7NCR7bvmfrOoeZT8CAQ5coWhaYl6cD+h0CrkDAykxmpGlwGmA6fuW9xYbBglwBANB3OwR7ALm5I5uc27rvkNT2daUBiVWAxfrZlziNC-h+jWi6HgAtGOZH2i5UAGSFxlrqZQV0bxj7NvAD5wOFLHeXE94dgCAUscWlG3PcmW0SxElwNFBXXC6aAdFZDz6XAWFCGQGlFXAbEcdVublZVDoAEK2OILZSEIchPiAwjSL1vmDVF7kDm1HVkPMuroBYHgAOqLMseo6PAOyfP8AggBAILpAkSSPCCJJkpSCy6HEN1ZDo5wLGZAxSOcOxXPecD8HAkLiQ6mgZCgQh6gkf0MM6KC0B0KDKTJpKzB0-DKSG5wkh0UjKbA3wdJCGZmbozzSIw+AJMwKLrRDN2lQ1YP2gQDOQ1y2Ww-9LJCHAABMNJwHzcA0ixjOJEIO2nHVABsVLi5FrWQ1UMCYLjIIsorQgS6rh5NVW2JwnALFDkgQ44Fymg3R2mBPEIy4ANIwMwcCEswYT1NoYAwGAhLG3+yvMBbVtwoHLGIzNEY0L+v4sbA+7PHH4csXItvYPbZAAOIhi8cIiEIjAJiACgwMg2AZFISBu4SmpwJqAfx8r7PwHCi2tRnWdCCsFrwIdHWI+tHc24kdsO87rvu9gNNSMwKwCNg7LK36hfF7YZfuwAxAyu8ACxgAyy8r-aa8l2XMtgHVAu68rEctzsbcj13Du99m5AgEPegn-ao-sJnE97gu1rjPKAtNGA0HkB2Y+J8z4b3gISHeAt04AE5-YdzgaXGAl9r630ivfSKrcuY-07mPABZAe59ytIPTqJC-7jwoZPEBs9mBDU-r4GBK9MGb0QZ8T4SBtZUk4crbh2DZa+EFngsigdlZG1kWZE4MBxA2Xcm6Q6vsPK3BwJAr8rNOhnCOCnFiFNfZSGpmAue9MYCSxBDeG80VHZlSNlQfacAJg0BoBQPCIBzjV2AEIYAChvG+KQMAKEZARIWSsvARR4gKCFlosEvxQh-LXEYBAfg2A5AhnDJGI8nwIAMDCVla8IJbIdGnCKMUEopQyjlIqZUqp1RwAAFRwAAIyKypFWOEBTfjaNQLlcKcBTCMCEC2FAVgbBHHwPkwpolypaMbIMiRPElz3G3OUhJUJzgdV0JiZi2ykmhJSQFRocgQSEhqARXwoUkAePZEc2wIT-FSEKUAA&html=DwCwLgtgNgfAUAAlAUwIYBN4O0iyyoLhgAOAtMgI4CuAlgG4C8A5AMID2AdmMt2QCoBPEsmYIAxlx7cWPAB5gA9OGgBuCSFQAnAM75G1MADMyADmZYcwHeK20SYCVvY6d7OwHNanBDq3jGACJiEh0ALkVFak4SAGsPADpJCEUtNHEwAAEARgA2KIh0VPSwBJJndGoM2i4EiG8EgCsdQJhgRRs7B0tsa1t7R1sXN09vX38gkPDI6LjE5OLUDLJ0dggc-OpCxeXViDKKqrAazjqG5tb2zoH4Pq7B51d3Wi8fPwDgsFJpqJj4pLWinK7HIYGEyB0OQArAl8sDQeCdGdThc2h1+t1EEhrg4nMNnq9xh8phFfnMASk0uJNFowJDsgkAMwJbIFIoAJWQ1O0dORTRaaJxYBgAEgscAoN5Ygg0lAgjowVAISBkPhAkQ0kZJl9QqTxOhOM0klB2NR0EYoNpkBTFKhGqg5IpJQAjHSKZ3UKAQVCKAAMCQA7CzFOJXO7Pd6+aGWghFD0JVKZcg5YEFYIlToVWqNcgtZ9vnqDUbxCazRarTa7Q6nbRXYojFIyKgAO4QtbIRQAFkDCV9IbDDb4LbbeCjrnVcbg7RVGCwwGd7HQggkltcQVQJHIkm4qG8yC0rSxvXQDAQtHQQWT9UzTc36pLqDXgSvtBvG5Il0UJ-ot3di8Ec7KJAsBAA&css=Q
