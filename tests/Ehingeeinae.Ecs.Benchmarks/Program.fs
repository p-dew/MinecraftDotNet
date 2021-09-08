module Ehingeeinae.Ecs.Benchmarks.Program

open System
open BenchmarkDotNet.Configs
open BenchmarkDotNet.Filters
open BenchmarkDotNet.Running

type private Marker = class end

[<EntryPoint>]
let main args =
    let summary =
        BenchmarkRunner.Run(
            typeof<Marker>.Assembly,
            ManualConfig.Create(DefaultConfig.Instance)
                .AddFilter(NameFilter(fun name -> match name with "Querying1" -> false | _ -> true))
        )
    0
