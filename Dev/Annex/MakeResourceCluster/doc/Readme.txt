===================
MakeResourceCluster
===================


----
説明

ゲーム用リソースデータファイルを作成する。


----
コマンド

MakeResourceCluster.exe /R SOURCE-DIR R-ROOT-DIR CLUSTER-FILE LICENSE-OUT-FILE

	リソースファイルの作成


MakeResourceCluster.exe /S STORAGE-DIR CLUSTER-FILE

	ストレージファイルの作成
	半角アンダースコアで始まるファイルとディレクトリを除外する。


----
補足

クラスタファイルの読み込み側は <src>\GameCommons\ResourceCluster.cs を想定する。

