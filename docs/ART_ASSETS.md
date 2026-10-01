# Eggs 美术资源清单

## 美术导出交付（2026-10-01）

- 原文件：`D:/01STUDY/2026gamejam/EGGART/EGGS.psd`，1920×1080，RGB / 8 bit，共 23 个图层；原文件未修改。
- 可复用资源：`Assets/Eggs/Art/`，9 张 PNG，已配套 Unity Sprite 导入 `.meta`。
- 参考资源：`docs/art/reference/`，8 张 PNG。
- 总览：`docs/art/asset_overview.jpg`；原设计合成预览：`docs/art/source_layout.png`。
- 来源记录：`docs/art/manifest.json`，包含全部图层名称、坐标、去重对应关系、导出尺寸和 SHA-256。
- 当前美术需求：**普通小角色（Creature）需要补画；造型不限物种，持弓角色和粉色守护角色仅保留作参考。**

美术导出阶段只整理资源，未更改场景、Prefab、玩法代码或里程碑验收结果。后续 Unit 术语迁移及当前验证状态见 `docs/HANDOFF.md`。

## 可复用资源

以下文件位于 `Assets/Eggs/Art/`。敌人和血条等后续内容仅备好美术，按 GAME_SCOPE 的里程碑接入。

| 文件 | 像素尺寸 | 用途与说明 |
| --- | --- | --- |
| `Environment/ground.png` | 1920×1080 | 单屏地面背景，不含角色、数值或区域 UI。 |
| `Environment/forest_foreground.png` | 1920×1080 | 独立森林遮挡层，中心透明；与地面共用画布和中心点。 |
| `Characters/Enemy/enemy_idle_left.png` | 215×196 | 同一种敌人的静态图。另一敌人图层是精确水平镜像，可用 SpriteRenderer Flip X。 |
| `Props/LoveNest/love_nest_base.png` | 255×255 | 原巢穴圆形底座，保留描边，可与蛋分别摆放。 |
| `Props/Egg/egg_neutral.png` | 94×106 | 中性色普通蛋候选。绿色配色保留在参考区，可替换配色；不表示另一种蛋或玩法。 |
| `UI/Labels/food_label_zh.png` | 117×61 | 固定“食物”标签；数值应使用运行时文本。 |
| `UI/Labels/population_label_zh.png` | 235×61 | 固定“种群数量”标签；数值应使用运行时文本。 |
| `UI/Shared/bar_base.png` | 710×72 | 通用黑色底板，后续可用于 Love Nest HP 显示。 |
| `UI/Shared/bar_fill.png` | 710×72 | 原橙色填充；与底板使用相同画布和原位置，叠放即可对齐。 |

### 导出与导入约定

- 保留原始像素尺寸与颜色，不放大重绘。除整屏背景外，主体周围保留至少 2 px 透明边距，避免裁掉描边。
- 背景与森林不裁切，均保留 1920×1080。条底板与填充共用画布，保持原始相对位置。
- Unity 设置：Sprite / Single、PPU 100、中心 Pivot、Full Rect、Bilinear、Clamp、sRGB、关闭 Mipmap、无压缩、Max Size 2048。
- 接入彩色贴图时，SpriteRenderer Color 使用白色，避免占位物体原有颜色再次染色。
- 地面和森林使用相同位置、缩放；森林的前景遮挡层级在接入场景时设置。未改动现有 M1A 场景。
- 文字 PNG 只适合固定标签；PSD 中的 `23` 和 `蛋的数量：3` 是示意数字，不能作为动态 HUD。

## 参考区

| 文件，位于 `docs/art/reference/` | 保留原因 |
| --- | --- |
| `Characters/archer_reference.png` | 三个“射手”图层像素完全一致，合并成一张；用户确认不作为普通小角色。 |
| `Characters/guardian_reference.png` | 原“守护蛋的角色”；用户确认仅供参考，不引入照料者或治疗系统。 |
| `Variants/egg_green_reference.png` | 三枚绿色蛋为同一张图的副本，只保留一个配色备选。 |
| `OldDesign/training_label_zh.png` | “训练”文字；训练系统不在当前游戏范围。 |
| `OldDesign/training_panel.png` | 原训练区图形，保留原设计来源。 |
| `OldDesign/enemy_label_zh.png` | 原敌人区域文字，尚未确定为正式 UI。 |
| `MockupText/egg_count_3.png` | 原构图说明，含固定数字 3。 |
| `MockupText/number_23.png` | 两个数值图层像素完全一致，去重保存。 |

23 个源图层均已在 manifest 中对应到导出文件；镜像和重复图层也保留了来源记录。

## 缺失与制作优先级

| 优先级 | 需要补齐 | 当前状态 / 最小交付 |
| --- | --- | --- |
| 优先 | **普通小角色** | 用户确认待补画。先交付一个透明底普通小角色静态造型即可，4 个小角色复用同一套美术，不需要职业区分。 |
| 优先 | **Food Zone 和 Defense Zone 的明确视觉标识** | PSD 没有对应的独立区域图。可补区域底图、图标或边框；当前原型继续使用文字和颜色占位也可。Love Nest 已有圆形底座。 |
| 后续 | 基础动画 / 反馈 | 现有角色和蛋均为单张静态图，无动画帧。按 M3 实际需求补最少的角色动作、蛋孵化反馈和敌人动作；无需提前扩展动画状态。 |
| 后续 | 来袭预警、胜利 / 失败、重开等 UI 表现 | PSD 未提供。可先用 Unity 文本和简单按钮完成，定稿后再替换美术。 |
| 后续 | 项目用中文字体文件 | PSD 有文字图层，但目录里没有独立字体文件；动态 HUD 需要项目可用的字体。 |
| 可选 | 食物 / 种群图标、巢穴细化 | 目前只有文字标签和简单巢穴形状，不影响先验证原型。 |

普通小角色交付建议：透明 PNG，动画帧保持统一画布、比例和脚底基准。可以先用 256×256 或 512×512 画布绘制，再按实际显示尺寸导出；无需为不同工作状态设计不同职业。

此原始目录也未提供音效或音乐，M3 的基础音频仍需另行准备。

## 检查结果与待确认事项

- 已逐图查看导出总览；确认透明背景、主体轮廓、前景遮挡分离和 UI 对齐。
- 17 张 PNG 已通过文件完整性、尺寸、RGBA 通道及 SHA-256 校验；23 个图层均有来源映射。9 份 Sprite 导入配置及全部文件夹 `.meta` 已通过严格 YAML 解析、重复键与 GUID 检查。
- 使用 psd-tools 1.22.0 重建带描边图层，并按原坐标重组合成图，与 PSD 缓存预览比较：平均每通道误差约 0.043 / 255，约 0.222% 像素存在大于 8 的单通道差异。差异主要来自描边与抗锯齿，不能称为逐像素完全一致。
- 守护角色上方的小黑点来自原图层，参考导出中保留；若将来复用该角色，可在原 PSD 中清理。
- 导出美术尚未实际验证 Unity 导入、显示比例、画面层级或 Play。美术接入状态为 **Pending Unity Import / Play Verification**。
- 美术导出阶段保留了源 PSD、既有玩法文件、Prefab、场景、Packages、ProjectSettings；未使用 Computer Use、未 commit / push。
