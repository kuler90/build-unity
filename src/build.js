const core = require('@actions/core');
const exec = require('@actions/exec');
const io = require('@actions/io');
const path = require('path');

async function run() {
    try {
        const unityPath = core.getInput('unity-path') || process.env.UNITY_PATH;
        if (!unityPath) {
            throw new Error('unity path not found');
        }
        const projectPath = core.getInput('project-path');
        const buildTatget = core.getInput('build-target', { required: true });
        const buildPath = core.getInput('build-path') || path.join('./builds', buildTatget);
        const buildVersion = core.getInput('build-version');
        const buildNumber = core.getInput('build-number');
        const buildDefines = core.getInput('build-defines');
        const buildOptions = core.getInput('build-options');
        const androidKeystoreBase64 = core.getInput('android-keystore-base64');
        const androidKeystorePass = core.getInput('android-keystore-pass');
        const androidKeyaliasName = core.getInput('android-keyalias-name');
        const androidKeyaliasPass = core.getInput('android-keyalias-pass');
        let buildMethod = core.getInput('build-method');
        const buildMethodArgs = core.getInput('build-method-args');

        if (!buildMethod) {
            buildMethod = 'kuler90.BuildCommand.Build';
            const src = path.join(__dirname, 'BuildCommand.cs');
            const dest = path.join(projectPath, 'Assets/kuler90/Editor');
            await io.mkdirP(dest);
            await io.cp(src, dest);
        }

        let unityCmd = '';
        if (process.platform === 'linux') {
            unityCmd = `xvfb-run --auto-servernum "${unityPath}"`;
        } else {
            unityCmd = `"${unityPath}"`;
        }

        let buildArgs = '';
        buildArgs += ` -projectPath "${projectPath}"`;
        buildArgs += ` -buildTarget "${buildTatget}"`;
        buildArgs += ` -buildPath "${buildPath}"`;
        buildArgs += ` -executeMethod "${buildMethod}"`;
        buildArgs += ` ${buildMethodArgs}`;
        if (buildVersion) {
            buildArgs += ` -buildVersion "${buildVersion}"`;
        }
        if (buildNumber) {
            buildArgs += ` -buildNumber "${buildNumber}"`;
        }
        if (buildDefines) {
            buildArgs += ` -buildDefines "${buildDefines}"`;
        }
        if (buildOptions) {
            buildArgs += ` -buildOptions "${buildOptions}"`;
        }
        if (androidKeystoreBase64) {
            buildArgs += ` -androidKeystoreBase64 "${androidKeystoreBase64}"`;
        }
        if (androidKeystorePass) {
            buildArgs += ` -androidKeystorePass "${androidKeystorePass}"`;
        }
        if (androidKeyaliasName) {
            buildArgs += ` -androidKeyaliasName "${androidKeyaliasName}"`;
        }
        if (androidKeyaliasPass) {
            buildArgs += ` -androidKeyaliasPass "${androidKeyaliasPass}"`;
        }

        await exec.exec(`${unityCmd} -batchmode -nographics -quit -logFile "-" ${buildArgs}`);

        core.setOutput('build-path', buildPath);
    } catch (error) {
        core.setFailed(error.message);
    }
}

run();

