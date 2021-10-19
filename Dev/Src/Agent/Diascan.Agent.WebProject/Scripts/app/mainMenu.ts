import { MenuConfig } from "./types/menu";

export let menuConfig: MenuConfig = {
    stateful: true,
    items: [
        {
            text: "Результаты расчётов",
            id: "menuCalculations",
            page: "calculations",
            url: "calculationsGrid",
            title: "Результаты расчётов",
        },
        {
            text: "Список идентификаторов",
            id: "menuIdentifiers",
            page: "identifiers",
            url: "identifiersGrid",
            title: "Список идентификаторов",
        },
    ]
};

export let appPages = window["AppPages"];