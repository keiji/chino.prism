# Chino.Prism
Xamarin向けの接触確認APIライブラリ「Cappuccino」をXamarin.Forms（Prism）を使って実装したサンプルアプリです。

## Setup

### Edit `Chino.Prism/Constants.cs`
Chino.Prismにはサーバーと連携して診断キーのアップロード、ダウンロードをする機能があります。

連携するサーバーをカスタムするには、上記の実装のサーバーを用意した上で`ApiEndpoint` を書き換えてください。
（デフォルト値 `https://en.keiji.dev/diagnosis_keys` は、動作確認用に用意したサーバーです）

`ClusterId` はサーバーを仮想的に区切るための値で、6桁の数字を指定します。

```
using Newtonsoft.Json;

namespace Sample.Common
{
    [JsonObject]
    public class ServerConfiguration
    {
        [JsonProperty("api_endpoint")]
        public string ApiEndpoint = "https://en.keiji.dev/diagnosis_keys";

        [JsonProperty("cluster_id")]
        public string ClusterId = "212458"; // 6 digits
    }
}
```

また、一度サンプルアプリを起動すると端末内に作成される`config/server_configuration.json`を書き換えると、設定値をオーバーライドできます。

サーバーの実装の詳細は次のURLを参照してください。

 * https://github.com/keiji/en-calibration-server
