class Observer {
    private observers: ((e?: any) => void)[] = [];

    subscribe(observer: (e?: any) => void) {
        this.observers.push(observer);
    }

    unsubscribe(observer: (e?: any) => void) {
        let index = this.observers.indexOf(observer);
        if (index > -1) {
            this.observers.splice(index, 1);
        }
    }
    ;
    notifyAll(e?: any) {
        for (let o of this.observers) {
            o(e);
        }
    }
}

class ObserverControl {
    private observers: { [id: string]: Observer } = {};

    subscribe(eventType: string, observer: (e?: any) => void) {
        if (!this.observers[eventType]) {
            this.observers[eventType] = new Observer();
        }
        this.observers[eventType].subscribe(observer);
    }

    unsubscribe(eventType: string, observer: (e?: any) => void) {
        if (this.observers[eventType]) {
            this.observers[eventType].unsubscribe(observer);
        }
    }

    subscribeAll(observer: (e?: any) => void) {
        for (let k in this.observers) {
            this.observers[k].subscribe(observer);
        }
    }

    unsubscribeAll(observer: (e?: any) => void) {
        for (let k in this.observers) {
            this.observers[k].unsubscribe(observer);
        }
    }

    protected notify(eventType: string, e?: any) {
        if (this.observers[eventType]) {
            this.observers[eventType].notifyAll(e);
        }
    }
}

export { Observer, ObserverControl };
export default ObserverControl;