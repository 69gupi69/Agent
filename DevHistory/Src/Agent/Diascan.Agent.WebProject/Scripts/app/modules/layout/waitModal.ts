const content = `
<div class="waitModal" style="display: none;">
    <span>Message</span>
    <div class="sk-fading-circle">
      <div class="sk-circle1 sk-circle"></div>
      <div class="sk-circle2 sk-circle"></div>
      <div class="sk-circle3 sk-circle"></div>
      <div class="sk-circle4 sk-circle"></div>
      <div class="sk-circle5 sk-circle"></div>
      <div class="sk-circle6 sk-circle"></div>
      <div class="sk-circle7 sk-circle"></div>
      <div class="sk-circle8 sk-circle"></div>
      <div class="sk-circle9 sk-circle"></div>
      <div class="sk-circle10 sk-circle"></div>
      <div class="sk-circle11 sk-circle"></div>
      <div class="sk-circle12 sk-circle"></div>
    </div>
</div>`;

class WaitModal {
    private _container: JQuery;

    constructor() {
        this._init();
    }

    private _init() {
        this._container = $(content).appendTo("body");

    }

    private _show(message?: string) {
        if (message) {
            this._container.find("span").text(message).show();
        }
        else {
            this._container.find("span").text('').hide();
        }
        this._container.show();
    }

    private _timer: number;

    public show(message?: string, duration?: number) {
        if (duration) {
            this._timer = setTimeout(() => this._show(message), duration);
        }
        else {
            this._show(message);
        }
    }

    public hide() {
        if (this._timer) {
            clearTimeout(this._timer);
            this._timer = undefined;
        }
        this._container.hide();
    }

    private static _wait: WaitModal = undefined;
    protected static get wait(): WaitModal {
        if (!WaitModal._wait) {
            WaitModal._wait = new WaitModal();
        }
        return WaitModal._wait;
    }

    public static show(message?: string, duration?: number) {
        WaitModal.wait.show(message, duration);
    }

    public static hide() {
        WaitModal.wait.hide();
    }

}

export default WaitModal;