export interface IDisposable {
    dispose()
}

export class DisposeManager {
    private items: IDisposable[] = [];

    push(item: IDisposable) {
        this.items.push(item);
        return this;
    }

    dispose() {
        if (this.items) {
            while (this.items.length > 0) {
                let item = this.items.pop();
                item.dispose();
            }
        }
    }
}

