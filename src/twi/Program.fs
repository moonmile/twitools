open System
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

type AppSet = {
    ApiKey: string
    ApiSecret: string
    AccessToken: string
    AccessTokenSecret: string
    }

let js = Newtonsoft.Json.JsonSerializer();
let fr = new System.IO.StreamReader(@"app.json")

let appSet = js.Deserialize(fr, typeof<AppSet>) :?> AppSet

// https://apps.twitter.com/ より取得して app.json に保存する
(*
{
   "ApiKey":"",
   "ApiSecret":"",
   "AccessToken":"",
   "AccessTokenSecret":"" 
}

*)
let ApiKey = appSet.ApiKey
let ApiSecret = appSet.ApiSecret
let AccessToken = appSet.AccessToken
let AccessTokenSecret = appSet.AccessTokenSecret

// フォロワー数を取得
let getfollowers (name:string) =
    let tokens = CoreTweet.Tokens.Create(ApiKey, ApiSecret, AccessToken, AccessTokenSecret)
    let mutable count = 1
    let mutable cursor:Nullable<int64> = System.Nullable()
    let mutable loop = true

    while loop do
        let ids = tokens.Followers.IdsAsync(name, cursor) |> Async.AwaitTask |> Async.RunSynchronously
        for id in ids do
            Console.WriteLine( String.Format("{0}\t{1}", count, id ))
            count <- count + 1
        // Console.WriteLine("cursor is " + ids.NextCursor.ToString())
        if ids.NextCursor = 0L then
            loop <- false
        else
            cursor <- new Nullable<int64>( ids.NextCursor )
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

// id からユーザー情報を取得
let id2user ( fname:string ) =

    let lines = System.IO.File.ReadAllLines( fname )
    let tokens = CoreTweet.Tokens.Create(ApiKey, ApiSecret, AccessToken, AccessTokenSecret)
    let mutable count = 1
    for line in lines do
        let s = line.Split('\t')
        let id = int64(s.[1])
        let user = tokens.Users.ShowAsync(id) |> Async.AwaitTask |> Async.RunSynchronously
        Console.WriteLine(
            String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}", 
                count, 
                user.Id, user.ScreenName, user.Name, 
                user.StatusesCount,
                user.FriendsCount, user.FollowersCount, 
                user.CreatedAt, user.IsProtected ))
        Task.Delay(1000) |> Async.AwaitTask |> Async.RunSynchronously
        count <- count + 1
    

[<EntryPoint>]
let main argv =
    if argv.Length = 0 then
        Console.WriteLine("twi follower <screenname>")
        Console.WriteLine("twi id2user <ids file>")
    else
        match argv.[0] with
        | "follower" -> getfollowers argv.[1]
        | "id2user"  -> id2user argv.[1]
        | _ -> 
            Console.WriteLine("twi floller <screenname>")
            Console.WriteLine("twi id2name <ids file>")
    0 // return an integer exit code
