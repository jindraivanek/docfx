// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Based partially on dotnet-proj-info by Enrico Sada (https://github.com/enricosada/dotnet-proj-info).

namespace Microsoft.DocAsCode.Metadata.ManagedReference.FSharp
open System
open System.Collections.Concurrent
open System.IO
open Dotnet.ProjInfo.Inspect


/// F# project information necessary for F# compiler invocation.
type internal FSharpProjectInfo = {
    /// List of source files.
    Srcs: string list
    /// List of assembly references.
    Refs: string list
    /// Arguments (without source files) for the F# compiler.
    Args: string list
}


/// F# project information necessary for F# compiler invocation.
module internal FSharpProjectInfo =

    /// Gets F# project information from a F# project file.
    let fromProjectFile projPath msbuildProps =
        let projPath = Path.GetFullPath projPath
        let projDir = Path.GetDirectoryName projPath

        Log.verbose "Parsing F# project %s" projPath

        let (opts, projectRefs, _) = ProjectCracker.GetProjectOptionsFromProjectFile projPath
        let allFscArgs = opts.OtherOptions |> Seq.toList
        
        // split compiler arguments into sources and options
        let fscArgs, srcs =
            allFscArgs
            |> List.filter (fun o -> not (o.StartsWith("--preferreduilang")))
            |> List.partition (fun arg -> arg.StartsWith("-"))

        // resolve source paths
        let srcs = srcs |> List.map (fun src -> Path.Combine(projDir, src))

        Log.verbose "F# project %s parsed" projPath
        Log.debug "sources:            %A" srcs
        Log.debug "references:         %A" projectRefs
        Log.debug "compiler arguments: %A" fscArgs

        {Srcs=srcs; Refs=projectRefs; Args=fscArgs}
