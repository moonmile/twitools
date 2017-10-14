﻿open System
open System.Threading.Tasks
open System.Net.Http
open System.Text;


/// 同期型のHttpClientを定義する
type HttpClient with
    member x.GetString(url:string) =
        let res = async {
            let httpClient = new HttpClient()
            let! response = httpClient.GetAsync(url) |> Async.AwaitTask
            response.EnsureSuccessStatusCode () |> ignore
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            return content
        }
        Async.RunSynchronously( res )

// https://apps.twitter.com/ より取得
let ApiKey = ""
let ApiSecret = ""
let AccessToken = ""
let AccessTokenSecret = ""

// フォロワー数を取得
let getfollowers (name:string) =
    let tokens = CoreTweet.Tokens.Create(ApiKey, ApiSecret, AccessToken, AccessTokenSecret)
    let mutable count = 1
    let mutable cursor = 0L
    let mutable loop = true

    while loop do
        let ids = tokens.Followers.IdsAsync(name) |> Async.AwaitTask |> Async.RunSynchronously
        for id in ids do
            Console.WriteLine( String.Format("{0}\t{1}", count, id ))
            count <- count + 1
        // Console.WriteLine("cursor is " + ids.NextCursor.ToString())
        if ids.NextCursor = 0L then
            loop <- false
        else
            cursor <- ids.NextCursor
            // 70秒待つ
            Task.Delay(1000*70) |> Async.AwaitTask |> Async.RunSynchronously

// id から name に変換する
let id2name (fname:string) =
    let lines = System.IO.File.ReadAllLines( fname )
    let hc = new HttpClient()
    let doc = HtmlAgilityPack.HtmlDocument()
    let mutable count = 1
    for line in lines do
        let s = line.Split('\t')
        let id = s.[1]

        let url = "https://twitter.com/intent/user?user_id=" + id.ToString()
        let html = hc.GetStringAsync(url) |> Async.AwaitTask |> Async.RunSynchronously
        doc.LoadHtml( html )
        let name = doc.DocumentNode.SelectSingleNode("//img[@class='photo']").ParentNode.InnerText.Trim();
        let sname = doc.DocumentNode.SelectSingleNode("//span[@class='nickname']").InnerText;
        
        Console.WriteLine(String.Format("{0}\t{1}\t{2}\t{3}", count, id, sname, name ))
        Task.Delay(500) |> Async.AwaitTask |> Async.RunSynchronously
        count <- count + 1


[<EntryPoint>]
let main argv =
    if argv.Length = 0 then
        Console.WriteLine("twi follower <screenname>")
        Console.WriteLine("twi id2name <ids file>")
    else
        match argv.[0] with
        | "follower" -> getfollowers argv.[1]
        | "id2name"  -> id2name argv.[1]
        | _ -> 
            Console.WriteLine("twi floller <screenname>")
            Console.WriteLine("twi id2name <ids file>")
    0 // return an integer exit code