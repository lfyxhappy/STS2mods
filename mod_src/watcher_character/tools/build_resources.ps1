param(
  [string]$WorkspaceRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path,
  [string]$Sts1Jar = 'C:\st\steamapps\common\SlayTheSpire\desktop-1.0.jar'
)

$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.Drawing

$projectRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$godotProject = Join-Path $projectRoot 'resources_godot'
$outPck = Join-Path $projectRoot '..\..\mods\watcher_character\watcher_character.pck'
$jar = [IO.Compression.ZipFile]::OpenRead($Sts1Jar)

function Ensure-Dir([string]$path) {
  New-Item -ItemType Directory -Path $path -Force | Out-Null
}

function Extract-Entry([string]$entryName, [string]$destPath) {
  $entry = $jar.GetEntry($entryName)
  if ($null -eq $entry) {
    throw "Missing STS1 resource: $entryName"
  }
  Ensure-Dir (Split-Path -Parent $destPath)
  [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $destPath, $true)
}

function New-Image([string]$path, [int]$w, [int]$h, [string]$color, [string]$text) {
  Ensure-Dir (Split-Path -Parent $path)
  $bmp = New-Object Drawing.Bitmap $w, $h
  $g = [Drawing.Graphics]::FromImage($bmp)
  $g.Clear([Drawing.ColorTranslator]::FromHtml($color))
  $brush = New-Object Drawing.SolidBrush ([Drawing.Color]::FromArgb(230, 245, 238, 255))
  $font = New-Object Drawing.Font 'Segoe UI', ([Math]::Max(10, [Math]::Min($w, $h) / 9)), ([Drawing.FontStyle]::Bold)
  $format = New-Object Drawing.StringFormat
  $format.Alignment = [Drawing.StringAlignment]::Center
  $format.LineAlignment = [Drawing.StringAlignment]::Center
  $rect = New-Object Drawing.RectangleF 0, 0, $w, $h
  $g.DrawString($text, $font, $brush, $rect, $format)
  $bmp.Save($path, [Drawing.Imaging.ImageFormat]::Png)
  $format.Dispose(); $font.Dispose(); $brush.Dispose(); $g.Dispose(); $bmp.Dispose()
}

function New-AtlasTexture([string]$path, [string]$texturePath, [int]$w, [int]$h) {
  Ensure-Dir (Split-Path -Parent $path)
  Set-Content -Path $path -Value @"
[gd_resource type="AtlasTexture" load_steps=2 format=3]

[ext_resource type="Texture2D" path="$texturePath" id="1"]

[resource]
atlas = ExtResource("1")
region = Rect2(0, 0, $w, $h)
"@ -Encoding UTF8
}

function New-ExportPresets([string]$path) {
  Set-Content -Path $path -Value @'
[preset.0]

name="Windows Desktop"
platform="Windows Desktop"
runnable=false
dedicated_server=false
custom_features=""
export_filter="all_resources"
include_filter=""
exclude_filter=""
export_path="watcher_character.exe"
encryption_include_filters=""
encryption_exclude_filters=""
encrypt_pck=false
encrypt_directory=false
script_export_mode=2

[preset.0.options]

custom_template/debug=""
custom_template/release=""
debug/export_console_wrapper=0
binary_format/embed_pck=false
texture_format/bptc=false
texture_format/s3tc=true
texture_format/etc=false
texture_format/etc2=false
binary_format/architecture="x86_64"
codesign/enable=false
codesign/timestamp=true
codesign/timestamp_server_url=""
codesign/digest_algorithm=1
codesign/description=""
codesign/custom_options=PackedStringArray()
application/modify_resources=false
application/icon=""
application/console_wrapper_icon=""
application/icon_interpolation=4
application/file_version=""
application/product_version=""
application/company_name=""
application/product_name=""
application/file_description=""
application/copyright=""
application/trademarks=""
ssh_remote_deploy/enabled=false
ssh_remote_deploy/host="user@host_ip"
ssh_remote_deploy/port="22"
ssh_remote_deploy/extra_args_ssh=""
ssh_remote_deploy/extra_args_scp=""
ssh_remote_deploy/run_script=""
ssh_remote_deploy/cleanup_script=""
texture_format/no_bptc_fallbacks=true
binary_format/64_bits=true
'@ -Encoding UTF8
}

if (Test-Path $godotProject) {
  Remove-Item -Path $godotProject -Recurse -Force
}
Ensure-Dir $godotProject

Set-Content -Path (Join-Path $godotProject 'project.godot') -Value @'
; Engine configuration file.

[application]
config/name="watcher_character_resources"
config/features=PackedStringArray("4.5", "C#", "Forward Plus")
'@ -Encoding UTF8

New-ExportPresets (Join-Path $godotProject 'export_presets.cfg')

$images = Join-Path $godotProject 'images'
$scenes = Join-Path $godotProject 'scenes'
$locBase = Join-Path $godotProject 'watcher_character\localization'
foreach ($dir in @(
  'creature_visuals',
  'ui\character_icons',
  'screens\char_select',
  'merchant\characters',
  'rest_site\characters',
  'combat\energy_counters',
  'vfx'
)) {
  Ensure-Dir (Join-Path $scenes $dir)
}

Extract-Entry 'images/ui/charSelect/watcherButton.png' (Join-Path $images 'packed\character_select\char_select_watcher_character.png')
Extract-Entry 'images/ui/charSelect/watcherButton.png' (Join-Path $images 'packed\character_select\char_select_watcher_character_locked.png')
Extract-Entry 'images/ui/charSelect/watcherButton.png' (Join-Path $images 'ui\top_panel\character_icon_watcher_character.png')
Extract-Entry 'images/ui/charSelect/watcherButton.png' (Join-Path $images 'ui\top_panel\character_icon_watcher_character_outline.png')
Extract-Entry 'images/ui/leaderboards/watcher.png' (Join-Path $images 'packed\map\icons\map_marker_watcher_character.png')
Extract-Entry 'images/characters/watcher/idle/the_watcher.png' (Join-Path $images 'watcher_character\the_watcher.png')
Extract-Entry 'images/characters/watcher/shoulder.png' (Join-Path $images 'watcher_character\watcher_portrait.png')
Extract-Entry 'images/largeRelics/clean_water.png' (Join-Path $images 'relics\purewater.png')
Extract-Entry 'images/largeRelics/violet_lotus.png' (Join-Path $images 'relics\violetlotus.png')
Extract-Entry 'images/relics/clean_water.png' (Join-Path $images 'atlases\relic_atlas.sprites\purewater.png')
Extract-Entry 'images/relics/violet_lotus.png' (Join-Path $images 'atlases\relic_atlas.sprites\violetlotus.png')
Extract-Entry 'images/relics/outline/clean_water.png' (Join-Path $images 'atlases\relic_outline_atlas.sprites\purewater.png')
Extract-Entry 'images/relics/outline/violet_lotus.png' (Join-Path $images 'atlases\relic_outline_atlas.sprites\violetlotus.png')

New-Image (Join-Path $images 'watcher_character\missing_card.png') 250 190 '#3B245F' 'Watcher'
New-Image (Join-Path $images 'powers\watcherstancepower.png') 128 128 '#6B3FA0' 'Stance'
New-Image (Join-Path $images 'powers\mantrapower.png') 128 128 '#8C6CD6' 'Mantra'
New-Image (Join-Path $images 'powers\nirvanapower.png') 128 128 '#6252A4' 'Nirvana'
New-Image (Join-Path $images 'powers\rushdownpower.png') 128 128 '#8539B5' 'Rush'
New-Image (Join-Path $images 'powers\talktothehandpower.png') 128 128 '#7240A8' 'Hand'
New-Image (Join-Path $images 'powers\mentalfortresspower.png') 128 128 '#564293' 'Fort'
New-Image (Join-Path $images 'powers\devaformpower.png') 128 128 '#B59CFF' 'Deva'

$cardDir = Join-Path $images 'atlases\card_atlas.sprites\watcher'
Ensure-Dir $cardDir
$cardNames = @(
  'strike_watcher','defend_watcher','eruption','vigilance','crescendo','tranquility','empty_body','empty_fist','empty_mind','cut_through_fate',
  'third_eye','rushdown','talk_to_the_hand','mental_fortress','lesson_learned','blasphemy','scrawl_watcher','prostrate','pray','worship',
  'empty_stance','follow_up','flurry_of_blows','conclude','carve_reality','smite','just_lucky','foresight','sands_of_time','deva_form',
  'wheel_kick','windmill_strike','halt','flying_sleeves','collect','sash_whip','fear_no_evil','indignation','inner_peace','wallop',
  'establishment','omniscience','vault','alpha','beta','omega','wish_watcher','foreign_influence','bowling_bash','brilliance',
  'conjure_blade','deceive_reality','devotion','evaluate','like_water','master_reality','nirvana','perseverance','study','swivel',
  'weave','pressure_points','sanctity','signature_move','spirit_shield','reach_heaven','through_violence'
)
foreach ($name in $cardNames) {
  Copy-Item -Path (Join-Path $images 'watcher_character\missing_card.png') -Destination (Join-Path $cardDir "$name.png") -Force
  New-AtlasTexture (Join-Path $cardDir "$name.tres") "res://images/atlases/card_atlas.sprites/watcher/$name.png" 250 190
}

$powerAtlasDir = Join-Path $images 'atlases\power_atlas.sprites'
Ensure-Dir $powerAtlasDir
foreach ($name in @('watcherstancepower','mantrapower','nirvanapower','rushdownpower','talktothehandpower','mentalfortresspower','devaformpower')) {
  Copy-Item -Path (Join-Path $images "powers\$name.png") -Destination (Join-Path $powerAtlasDir "$name.png") -Force
  New-AtlasTexture (Join-Path $powerAtlasDir "$name.tres") "res://images/atlases/power_atlas.sprites/$name.png" 128 128
}

$powerIconAliases = [ordered]@{
  'watcher_stance_power'='watcherstancepower'
  'mantra_power'='mantrapower'
  'nirvana_power'='nirvanapower'
  'rushdown_power'='rushdownpower'
  'talk_to_the_hand_power'='talktothehandpower'
  'mental_fortress_power'='mentalfortresspower'
  'deva_form_power'='devaformpower'
}
foreach ($alias in $powerIconAliases.Keys) {
  $source = $powerIconAliases[$alias]
  Copy-Item -Path (Join-Path $images "powers\$source.png") -Destination (Join-Path $images "powers\$alias.png") -Force
  Copy-Item -Path (Join-Path $powerAtlasDir "$source.png") -Destination (Join-Path $powerAtlasDir "$alias.png") -Force
  New-AtlasTexture (Join-Path $powerAtlasDir "$alias.tres") "res://images/atlases/power_atlas.sprites/$alias.png" 128 128
}

New-AtlasTexture (Join-Path $images 'atlases\relic_atlas.sprites\purewater.tres') 'res://images/atlases/relic_atlas.sprites/purewater.png' 128 128
New-AtlasTexture (Join-Path $images 'atlases\relic_atlas.sprites\violetlotus.tres') 'res://images/atlases/relic_atlas.sprites/violetlotus.png' 128 128
New-AtlasTexture (Join-Path $images 'atlases\relic_outline_atlas.sprites\purewater.tres') 'res://images/atlases/relic_outline_atlas.sprites/purewater.png' 128 128
New-AtlasTexture (Join-Path $images 'atlases\relic_outline_atlas.sprites\violetlotus.tres') 'res://images/atlases/relic_outline_atlas.sprites/violetlotus.png' 128 128

Copy-Item -Path (Join-Path $images 'relics\purewater.png') -Destination (Join-Path $images 'relics\pure_water.png') -Force
Copy-Item -Path (Join-Path $images 'relics\violetlotus.png') -Destination (Join-Path $images 'relics\violet_lotus.png') -Force
Copy-Item -Path (Join-Path $images 'atlases\relic_atlas.sprites\purewater.png') -Destination (Join-Path $images 'atlases\relic_atlas.sprites\pure_water.png') -Force
Copy-Item -Path (Join-Path $images 'atlases\relic_atlas.sprites\violetlotus.png') -Destination (Join-Path $images 'atlases\relic_atlas.sprites\violet_lotus.png') -Force
Copy-Item -Path (Join-Path $images 'atlases\relic_outline_atlas.sprites\purewater.png') -Destination (Join-Path $images 'atlases\relic_outline_atlas.sprites\pure_water.png') -Force
Copy-Item -Path (Join-Path $images 'atlases\relic_outline_atlas.sprites\violetlotus.png') -Destination (Join-Path $images 'atlases\relic_outline_atlas.sprites\violet_lotus.png') -Force
New-AtlasTexture (Join-Path $images 'atlases\relic_atlas.sprites\pure_water.tres') 'res://images/atlases/relic_atlas.sprites/pure_water.png' 128 128
New-AtlasTexture (Join-Path $images 'atlases\relic_atlas.sprites\violet_lotus.tres') 'res://images/atlases/relic_atlas.sprites/violet_lotus.png' 128 128
New-AtlasTexture (Join-Path $images 'atlases\relic_outline_atlas.sprites\pure_water.tres') 'res://images/atlases/relic_outline_atlas.sprites/pure_water.png' 128 128
New-AtlasTexture (Join-Path $images 'atlases\relic_outline_atlas.sprites\violet_lotus.tres') 'res://images/atlases/relic_outline_atlas.sprites/violet_lotus.png' 128 128

Ensure-Dir (Join-Path $godotProject 'materials\transitions')
Set-Content -Path (Join-Path $godotProject 'materials\transitions\watcher_character_transition_mat.tres') -Value @'
[gd_resource type="ShaderMaterial" load_steps=2 format=3]

[sub_resource type="Shader" id="Shader_watcher_character_transition"]
code = "shader_type canvas_item;

uniform float threshold : hint_range(0,1);

void fragment() {
    COLOR.a = threshold;
}
"

[resource]
resource_local_to_scene = true
shader = SubResource("Shader_watcher_character_transition")
shader_parameter/threshold = 1.0
'@ -Encoding UTF8

Set-Content -Path (Join-Path $scenes 'creature_visuals\watcher_character.tscn') -Value @'
[gd_scene load_steps=3 format=3]

[ext_resource type="Script" path="res://src/Core/Nodes/Combat/NCreatureVisuals.cs" id="1"]
[ext_resource type="Texture2D" path="res://images/watcher_character/the_watcher.png" id="2"]

[node name="WatcherCharacter" type="Node2D"]
script = ExtResource("1")

[node name="Visuals" type="Sprite2D" parent="."]
unique_name_in_owner = true
texture = ExtResource("2")
position = Vector2(0, -120)
scale = Vector2(0.9, 0.9)

[node name="Bounds" type="Control" parent="."]
unique_name_in_owner = true
layout_mode = 3
anchors_preset = 0
offset_left = -120.0
offset_top = -260.0
offset_right = 120.0
offset_bottom = 0.0
mouse_filter = 2

[node name="CenterPos" type="Marker2D" parent="."]
unique_name_in_owner = true
position = Vector2(0, -150)

[node name="IntentPos" type="Marker2D" parent="."]
unique_name_in_owner = true
position = Vector2(0, -300)
'@ -Encoding UTF8

Set-Content -Path (Join-Path $scenes 'ui\character_icons\watcher_character_icon.tscn') -Value @'
[gd_scene load_steps=2 format=3]

[ext_resource type="Texture2D" path="res://images/ui/top_panel/character_icon_watcher_character.png" id="1"]

[node name="CharacterIcon" type="TextureRect"]
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
grow_horizontal = 2
grow_vertical = 2
mouse_filter = 2
texture = ExtResource("1")
expand_mode = 1
stretch_mode = 5
'@ -Encoding UTF8

Set-Content -Path (Join-Path $scenes 'screens\char_select\char_select_bg_watcher_character.tscn') -Value @'
[gd_scene load_steps=2 format=3]

[ext_resource type="Texture2D" path="res://images/watcher_character/watcher_portrait.png" id="1"]

[node name="WatcherCharacterBg" type="Control"]
layout_mode = 3
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0

[node name="Portrait" type="TextureRect" parent="."]
layout_mode = 1
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
grow_horizontal = 2
grow_vertical = 2
texture = ExtResource("1")
expand_mode = 1
stretch_mode = 6
'@ -Encoding UTF8

Set-Content -Path (Join-Path $scenes 'merchant\characters\watcher_character_merchant.tscn') -Value @'
[gd_scene load_steps=2 format=3]

[ext_resource type="Texture2D" path="res://images/watcher_character/the_watcher.png" id="1"]

[node name="WatcherMerchant" type="Node2D"]

[node name="Sprite2D" type="Sprite2D" parent="."]
texture = ExtResource("1")
position = Vector2(0, -100)
scale = Vector2(0.6, 0.6)
'@ -Encoding UTF8

Set-Content -Path (Join-Path $scenes 'rest_site\characters\watcher_character_rest_site.tscn') -Value @'
[gd_scene load_steps=2 format=3]

[ext_resource type="Texture2D" path="res://images/watcher_character/the_watcher.png" id="1"]

[node name="WatcherRestSite" type="Node2D"]

[node name="Sprite2D" type="Sprite2D" parent="."]
texture = ExtResource("1")
position = Vector2(0, -100)
scale = Vector2(0.6, 0.6)
'@ -Encoding UTF8

$baseEnergyCounter = Join-Path $WorkspaceRoot 'pck_extracted_assets\scenes\combat\energy_counters\ironclad_energy_counter.tscn'
if (Test-Path $baseEnergyCounter) {
  $energyCounterText = Get-Content -Raw -Path $baseEnergyCounter
  $energyCounterText = $energyCounterText.Replace('[node name="IroncladEnergyCounter" type="Control"]', '[node name="WatcherEnergyCounter" type="Control"]')
  Set-Content -Path (Join-Path $scenes 'combat\energy_counters\watcher_character_energy_counter.tscn') -Value $energyCounterText -Encoding UTF8
}
else {
  throw "Base energy counter scene not found: $baseEnergyCounter"
}

$baseCardTrail = Join-Path $WorkspaceRoot 'pck_extracted_assets\scenes\vfx\card_trail_ironclad.tscn'
if (Test-Path $baseCardTrail) {
  $cardTrailText = Get-Content -Raw -Path $baseCardTrail
  $cardTrailText = $cardTrailText.Replace('CardTrailIronclad', 'CardTrailWatcherCharacter')
  Set-Content -Path (Join-Path $scenes 'vfx\card_trail_watcher_character.tscn') -Value $cardTrailText -Encoding UTF8
}
else {
  throw "Base card trail scene not found: $baseCardTrail"
}

$engCards = [ordered]@{
  'WATCHER_SCRY.selectionScreenPrompt' = 'Choose any cards to discard.'
}
$zhsCards = [ordered]@{
  'WATCHER_SCRY.selectionScreenPrompt' = '选择任意牌丢弃。'
}

$cardTitleMap = [ordered]@{
  'STRIKE_WATCHER' = 'Strike'
  'DEFEND_WATCHER' = 'Defend'
  'ERUPTION' = 'Eruption'
  'VIGILANCE' = 'Vigilance'
  'CRESCENDO' = 'Crescendo'
  'TRANQUILITY' = 'Tranquility'
  'EMPTY_BODY' = 'Empty Body'
  'EMPTY_FIST' = 'Empty Fist'
  'EMPTY_MIND' = 'Empty Mind'
  'CUT_THROUGH_FATE' = 'Cut Through Fate'
  'THIRD_EYE' = 'Third Eye'
  'RUSHDOWN' = 'Rushdown'
  'TALK_TO_THE_HAND' = 'Talk to the Hand'
  'MENTAL_FORTRESS' = 'Mental Fortress'
  'LESSON_LEARNED' = 'Lesson Learned'
  'BLASPHEMY' = 'Blasphemy'
  'SCRAWL_WATCHER' = 'Scrawl'
  'PROSTRATE' = 'Prostrate'
  'PRAY' = 'Pray'
  'WORSHIP' = 'Worship'
  'EMPTY_STANCE' = 'Empty Stance'
  'FOLLOW_UP' = 'Follow-Up'
  'FLURRY_OF_BLOWS' = 'Flurry of Blows'
  'CONCLUDE' = 'Conclude'
  'CARVE_REALITY' = 'Carve Reality'
  'SMITE' = 'Smite'
  'JUST_LUCKY' = 'Just Lucky'
  'FORESIGHT' = 'Foresight'
  'SANDS_OF_TIME' = 'Sands of Time'
  'DEVA_FORM' = 'Deva Form'
  'WHEEL_KICK' = 'Wheel Kick'
  'WINDMILL_STRIKE' = 'Windmill Strike'
  'HALT' = 'Halt'
  'FLYING_SLEEVES' = 'Flying Sleeves'
  'COLLECT' = 'Collect'
  'SASH_WHIP' = 'Sash Whip'
  'FEAR_NO_EVIL' = 'Fear No Evil'
  'INDIGNATION' = 'Indignation'
  'INNER_PEACE' = 'Inner Peace'
  'WALLOP' = 'Wallop'
  'ESTABLISHMENT' = 'Establishment'
  'OMNISCIENCE' = 'Omniscience'
  'VAULT' = 'Vault'
  'ALPHA' = 'Alpha'
  'BETA' = 'Beta'
  'OMEGA' = 'Omega'
  'WISH_WATCHER' = 'Wish'
  'FOREIGN_INFLUENCE' = 'Foreign Influence'
  'BOWLING_BASH' = 'Bowling Bash'
  'BRILLIANCE' = 'Brilliance'
  'CONJURE_BLADE' = 'Conjure Blade'
  'DECEIVE_REALITY' = 'Deceive Reality'
  'DEVOTION' = 'Devotion'
  'EVALUATE' = 'Evaluate'
  'LIKE_WATER' = 'Like Water'
  'MASTER_REALITY' = 'Master Reality'
  'NIRVANA' = 'Nirvana'
  'PERSEVERANCE' = 'Perseverance'
  'STUDY' = 'Study'
  'SWIVEL' = 'Swivel'
  'WEAVE' = 'Weave'
  'PRESSURE_POINTS' = 'Pressure Points'
  'SANCTITY' = 'Sanctity'
  'SIGNATURE_MOVE' = 'Signature Move'
  'SPIRIT_SHIELD' = 'Spirit Shield'
  'REACH_HEAVEN' = 'Reach Heaven'
  'THROUGH_VIOLENCE' = 'Through Violence'
}
foreach ($key in $cardTitleMap.Keys) {
  $engCards["$key.title"] = $cardTitleMap[$key]
  $engCards["$key.description"] = 'Watcher card port. Effects are adapted for STS2.'
  $zhsCards["$key.title"] = $cardTitleMap[$key]
  $zhsCards["$key.description"] = '观者移植卡牌，效果已适配 STS2。'
}

$engCharacters = [ordered]@{
  'WATCHER_CHARACTER.title'='The Watcher'
  'WATCHER_CHARACTER.titleObject'='The Watcher'
  'WATCHER_CHARACTER.description'='A blind ascetic who channels Calm, Wrath, Divinity, Mantra, Scry, and Retain.'
  'WATCHER_CHARACTER.unlockText'='Private mod character.'
  'WATCHER_CHARACTER.possessiveAdjective'='her'
  'WATCHER_CHARACTER.pronounObject'='her'
  'WATCHER_CHARACTER.pronounPossessive'='hers'
  'WATCHER_CHARACTER.pronounSubject'='she'
  'WATCHER_CHARACTER.cardsModifierTitle'='Watcher Cards'
  'WATCHER_CHARACTER.cardsModifierDescription'='Watcher cards will now appear in rewards and shops.'
  'WATCHER_CHARACTER.eventDeathPrevention'='I am not finished.'
}
$zhsCharacters = [ordered]@{
  'WATCHER_CHARACTER.title'='观者'
  'WATCHER_CHARACTER.titleObject'='观者'
  'WATCHER_CHARACTER.description'='来自杀戮尖塔 1 的盲眼修行者，使用平静、愤怒、神格、真言、预见与保留。'
  'WATCHER_CHARACTER.unlockText'='私用 Mod 人物。'
  'WATCHER_CHARACTER.possessiveAdjective'='她的'
  'WATCHER_CHARACTER.pronounObject'='她'
  'WATCHER_CHARACTER.pronounPossessive'='她的'
  'WATCHER_CHARACTER.pronounSubject'='她'
  'WATCHER_CHARACTER.cardsModifierTitle'='观者卡牌'
  'WATCHER_CHARACTER.cardsModifierDescription'='观者卡牌会出现在奖励和商店中。'
  'WATCHER_CHARACTER.eventDeathPrevention'='我尚未结束。'
}

$engPowers = [ordered]@{
  'WATCHER_STANCE_POWER.title'='Stance'
  'WATCHER_STANCE_POWER.description'='Current Watcher stance. Wrath doubles attack damage dealt and received. Divinity triples attack damage dealt.'
  'WATCHER_STANCE_POWER.smartDescription'='Current Watcher stance. Wrath doubles attack damage dealt and received. Divinity triples attack damage dealt.'
  'MANTRA_POWER.title'='Mantra'
  'MANTRA_POWER.description'='At 10 Mantra, enter Divinity.'
  'MANTRA_POWER.smartDescription'='At 10 Mantra, enter Divinity. Current: [blue]{Amount}[/blue].'
  'NIRVANA_POWER.title'='Nirvana'
  'NIRVANA_POWER.description'='Whenever you Scry, gain Block.'
  'NIRVANA_POWER.smartDescription'='Whenever you Scry, gain [blue]{Amount}[/blue] Block.'
  'RUSHDOWN_POWER.title'='Rushdown'
  'RUSHDOWN_POWER.description'='Whenever you enter Wrath, draw cards.'
  'RUSHDOWN_POWER.smartDescription'='Whenever you enter Wrath, draw [blue]{Amount}[/blue] cards.'
  'TALK_TO_THE_HAND_POWER.title'='Talk to the Hand'
  'TALK_TO_THE_HAND_POWER.description'='Attacks against this enemy grant Block.'
  'TALK_TO_THE_HAND_POWER.smartDescription'='Attacks against this enemy grant [blue]{Amount}[/blue] Block.'
  'MENTAL_FORTRESS_POWER.title'='Mental Fortress'
  'MENTAL_FORTRESS_POWER.description'='Whenever you change Stance, gain Block.'
  'MENTAL_FORTRESS_POWER.smartDescription'='Whenever you change Stance, gain [blue]{Amount}[/blue] Block.'
  'DEVA_FORM_POWER.title'='Deva Form'
  'DEVA_FORM_POWER.description'='At the start of each turn, gain increasing Energy.'
  'DEVA_FORM_POWER.smartDescription'='At the start of each turn, gain [blue]{Amount}[/blue] Energy, then increase this by 1.'
}
$zhsPowers = [ordered]@{}
foreach ($key in $engPowers.Keys) { $zhsPowers[$key] = $engPowers[$key] }
$zhsPowers['WATCHER_STANCE_POWER.title']='姿态'
$zhsPowers['MANTRA_POWER.title']='真言'
$zhsPowers['NIRVANA_POWER.title']='涅槃'
$zhsPowers['RUSHDOWN_POWER.title']='猛冲'
$zhsPowers['TALK_TO_THE_HAND_POWER.title']='当头棒喝'
$zhsPowers['MENTAL_FORTRESS_POWER.title']='心灵堡垒'
$zhsPowers['DEVA_FORM_POWER.title']='天人形态'

$engRelics = [ordered]@{
  'PURE_WATER.title'='Pure Water'
  'PURE_WATER.description'='Starter relic for the Watcher.'
  'PURE_WATER.flavor'='A modest vessel of clear water.'
  'VIOLET_LOTUS.title'='Violet Lotus'
  'VIOLET_LOTUS.description'='Whenever you leave Calm, gain 1 additional Energy.'
  'VIOLET_LOTUS.flavor'='The calm before the storm.'
}
$zhsRelics = [ordered]@{
  'PURE_WATER.title'='净水'
  'PURE_WATER.description'='观者的初始遗物。'
  'PURE_WATER.flavor'='一杯清澈的水。'
  'VIOLET_LOTUS.title'='紫莲花'
  'VIOLET_LOTUS.description'='每当你离开平静，额外获得 1 点能量。'
  'VIOLET_LOTUS.flavor'='暴风雨前的宁静。'
}

Ensure-Dir (Join-Path $locBase 'eng')
Ensure-Dir (Join-Path $locBase 'zhs')
$engCards | ConvertTo-Json -Depth 4 | Set-Content -Path (Join-Path $locBase 'eng\cards.json') -Encoding UTF8
$zhsCards | ConvertTo-Json -Depth 4 | Set-Content -Path (Join-Path $locBase 'zhs\cards.json') -Encoding UTF8
$engCharacters | ConvertTo-Json -Depth 4 | Set-Content -Path (Join-Path $locBase 'eng\characters.json') -Encoding UTF8
$zhsCharacters | ConvertTo-Json -Depth 4 | Set-Content -Path (Join-Path $locBase 'zhs\characters.json') -Encoding UTF8
$engPowers | ConvertTo-Json -Depth 4 | Set-Content -Path (Join-Path $locBase 'eng\powers.json') -Encoding UTF8
$zhsPowers | ConvertTo-Json -Depth 4 | Set-Content -Path (Join-Path $locBase 'zhs\powers.json') -Encoding UTF8
$engRelics | ConvertTo-Json -Depth 4 | Set-Content -Path (Join-Path $locBase 'eng\relics.json') -Encoding UTF8
$zhsRelics | ConvertTo-Json -Depth 4 | Set-Content -Path (Join-Path $locBase 'zhs\relics.json') -Encoding UTF8

$godot = Join-Path $WorkspaceRoot 'Godot_v4.5.1-stable_mono_win64\Godot_v4.5.1-stable_mono_win64_console.exe'
if (!(Test-Path $godot)) {
  throw "Godot console executable not found: $godot"
}

Ensure-Dir (Split-Path -Parent $outPck)
$outPckGodot = $outPck.Replace('\', '/')
$packerScript = Join-Path $godotProject 'pack_mod.gd'
Set-Content -Path $packerScript -Value @"
extends SceneTree

const OUTPUT := "$outPckGodot"

var packer := PCKPacker.new()
var packed_count := 0

func _init() -> void:
	var err := packer.pck_start(OUTPUT)
	if err != OK:
		push_error("pck_start failed: %s" % err)
		quit(1)
		return

	for dir in ["res://images", "res://materials", "res://scenes", "res://watcher_character", "res://.godot/imported"]:
		_pack_dir(dir)

	err = packer.flush()
	if err != OK:
		push_error("pck flush failed: %s" % err)
		quit(1)
		return

	print("Packed watcher_character resources: %d files" % packed_count)
	quit()

func _pack_dir(res_dir: String) -> void:
	var dir := DirAccess.open(res_dir)
	if dir == null:
		push_warning("Skipping missing directory: %s" % res_dir)
		return

	dir.list_dir_begin()
	var entry := dir.get_next()
	while entry != "":
		if entry == "." or entry == "..":
			entry = dir.get_next()
			continue
		var res_path := res_dir.path_join(entry)
		if dir.current_is_dir():
			_pack_dir(res_path)
		else:
			_pack_file(res_path)
		entry = dir.get_next()
	dir.list_dir_end()

func _pack_file(res_path: String) -> void:
	if res_path.ends_with(".uid"):
		return
	var source := ProjectSettings.globalize_path(res_path)
	var err := packer.add_file(res_path, source)
	if err != OK:
		push_error("add_file failed %s from %s: %s" % [res_path, source, err])
		quit(1)
		return
	packed_count += 1
"@ -Encoding UTF8

Push-Location $godotProject
try {
  & $godot --headless --import
  if ($LASTEXITCODE -ne 0) {
    throw "Godot import failed with exit code $LASTEXITCODE"
  }

  & $godot --headless --script $packerScript
  if ($LASTEXITCODE -ne 0) {
    throw "Godot PCK pack failed with exit code $LASTEXITCODE"
  }
}
finally {
  Pop-Location
  $jar.Dispose()
}

Get-Item $outPck | Select-Object FullName, Length, LastWriteTime
