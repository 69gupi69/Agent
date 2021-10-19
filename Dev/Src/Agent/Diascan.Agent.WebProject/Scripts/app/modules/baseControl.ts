import { replaceElement } from "./utils/container";

interface IEventArgs {
    type: string,
    data?: any
}

abstract class BaseObserver {
    protected observers: ((e: IEventArgs) => void)[] = [];

    subscribe(observer: (e: IEventArgs) => void) {
        this.observers.push(observer);
    }

    unsubscribe(observer: (e: IEventArgs) => void) {
        let index = this.observers.indexOf(observer);
        if (index > -1) {
            this.observers.splice(index, 1);
        }
    }


}


class Observer extends BaseObserver {
    private _type: string;
    public get type(): string {
        return this._type;
    }

    constructor(type: string) {
        super();
        this._type = type;
    }

    notifyAll(data?: any) {
        for (let o of this.observers) {
            o({ type: this._type, data: data });
        }
    }
}

class GlobalObserver extends BaseObserver {
    notifyAll(type: string, data?: any) {
        for (let o of this.observers) {
            o({ type: type, data: data });
        }
    }
}

abstract class BaseControl {
    get id(): string {
        return this._id;
    }

    private _observers: { [id: string]: Observer } = {};
    private _globalObserver: GlobalObserver = new GlobalObserver();
    private _container: JQuery;
    private _id: string;
    private _dataId: string | number;

    public get dataId(): string | number {
        return this._dataId;
    }
    protected setDataId(dataId: string | number) {
        this._dataId = dataId;
    }

    subscribe(eventType: string, observer: (e: IEventArgs) => void) {
        if (!this._observers[eventType]) {
            this._observers[eventType] = new Observer(eventType);
        }
        this._observers[eventType].subscribe(observer);
    }

    unsubscribe(eventType: string, observer: (e: IEventArgs) => void) {
        if (this._observers[eventType]) {
            this._observers[eventType].unsubscribe(observer);
        }
    }

    subscribeGlobal(observer: (e: IEventArgs) => void) {
        this._globalObserver.subscribe(observer);
    }

    unsubscribeGlobal(observer: (e: IEventArgs) => void) {
        this._globalObserver.unsubscribe(observer);
    }

    protected notify(eventType: string, data?: any) {
        if (this._observers[eventType]) {
            this._observers[eventType].notifyAll(data);
        }
        this._globalObserver.notifyAll(eventType, data);
    }


    constructor(container: JQuery, id?: string, dataId: string | number = "") {
        this._container = container;
        this._id = id;
        this._dataId = dataId;
    }

    public get container(): JQuery {
        return this._container;
    }

    protected replaceContainer(content: string) {
        this._container = replaceElement(this._container, content);
    }

    public refresh() {
    };

    public abstract dispose();

    public abstract init();
}

export default BaseControl