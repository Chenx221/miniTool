# minitool

## 项目说明
- minitool1

    这个程序是用来拆 イモコネ—届けたい恋心 的arc封包的，传入游戏的StreamingAssets路径作为参数即可使用，拆出的文件会被放到output文件夹

    .asset文件可搭配Garbro、AssetRipper、AssetStudio之类的工具手动提取，批量提取请检查设备可用运行内存是否足够(

    **温馨提醒:预留至少50G空间存放output

- minitool2

    这个程序是设计来拆シロクロ ～色情症の幼馴染を世話することになった、彼女にナイショで～的arc封包，传入参数只要求arc文件路径

    提取出的assets文件可用[AssetRipper](https://github.com/AssetRipper/AssetRipper)进一步提取

    Assets\prevdata
    需要修改.bytes后缀
    img:PNG, ogg: OGG, src: lua

- minitool3

    这个程序是设计来解密DMM/FANZA游戏 あいりすミスティリア(R)的加密封包，只需要拖拽*.encrypted文件到程序上运行即可，解密的文件会保存在dec文件夹下

    提取出的assets文件... 看上面两条吧

    注: 只简单测试了Windows、Android、Web R18版本与Android 全年龄版本的.encrypted封包文件
    （web版wasm有点感人，我是从Android版开搞的😂）

- minitool4

    这个工具并没有什么用，解密&解压后的vrm会被告知缺少PIXIV_texture_basis进而无法使用💢，所以我懒得发release。等一个解决方法。

- minitool5

    重命名用

- minitool6

    合成拔作岛1/2(抜きゲーみたいな島に住んでる貧乳はどうすりゃいいですか？1/2) 的CG (不支持立绘)，该拆的封包拆出来，然后把路径作为参数运行。成品会出现在output。仅适配重制版——Artemis引擎的版本

    ![](img/minitool6_1.png)

- minitool7

  适用于[恋爱与选举与巧克力 steam](https://store.steampowered.com/app/3027600/_/)多语言版 .dat的解包，直接拖拽.dat到程序上

- minitool8

  适用于[月に寄りそう乙女の作法 2 steam版](https://store.steampowered.com/app/3446150/Tsuki_ni_Yorisou_Otome_no_Sahou_2/?l=japanese) .pac的解包，直接拖拽.pac到程序上；完成后用AssetRipper或AssetStudio处理整个output文件夹；`scenario.s`不感兴趣，没测

- minitool9

    过Triangle PIX GAME STUDIO引擎游戏的序列号验证补丁，直接拖拽游戏主程序到`minitool9.exe`即可，自动备份

    ```
    已测试
    魔法戦士エクストラバースト  ～天使断罪～
    幻聖剣姫セイクリッドアーク
    魔法閃士フェアリーバレット
    魔法戦士EXTRA IGNITION
    光装剣姫アークブレイバー 獣欲に堕ちる勇者
    光装剣姫アークブレイバー 楽園天獄
    光装剣姫アークブレイバー 魔族篇胞
    魔法戦士 After the Final ～黒銀の魔王～
    魔法戦士 CHRISTMAS IGNITION
    魔法戦士 FINAL IGNITION
    魔法戦士　memory of gray
    魔法戦士レムティアナイツ２ -こわれゆく世界の女神たち-
    ```

- minitool10

    拆 贄の匣庭 EXFS 游戏封包

- minitool11

    AdvHD.exe 去guid检查

- minitool12

    Yu-Ris 过序列号(只测试了Zwei Worter HDリマスター 别的以后再说)

- minitool13

    VIRTUAL GIRL @ WORLD'S END解包

