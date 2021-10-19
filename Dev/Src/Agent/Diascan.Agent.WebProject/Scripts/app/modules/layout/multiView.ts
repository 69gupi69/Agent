import { Stack } from "../../types/datastructures";
import BaseControl from "../baseControl";

const panContent = `<div style="height: 100%;width: 100%; box-sizing: border-box"></div>`;

class MultiView extends BaseControl {
    private _stack: Stack<BaseControl> = new Stack<BaseControl>();

    protected find<T extends BaseControl>(constructor: { new(...args: any[]): T }): T {
        return <T>this._stack.find(i => i instanceof constructor);
    }

    dispose() {
        let item = this._stack.pop();
        while (item) {
            item.dispose();
            item = this._stack.pop();
        }
    }

    constructor(container: JQuery) {
        super(container);
    }

    public init() {

    }


    public push(create: (c: JQuery) => BaseControl);
    push(create: (c: JQuery) => any);
    public push(create: ((c: JQuery) => any) | ((c: JQuery) => any)) {
        if (this._stack.get(0)) {
            this._stack.get(0).container.hide();
        }
        let newItem = $(panContent);
        newItem.appendTo(this.container);
        this._stack.push(create(newItem));
    }

    public pushTo<T extends BaseControl>(create: (c: JQuery) => T, constructor: { new(...args: any[]): T }, controlId?: string): T {
        let result: T = undefined;
        result = this.find(constructor);
        if (result && (!controlId || result.id === controlId)) {
            return this.popTo(result);
        }
        this.push(create);
        return <T>this.current;

    }

    public pushAfter<T extends BaseControl>(create: (c: JQuery) => T, constructor: { new(...args: any[]): BaseControl }) {
        this.popTo(constructor, true);
        this.push(create);
    }

    public pop(silent: boolean = false) {
        let current = this._stack.pop();
        if (current) {
            current.container.hide();
            current.dispose();
            current.container.remove();

        }
        if (this.current) {
            this.current.container.show();
            if (!silent)
                this.current.refresh();
        }
    }

    public popTo<T extends BaseControl>(item: T, silent?: boolean): T;
    public popTo<T extends BaseControl>(constructor: { new(...args: any[]): T }, silent?: boolean): T;
    public popTo<T extends BaseControl>(itemOrConstructor?: T | { new(...args: any[]): T }, silent: boolean = false): T {


        if (itemOrConstructor instanceof BaseControl) {
            let item = itemOrConstructor as T;
            while (this.current && !(this.current == item)) {

                this.pop(silent);
            }
            return <T>this.current;
        } else {
            let constructor = itemOrConstructor as { new(...args: any[]): T };
            while (this.current && !(this.current instanceof constructor)) {

                this.pop(silent);
            }
            return <T>this.current;
        }

    }

    public get current() {
        return this._stack.get(0);
    }

    static createControlId(controlId: string, objectId?: string | number) {
        if (objectId) {
            return controlId + "_" + objectId;
        } else {
            return controlId;
        }
    }

}

export { MultiView }
export default MultiView