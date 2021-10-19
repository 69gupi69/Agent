import BaseControl from "../baseControl";

interface IIdentifiersEditorEvents {
    create: (e: IdentifiersEditorModel) => void;
    cancel: () => void;
}
interface IIdentifiersEditorOptions {
    events?: IIdentifiersEditorEvents;
    created?: (e?: any) => void;
    cancel?: () => void,
}

const  change = "_change", type = "_type", sensorCount = "_sensorCount", carrierDiametr = "_carrierDiametr",
    numberSensorBlock = "_numberSensorBlock", speedMin = "_speedMin", speedMax = "_speedMax", defectoscope = "_defectoscope",
    editIsEnabled = "_editIsEnabled", create = "_create", cancel = "_cancel", validate = "_validate";


const content = `<div class="k-panel">
    <div class="k-header">Добавление нового идентификатора</div>
    <div class="form-horizontal panel-body" data-role="validator">
  <div class="form-group row">
                
  <div class="form-group row">
                <label class="col-sm-3 control-label" for="type">Тип носителя</label>
                <div class="col-sm-8">
                    <input name="type" class="k-textbox" style="width: 100%" placeholder="Введите тип носителя"
                           data-bind="value:${type}, events:{change: ${validate}}" required
                           validationmessage="Необходимо заполнить поле"/>
                    <div class="text-right">
                        <span class="k-invalid-msg" data-for="type"></span>
                    </div>
                </div>
            </div>
  <div class="form-group row">
                <label class="col-sm-3 control-label" for="sensorCount">Количество датчиков</label>
                <div class="col-sm-8">
                    <input name="sensorCount" class="k-textbox" style="width: 100%" placeholder="Введите количество датчиков"
                           data-bind="value:${sensorCount}, events:{change:${validate}}" required
                           validationmessage="Необходимо заполнить поле"/>
                    <div class="text-right">
                        <span class="k-invalid-msg" data-for="sensorCount"></span>
                    </div>
                </div>
            </div>
  <div class="form-group row">
                <label class="col-sm-3 control-label" for="carrierDiametr">Диаметр носителя, дюймы</label>
                <div class="col-sm-8">
                    <input name="carrierDiametr" class="k-textbox" style="width: 100%" placeholder="Введите диаметр носителя, дюймы"
                           data-bind="value:${carrierDiametr}, events:{change:${validate}}" required
                           validationmessage="Необходимо заполнить поле"/>
                    <div class="text-right">
                        <span class="k-invalid-msg" data-for="carrierDiametr"></span>
                    </div>
                </div>
            </div>
  <div class="form-group row">
                <label class="col-sm-3 control-label" for="numberSensorsBlock">Количество датчиков в блоке датчиков</label>
                <div class="col-sm-8">
                    <input name="numberSensorsBlock" class="k-textbox" style="width: 100%" placeholder="Введите количество датчиков в блоке датчиков"
                           data-bind="value:${numberSensorBlock}, events:{change:${validate}}" required
                           validationmessage="Необходимо заполнить поле"/>
                    <div class="text-right">
                        <span class="k-invalid-msg" data-for="numberSensorsBlock"></span>
                    </div>
                </div>
            </div>
  <div class="form-group row">
                <label class="col-sm-3 control-label" for="speedMin">Минимальное значение допустимой скорости</label>
                <div class="col-sm-8">
                    <input name="speedMin" class="k-textbox" style="width: 100%" placeholder="Введите минимальное значение допустимой скорости"
                           data-bind="value:${speedMin}, events:{change:${validate}}" required
                           validationmessage="Необходимо заполнить поле"/>
                    <div class="text-right">
                        <span class="k-invalid-msg" data-for="speedMin"></span>
                    </div>
                </div>
            </div>
<div class="form-group row">
                <label class="col-sm-3 control-label" for="speedMax">Максимальное значение допустимой скорости</label>
                <div class="col-sm-8">
                    <input name="speedMax" class="k-textbox" style="width: 100%" placeholder="Введите максимальное значение допустимой скорости"
                           data-bind="value:${speedMax}, events:{change:${validate}}" required
                           validationmessage="Необходимо заполнить поле"/>
                    <div class="text-right">
                        <span class="k-invalid-msg" data-for="speedMax"></span>
                    </div>
                </div>
            </div>
    <div class="form-group row">
                <label class="col-sm-3 control-label" for="defectoscope">Наименование дефектоскопа</label>
                <div class="col-sm-8">
                    <input name="defectoscope" class="k-textbox" style="width: 100%" placeholder="Введите наименование дефектоскопа"
                           data-bind="value:${defectoscope}, events:{change:${validate}}" required
                           validationmessage="Необходимо заполнить поле"/>
                    <div class="text-right">
                        <span class="k-invalid-msg" data-for="defectoscope"></span>
                    </div>
                </div>
            </div>
            <div class="row panel-group">
                 <div class="col-md-2"></div>
                 <div class="col-md-8 validationMessage"></div>
            </div>
    </div> 
 <div class="form-group row">
                    <label class="col-sm-3 control-label">Изменение CDS/CDL</label>
                    <div class="col-sm-8">
                        <input type="checkbox" name="change" data-type="boolean"
                               data-bind="checked:${change}"/>
                    </div>
                </div>

    <div class="panel-footer">
        <button data-bind="enabled:${editIsEnabled}, events:{click:${create}" class="k-button k-button-icontext"><span class="k-icon k-i-save"></span>${("Сохранить")}</button>
        <button data-bind="events:{click:${cancel}}" class="k-button k-button-icontext" ><span class="k-icon k-i-cancel"></span>${("Отмена")}</button>
    </div>
</div>`;


class IdentifiersEditor extends BaseControl {
    private _viewModel: IdentifiersEditorModel;
    private _options: IIdentifiersEditorOptions;

   
    constructor(container: JQuery, options: IIdentifiersEditorOptions) {
        super(container);
        this._options = options;
        this.replaceContainer(content);
        this._viewModel = new IdentifiersEditorModel();
       
        this._viewModel.create = () => { options.events.create(this._viewModel) };
        this._viewModel.cancel = () => { options.events.cancel() };
        this._viewModel.validate = this.validate;
    }
    init() {
        kendo.bind(this.container, this._viewModel);
        if (this._options && this._options.created) {
            this.subscribe("created", this._options.created);
        }
        if (this._options && this._options.cancel) {
            this.subscribe("cancel", this._options.cancel);
        }
    }

  
    dispose() {
        if (this._options && this._options.created) {
            this.unsubscribe("created", this._options.created);
        }
        if (this._options && this._options.cancel) {
            this.unsubscribe("cancel", this._options.cancel);
        }
    }
    private validate = () => {
        if (this._viewModel.type && this._viewModel.speedMin && this._viewModel.speedMax && this._viewModel.sensorCount
            && this._viewModel.numberSensorBlock && this._viewModel.carrierDiametr && this._viewModel.defectoscope) {
            this._viewModel.editIsEnabled = true;
            return;
        }
        this._viewModel.editIsEnabled = false;
    };

  
}
class IdentifiersEditorModel extends kendo.data.ObservableObject {
    get change() {
        return super.get(change);
    }
    set change(value: string) {
        super.set(change, value);
    }
    get type() {
        return super.get(type);
    }
    set type(value: string) {
        super.set(type, value);
    }
    get sensorCount() {
        return super.get(sensorCount);
    }
    set sensorCount(value: string) {
        super.set(sensorCount, value);
    }
    get carrierDiametr() {
        return super.get(carrierDiametr);
    }
    set carrierDiametr(value: string) {
        super.set(carrierDiametr, value);
    }
    get numberSensorBlock() {
        return super.get(numberSensorBlock);
    }
    set numberSensorBlock(value: string) {
        super.set(numberSensorBlock, value);
    }
    get speedMin() {
        return super.get(speedMin);
    }
    set speedMin(value: string) {
        super.set(speedMin, value);
    }
    get speedMax() {
        return super.get(speedMax);
    }
    set speedMax(value: string) {
        super.set(speedMax, value);
    }
    get defectoscope() {
        return super.get(defectoscope);
    }
    set defectoscope(value: string) {
        super.set(defectoscope, value);
    }

    set create(value: (e: any) => void) {
        super.set(create, value);
    }

    set cancel(value: () => void) {
        super.set(cancel, value);
    }
    set validate(value: () => void) {
        super.set(validate, value);
    }
    set editIsEnabled(value: boolean) {
        super.set(editIsEnabled, value);
    }
    
}
export { IdentifiersEditor, IdentifiersEditorModel}
export default IdentifiersEditor