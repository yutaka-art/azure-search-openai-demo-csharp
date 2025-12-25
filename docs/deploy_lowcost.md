# 最小コストでのデプロイ

このAI RAGチャットアプリケーションは、Azure Developer CLIを使用して簡単にデプロイできるように設計されており、`infra`フォルダー内のBicepファイルに従ってインフラストラクチャをプロビジョニングします。これらのファイルは、必要な各Azureリソースを記述し、そのSKU（価格レベル）やその他のパラメーターを設定します。多くのAzureサービスには無料レベルがありますが、このプロジェクトのインフラストラクチャファイルは、無料レベルには制限があることが多いため、デフォルトでは無料レベルに設定されて*いません*。

ただし、アプリケーションのプロトタイピング中にコストを最小限に抑えることが目標である場合は、`azd up`を実行する*前*に以下の手順に従ってください。これらの手順を完了したら、[デプロイ手順](../README.md#deployment)に戻ってください。

[📺 ライブストリーム: 無料アカウントからのデプロイ](https://youtu.be/V1ZLzXU4iiw)

1. Azure Developer CLIを使用してAzureアカウントにログインします：

    ```shell
    azd auth login
    ```

1. 無料リソースグループ用の新しいazd環境を作成します：

    ```shell
    azd env new
    ```

    リソースグループに使用される名前を入力してください。
    これにより、`.azure`フォルダー内に新しいフォルダーが作成され、今後の`azd`の呼び出しのアクティブな環境として設定されます。

1. **Azure AI Document Intelligence**（以前は[Form Recognizer](https://learn.microsoft.com/ja-jp/azure/ai-services/document-intelligence/overview?view=doc-intel-4.0.0)として知られていました）の無料レベルを使用します：

    ```shell
    azd env set AZURE_FORMRECOGNIZER_SERVICE_SKU F0
    ```

1. **Azure AI Search**の無料レベルを使用します：

    ```shell
    azd env set AZURE_SEARCH_SERVICE_SKU free
    azd env set AZURE_SEARCH_SEMANTIC_RANKER disabled
    ```

    制限事項：
    1. すべてのリージョンで1つの無料検索サービスのみが許可されます。
    既に1つある場合は、そのサービスを削除するか、[既存の検索サービス](../README.md#use-existing-resources)を再利用する手順に従ってください。
    2. 無料レベルではセマンティックランカーがサポートされていません。これにより一般的に[検索の関連性が低下](https://techcommunity.microsoft.com/t5/ai-azure-ai-services-blog/azure-ai-search-outperforming-vector-search-with-hybrid/ba-p/3929167)することに注意してください。

1. **Azure Monitor**（Application Insights）をオフにします：

    ```shell
    azd env set AZURE_USE_APPLICATION_INSIGHTS false
    ```

    Application Insightsは既に非常に安価であるため、これをオフにしても節約できるコストに見合わない可能性がありますが、コストを最小限に抑えたい方のためのオプションです。

1. （オプション）Azure OpenAIの代わりに**OpenAI.com**を使用します。

    OpenAIで無料アカウントを作成し、[OpenAIモデルを使用するためのキーをリクエスト](https://platform.openai.com/docs/quickstart/create-and-export-an-api-key)できます。これを取得したら、Azure OpenAI Servicesの使用を無効にし、OpenAI APIを使用できます。

    ```shell
    azd env set USE_AOAI false
    azd env set USE_VISION false
    azd env set OPENAI_CHATGPT_DEPLOYMENT gpt-4o-mini
    azd env set OPENAI_API_KEY <ここにopenai.comのキーを入力>    
    ```

    ***注意：** Azure OpenAIとopenai.comのOpenAIアカウントの両方で、使用されたトークンに基づいてコストが発生しますが、サンプルデータの量に対してコストはかなり低額です（10ドル未満）。*

1. 必要なカスタマイズを行ったら、READMEの手順に従って[`azd up`を実行](../README.md#deploying-from-scratch)してください。可用性の理由から、リージョンとして「eastus」を使用することをお勧めします。