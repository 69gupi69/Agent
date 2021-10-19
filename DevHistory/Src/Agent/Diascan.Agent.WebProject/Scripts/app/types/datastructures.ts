export class Stack<T> {
    private data: T[];
    public readonly depth?: number;

    constructor(depth?: number) {
        this.depth = depth;
        this.data = [];
    }

    public push(item: T) {
        if (this.depth && this.data.length === this.depth) {
            this.data.shift();
        }
        this.data.push(item);
    }

    public pop(): T {
        if (this.data.length > 0) {
            return this.data.pop();
        }
        return null;
    }

    public get(index: number): T {
        if (index < this.data.length) {
            return this.data[this.data.length - 1 - index];
        }
        return null;
    }

    public find(selector: (item: T) => boolean): T {
        for (let i = 0; i < this.data.length; i++) {
            let item = this.get(i);
            if (selector(item)) {
                return item;
            }
        }
        return null;
    }
}