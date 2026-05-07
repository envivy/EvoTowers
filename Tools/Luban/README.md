# Luban Data Pipeline

This folder contains the project-side Luban export setup.

## Layout

- `Datas/`: source tables. The sample uses CSV so the setup works without Excel.
- `Defines/`: Luban table schema definitions.
- `Gen.bat`: Windows one-click export script.
- `luban.conf`: project generation config.

Generated runtime files are written to:

- C# code: `Assets/Scripts/Generated/Luban`
- JSON data: `Assets/Resources/Generated/DataTables`

## First Setup

1. Download a Luban release from the official Luban repository.
2. Put `Luban.dll` at `Tools/Luban/Luban/Luban.dll`.
3. Let Unity resolve the `com.code-philosophy.luban` package from `Packages/manifest.json`.
4. Run `Tools/Luban/Gen.bat`, or use Unity menu `EvoTowers/Luban/Generate Tables`.
5. To push tower table values into the current playable scene, use Unity menu `EvoTowers/Luban/Import Tower Configs To Battle Scene`.

The generated JSON files are under `Resources`, so runtime code can load them with `Resources.Load<TextAsset>`.

The current prototype still stores playable tower configs on `GameManager.towerConfigs`. `Import Tower Configs To Battle Scene` reads `Datas/Tower.csv` and updates `Assets/Scenes/Battle.unity` with those values.

## Adding Tables

1. Add a table file under `Datas/`.
2. Add the table schema under `Defines/`.
3. Register the table in `Defines/__tables__.xml`.
4. Run the generator.
