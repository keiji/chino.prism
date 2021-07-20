# Chino.Prism
Xamarin向けの接触確認APIライブラリ「Cappuccino」をXamarin.Forms（Prism）を使って実装したサンプルアプリです。

## Setup

### Edit `Chino.Prism/Constants.cs`
Chino.Prismにはサーバーと連携して診断キーのアップロード、ダウンロードをする機能があります。

連携するサーバーをカスタムするには、上記の実装のサーバーを用意した上で`API_ENDPOINT` を書き換えてください。
（デフォルト値 `https://en.keiji.dev/diagnosis_keys` は、動作確認用に用意したサーバーです）

`CUSTER_ID` はサーバーを仮想的に区切るための値で、6桁の数字として記述します。

```
namespace Chino.Prism
{
    public static class Constants
    {
        public const string API_ENDPOINT = "https://en.keiji.dev/diagnosis_keys";

        public const string CLUSTER_ID = "212458"; // 6 digits
    }
}
```

サーバーの実装は次のURLを参照してください。

 * https://github.com/keiji/en-calibration-server
