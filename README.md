# twitools

Twitter API を呼び出す簡易ツールです。

- twi follower 指定アカウントのID一覧を取得します。
- twi id2name  IDからスクリーン名に変換します。


# 機能

.net core 2.0 で動作するので、linux 上でも動作します。

dotnet run follower <スクリーン名>

指定アカウントのIDを一括取得して、標準出力に表示します。リダイレクトにファイルに落としてください。


dotnet run id2name  <ファイル名>

follower で落としたファイルを指定して、id からスクリーン名に変換します。

# 動作環境


https://apps.twitter.com/ より取得して、キーを入れてください。
```
let ApiKey = ""
let ApiSecret = ""
let AccessToken = ""
let AccessTokenSecret = ""
```

- .NET Core 2.0 をインストール
- cd src/twi
- dotnet build
- dotnet run follower screeen_name

# ライセンス

MIT License

# 履歴

- 2017/10/14 初回公開


