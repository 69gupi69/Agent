export interface MenuItem {
    id?: string;
    text: string;
    items?: MenuItem[];
    page?: string;
    url?: string;
    title?: string;
}

export interface MenuConfig {
    stateful?: boolean;
    items: MenuItem[];
}