# Azure Developer CLI (azd) のメカニズム
azdは、Azureアプリケーションの開発→ビルド→展開を統一的に管理するCLIツールです。

基本的な流れ
1. azd init - プロジェクトの初期化
2. azd provision - Azure リソースのプロビジョニング（このinfraフォルダが使われる）
3. azd deploy - アプリケーションコードのデプロイ
4. azd up - provision + deploy を一度に実行

# このプロジェクトの展開フロー
```
ソースコード → Docker Image → Azure Container Apps/App Service
     ↓
Infrastructure as Code (Bicep) → Azure リソース作成
```

1. インフラストラクチャ層（infraフォルダ）
```
infra/
├── main.bicep                 # エントリーポイント - 全体の構成を定義
├── main.parameters.json       # パラメータファイル - 環境固有の値
├── abbreviations.json         # Azure リソース名の省略形定義
├── app/                       # アプリケーション層
│   ├── function.bicep         # Azure Functions
│   └── web.bicep             # Web アプリケーション
└── core/                     # 基盤サービス層
    ├── ai/                   # AI サービス（Cognitive Services）
    ├── host/                 # ホスティング（App Service, Container Apps）
    ├── monitor/              # 監視（Application Insights）
    ├── search/               # 検索サービス（Azure Search）
    ├── security/             # セキュリティ（Key Vault, RBAC）
    └── storage/              # ストレージ
```

2. 展開の仕組み
Step 1: リソースプロビジョニング
```
azd provision
```

- main.bicepが実行される
- 必要なAzureリソース（App Service、Cognitive Services、Azure Search等）が作成される
- リソース間の接続設定（接続文字列、シークレット等）が構成される

Step 2: アプリケーションデプロイ
```
azd deploy
```

- ソースコードがDockerイメージにビルドされる
- Container AppsまたはApp Serviceにデプロイされる

このプロジェクトの特徴
このプロジェクトはAzure OpenAI + Azure Searchのデモアプリケーション

- AI層: OpenAIサービス（GPT等）
- 検索層: Azure Cognitive Search
- ホスト層: Container AppsまたはApp Service
- セキュリティ層: Key Vault でシークレット管理
- 監視層: Application Insights


