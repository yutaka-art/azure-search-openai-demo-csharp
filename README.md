---
page_type: sample
languages:
- azdeveloper
- csharp
- html
- bicep
products:
- ai-services
- azure-blob-storage
- azure-container-apps
- azure-cognitive-search
- azure-openai
- aspnet-core
- blazor
- defender-for-cloud
- azure-monitor
- dotnet-maui
urlFragment: azure-search-openai-demo-csharp
name: ChatGPT + Enterprise data (csharp)
description: A csharp sample app that chats with your data using OpenAI and AI Search.
---
<!-- YAML front-matter schema: https://review.learn.microsoft.com/en-us/help/contribute/samples/process/onboarding?branch=main#supported-metadata-fields-for-readmemd -->

## 目次

- [機能](#機能)
- [アプリケーションアーキテクチャ](#アプリケーションアーキテクチャ)
- [Azureアカウント要件](#アカウント要件)
- [はじめに](#はじめに)
  - [コスト見積もり](#コスト見積もり)
  - [プロジェクトのセットアップ](#プロジェクトのセットアップ)
    - [GitHub Codespaces](#github-codespaces)
    - [VS Code Dev Containers](#vs-code-remote-containers)
    - [ローカル環境](#ローカル環境)
  - [デプロイ](#デプロイ)
    - [ゼロからのデプロイ](#ゼロからのデプロイ)
    - [既存のAzureリソースを使用したデプロイ](#既存のリソースを使用)
    - [再デプロイ](#リポジトリのローカルクローンのデプロイまたは再デプロイ)
    - [App Spacesのデプロイ](#app-spacesを使用したリポジトリのデプロイ)
    - [ローカルでの実行](#ローカルでの実行)
    - [環境の共有](#環境の共有)
    - [リソースのクリーンアップ](#リソースのクリーンアップ)
  - [アプリの使用](#アプリの使用)
- [オプション機能の有効化](#オプション機能の有効化)
  - [Application Insightsの有効化](#オプション機能の有効化)
  - [認証の有効化](#認証の有効化)
  - [GPT-4Vサポートの有効化](#gpt-4vサポートの有効化)
- [ガイダンス](#ガイダンス) 
  - [本番環境化](#本番環境化)
  - [リソース](#リソース)
  - [FAQ](#faq)

# Azure OpenAIとAzure AI Searchを使用したChatGPT + エンタープライズデータ (.NET)

![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/Azure-Samples/azure-search-openai-demo-csharp/dotnet-build.yml?label=BUILD%20%26%20TEST&logo=github&style=for-the-badge)
[![Open in GitHub - Codespaces](https://img.shields.io/static/v1?style=for-the-badge&label=GitHub+Codespaces&message=Open&color=brightgreen&logo=github)](https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=624102171&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fdevcontainer.json&location=WestUs2)
[![Open in Remote - Containers](https://img.shields.io/static/v1?style=for-the-badge&label=Remote%20-%20Containers&message=Open&color=blue&logo=visualstudiocode)](https://vscode.dev/redirect?url=vscode://ms-vscode-remote.remote-containers/cloneInVolume?url=https://github.com/azure-samples/azure-search-openai-demo-csharp)

このサンプルは、Retrieval Augmented Generation（RAG）パターンを使用して、独自のデータに対してChatGPTのような体験を作成するためのいくつかのアプローチを示しています。Azure OpenAI Serviceを使用してChatGPTモデル（`gpt-4o-mini`）にアクセスし、Azure AI Searchをデータのインデックス作成と検索に使用します。

このリポジトリにはサンプルデータが含まれているため、すぐにエンドツーエンドで試すことができます。このサンプルアプリケーションでは、Contoso Electronicsという架空の会社を使用しており、従業員が福利厚生、社内ポリシー、職務内容、役割について質問できる体験を提供します。

![RAG Architecture](docs/appcomponents.png)

このアプリケーションの構築方法の詳細については、以下を参照してください：

- [Transform your business with smart .NET apps powered by Azure and ChatGPT blog post](https://aka.ms/build-dotnet-ai-blog)
- [Build Intelligent Apps with .NET and Azure - Build Session](https://build.microsoft.com/sessions/f8f953f3-2e58-4535-92ae-5cb30ef2b9b0)

ご意見をお聞かせください！インテリジェントアプリの構築に興味がある、または現在構築中ですか？数分でアンケートにご協力ください。

[**アンケートに回答する**](https://aka.ms/dotnet-build-oai-survey)

## 機能

- 音声チャット、チャット、Q&Aインターフェース
- 引用、ソースコンテンツの追跡などにより、ユーザーが応答の信頼性を評価するのに役立つさまざまなオプションを提供
- データ準備、プロンプト構築、モデル（ChatGPT）とリトリーバー（Azure AI Search）間のインタラクションのオーケストレーションに関する可能なアプローチを表示
- UX内で直接設定を調整し、オプションを試すことができる

![Chat screen](docs/chatscreen.png)

## アプリケーションアーキテクチャ

- **ユーザーインタフェース** - アプリケーションのインタフェースは [Blazor WebAssembly](https://learn.microsoft.com/aspnet/core/blazor/) で行います。 このインターフェースは、ユーザーのクエリを受け入れ、リクエストをアプリケーションバックエンドにルーティングし、生成された応答を表示します。
- **バックエンド** - アプリケーションバックエンドは[ASP.NET Core Minimal API](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/overview). バックエンドはBlazor静的Webアプリケーションをホストし、異なるサービス間のインタラクションをオーケストレーションします。このアプリケーションで使用されるサービスには以下が含まれます:
   - [**Azure AI Search**](https://learn.microsoft.com/azure/search/search-what-is-azure-search) – Azure Storage Accountに保存されたデータからドキュメントをインデックス化します。これにより、 [vector search](https://learn.microsoft.com/azure/search/search-get-started-vector) 機能を使用してドキュメントを検索できるようになります。 
   - [**Azure OpenAI Service**](https://learn.microsoft.com/azure/ai-services/openai/overview) – 応答を生成するための大規模言語モデルを提供します。 [Semantic Kernel](https://learn.microsoft.com/semantic-kernel/whatissk) は、Azure OpenAI Serviceと組み合わせて、より複雑なAIワークフローをオーケストレーションするために使用されます。

## はじめに

### アカウント要件

この例をデプロイして実行するには、以下が必要です：

- **Azureアカウント** - Azureが初めての場合は、[無料のAzureアカウント](https://aka.ms/free)を取得すると、開始するための無料のAzureクレジットが得られます。
- **Azureアカウントの権限** - Azureアカウントには、[ユーザーアクセス管理者](https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#user-access-administrator)や[所有者](https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#owner)などの`Microsoft.Authorization/roleAssignments/write`権限が必要です。

> [!WARNING]<br>
> デフォルトでは、このサンプルは月額コストが発生するAzure Container AppとAzure AI Searchリソース、およびドキュメントページごとにコストが発生するAzure AI Document Intelligenceリソースを作成します。infraフォルダー配下のパラメータファイルを変更することで、これらのコストを回避するために無料版に切り替えることができます（ただし、考慮すべき制限があります。たとえば、サブスクリプションごとに無料のAzure AI Searchリソースは1つまで、無料のAzure AI Document Intelligenceリソースは各ドキュメントの最初の2ページのみを分析します）。

### コスト見積もり

価格は地域と使用量によって異なるため、正確なコストを予測することはできません。ただし、以下のリソースについては[Azure料金計算ツール](https://azure.microsoft.com/pricing/calculator/)を試すことができます：

- [**Azure Container Apps**](https://azure.microsoft.com/pricing/details/container-apps/)。環境タイプ：消費のみ。このソリューションは、特定のハードウェア要件がないため、消費プランを使用します。
- [**Azure OpenAI Service**](https://azure.microsoft.com/pricing/details/cognitive-services/openai-service/)。標準層、GPTおよびAdaモデル。使用された1Kトークンあたりの価格で、質問ごとに少なくとも1Kトークンが使用されます。
- [**Azure AI Document Intelligence**](https://azure.microsoft.com/pricing/details/ai-document-intelligence/)。事前構築レイアウトを使用するSO（標準）層。ドキュメントページあたりの価格で、サンプルドキュメントは合計261ページです。
- [**Azure AI Search**](https://azure.microsoft.com/pricing/details/search/) Basic層、1レプリカ、セマンティック検索の無料レベル。時間あたりの価格。
- [**Azure Blob Storage**](https://azure.microsoft.com/pricing/details/storage/blobs/)。ZRS（ゾーン冗長ストレージ）を使用した標準層。ストレージおよび読み取り操作あたりの価格。
- [**Azure Monitor**](https://azure.microsoft.com/pricing/details/monitor/)。従量課金制層。取り込まれたデータに基づくコスト。

コストを削減するために、さまざまなサービスの無料SKUに切り替えることができますが、これらのSKUには制限があります。詳細については、この[最小コストでのデプロイガイド](./docs/deploy_lowcost.md)を参照してください。

⚠️ 不要なコストを避けるため、アプリが使用されなくなった場合は、ポータルでリソースグループを削除するか、`azd down`を実行してアプリを削除することを忘れないでください。

### プロジェクトのセットアップ

このプロジェクトをセットアップするにはいくつかのオプションがあります。最も簡単な方法はGitHub Codespacesで、すべてのツールを自動的にセットアップしますが、必要に応じて[ローカル](#ローカル環境)でセットアップすることもできます。

#### GitHub Codespaces

GitHub Codespacesを使用してこのリポジトリを仮想的に実行できます。これにより、ブラウザでWebベースのVS Codeが開きます：

[![Open in GitHub - Codespaces](https://img.shields.io/static/v1?style=for-the-badge&label=GitHub+Codespaces&message=Open&color=brightgreen&logo=github)](https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=624102171&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fdevcontainer.json&location=WestUs2)

#### VS Code Remote Containers

関連するオプションとして、VS Code Remote Containersがあります。これは[Dev Containers](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)拡張機能を使用して、ローカルのVS Codeでプロジェクトを開きます：

[![Open in Remote - Containers](https://img.shields.io/static/v1?style=for-the-badge&label=Remote%20-%20Containers&message=Open&color=blue&logo=visualstudiocode)](https://vscode.dev/redirect?url=vscode://ms-vscode-remote.remote-containers/cloneInVolume?url=https://github.com/azure-samples/azure-search-openai-demo-csharp)

#### ローカル環境

以下の前提条件をインストールします:

- [Azure Developer CLI](https://aka.ms/azure-dev/install)
- [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Git](https://git-scm.com/downloads)
- [Powershell 7+ (pwsh)](https://github.com/powershell/powershell) - Windowsユーザーのみ。
  
   > **重要**: 
   > PowerShellコマンドから`pwsh.exe`を実行できることを確認してください。失敗する場合は、PowerShellをアップグレードする必要がある可能性があります。

- [Docker](https://www.docker.com/products/docker-desktop/)

   > **重要**:
   > `azd`のプロビジョニング/デプロイコマンドを実行する前に、Dockerが実行されていることを確認してください。

その後、以下のコマンドを実行して、プロジェクトをローカル環境に取得します:

   1. `azd auth login`を実行
   1. リポジトリをクローンするか、`azd init -t azure-search-openai-demo-csharp`を実行
   1. `azd env new azure-search-openai-demo-csharp`を実行

### デプロイ

#### ゼロからのデプロイ

[📺 ライブストリーム: CodeSpacesでゼロからデプロイ](https://youtu.be/TORUsRNimM0)
[📺 ライブストリーム: Windows 11でゼロからデプロイ](https://youtu.be/wgSnkxGH2Sk?si=C4zAbLKhK3LoAS43)

> **重要**:
> `azd`のプロビジョニング/デプロイコマンドを実行する前に、Dockerが実行されていることを確認してください。

既存のAzureサービスがなく、新規デプロイから始めたい場合は、以下のコマンドを実行します。

1. `azd up`を実行 - これによりAzureリソースがプロビジョニングされ、`./data`フォルダーにあるファイルに基づいて検索インデックスを構築することを含め、このサンプルがそれらのリソースにデプロイされます。
   - デプロイ先の地域については、このサンプルで使用されるモデルを現在サポートしている地域は**米国東部**です。最新の地域とモデルのリストについては、[こちら](https://learn.microsoft.com/azure/cognitive-services/openai/concepts/models)を確認してください。
   - 複数のAzureサブスクリプションにアクセスできる場合は、使用するサブスクリプションを選択するよう求められます。1つのサブスクリプションのみにアクセスできる場合は、自動的に選択されます。

   > **注意**:
   > このアプリケーションは`gpt-4o-mini`モデルを使用します。デプロイする地域を選択する際は、その地域で利用可能であることを確認してください(例: EastUS)。詳細については、[Azure OpenAI Serviceドキュメント](https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/models#standard-deployment-model-availability)を参照してください。

1. アプリケーションが正常にデプロイされると、コンソールにURLが出力されます。そのURLをクリックして、ブラウザでアプリケーションを操作します。

次のように表示されます:

!['azd upの実行結果'](assets/endpoint.png)

> [!NOTE]:
> アプリケーションが完全にデプロイされるまでに数分かかる場合があります。
> アプリケーションがデプロイされた後、ベクトルデータベースに追加するドキュメントを処理するのにも数分かかります。

#### 既存のリソースを使用

Azureで使用したい既存のリソースがある場合は、以下の`azd`環境変数を設定することで、それらを使用するように`azd`を構成できます:

1. `azd env set AZURE_OPENAI_SERVICE {既存のOpenAIサービスの名前}`を実行
1. `azd env set AZURE_OPENAI_RESOURCE_GROUP {OpenAIサービスがプロビジョニングされている既存のリソースグループの名前}`を実行
1. `azd env set AZURE_OPENAI_CHATGPT_DEPLOYMENT {既存のChatGPTデプロイの名前}`を実行。ChatGPTデプロイがデフォルトの'chat'でない場合のみ必要です。
1. `azd env set AZURE_OPENAI_EMBEDDING_DEPLOYMENT {既存の埋め込みモデルデプロイの名前}`を実行。埋め込みモデルデプロイがデフォルトの`embedding`でない場合のみ必要です。
1. `azd up`を実行

> [!NOTE]<br>
> 既存のSearchおよびStorageアカウントも使用できます。既存のリソースを構成するために`azd env set`に渡す環境変数のリストについては、`./infra/main.parameters.json`を参照してください。

#### リポジトリのローカルクローンのデプロイまたは再デプロイ

> [!IMPORTANT]<br>
> `azd`のプロビジョニング/デプロイコマンドを実行する前に、Dockerが実行されていることを確認してください。

- `azd up`を実行

#### App Spacesを使用したリポジトリのデプロイ

> [!NOTE]<br>
> リポジトリにAZDでサポートされているbicepファイルがあることを確認し、手動でトリガーできる(初期デプロイ用)、またはコード変更時(最新の変更で自動的に再デプロイ)にトリガーできる初期のGitHub Actions Workflowファイルを追加してください。
> リポジトリをApp Spacesと互換性のあるものにするには、AZDが適切なタグを持つ既存のリソースグループにデプロイできるように、メインのbicepとメインパラメータファイルに変更を加える必要があります。

1. App SpacesがGitHub Actions workflowファイルで設定した環境変数から値を読み取るために、メインパラメータファイルにAZURE_RESOURCE_GROUPを追加します。
   ```json
   "resourceGroupName": {
      "value": "${AZURE_RESOURCE_GROUP}"
    }
   ```
2. App SpacesがGitHub Actions workflowファイルで設定した環境変数から値を読み取るために、メインパラメータファイルにAZURE_TAGSを追加します。
   ```json
   "tags": {
      "value": "${AZURE_TAGS}"
    }
   ```
3. App Spacesによって設定される値を読み取るために、メインbicepファイルでリソースグループとタグのサポートを追加します。
   ```bicep
   param resourceGroupName string = ''
   param tags string = ''
   ```
4. Azdによって設定されたデフォルトのタグとApp Spacesによって設定されたタグを結合します。メインbicepファイルの_tagsの初期化_を次のように置き換えます -
   ````bicep
   var baseTags = { 'azd-env-name': environmentName }
   var updatedTags = union(empty(tags) ? {} : base64ToJson(tags), baseTags)
   bicepファイルで作成されたリソースグループに"tags"を割り当てる際には必ず"updatedTags"を使用し、他のリソースには"tags"の代わりに"baseTags"を使用するように更新してください。例 -
   ```json
   resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
     name: !empty(resourceGroupName) ? resourceGroupName : '${abbrs.resourcesResourceGroups}${environmentName}'
     location: location
     tags: updatedTags
   }
   ````

#### ローカルでの実行

> [!IMPORTANT]<br>
> `azd`のプロビジョニング/デプロイコマンドを実行する前に、Dockerが実行されていることを確認してください。

1. `azd auth login`を実行
1. アプリケーションがデプロイされた後、環境変数`AZURE_KEY_VAULT_ENDPOINT`を設定します。値は_.azure/YOUR-ENVIRONMENT-NAME/.env_ファイルまたはAzureポータルで確認できます。
1. 以下の.NET CLIコマンドを実行して、ASP.NET Core Minimal APIサーバー(クライアントホスト)を起動します:

   ```dotnetcli
   dotnet run --project ./app/backend/MinimalApi.csproj --urls=http://localhost:7181/
   ```

<http://localhost:7181>に移動して、アプリをテストします。

#### .NET MAUIクライアントでローカルで実行

このサンプルには.NET MAUIクライアントが含まれており、Windows/macOSデスクトップまたはAndroidおよびiOSデバイスで実行できるアプリとして体験をパッケージ化しています。ここのMAUIクライアントはBlazorハイブリッドを使用して実装されており、Webサイトのフロントエンドとコードのほとんどを共有できます。

1. _app/app-maui.sln_を開いて、MAUIクライアントを含むソリューションを開きます

1. _app/maui-blazor/MauiProgram.cs_を編集し、`client.BaseAddress`をバックエンドのURLで更新します。

   Azureで実行している場合は、上記の手順からサービスバックエンドのURLを使用します。ローカルで実行している場合は、<http://localhost:7181>を使用します。

1. **MauiBlazor**をスタートアッププロジェクトとして設定し、アプリを実行します

#### 環境の共有

デプロイされた既存の環境へのアクセスを他の人に付与したい場合は、以下を実行します。

1. [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)をインストールします
1. `azd init -t azure-search-openai-demo-csharp`を実行します
1. `azd env refresh -e {環境名}`を実行します - このコマンドを実行するには、azd環境名、サブスクリプションID、および場所が必要です - これらの値は`./azure/{env name}/.env`ファイルで確認できます。これにより、ローカルでアプリを実行するために必要なすべての設定がazd環境の.envファイルに入力されます。
1. `pwsh ./scripts/roles.ps1`を実行します - これにより、ユーザーがローカルでアプリを実行できるように必要なすべてのロールが割り当てられます。サブスクリプションでロールを作成する権限がない場合は、このスクリプトを代わりに実行する必要があるかもしれません。azd .envファイルまたはアクティブシェルで`AZURE_PRINCIPAL_ID`環境変数をそのユーザーのAzure IDに設定してください。Azure IDは`az account show`で取得できます。

#### リソースのクリーンアップ

`azd down`を実行します

### アプリの使用

- Azureで: `azd`によってデプロイされたAzure Container Appに移動します。URLは`azd`完了時に(「Endpoint」として)出力されるか、Azureポータルで確認できます。
- ローカルで実行する場合は、クライアントアプリは<http://localhost:7181>、Open APIサーバーページは<http://localhost:7181/swagger>に移動します。

Webアプリ内で:

- **Voice Chat**ページで、音声設定ダイアログを選択し、テキスト読み上げの設定を構成します。
  - Blazor Clippyとやり取りするためにメッセージを入力するか、Speakトグルボタンを選択して音声入力を使用できます。
- **Chat**コンテキストでさまざまなトピックを試してください。チャットでは、フォローアップ質問、明確化、回答の簡素化または詳細化の依頼などを試してください。
- 引用とソースを探索します
- 「settings」アイコンをクリックして、さまざまなオプションを試したり、プロンプトを調整したりできます。

## オプション機能の有効化

### Application Insightsの有効化

Application Insightsを有効にして各リクエストのトレースとエラーのログ記録を行うには、`azd up`を実行する前に`AZURE_USE_APPLICATION_INSIGHTS`変数をtrueに設定します。

1. `azd env set AZURE_USE_APPLICATION_INSIGHTS true`を実行
1. `azd up`を実行

パフォーマンスデータを確認するには、リソースグループ内のApplication Insightsリソースに移動し、「Investigate -> Performance」ブレードをクリックし、任意のHTTPリクエストに移動してタイミングデータを確認します。
チャットリクエストのパフォーマンスを検査するには、「Drill into Samples」ボタンを使用して、任意のチャットリクエストに対して行われたすべてのAPI呼び出しのエンドツーエンドのトレースを確認します:

![トレースのスクリーンショット](docs/transaction-tracing.png)

例外やサーバーエラーを確認するには、「Investigate -> Failures」ブレードに移動し、フィルタリングツールを使用して特定の例外を見つけます。右側にPythonスタックトレースが表示されます。

### 認証の有効化

デフォルトでは、デプロイされたAzureコンテナアプリには認証やアクセス制限が有効になっておらず、コンテナアプリへのルーティング可能なネットワークアクセスを持つ誰でも、インデックス化されたデータとチャットできます。[Add container app authentication](https://learn.microsoft.com/azure/container-apps/authentication-azure-active-directory)チュートリアルに従ってAzure Active Directoryへの認証を要求し、デプロイされたコンテナアプリに対して設定することができます。

特定のユーザーまたはグループへのアクセスを制限するには、エンタープライズアプリケーションの「Assignment Required?」オプションを変更し、ユーザー/グループにアクセスを割り当てることで、[Restrict your Azure AD app to a set of users](https://learn.microsoft.com/azure/active-directory/develop/howto-restrict-your-app-to-a-set-of-users)の手順に従うことができます。明示的なアクセスが付与されていないユーザーは、エラーメッセージ-AADSTS50105: Your administrator has configured the application <app_name> to block users unless they are specifically granted ('assigned') access to the application.-を受け取ります。

### ビジョン（マルチモーダル）サポートの有効化

`GPT-4o-mini`を使用すると、テキストと画像の両方をソースコンテンツとして提供することで、充実したRetrieval Augmented Generationをサポートすることが可能です。ビジョンサポートを有効にするには、プロビジョニング時に`USE_VISION`を有効にし、`GPT-4o`または`GPT-4o-mini`モデルを使用する必要があります。

> [!NOTE]
> 以前にアプリケーションをデプロイ済みの場合、GPT-4oサポートを有効にした後にサポート資料の再インデックスとアプリケーションの再デプロイが必要です。これは、GPT-4oサポートを有効にすると検索インデックスに新しいフィールドを追加する必要があるためです。

Azure OpenAI ServiceでGPT-4Vサポートを有効にするには、以下のコマンドを実行します:

```bash
azd env set USE_VISION true
azd env set USE_AOAI true
azd env set AZURE_OPENAI_CHATGPT_MODEL_NAME gpt-4o-mini
azd env set AZURE_OPENAI_RESOURCE_LOCATION eastus # gptモデルの利用可能性の詳細を確認してください。
azd up
```

OpenAIでビジョンサポートを有効にするには、以下のコマンドを実行します:

```bash
azd env set USE_VISION true
azd env set USE_AOAI false
azd env set OPENAI_CHATGPT_DEPLOYMENT gpt-4o
azd up
```

以前にデプロイしたリソースをクリーンアップするには、以下のコマンドを実行します:

```bash
azd down --purge
azd env set AZD_PREPDOCS_RAN false # 新しいフィールドでドキュメントが再インデックス化されることを保証します。
```

## ガイダンス

以下のヒント以外にも、[docs](./docs)フォルダーに詳細なドキュメントがあります。

### 本番環境化

このサンプルは、独自の本番アプリケーションの出発点として設計されていますが、本番環境にデプロイする前に、セキュリティとパフォーマンスの徹底的なレビューを行う必要があります。考慮すべき事項は次のとおりです:

- **OpenAI容量**: デフォルトのTPM(tokens per minute)は30Kに設定されています。これは約30の会話/分に相当します(ユーザーメッセージ/レスポンスあたり1Kを想定)。`infra/main.bicep`の`chatGptDeploymentCapacity`および`embeddingDeploymentCapacity`パラメータをアカウントの最大容量に変更することで、容量を増やすことができます。[Azure OpenAI studio](https://oai.azure.com/)のQuotasタブを表示して、使用可能な容量を確認することもできます。

- **Azure Storage**: デフォルトのストレージアカウントは`Standard_LRS` SKUを使用しています。回復性を向上させるため、本番環境のデプロイには`Standard_ZRS`の使用をお勧めします。これは`infra/main.bicep`の`storage`モジュール配下の`sku`プロパティを使用して指定できます。

- **Azure AI Search**: 検索サービス容量の超過に関するエラーが表示される場合は、`infra/core/search/search-services.bicep`で`replicaCount`を変更するか、Azureポータルから手動でスケーリングすることで、レプリカ数を増やすと役立つ場合があります。

- **Azure Container Apps**: デフォルトでは、このアプリケーションは0.5 CPUコアと1GBのメモリを持つコンテナをデプロイします。最小レプリカは1、最大は10です。このアプリの場合、`infra/core/host/container-app.bicep`ファイルで`containerCpuCoreCount`、`containerMaxReplicas`、`containerMemory`、`containerMinReplicas`などの値を必要に応じて設定できます。自動スケーリングルールまたはスケジュールされたスケーリングルールを使用でき、負荷に基づいて[最大/最小](https://learn.microsoft.com/azure/container-apps/scale-app)をスケールアップできます。

- **認証**: デフォルトでは、デプロイされたアプリは公開アクセス可能です。認証されたユーザーへのアクセスを制限することをお勧めします。認証を有効にする方法については、上記の[認証の有効化](#認証の有効化)を参照してください。

- **ネットワーキング**: 仮想ネットワーク内にデプロイすることをお勧めします。アプリが社内専用の場合は、プライベートDNSゾーンを使用してください。また、ファイアウォールやその他の保護形式としてAzure API Management(APIM)の使用も検討してください。詳細については、[Azure OpenAI Landing Zone reference architecture](https://techcommunity.microsoft.com/t5/azure-architecture-blog/azure-openai-landing-zone-reference-architecture/ba-p/3882102)をお読みください。

- **負荷テスト**: 予想されるユーザー数に対して負荷テストを実行することをお勧めします。

- **Webクライアントへのストリーミングトークン**: クライアントとサーバー間の双方向通信を可能にするSignalRライブラリの使用を検討してください。トークンをクライアントにストリーミングダウンし、クライアントからシグナルを受信するための単一の永続的接続を簡単に再利用できます。SignalRは、トークンのキャンセルなど、AIチャットボットの一般的な機能の実装を簡素化します。[Azure SignalRサービス](https://learn.microsoft.com/azure/azure-signalr/signalr-overview)を活用すると、100万接続まで簡単にスケールできます。さらに、Azure SignalRはサーバーレスデプロイに適しており、サーバー管理を気にすることなくコスト効率の高いソリューションを提供します。コードサンプルについては、[signalrブランチ](https://github.com/Azure-Samples/azure-search-openai-demo-csharp/tree/signalr)を参照してください。

### リソース

- [Revolutionize your Enterprise Data with ChatGPT: Next-gen Apps w/ Azure OpenAI and Azure AI Search](https://aka.ms/entgptsearchblog)
- [Azure AI Search](https://learn.microsoft.com/azure/search/search-what-is-azure-search)
- [Azure OpenAI Service](https://learn.microsoft.com/azure/cognitive-services/openai/overview)
- [`Azure.AI.OpenAI` NuGet package](https://www.nuget.org/packages/Azure.AI.OpenAI)
- [Original Blazor App](https://github.com/IEvangelist/blazor-azure-openai)


> [!NOTE]<br>
> このデモで使用されているPDFドキュメントには、言語モデル(Azure OpenAI Service)を使用して生成された情報が含まれています。これらのドキュメントに含まれる情報はデモンストレーション目的のみであり、Microsoftの意見や信念を反映するものではありません。Microsoftは、このドキュメントに含まれる情報の完全性、正確性、信頼性、適合性、または可用性について、明示的または黙示的を問わず、いかなる表明または保証も行いません。すべての権利はMicrosoftに帰属します。


### FAQ

**_質問_**: Azure AI Searchが大きなドキュメントの検索をサポートしているのに、なぜPDFをチャンクに分割する必要があるのですか？

**_回答_**: チャンキングにより、トークン制限のためにOpenAIに送信する情報量を制限できます。コンテンツを分割することで、OpenAIに挿入できる潜在的なテキストチャンクを簡単に見つけることができます。使用するチャンキング手法は、テキストのスライディングウィンドウを活用しており、1つのチャンクを終了する文が次のチャンクを開始するようになっています。これにより、テキストのコンテキストを失う可能性を減らすことができます。
