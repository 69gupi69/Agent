let modules: { [id: string]: string; } = {};

function init(m: { [id: string]: string; }) {
    modules = m;
}

export function redirect(module: string, url?: string) {
    let moduleUrl = modules[module];
    if (moduleUrl) {
        window.location.href = url ? `${moduleUrl}#!${url}` : moduleUrl;
    }
    else {
        console.error(`Url for module ${module} not found`);
    }
}

init(window["AppPages"]);