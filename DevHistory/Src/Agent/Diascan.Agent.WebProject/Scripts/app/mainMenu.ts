import { MenuConfig } from "./types/menu";

export let menuConfig: MenuConfig = {
    stateful: true,
    items: [
        {
            text: "Результаты расчётов",
            id: "menuCalculations",
            page: "calculations",
            url: "grid",
            title: "Результаты расчётов",
        },
    ]
};

export let appPages = window["AppPages"];