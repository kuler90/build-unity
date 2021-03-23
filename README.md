# build-unity

GitHub Action to build Unity project.

Works on Ubuntu, macOS and Windows.

## Inputs

### `unity-path`

Path to Unity executable. `UNITY_PATH` env will be used if not provided.

### `project-path`

Path to Unity project. Used to find Unity version. Default `${{ github.workspace }}`.

### `build-target`

**Required** Build target platform.

### `build-path`

Path to build output. Only for default build method.

### `build-version`

Set application version. Only for default build method.

### `build-number`

Set application build number. Only for default build method.

### `build-defines`

Set scripting define symbols. For example, `RELEASE_VERSION;ENG_VERSION`. Only for default build method.

### `build-options`

List of additional [BuildOptions](https://docs.unity3d.com/ScriptReference/BuildOptions.html). For example, `SymlinkLibraries, CompressWithLz4HC`. Only for default build method.

### `android-keystore-base64`

The base64 contents of the android keystore file. Only for default build method.

### `android-keystore-pass`

The android keystore password. Only for default build method.

### `android-keyalias-name`

The android keyalias name. Only for default build method.

### `android-keyalias-pass`

The android keyalias password. Only for default build method.

### `build-method`

Path to build method. For example, `MyEditorScript.PerformBuild`. Default build method will be used if not provided.

### `build-method-args`

Custom arguments for build method.

## Outputs

### `build-path`

Path to build output.

## Example usage

```yaml
- name: Checkout project
  uses: actions/checkout@v2

- name: Setup Unity
  uses: kuler90/setup-unity@v1
  with:
    unity-modules: android

- name: Activate Unity
  uses: kuler90/activate-unity@v1
  with:
    unity-username: ${{ secrets.UNITY_USERNAME }}
    unity-password: ${{ secrets.UNITY_PASSWORD }}
    unity-authenticator-key: ${{ secrets.UNITY_AUTHENTICATOR_KEY }}

- name: Build Unity
  uses: kuler90/build-unity@v1
  with:
    build-target: Android
    build-path: ./build.apk
```